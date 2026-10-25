// Project D  |  Codename: Zantetsuken
// DuelModelFolders.cs  (Editor-only)
//
// Creates the drop folders for the duel demo's 3D models, automatically when the editor loads (and on demand):
//
//   Assets/Models/WarriorOfLight  Firion  Cecil  Garland  Emperor  Golbez  Shiva  Garuda
//
// Paste a character's "Raw Files" contents (the .fbx and its .tga textures, with sub-folders such as Model/ and
// Weapon/ if you like) into that character's folder. Unity imports it, DuelModelImportPrep tidies the import for
// phones and fixes the materials for URP, and Link Duel Demo Models finds the model by its folder name.
//
//   Tools > Project D > Create Model Folders
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectD.EditorTools
{
    [InitializeOnLoad]
    public static class DuelModelFolders
    {
        public const string Root = "Assets/Models";

        public static readonly string[] Names =
        {
            "WarriorOfLight", "Firion", "Cecil", "Garland", "Emperor", "Golbez", "Shiva", "Garuda"
        };

        private const string Readme =
            "Duel demo model drop folders\r\n" +
            "============================\r\n" +
            "Paste each character's Raw Files contents into that character's folder:\r\n" +
            "  the .fbx and all of its .tga textures (sub-folders such as Model/ and Weapon/ are fine).\r\n" +
            "Then: Tools > Project D > Link Duel Demo Models, then Tools > Project D > Build Duel Demo Scene.\r\n\r\n" +
            "This folder is ignored by git on purpose (licensed art stays local until its chain of title is documented).\r\n";

        static DuelModelFolders()
        {
            EditorApplication.delayCall += () => Ensure(false);
        }

        [MenuItem("Tools/Project D/Create Model Folders", priority = 45)]
        public static void Menu()
        {
            Ensure(true);
            var folder = AssetDatabase.LoadAssetAtPath<Object>(Root);
            if (folder != null) EditorGUIUtility.PingObject(folder);
            Debug.Log("[Project D] Model drop folders are under " + Root + ": " + string.Join(", ", Names));
        }

        public static void Ensure(bool force)
        {
            bool changed = false;
            if (!AssetDatabase.IsValidFolder(Root))
            {
                AssetDatabase.CreateFolder("Assets", "Models");
                changed = true;
            }
            foreach (var n in Names)
            {
                string path = Root + "/" + n;
                if (AssetDatabase.IsValidFolder(path)) continue;
                AssetDatabase.CreateFolder(Root, n);
                changed = true;
            }

            string readmePath = Root + "/README.txt";
            if (!File.Exists(readmePath))
            {
                File.WriteAllText(readmePath, Readme);
                changed = true;
            }
            if (changed || force) AssetDatabase.Refresh();
        }
    }
}
#endif
