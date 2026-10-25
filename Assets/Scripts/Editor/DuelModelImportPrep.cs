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

            if (baseTex == null) baseTex = FindTexture(unitFolder, key, new[] { "base", "basecolor", "albedo", "diffuse", "color" });
            if (bumpTex == null) bumpTex = FindTexture(unitFolder, key, new[] { "norm", "normal", "nrm" });
            var emisTex = FindTexture(unitFolder, key, new[] { "emis", "emissive", "emission" });

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

        private static string Normalize(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s.ToLowerInvariant()) if (char.IsLetterOrDigit(c)) sb.Append(c);
            return sb.ToString();
        }
    }
}
#endif
