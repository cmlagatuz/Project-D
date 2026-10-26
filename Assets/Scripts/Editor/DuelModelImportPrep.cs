// Project D  |  Codename: Zantetsuken
// DuelModelImportPrep.cs  (Editor-only)
//
// Automatically prepares anything imported under Assets/Models/ for the duel demo on a phone:
//   models   : no cameras/lights/blend shapes, compressed meshes, 4 bone weights per vertex
//   textures : capped at 1024 px, ASTC on Android, normal/data maps detected by file name
//   materials: switched to URP Lit (so they are not pink); base / normal / emission textures are found by name when
//              the FBX did not link them (e.g. t_..._body_a_base.tga, ..._norm.tga, ..._emis.tga)
//
//   Tools > Project D > Fix Model Materials (reimport models)   re-runs all of it on models already imported.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public sealed class DuelModelImportPrep : AssetPostprocessor
    {
        private const string Root = "Assets/Models/";
        private const int MaxTextureSize = 1024;

        private static readonly string[] DataMapWords = { "mask", "roug", "occl", "thic", "meta", "flow", "disp", "specular", "metal", "height", "ao" };

        [MenuItem("Tools/Project D/Fix Model Materials (reimport models)", priority = 46)]
        public static void ReimportMenu()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Models"))
            {
                Debug.LogWarning("[Project D] No Assets/Models folder yet. Run Tools > Project D > Create Model Folders.");
                return;
            }
            AssetDatabase.ImportAsset("Assets/Models", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            Debug.Log("[Project D] Reimported Assets/Models with the phone/URP preparation.");
        }

        // ------------------------------------------------------------------ models

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Root)) return;
            var importer = (ModelImporter)assetImporter;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.skinWeights = ModelImporterSkinWeights.Standard;

            // Champion bodies get a Humanoid avatar so the demo can pose them (arms down, walk, attack) even though rips
            // have no clips, and so real humanoid clips (e.g. Mixamo) can be retargeted onto them later.
            if (UseHumanoidRigs && IsChampionBody(assetPath))
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            }
        }

        /// <summary>Set to false if a model's Humanoid avatar fails to build and you would rather keep it Generic.</summary>
        private const bool UseHumanoidRigs = true;

        private static readonly string[] ChampionFolders = { "warrioroflight", "firion", "cecil", "garland", "emperor", "golbez" };

        private static bool IsChampionBody(string path)
        {
            string rest = path.Substring(Root.Length);
            int slash = rest.IndexOf('/');
            if (slash < 0) return false;
            string folder = Normalize(rest.Substring(0, slash));
            if (System.Array.IndexOf(ChampionFolders, folder) < 0) return false;
            string lower = path.ToLowerInvariant();
            return !lower.Contains("weapon") && !lower.Contains("@");
        }

        // ------------------------------------------------------------------ textures

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root)) return;
            var importer = (TextureImporter)assetImporter;
            string name = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();

            if (name.EndsWith("_norm") || name.EndsWith("_nrm") || name.EndsWith("_normal") || name.EndsWith("_n"))
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else
            {
                foreach (var word in DataMapWords)
                {
                    if (name.Contains(word)) { importer.sRGBTexture = false; break; }
                }
            }

            importer.maxTextureSize = MaxTextureSize;
            importer.mipmapEnabled = true;
            importer.isReadable = false;

            var android = new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = MaxTextureSize,
                format = TextureImporterFormat.ASTC_6x6,
                textureCompression = TextureImporterCompression.Compressed
            };
            importer.SetPlatformTextureSettings(android);
        }

        // ------------------------------------------------------------------ materials

        private void OnPostprocessMaterial(Material material)
        {
            if (!assetPath.StartsWith(Root)) return;
            ConvertToUrpLit(material, assetPath);
        }

        private static void ConvertToUrpLit(Material m, string modelPath)
        {
            var urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp == null) return;   // not a URP project: leave the importer's material alone

            Texture baseTex = null;
            Texture bumpTex = null;
            var color = Color.white;
            if (m.HasProperty("_BaseMap")) baseTex = m.GetTexture("_BaseMap");
            if (baseTex == null && m.HasProperty("_MainTex")) baseTex = m.GetTexture("_MainTex");
            if (m.HasProperty("_BumpMap")) bumpTex = m.GetTexture("_BumpMap");
            if (m.HasProperty("_BaseColor")) color = m.GetColor("_BaseColor");
            else if (m.HasProperty("_Color")) color = m.GetColor("_Color");

            if (m.shader != urp) m.shader = urp;

            string unitFolder = UnitFolder(modelPath);
            string key = Normalize(m.name);
            Texture emisFromFbx = null;

            // 1) the material -> texture links written inside the FBX file itself (Unity's importer often ignores them in rips)
            var fbx = DuelFbxTextureMap.Load(modelPath);
            if (fbx != null && fbx.ByMaterial.Count > 0)
            {
                Dictionary<string, string> slots = null;
                if (!fbx.ByMaterial.TryGetValue(m.name, out slots))
                    foreach (var kv in fbx.ByMaterial) if (Normalize(kv.Key) == key) { slots = kv.Value; break; }
                if (slots != null)
                {
                    if (baseTex == null) baseTex = ResolveFile(modelPath, unitFolder, PickSlot(slots, true, "diffuse", "base", "color", "albedo"));
                    if (bumpTex == null) bumpTex = ResolveFile(modelPath, unitFolder, PickSlot(slots, false, "normal", "bump"));
                    emisFromFbx = ResolveFile(modelPath, unitFolder, PickSlot(slots, false, "emissive", "emission"));
                }
            }

            // 2) names: "sm_5_m_body_a" -> "body_a" ... "t_c6005b0001_body_a_base.tga"
            string shortKey = ShortKey(m.name);
            if (baseTex == null) baseTex = FindTexture(unitFolder, key, new[] { "base", "basecolor", "albedo", "diffuse", "color" });
            if (baseTex == null && shortKey.Length >= 3) baseTex = FindTexture(unitFolder, shortKey, new[] { "base", "basecolor", "albedo", "diffuse", "color" });
            if (baseTex == null) baseTex = FindTextureLoose(unitFolder, m.name);
            if (bumpTex == null) bumpTex = FindTexture(unitFolder, key, new[] { "norm", "normal", "nrm" });
            if (bumpTex == null && shortKey.Length >= 3) bumpTex = FindTexture(unitFolder, shortKey, new[] { "norm", "normal", "nrm" });
            var emisTex = emisFromFbx != null ? emisFromFbx : FindTexture(unitFolder, key, new[] { "emis", "emissive", "emission" });
            if (emisTex == null && shortKey.Length >= 3) emisTex = FindTexture(unitFolder, shortKey, new[] { "emis", "emissive", "emission" });

            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", baseTex);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.25f);

            if (bumpTex != null && m.HasProperty("_BumpMap"))
            {
                m.SetTexture("_BumpMap", bumpTex);
                m.EnableKeyword("_NORMALMAP");
            }
            if (emisTex != null && m.HasProperty("_EmissionMap"))
            {
                m.SetTexture("_EmissionMap", emisTex);
                m.SetColor("_EmissionColor", Color.white);
                m.EnableKeyword("_EMISSION");
            }
        }

        /// <summary>"sm_5_m_body_a" -> "bodya"; "model_1_mat_3" -> "" (no readable part).</summary>
        private static string ShortKey(string materialName)
        {
            string n = materialName.ToLowerInvariant();
            int m = n.IndexOf("_m_", System.StringComparison.Ordinal);
            if (m >= 0) n = n.Substring(m + 3);
            else if (n.StartsWith("m_")) n = n.Substring(2);
            else return "";
            return Normalize(n);
        }

        private static string PickSlot(Dictionary<string, string> slots, bool allowOnlyEntry, params string[] words)
        {
            foreach (var kv in slots)
            {
                string k = kv.Key.ToLowerInvariant();
                foreach (var w in words) if (k.Contains(w)) return kv.Value;
            }
            if (allowOnlyEntry && slots.Count == 1) foreach (var kv in slots) return kv.Value;
            return null;
        }

        /// <summary>Finds the texture asset for a file name written in the FBX; prefers the one next to the FBX.</summary>
        private static Texture ResolveFile(string modelPath, string unitFolder, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !AssetDatabase.IsValidFolder(unitFolder)) return null;
            string dir = Path.GetDirectoryName(modelPath).Replace('\\', '/');
            string stem = Path.GetFileNameWithoutExtension(fileName);
            string bestPath = null;
            int bestScore = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { unitFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string pathStem = Path.GetFileNameWithoutExtension(path);
                int score = 0;
                if (string.Equals(Path.GetFileName(path), fileName, System.StringComparison.OrdinalIgnoreCase)) score = 2;
                else if (string.Equals(pathStem, stem, System.StringComparison.OrdinalIgnoreCase)) score = 1;
                if (score == 0) continue;
                if (Path.GetDirectoryName(path).Replace('\\', '/') == dir) score += 10;
                if (score > bestScore) { bestScore = score; bestPath = path; }
            }
            return bestPath == null ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(bestPath);
        }

        /// <summary>"Assets/Models/Cecil/Model/Cecil Harvey.fbx" -> "Assets/Models/Cecil" (textures are searched in here).</summary>
        private static string UnitFolder(string modelPath)
        {
            string rest = modelPath.Substring(Root.Length);
            int slash = rest.IndexOf('/');
            return slash < 0 ? "Assets/Models" : Root + rest.Substring(0, slash);
        }

        private static Texture FindTexture(string folder, string materialKey, string[] suffixes)
        {
            if (string.IsNullOrEmpty(materialKey) || !AssetDatabase.IsValidFolder(folder)) return null;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string tex = Normalize(Path.GetFileNameWithoutExtension(path));
                if (!tex.Contains(materialKey)) continue;
                foreach (var suffix in suffixes)
                {
                    if (tex.EndsWith(suffix)) return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
            }
            return null;
        }

        /// <summary>
        /// Fallback for rips whose textures are numbered (0.tga ... 29.tga) or named exactly like the material:
        /// exact name match, then the material's trailing number against a numbered texture.
        /// </summary>
        private static Texture FindTextureLoose(string folder, string materialName)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return null;
            string key = Normalize(materialName);
            string digits = TrailingDigits(materialName);
            Texture numbered = null;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string tex = Normalize(Path.GetFileNameWithoutExtension(path));
                if (tex.EndsWith("norm") || tex.EndsWith("nrm") || tex.EndsWith("normal") || tex.EndsWith("emis")) continue;
                if (tex == key) return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (numbered == null && digits.Length > 0 && IsAllDigits(tex) && int.Parse(tex) == int.Parse(digits))
                    numbered = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return numbered;
        }

        private static string TrailingDigits(string s)
        {
            int end = s.Length;
            while (end > 0 && !char.IsDigit(s[end - 1])) end--;   // "mat_12 (Instance)" style endings are rare; skip trailing text
            int start = end;
            while (start > 0 && char.IsDigit(s[start - 1])) start--;
            return end > start && end - start < 9 ? s.Substring(start, end - start) : "";
        }

        private static bool IsAllDigits(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length > 8) return false;
            foreach (char c in s) if (!char.IsDigit(c)) return false;
            return true;
        }

        private static string Normalize(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s.ToLowerInvariant()) if (char.IsLetterOrDigit(c)) sb.Append(c);
            return sb.ToString();
        }
    }
}
#endif
