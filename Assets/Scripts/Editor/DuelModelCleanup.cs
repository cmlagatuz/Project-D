// Project D  |  Codename: Zantetsuken
// DuelModelCleanup.cs  (Editor-only)
//
//   Tools > Project D > Move Unsupported Model Files Out of Assets
//
// Unity reserves the ".mesh" extension for its OWN serialized meshes. Game rips often contain ".mesh" files in a
// different, engine-specific format; Unity tries to load them and logs
//   "Failed to load '.../generic_item.mesh'. File may be corrupted or was serialized with a newer version of Unity."
// They cannot be used in Unity anyway, so this tool MOVES them (it never deletes) from Assets/Models to
// <project>/ModelsUnsupported/, keeping their sub-folders, and removes their .meta files.
// "Link Duel Demo Models" and "Build Duel Demo Scene" run it automatically.
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public static class DuelModelCleanup
    {
        [MenuItem("Tools/Project D/Move Unsupported Model Files Out of Assets", priority = 47)]
        public static void Menu()
        {
            int moved = MoveUnsupported();
            string msg = moved == 0
                ? "No unsupported .mesh files found under Assets/Models."
                : "Moved " + moved + " .mesh file(s) to <project>/ModelsUnsupported/ (nothing was deleted).";
            Debug.Log("[Project D] " + msg);
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Project D", msg, "OK");
        }

        /// <summary>Moves *.mesh files out of Assets/Models. Returns how many were moved.</summary>
        public static int MoveUnsupported()
        {
            string modelsDir = Path.Combine(Application.dataPath, "Models");
            if (!Directory.Exists(modelsDir)) return 0;

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outRoot = Path.Combine(projectRoot, "ModelsUnsupported");
            int moved = 0;

            foreach (var file in Directory.GetFiles(modelsDir, "*.mesh", SearchOption.AllDirectories))
            {
                if (!file.EndsWith(".mesh", StringComparison.OrdinalIgnoreCase)) continue;   // *.mesh also matches longer extensions on some systems
                string rel = file.Substring(modelsDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string dest = Path.Combine(outRoot, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                if (File.Exists(dest)) File.Delete(dest);
                File.Move(file, dest);
                string meta = file + ".meta";
                if (File.Exists(meta)) File.Delete(meta);
                moved++;
            }

            if (moved > 0)
            {
                Debug.Log("[Project D] Moved " + moved + " unsupported .mesh file(s) from Assets/Models to " + outRoot);
                AssetDatabase.Refresh();
            }
            return moved;
        }
    }
}
#endif
