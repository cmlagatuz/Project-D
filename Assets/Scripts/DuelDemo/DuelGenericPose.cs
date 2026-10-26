// Project D | Codename: Zantetsuken — DuelGenericPose.cs
// For champion rips whose skeleton Unity cannot turn into a Humanoid avatar (numbered bones such as bone_26): finds the
// two outstretched arms by geometry (the bones furthest left and right at shoulder height), and swings each arm down
// from the T-pose with one rotation at the shoulder. Static pose, no walk cycle. Best effort: returns false and changes
// nothing if the skeleton does not look like a T-posed biped.
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.DuelDemo
{
    public static class DuelGenericPose
    {
        public static bool LowerArms(GameObject instance)
        {
            var smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (smrs.Length == 0) return false;

            var bones = new HashSet<Transform>();
            var bounds = smrs[0].bounds;
            for (int i = 0; i < smrs.Length; i++)
            {
                bounds.Encapsulate(smrs[i].bounds);
                var sb = smrs[i].bones;
                for (int k = 0; k < sb.Length; k++) if (sb[k] != null) bones.Add(sb[k]);
            }
            if (bones.Count < 10) return false;

            float h = bounds.size.y;
            if (h < 0.01f) return false;
            float lo = bounds.min.y + 0.55f * h;
            float hi = bounds.min.y + 0.95f * h;

            // Which horizontal axis do the arms stick out along? (models face +Z or +X depending on the rip)
            float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
            foreach (var b in bones)
            {
                var p = b.position;
                if (p.y < lo || p.y > hi) continue;
                minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                minZ = Mathf.Min(minZ, p.z); maxZ = Mathf.Max(maxZ, p.z);
            }
            if (maxX < minX) return false;
            Vector3 axis = (maxX - minX) >= (maxZ - minZ) ? Vector3.right : Vector3.forward;
            Vector3 center = bounds.center;

            Transform tipPos = null, tipNeg = null;
            float latPos = 0f, latNeg = 0f;
            foreach (var b in bones)
            {
                var p = b.position;
                if (p.y < lo || p.y > hi) continue;
                float lat = Vector3.Dot(p - center, axis);
                if (lat > latPos) { latPos = lat; tipPos = b; }
                if (lat < latNeg) { latNeg = lat; tipNeg = b; }
            }
            if (tipPos == null || tipNeg == null) return false;
            float ratio = latPos / -latNeg;
            if (latPos < 0.25f * h || -latNeg < 0.25f * h || ratio < 0.6f || ratio > 1.6f) return false;

            bool a = LowerOne(instance.transform, tipPos, +1f, axis, center, h);
            bool b2 = LowerOne(instance.transform, tipNeg, -1f, axis, center, h);
            return a && b2;
        }

        private static bool LowerOne(Transform stop, Transform tip, float side, Vector3 axis, Vector3 center, float h)
        {
            // Walk from the hand towards the body until we are back near the centre line.
            var chain = new List<Transform>();
            for (var t = tip; t != null && t != stop; t = t.parent)
            {
                float lat = side * Vector3.Dot(t.position - center, axis);
                if (lat < 0.06f * h) break;
                chain.Add(t);
            }
            if (chain.Count < 3) return false;

            // The joint to swing: the first bone (from the body outwards) that is clearly out in the arm.
            Transform joint = chain[chain.Count - 1];
            for (int i = chain.Count - 1; i >= 0; i--)
            {
                if (side * Vector3.Dot(chain[i].position - center, axis) >= 0.12f * h) { joint = chain[i]; break; }
            }

            Vector3 dir = tip.position - joint.position;
            if (dir.sqrMagnitude < 1e-6f) return false;
            Vector3 target = (Vector3.down * 0.96f + (-side * axis) * 0.28f).normalized;
            joint.rotation = Quaternion.FromToRotation(dir.normalized, target) * joint.rotation;
            return true;
        }
    }
}
