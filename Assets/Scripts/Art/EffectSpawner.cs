// Project D  |  Codename: Zantetsuken
// EffectSpawner.cs  (Art runtime — turns cue keys into spawned VFX)
//
// Subscribe this to an animator's OnVfxCue event. It resolves the key via the
// EffectCatalog and spawns the VFX according to its SpawnRule: attaches to a
// socket, spawns at a target/ground point, or hands a travelling projectile to
// the projectile system. Applies pooling, per-key concurrency limits, team tint,
// and the "reduce effects" accessibility setting (never culls danger telegraphs).
//
// Socket resolution: the cue's socket string (or the definition's defaultSocket)
// is matched against the rig's bone Transforms (contains, case-insensitive), with
// friendly aliases (weapon_tip, hand_r, chest, feet, shield, root, ground).

using System.Collections.Generic;
using UnityEngine;
using ProjectD.Art.Effects;

namespace ProjectD.Art.Effects
{
    public sealed class EffectSpawner : MonoBehaviour
    {
        [SerializeField] private EffectCatalog catalog;
        [Tooltip("Owner team for team-colored effects (0 = blue, 1 = red).")]
        [SerializeField] private int ownerTeam = 0;
        [Tooltip("Accessibility: reduce non-essential effect density. Danger telegraphs are never reduced.")]
        [SerializeField] private bool reduceEffects = false;

        [Header("Team tint colors")]
        [SerializeField] private Color blueTint = new Color(0.4f, 0.65f, 1f);
        [SerializeField] private Color redTint = new Color(1f, 0.45f, 0.45f);

        // Simple per-key pool + live-count tracking.
        private readonly Dictionary<string, Queue<GameObject>> _pool = new();
        private readonly Dictionary<string, int> _live = new();
        private Transform _root;

        /// <summary>Optional: routes travelling projectiles to your projectile system.</summary>
        public System.Action<EffectDefinition, Vector3, Vector3, int> ProjectileHandler;

        private void Awake() => _root = transform;

        public void SetCatalog(EffectCatalog c) => catalog = c;
        public void SetOwnerTeam(int team) => ownerTeam = team;
        public void SetReduceEffects(bool v) => reduceEffects = v;

        /// <summary>Hook to an animator: animator.OnVfxCue += spawner.Play;</summary>
        public void Play(string key, string socketOverride)
        {
            if (catalog == null || !catalog.TryResolve(key, out var def) || def.kind != EffectKind.Vfx) return;

            // Accessibility / performance budget: skip non-essential effects when reduced.
            if (reduceEffects && !def.isDangerTelegraph && IsNonEssential(def.category)) return;

            // Concurrency cap (readability/perf).
            _live.TryGetValue(key, out int live);
            if (def.maxConcurrent > 0 && live >= def.maxConcurrent) return;

            string socket = !string.IsNullOrEmpty(socketOverride) ? socketOverride : def.defaultSocket;

            switch (def.spawnRule)
            {
                case SpawnRule.ProjectileTravel:
                    // Origin = socket, destination handled by the projectile system.
                    var origin = ResolveSocket(socket)?.position ?? _root.position;
                    ProjectileHandler?.Invoke(def, origin, _root.position + _root.forward * 8f, ownerTeam);
                    return;

                case SpawnRule.AttachToSocket:
                    SpawnParented(def, key, ResolveSocket(socket), true);
                    return;

                case SpawnRule.SpawnAtSocket:
                    SpawnParented(def, key, ResolveSocket(socket), def.followSocket);
                    return;

                case SpawnRule.SpawnAtRoot:
                    SpawnAt(def, key, _root.position, _root.rotation);
                    return;

                case SpawnRule.SpawnAtGroundPoint:
                case SpawnRule.WorldZone:
                case SpawnRule.SpawnAtTarget:
                    // Ground/target position is provided by the ability system in a full
                    // build; the prototype places it in front of the caster on the ground.
                    var p = _root.position + _root.forward * 4f; p.y = 0f;
                    SpawnAt(def, key, p, Quaternion.identity);
                    return;
            }
        }

        private void SpawnParented(EffectDefinition def, string key, Transform socket, bool follow)
        {
            var go = Rent(def, key);
            if (go == null) return;
            Transform parent = follow ? (socket ?? _root) : null;
            var pos = (socket ?? _root).position;
            var rot = (socket ?? _root).rotation;
            go.transform.SetParent(parent, worldPositionStays: false);
            if (parent == null) { go.transform.position = pos; go.transform.rotation = rot; }
            else { go.transform.localPosition = Vector3.zero; go.transform.localRotation = Quaternion.identity; }
            Finish(def, key, go);
        }

        private void SpawnAt(EffectDefinition def, string key, Vector3 pos, Quaternion rot)
        {
            var go = Rent(def, key);
            if (go == null) return;
            go.transform.SetParent(null);
            go.transform.SetPositionAndRotation(pos, rot);
            Finish(def, key, go);
        }

        private void Finish(EffectDefinition def, string key, GameObject go)
        {
            if (def.teamColored) ApplyTint(go, ownerTeam == 0 ? blueTint : redTint);
            go.SetActive(true);
            _live[key] = (_live.TryGetValue(key, out var n) ? n : 0) + 1;
            float life = def.lifetime <= 0f ? 2f : Mathf.Min(def.lifetime, 9999f);
            StartCoroutine(Despawn(def, key, go, life));
        }

        private System.Collections.IEnumerator Despawn(EffectDefinition def, string key, GameObject go, float life)
        {
            yield return new WaitForSeconds(life);
            if (go == null) yield break;
            _live[key] = Mathf.Max(0, (_live.TryGetValue(key, out var n) ? n : 1) - 1);
            if (def.pooled) { go.SetActive(false); Pool(key).Enqueue(go); }
            else Destroy(go);
        }

        // ---- pooling ---- //
        private GameObject Rent(EffectDefinition def, string key)
        {
            if (def.vfxPrefab == null) return null; // artist hasn't bound a prefab yet
            var q = Pool(key);
            if (def.pooled && q.Count > 0) return q.Dequeue();
            return Instantiate(def.vfxPrefab);
        }

        private Queue<GameObject> Pool(string key)
        {
            if (!_pool.TryGetValue(key, out var q)) { q = new Queue<GameObject>(); _pool[key] = q; }
            return q;
        }

        // ---- helpers ---- //
        private static bool IsNonEssential(EffectCategory c)
            => c == EffectCategory.WeaponTrail || c == EffectCategory.Aura ||
               c == EffectCategory.Buff || c == EffectCategory.Environment;

        private Transform ResolveSocket(string socket)
        {
            if (string.IsNullOrEmpty(socket) || socket == "root" || socket == "ground") return _root;
            string needle = socket.ToLowerInvariant();
            foreach (var t in GetComponentsInChildren<Transform>(true))
                if (t.name.ToLowerInvariant().Contains(needle)) return t;
            return _root;
        }

        private static void ApplyTint(GameObject go, Color tint)
        {
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                var sc = main.startColor;
                sc.color = tint;
                main.startColor = sc;
            }
        }
    }
}
