// Project D | Codename: Zantetsuken — DuelRigFinder.cs
// Finds the bones the procedural animator needs (hips, spine, head, arms, legs) on a champion model.
//  - Humanoid avatar: asks Unity (Animator.GetBoneTransform).
//  - Anything else (numbered bones like bone_26): finds them by geometry from the T-pose: the hands are the bones
//    furthest left and right at shoulder height, the feet the lowest bones, the hips where the two legs meet.
// Index convention everywhere: [0] = the -lateral side, [1] = the +lateral side (the character's right when it faces forward).
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.DuelDemo
{
    public sealed class DuelRig
    {
        public Transform Hips, Spine, Chest, Head;
        public readonly Transform[] UpperArm = new Transform[2];
        public readonly Transform[] LowerArm = new Transform[2];
        public readonly Transform[] Hand = new Transform[2];
        public readonly Transform[] UpperLeg = new Transform[2];
        public readonly Transform[] LowerLeg = new Transform[2];
        public readonly Transform[] Foot = new Transform[2];
        public float FrameYaw;          // extra yaw so that the frame's "right" axis really is the body's lateral axis
        public string Source = "";

        public bool Complete
        {
            get
            {
                if (Hips == null) return false;
                for (int i = 0; i < 2; i++)
                    if (UpperArm[i] == null || LowerArm[i] == null || Hand[i] == null ||
                        UpperLeg[i] == null || LowerLeg[i] == null || Foot[i] == null) return false;
                return true;
            }
        }
    }

    public static class DuelRigFinder
    {
        public static DuelRig Find(GameObject instance, Transform holder, out string why)
        {
            why = "";
            var named = FromStandardNames(instance, holder);   // the game's own bone_N skeleton: exact, and immune to Unity's auto-mapping
            if (named != null && named.Complete) return named;
            var anim = instance.GetComponentInChildren<Animator>();
            if (anim != null && anim.avatar != null && anim.avatar.isValid && anim.avatar.isHuman)
            {
                var rig = FromHumanoid(anim, holder);
                if (rig != null && rig.Complete) return rig;
                why = "humanoid avatar is missing bones; trying geometry. ";
            }
            var geo = FromGeometry(instance, holder, out string geoWhy);
            if (geo != null) return geo;
            why += geoWhy;
            return null;
        }

        // ------------------------------------------------------------------ standard bone names

        /// <summary>
        /// The champion rips share one skeleton: bone_2 hips, bone_3/4 thighs, 5/6 knees, 7/8 ankles, 9 spine, 10 chest, 12 head,
        /// 15/16 upper arms, 17/18 forearms, 19/20 wrists (Garland's has no separate clavicle, so his arms start one bone later).
        /// Unity's Humanoid auto-mapping got Garland's head and left arm wrong, so names win over the avatar.
        /// </summary>
        private static DuelRig FromStandardNames(GameObject instance, Transform holder)
        {
            var map = new Dictionary<string, Transform>();
            var all = instance.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++) if (!map.ContainsKey(all[i].name)) map[all[i].name] = all[i];

            Transform hips = Bone(map, 2), t3 = Bone(map, 3), t4 = Bone(map, 4);
            Transform b15 = Bone(map, 15);
            if (hips == null || t3 == null || t4 == null || b15 == null) return null;

            bool garlandStyle = b15.parent != null && b15.parent == Bone(map, 11);   // arms hang straight off the neck bone
            int up = garlandStyle ? 17 : 15;

            var rig = new DuelRig { Source = garlandStyle ? "standard bone names (no-clavicle variant)" : "standard bone names", Hips = hips };
            rig.Spine = Bone(map, 9);
            rig.Chest = Bone(map, 10);
            rig.Head = Bone(map, 12);

            // index 0 = the -lateral side: bone_3 is the character's left unless the model is mirrored
            bool threeIsNegative = Vector3.Dot(t3.position - t4.position, holder.right) < 0f;
            for (int k = 0; k < 2; k++)
            {
                bool first = (k == 0) == threeIsNegative;   // does this index use the odd (bone_3 / bone_15 ...) side?
                int d = first ? 0 : 1;
                rig.UpperLeg[k] = Bone(map, 3 + d);
                rig.LowerLeg[k] = Bone(map, 5 + d);
                rig.Foot[k] = Bone(map, 7 + d);
                rig.UpperArm[k] = Bone(map, up + d);
                rig.LowerArm[k] = Bone(map, up + 2 + d);
                rig.Hand[k] = Bone(map, up + 4 + d);
            }
            if (!rig.Complete) return null;

            // sanity: the legs must hang below the hips and the feet below the knees
            for (int k = 0; k < 2; k++)
            {
                if (rig.Foot[k].position.y >= rig.LowerLeg[k].position.y || rig.LowerLeg[k].position.y >= rig.UpperLeg[k].position.y) return null;
                if (rig.UpperLeg[k].position.y > hips.position.y + 0.01f * Mathf.Abs(hips.position.y)) return null;
            }
            return rig;
        }

        private static Transform Bone(Dictionary<string, Transform> map, int n)
        {
            map.TryGetValue("bone_" + n, out var t);
            return t;
        }

        // ------------------------------------------------------------------ humanoid

        private static DuelRig FromHumanoid(Animator a, Transform holder)
        {
            var rig = new DuelRig { Source = "humanoid avatar" };
            rig.Hips = a.GetBoneTransform(HumanBodyBones.Hips);
            rig.Spine = a.GetBoneTransform(HumanBodyBones.Spine);
            rig.Chest = a.GetBoneTransform(HumanBodyBones.Chest);
            rig.Head = a.GetBoneTransform(HumanBodyBones.Head);

            var lu = a.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var ru = a.GetBoneTransform(HumanBodyBones.RightUpperArm);
            if (rig.Hips == null || lu == null || ru == null) return null;

            // Anatomical left is index 0 only if it sits on the -lateral side; check with the real positions.
            bool leftIsNegative = Vector3.Dot(lu.position - ru.position, holder.right) < 0f;
            int L = leftIsNegative ? 0 : 1, R = 1 - L;

            Set(rig.UpperArm, L, a, HumanBodyBones.LeftUpperArm); Set(rig.UpperArm, R, a, HumanBodyBones.RightUpperArm);
            Set(rig.LowerArm, L, a, HumanBodyBones.LeftLowerArm); Set(rig.LowerArm, R, a, HumanBodyBones.RightLowerArm);
            Set(rig.Hand, L, a, HumanBodyBones.LeftHand); Set(rig.Hand, R, a, HumanBodyBones.RightHand);
            Set(rig.UpperLeg, L, a, HumanBodyBones.LeftUpperLeg); Set(rig.UpperLeg, R, a, HumanBodyBones.RightUpperLeg);
            Set(rig.LowerLeg, L, a, HumanBodyBones.LeftLowerLeg); Set(rig.LowerLeg, R, a, HumanBodyBones.RightLowerLeg);
            Set(rig.Foot, L, a, HumanBodyBones.LeftFoot); Set(rig.Foot, R, a, HumanBodyBones.RightFoot);
            return rig;
        }

        private static void Set(Transform[] arr, int index, Animator a, HumanBodyBones bone) { arr[index] = a.GetBoneTransform(bone); }

        // ------------------------------------------------------------------ geometry

        private static DuelRig FromGeometry(GameObject instance, Transform holder, out string why)
        {
            why = "";
            var smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (smrs.Length == 0) { why = "no skinned meshes"; return null; }

            var bones = new HashSet<Transform>();
            var bounds = smrs[0].bounds;
            for (int i = 0; i < smrs.Length; i++)
            {
                bounds.Encapsulate(smrs[i].bounds);
                var sb = smrs[i].bones;
                for (int k = 0; k < sb.Length; k++) if (sb[k] != null) bones.Add(sb[k]);
            }
            if (bones.Count < 15) { why = "too few bones (" + bones.Count + ")"; return null; }

            float h = bounds.size.y;
            if (h < 0.01f) { why = "model has no height"; return null; }

            // Try the holder's right axis first; if the arms stick out along forward instead, the model is turned 90 degrees.
            var rig = TryAxis(instance, bones, bounds, holder.right, 0f, out string w1);
            if (rig != null) return rig;
            rig = TryAxis(instance, bones, bounds, holder.forward, -90f, out string w2);
            if (rig != null) return rig;
            why = "right axis: " + w1 + "; forward axis: " + w2;
            return null;
        }

        private static DuelRig TryAxis(GameObject instance, HashSet<Transform> bones, Bounds bounds, Vector3 axis, float frameYaw, out string why)
        {
            why = "";
            float h = bounds.size.y;
            float minY = bounds.min.y;
            Vector3 center = bounds.center;
            float lo = minY + 0.55f * h, hi = minY + 0.95f * h;

            // --- hands: bones furthest out on each side at shoulder height
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
            if (tipPos == null || tipNeg == null) { why = "no outstretched arms"; return null; }
            float ratio = latPos / -latNeg;
            if (latPos < 0.25f * h || -latNeg < 0.25f * h || ratio < 0.6f || ratio > 1.6f) { why = "arms not symmetric/long enough"; return null; }

            // --- feet: lowest bone on each side
            Transform footPos = null, footNeg = null;
            float yPos = float.MaxValue, yNeg = float.MaxValue;
            foreach (var b in bones)
            {
                var p = b.position;
                if (p.y > minY + 0.15f * h) continue;
                float lat = Vector3.Dot(p - center, axis);
                if (lat > 0.01f * h && p.y < yPos) { yPos = p.y; footPos = b; }
                if (lat < -0.01f * h && p.y < yNeg) { yNeg = p.y; footNeg = b; }
            }
            if (footPos == null || footNeg == null) { why = "no feet found"; return null; }

            // --- hips: where the two legs meet
            var ancPos = Ancestors(footPos, instance.transform);
            var ancNeg = Ancestors(footNeg, instance.transform);
            var negSet = new HashSet<Transform>(ancNeg);
            Transform hips = null;
            foreach (var t in ancPos) if (negSet.Contains(t)) { hips = t; break; }
            if (hips == null || hips == footPos || hips == footNeg) { why = "legs do not meet"; return null; }
            if (hips.position.y < minY + 0.3f * h || hips.position.y > minY + 0.75f * h) { why = "hips at implausible height"; return null; }

            var rig = new DuelRig { Source = "geometry", FrameYaw = frameYaw, Hips = hips };
            if (!BuildLeg(rig, 1, footPos, ancPos, hips, minY, h) || !BuildLeg(rig, 0, footNeg, ancNeg, hips, minY, h)) { why = "leg chain too short"; return null; }
            if (!BuildArm(rig, 1, tipPos, +1f, axis, center, h, instance.transform) || !BuildArm(rig, 0, tipNeg, -1f, axis, center, h, instance.transform)) { why = "arm chain too short"; return null; }

            // --- spine / head: climb the centre line from the hips
            var legSet = new HashSet<Transform>(ancPos); legSet.UnionWith(ancNeg);
            var path = new List<Transform> { hips };
            var cur = hips;
            for (int guard = 0; guard < 40; guard++)
            {
                Transform next = null;
                float bestY = cur.position.y;
                for (int c = 0; c < cur.childCount; c++)
                {
                    var ch = cur.GetChild(c);
                    if (!bones.Contains(ch) || legSet.Contains(ch)) continue;
                    if (Mathf.Abs(Vector3.Dot(ch.position - center, axis)) > 0.06f * h) continue;
                    if (ch.position.y > bestY + 0.001f) { bestY = ch.position.y; next = ch; }
                }
                if (next == null) break;
                path.Add(next);
                cur = next;
            }
            if (path.Count >= 4)
            {
                rig.Head = path[path.Count - 1];
                rig.Spine = path[Mathf.Clamp(Mathf.RoundToInt((path.Count - 1) * 0.30f), 1, path.Count - 2)];
                rig.Chest = path[Mathf.Clamp(Mathf.RoundToInt((path.Count - 1) * 0.60f), 1, path.Count - 2)];
                if (rig.Chest == rig.Spine) rig.Chest = null;
            }
            return rig.Complete ? rig : null;
        }

        private static List<Transform> Ancestors(Transform t, Transform stop)
        {
            var list = new List<Transform>();
            for (var c = t; c != null && c != stop; c = c.parent) list.Add(c);
            return list;
        }

        /// <summary>The leg chain is the path from the foot up to (not including) the hips.</summary>
        private static bool BuildLeg(DuelRig rig, int index, Transform foot, List<Transform> ancestorsFromFoot, Transform hips, float minY, float h)
        {
            var chain = new List<Transform>();   // top (next to hips) -> foot
            foreach (var t in ancestorsFromFoot) { if (t == hips) break; chain.Add(t); }
            chain.Reverse();
            if (chain.Count < 3) return false;

            // ankle: the chain bone closest to ankle height
            int ankle = 0;
            float bestA = float.MaxValue;
            for (int i = 1; i < chain.Count; i++)
            {
                float d = Mathf.Abs(chain[i].position.y - (minY + 0.06f * h));
                if (d < bestA) { bestA = d; ankle = i; }
            }
            if (ankle < 2) return false;

            // knee: between thigh and ankle, closest to the middle height
            float mid = (chain[0].position.y + chain[ankle].position.y) * 0.5f;
            int knee = 1;
            float bestK = float.MaxValue;
            for (int i = 1; i < ankle; i++)
            {
                float d = Mathf.Abs(chain[i].position.y - mid);
                if (d < bestK) { bestK = d; knee = i; }
            }

            rig.UpperLeg[index] = chain[0];
            rig.LowerLeg[index] = chain[knee];
            rig.Foot[index] = chain[ankle];
            return (chain[0].position - chain[knee].position).magnitude > 0.05f * h
                && (chain[knee].position - chain[ankle].position).magnitude > 0.05f * h;
        }

        /// <summary>The arm chain is the path from the hand tip back to the centre line.</summary>
        private static bool BuildArm(DuelRig rig, int index, Transform tip, float side, Vector3 axis, Vector3 center, float h, Transform stop)
        {
            var chain = new List<Transform>();   // tip -> body
            for (var t = tip; t != null && t != stop; t = t.parent)
            {
                float lat = side * Vector3.Dot(t.position - center, axis);
                if (lat < 0.06f * h) break;
                chain.Add(t);
            }
            if (chain.Count < 3) return false;
            chain.Reverse();   // body -> tip

            int shoulder = 0;
            for (int i = 0; i < chain.Count; i++)
            {
                if (side * Vector3.Dot(chain[i].position - center, axis) >= 0.12f * h) { shoulder = i; break; }
            }
            Vector3 s = chain[shoulder].position;
            float armLen = (tip.position - s).magnitude;
            if (armLen < 0.1f * h) return false;

            int elbow = shoulder, hand = chain.Count - 1;
            float bestE = float.MaxValue, bestH = float.MaxValue;
            for (int i = shoulder + 1; i < chain.Count; i++)
            {
                float d = (chain[i].position - s).magnitude;
                if (Mathf.Abs(d - 0.5f * armLen) < bestE) { bestE = Mathf.Abs(d - 0.5f * armLen); elbow = i; }
                if (Mathf.Abs(d - 0.92f * armLen) < bestH) { bestH = Mathf.Abs(d - 0.92f * armLen); hand = i; }
            }
            if (elbow <= shoulder || hand <= elbow) return false;

            rig.UpperArm[index] = chain[shoulder];
            rig.LowerArm[index] = chain[elbow];
            rig.Hand[index] = chain[hand];
            return true;
        }
    }
}
