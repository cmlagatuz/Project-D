// Project D  |  Codename: Zantetsuken
// DuelSceneBuilder.cs  (Editor-only)
//
// One click builds the 1v1 duel demo scene used to test the summoner spells:
//   Tools > Project D > Build Duel Demo Scene
// It creates Assets/Scenes/DuelDemo.unity, saves the placeholder materials as assets (so phone builds keep the
// URP shaders - Shader.Find at runtime can be stripped), and wires them into the controller.
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using ProjectD.DuelDemo;
using ProjectD.Gameplay.CameraRig;

namespace ProjectD.EditorTools
{
    public static class DuelSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/DuelDemo.unity";
        private const string MaterialFolder = "Assets/Materials/DuelDemo";

        [MenuItem("Tools/Project D/Build Duel Demo Scene", priority = 41)]
        public static void Build()
        {
            string models = BuildScene();
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Project D",
                    "Duel demo scene built:\n" + ScenePath + "\n\n" + models +
                    "\n\nPress Play.\nWASD move, Space attack, 1-4 = Q/W/E/R, 5/6 (or Z/X) = summoner spells.\n" +
                    "Use SPELLS to pick any of the 11 spells and TOOLS for test helpers.", "OK");
            }
        }

        /// <summary>Builds and saves the scene. Safe to call from other editor tools and from batch mode.</summary>
        public static string BuildScene()
        {
            string modelReport = DuelModelLinker.LinkAll();   // finds Shiva, Garuda, Firion, ... and fills the model library
            var library = AssetDatabase.LoadAssetAtPath<DuelModelLibrary>(DuelModelLinker.LibraryPath);
            if (library == null) Debug.LogError("[Project D] Could not load " + DuelModelLinker.LibraryPath + " - 3D models will not show.");
            else Debug.Log("[Project D] Model library has " + library.entries.Count + " entries; assigning it to the scene.");

            var ground = MakeMaterial("DuelGround", new Color(0.20f, 0.23f, 0.28f), false);
            var blue = MakeMaterial("DuelBlue", new Color(0.25f, 0.5f, 0.95f), false);
            var red = MakeMaterial("DuelRed", new Color(0.9f, 0.3f, 0.3f), false);
            var neutral = MakeMaterial("DuelNeutral", new Color(0.6f, 0.6f, 0.62f), false);
            var ward = MakeMaterial("DuelWard", new Color(0.95f, 0.88f, 0.4f), false);
            var flash = MakeMaterial("DuelFlash", new Color(0.6f, 0.9f, 1f, 0.40f), true);
            var mark = MakeMaterial("DuelMark", new Color(1f, 0.85f, 0.2f), false);
            var fx = MakeFxMaterial("DuelFx");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.96f, 0.9f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.55f);

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(ArenaCameraRig));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.1f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 100f;
            cam.fieldOfView = 45f;
            camGo.transform.position = new Vector3(-6f, 14f, -16f);
            camGo.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var ctrlGo = new GameObject("DuelDemo");
            var ctrl = ctrlGo.AddComponent<DuelDemoController>();
            var so = new SerializedObject(ctrl);
            Assign(so, "groundMat", ground);
            Assign(so, "blueMat", blue);
            Assign(so, "redMat", red);
            Assign(so, "neutralMat", neutral);
            Assign(so, "wardMat", ward);
            Assign(so, "flashMat", flash);
            Assign(so, "markMat", mark);
            Assign(so, "fxMat", fx);
            Assign(so, "models", library);
            Assign(so, "cameraRig", camGo.GetComponent<ArenaCameraRig>());
            Assign(so, "viewCamera", cam);
            so.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Project D] Duel demo scene created at " + ScenePath + "\n" + modelReport);
            return modelReport;
        }

        private static void Assign(SerializedObject so, string field, Object value)
        {
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning("[Project D] DuelDemoController has no serialized field '" + field + "'."); return; }
            p.objectReferenceValue = value;
        }

        private static Material MakeMaterial(string name, Color color, bool transparent)
        {
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialFolder);
            string path = MaterialFolder + "/" + name + ".mat";

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = shader;
            m.color = color;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.1f);

            if (transparent)
            {
                if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
                if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
                if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = 3000;
            }
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Additive URP particle material for the summon effects. Returns null if the shader is missing (the game then falls back).</summary>
        private static Material MakeFxMaterial(string name)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) return null;
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialFolder);
            string path = MaterialFolder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = shader;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 2f);   // additive
            if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3000;
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
