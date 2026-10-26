// Project D  |  Codename: Zantetsuken
// DuelModelReport.cs  (Editor-only)
//
//   Tools > Project D > Save Model Material Report
//
// Writes Builds/ModelMaterialReport.txt: for every model in Assets/Models, its rig type, whether Unity built a Humanoid
// avatar, each material and which texture it ended up with, and the texture files in the folder. Use it to see why a
// model is white (materials with no texture) or still in a T-pose (no valid Humanoid avatar).
#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public static class DuelModelReport
    {
        private const string OutputPath = "Builds/ModelMaterialReport.txt";

        [MenuItem("Tools/Project D/Save Model Material Report", priority = 47)]
        public static void Menu()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Project D - model report  " + System.DateTime.Now);
            if (!AssetDatabase.IsValidFolder("Assets/Models")) sb.AppendLine("No Assets/Models folder.");
            else
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/Models" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.ToLowerInvariant().Contains("weapon")) continue;
                    sb.AppendLine();
                    sb.AppendLine("== " + path);
                    var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer != null) sb.AppendLine("rig: " + importer.animationType + "  avatarSetup: " + importer.avatarSetup);

                    bool humanAvatar = false;
                    int mats = 0, untextured = 0;
                    foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
                    {
                        var avatar = o as Avatar;
                        if (avatar != null)
                        {
                            sb.AppendLine("avatar: " + avatar.name + " valid=" + avatar.isValid + " human=" + avatar.isHuman);
                            if (avatar.isValid && avatar.isHuman) humanAvatar = true;
                        }
                        var m = o as Material;
                        if (m == null) continue;
                        mats++;
                        Texture t = null;
                        if (m.HasProperty("_BaseMap")) t = m.GetTexture("_BaseMap");
                        if (t == null && m.HasProperty("_MainTex")) t = m.GetTexture("_MainTex");
                        if (t == null) untextured++;
                        sb.AppendLine("  material '" + m.name + "'  shader=" + (m.shader != null ? m.shader.name : "null") +
                                      "  base=" + (t != null ? t.name : "NONE"));
                    }
                    sb.AppendLine("summary: " + mats + " materials, " + untextured + " without a base texture; humanoid avatar: " + humanAvatar);

                    string folder = Path.GetDirectoryName(path).Replace('\\', '/');
                    int n = 0;
                    var names = new StringBuilder();
                    foreach (var tg in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
                    {
                        if (n++ < 60) names.Append(Path.GetFileName(AssetDatabase.GUIDToAssetPath(tg))).Append("  ");
                    }
                    sb.AppendLine("textures in folder: " + n + (n > 60 ? " (first 60)" : "") + ": " + names);
                }
            }

            Directory.CreateDirectory("Builds");
            File.WriteAllText(OutputPath, sb.ToString());
            Debug.Log("[Project D] Model report saved to " + OutputPath + "\n" + sb);
            if (!Application.isBatchMode) EditorUtility.RevealInFinder(OutputPath);
        }
    }
}
#endif
