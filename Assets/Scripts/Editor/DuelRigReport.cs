// Project D  |  Codename: Zantetsuken
// DuelRigReport.cs  (Editor-only)
//
//   Tools > Project D > Save Model Rig Report
//
// Writes Builds/ModelRigReport.txt: for every linked model, the bones the animator found (hips, spine, arms, legs) and the
// whole skeleton as text (bone name, parent, position, rotation, in the same pose the game starts from). That is enough to
// check and tune the procedural animation offline without the licensed art ever leaving your machine or entering git.
#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using ProjectD.DuelDemo;

namespace ProjectD.EditorTools
{
    public static class DuelRigReport
    {
        private const int MaxBones = 800;

        [MenuItem("Tools/Project D/Save Model Rig Report", priority = 47)]
        public static void Menu()
        {
            var lib = AssetDatabase.LoadAssetAtPath<DuelModelLibrary>(DuelModelLinker.LibraryPath);
            if (lib == null)
            {
                EditorUtility.DisplayDialog("Project D", "No model library yet. Run Tools > Project D > Link Duel Demo Models first.", "OK");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Project D rig report. Positions are world units in the game's start pose (model not yet scaled), rotations are local Euler degrees.");
            int models = 0;
            foreach (var e in lib.entries)
            {
                if (e == null || e.prefab == null) continue;
                models++;
                Report(sb, e);
            }

            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", "Builds");
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, "ModelRigReport.txt");
            File.WriteAllText(file, sb.ToString());
            string msg = "Rig report for " + models + " model(s) saved to:\n" + file + "\n\nAttach that file in the chat.";
            Debug.Log("[Project D] " + msg);
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Project D - rig report", msg, "OK");
        }

        private static void Report(StringBuilder sb, DuelModelLibrary.Entry e)
        {
            var holder = new GameObject("RigReportHolder");
            try
            {
                holder.transform.rotation = Quaternion.Euler(0f, e.yawOffset, 0f);
                var inst = (GameObject)Object.Instantiate(e.prefab, holder.transform);
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;

                sb.AppendLine();
                sb.AppendLine("== " + e.id + "  " + AssetDatabase.GetAssetPath(e.prefab) + "  yaw=" + e.yawOffset + " height=" + e.height);

                var renderers = inst.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    var b = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                    sb.AppendLine("bounds center " + V(b.center) + " size " + V(b.size));
                }

                var anim = inst.GetComponentInChildren<Animator>();
                sb.AppendLine("animator: " + (anim == null ? "none" : "avatar " + (anim.avatar != null && anim.avatar.isValid ? (anim.avatar.isHuman ? "humanoid" : "generic") : "none") +
                                              ", controller " + (anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "none")));

                var rig = DuelRigFinder.Find(inst, holder.transform, out string why);
                if (rig == null) sb.AppendLine("rig: NOT FOUND (" + why + ")");
                else
                {
                    sb.AppendLine("rig: " + rig.Source + ", frameYaw " + rig.FrameYaw + ", complete " + rig.Complete + (why.Length > 0 ? " (" + why + ")" : ""));
                    sb.AppendLine("  hips " + N(rig.Hips) + " spine " + N(rig.Spine) + " chest " + N(rig.Chest) + " head " + N(rig.Head));
                    for (int i = 0; i < 2; i++)
                        sb.AppendLine("  side[" + i + "] upperArm " + N(rig.UpperArm[i]) + " lowerArm " + N(rig.LowerArm[i]) + " hand " + N(rig.Hand[i]) +
                                      " | upperLeg " + N(rig.UpperLeg[i]) + " lowerLeg " + N(rig.LowerLeg[i]) + " foot " + N(rig.Foot[i]));
                }

                sb.AppendLine("bones: depth | name | parent | world position | local rotation");
                int count = 0;
                Dump(inst.transform, 0, sb, ref count);
                if (count >= MaxBones) sb.AppendLine("(truncated at " + MaxBones + " bones)");
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }

        private static void Dump(Transform t, int depth, StringBuilder sb, ref int count)
        {
            if (count >= MaxBones) return;
            bool mesh = t.GetComponent<Renderer>() != null;
            if (!mesh)
            {
                count++;
                sb.AppendLine(depth + " | " + t.name + " | " + (t.parent != null ? t.parent.name : "-") + " | " + V(t.position) + " | " + V(t.localEulerAngles));
            }
            for (int i = 0; i < t.childCount; i++) Dump(t.GetChild(i), depth + 1, sb, ref count);
        }

        private static string N(Transform t) { return t == null ? "-" : t.name; }

        private static string V(Vector3 v) { return v.x.ToString("0.###") + "," + v.y.ToString("0.###") + "," + v.z.ToString("0.###"); }
    }
}
#endif
