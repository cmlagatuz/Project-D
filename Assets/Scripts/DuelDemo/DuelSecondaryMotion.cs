// Project D | Codename: Zantetsuken — DuelSecondaryMotion.cs
// Procedural secondary motion for model rips that have no animation clips:
//   Cloth - Shiva's gown, skirt and hair: the skirt/cloth/cape/hair bones stream back in an icy wind and ripple in a
//           travelling wave (the further down a chain, the bigger the swing), and gust harder when a champion is near.
//   Wings - Garuda's (and Bahamut's) wings: the wing bones beat up and down about the body's front-back axis, outer
//           feathers lagging behind the shoulder, faster and wider when the summon is alerted.
// Bones are found by name, so it works on any skeleton that names them (Shiva: j_skirt*, Garuda: j_lowerwing_*).
// Rotations are applied in world axes, so it does not matter which way each bone's local axes point.
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.DuelDemo
{
    public enum SecondaryKind { Cloth, Wings }

    public sealed class DuelSecondaryMotion : MonoBehaviour
    {
        private sealed class Bone
        {
            public Transform T;
            public Quaternion Rest;
            public int Depth;       // number of selected ancestors: 0 = the root of a chain
            public float Side;      // +1 right of the body, -1 left
            public float Phase;     // spreads the wave along the body
            public float Weight;    // how much this bone moves (hair less than the gown)
        }

        private static readonly string[] ClothWords = { "skirt", "cloth", "cape", "mantle", "robe", "scarf", "veil", "ribbon", "drape", "hair", "train" };
        private static readonly string[] WingWords = { "wing", "feather" };
        private static readonly string[] SkipWords = { "press", "roll", "src", "mesh", "collider", "finger" };   // helper bones that drive other bones

        private readonly List<Bone> _bones = new List<Bone>();
        private SecondaryKind _kind;
        private Transform _holder;
        private DuelSummonGuard _guard;

        public int Count { get { return _bones.Count; } }

        /// <summary>Finds the bones and attaches the component to the holder. Returns null (and says why) if no matching bones exist.</summary>
        public static DuelSecondaryMotion Attach(GameObject instance, Transform holder, SecondaryKind kind, DuelSummonGuard guard, out string report)
        {
            var words = kind == SecondaryKind.Cloth ? ClothWords : WingWords;
            var selected = new HashSet<Transform>();
            var all = instance.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == instance.transform) continue;
                if (t.GetComponent<Renderer>() != null) continue;   // a mesh node, not a bone
                string n = t.name.ToLowerInvariant();
                if (!ContainsAny(n, words) || ContainsAny(n, SkipWords)) continue;
                selected.Add(t);
            }
            if (selected.Count == 0)
            {
                report = "no bones named like " + string.Join("/", words);
                return null;
            }

            var comp = holder.gameObject.AddComponent<DuelSecondaryMotion>();
            comp._kind = kind;
            comp._holder = holder;
            comp._guard = guard;
            foreach (var t in selected)
            {
                int depth = 0;
                for (var a = t.parent; a != null && a != instance.transform; a = a.parent) if (selected.Contains(a)) depth++;
                float lat = Vector3.Dot(t.position - holder.position, holder.right);
                string n = t.name.ToLowerInvariant();
                comp._bones.Add(new Bone
                {
                    T = t,
                    Rest = t.localRotation,
                    Depth = depth,
                    Side = lat >= 0f ? 1f : -1f,
                    Phase = (t.position.x * 1.7f + t.position.z * 2.3f),
                    Weight = n.Contains("hair") ? 0.6f : 1f
                });
            }
            comp._bones.Sort((x, y) => x.Depth.CompareTo(y.Depth));   // parents are posed before their children
            report = comp._bones.Count + " bones";
            return comp;
        }

        private static bool ContainsAny(string s, string[] words)
        {
            for (int i = 0; i < words.Length; i++) if (s.Contains(words[i])) return true;
            return false;
        }

        private void LateUpdate()
        {
            if (_holder == null) return;
            float t = Time.time;
            float alert = _guard != null ? _guard.Alert01 : 0f;
            Vector3 right = _holder.right, fwd = _holder.forward;

            for (int i = 0; i < _bones.Count; i++) _bones[i].T.localRotation = _bones[i].Rest;

            if (_kind == SecondaryKind.Wings)
            {
                float hz = 1.15f + 0.9f * alert;
                float phase = t * Mathf.PI * 2f * hz;
                for (int i = 0; i < _bones.Count; i++)
                {
                    var b = _bones[i];
                    float amp = (b.Depth == 0 ? 26f : 9f) * (1f + 0.4f * alert);
                    float lift = b.Depth == 0 ? 5f : 1.5f;   // wings ride a little above level
                    float angle = (lift + amp * Mathf.Sin(phase - b.Depth * 0.6f)) * b.Side;
                    b.T.rotation = Quaternion.AngleAxis(angle, fwd) * b.T.rotation;
                }
            }
            else
            {
                float gust = 1f + 0.8f * alert;
                for (int i = 0; i < _bones.Count; i++)
                {
                    var b = _bones[i];
                    float w = Mathf.Min(1f, 0.45f + 0.28f * b.Depth) * b.Weight;
                    float wave = Mathf.Sin(t * 2.6f + b.Phase - b.Depth * 0.9f);
                    float back = (6f + 3f * Mathf.Sin(t * 0.8f + b.Phase * 0.3f)) * w * gust;   // streaming away from the wind
                    float swing = 7f * w * wave * gust;
                    float side = 5f * w * Mathf.Sin(t * 1.9f + b.Phase * 1.4f - b.Depth * 0.7f) * gust;
                    b.T.rotation = Quaternion.AngleAxis(back + swing, right) * Quaternion.AngleAxis(side, fwd) * b.T.rotation;
                }
            }
        }
    }
}
