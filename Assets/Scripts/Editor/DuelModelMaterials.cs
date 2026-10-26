// Project D  |  Codename: Zantetsuken
// DuelModelMaterials.cs  (Editor-only)
//
//   Tools > Project D > Apply Model Textures (from FBX)
//
// Rip FBX files carry material -> texture links that Unity's importer drops, so models come in white. This reads the
// links out of each FBX (DuelFbxTextureMap), creates one URP Lit material per FBX material next to the model
// (Assets/Models/<unit>/Materials/), puts the base texture on it and remaps the model to use those materials.
// It runs in the normal editor process (not in an import worker), so it can look up textures reliably.
// "Link Duel Demo Models", "Build Duel Demo Scene" and "Fix Model Materials" all run it; it skips models that are done.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public static class DuelModelMaterials
    {
        private static readonly string[] BaseSuffixes = { "base", "basecolor", "albedo", "diffuse", "color" };

        [MenuItem("Tools/Project D/Apply Model Textures (from FBX)", priority = 45)]
        public static void Menu()
        {
            string result = ApplyAll(true);
            Debug.Log("[Project D] " + result);
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Project D - model textures", result, "OK");
        }

        /// <summary>Applies textures to every model under Assets/Models. force = redo models that were already done.</summary>
        public static string ApplyAll(bool force)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Models")) return "";
            var sb = new StringBuilder();
            var guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Models" });
            int changed = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLowerInvariant().Contains("weapon")) continue;   // weapons are not attached in the demo yet
                var rigImporter = AssetImporter.GetAtPath(path) as ModelImporter;
                if (DuelModelImportPrep.ApplyRig(rigImporter, path))
                {
                    rigImporter.SaveAndReimport();
                    sb.AppendLine(Path.GetFileName(path) + ": rig set to " + rigImporter.animationType);
                    changed++;
                }
                string line = ApplyFor(path, force);
                if (line != null) { sb.AppendLine(line); changed++; }
            }
            if (changed == 0) return "Model textures: nothing to do.";
            AssetDatabase.SaveAssets();
            return "Model textures applied:\n" + sb;
        }

        private static string ApplyFor(string fbxPath, bool force)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null) return null;

            var map = DuelFbxTextureMap.Load(fbxPath);
            var names = new List<string>();
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
            {
                var m = o as Material;
                if (m != null && !names.Contains(m.name)) names.Add(m.name);
            }
            var remaps = importer.GetExternalObjectMap();
            bool allRemapped = names.Count > 0;
            foreach (var kv in remaps)
            {
                if (kv.Key.type != typeof(Material)) continue;
                if (!names.Contains(kv.Key.name)) names.Add(kv.Key.name);
            }
            if (map != null) foreach (var k in map.ByMaterial.Keys) if (!names.Contains(k)) names.Add(k);
            if (names.Count == 0) return null;

            if (!force)
            {
                allRemapped = true;
                foreach (var n in names)
                {
                    var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), n);
                    if (!remaps.TryGetValue(key, out var target) || target == null) { allRemapped = false; break; }
                }
                if (allRemapped) return null;
            }

            string dir = Path.GetDirectoryName(fbxPath).Replace('\\', '/');
            string folder = dir + "/Materials";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(dir, "Materials");
            string stem = Path.GetFileNameWithoutExtension(fbxPath).Replace(' ', '_');
            string unit = DuelModelImportPrep.UnitFolder(fbxPath);

            var urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp == null) return Path.GetFileName(fbxPath) + ": URP Lit shader not found, skipped.";

            int withTexture = 0;
            var missing = new List<string>();
            foreach (var name in names)
            {
                Texture baseTex = null;
                if (map != null && map.ByMaterial.Count > 0)
                {
                    Dictionary<string, string> slots = null;
                    if (!map.ByMaterial.TryGetValue(name, out slots))
                    {
                        string nk = DuelModelImportPrep.Normalize(name);
                        foreach (var kv in map.ByMaterial) if (DuelModelImportPrep.Normalize(kv.Key) == nk) { slots = kv.Value; break; }
                    }
                    if (slots != null)
                        baseTex = DuelModelImportPrep.ResolveFile(fbxPath, unit, DuelModelImportPrep.PickSlot(slots, true, "diffuse", "base", "color", "albedo"));
                }
                if (baseTex == null)   // models whose FBX has no links: match by name ("sm_5_m_body_a" -> "..._body_a_base")
                {
                    baseTex = DuelModelImportPrep.FindTexture(unit, DuelModelImportPrep.Normalize(name), BaseSuffixes);
                    string shortKey = DuelModelImportPrep.ShortKey(name);
                    if (baseTex == null && shortKey.Length >= 3) baseTex = DuelModelImportPrep.FindTexture(unit, shortKey, BaseSuffixes);
                }

                string matPath = folder + "/" + stem + "__" + Sanitize(name) + ".mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(urp);
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                mat.shader = urp;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.2f);
                if (baseTex != null) { mat.SetTexture("_BaseMap", baseTex); withTexture++; }
                else if (missing.Count < 6) missing.Add(name);
                EditorUtility.SetDirty(mat);

                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name), mat);
            }

            importer.SaveAndReimport();
            string result = Path.GetFileName(fbxPath) + ": " + withTexture + " of " + names.Count + " materials got a texture";
            if (missing.Count > 0) result += " (no texture found for e.g. " + string.Join(", ", missing.ToArray()) + ")";
            return result;
        }

        private static string Sanitize(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s) sb.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_');
            return sb.ToString();
        }
    }
}
#endif
