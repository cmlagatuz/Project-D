// Project D | Codename: Zantetsuken — DuelDemoController.cs
// Unity view for the 1v1 duel demo. Owns the engine-agnostic DuelWorld, ticks it at a fixed step, mirrors units into
// the scene (the artist's 3D models, or primitive placeholder shapes), animates them (DuelCharacterAnimator for the
// champions, DuelSummonGuard for the summons), flashes ability areas, and exposes the commands the touch UI and the
// editor keyboard call. All rules live in ProjectD.Duel; this file only draws and forwards input.
using System.Collections.Generic;
using UnityEngine;
using ProjectD.Duel;
using ProjectD.Gameplay.CameraRig;
using ProjectD.Modes;
using ProjectD.SummonerSpells;

namespace ProjectD.DuelDemo
{
    public sealed class DuelDemoController : MonoBehaviour
    {
        public const float Step = 1f / 30f;
        private const float DefaultZoom = 0.72f;

        [SerializeField] private Material groundMat;
        [SerializeField] private Material blueMat;
        [SerializeField] private Material redMat;
        [SerializeField] private Material neutralMat;
        [SerializeField] private Material wardMat;
        [SerializeField] private Material flashMat;
        [SerializeField] private Material markMat;
        [SerializeField] private Material fxMat;   // additive particle material for the summon effects (optional; falls back to Sprites/Default)
        [SerializeField] private DuelModelLibrary models;
        [SerializeField] private ArenaCameraRig cameraRig;
        [SerializeField] private Camera viewCamera;

        public DuelWorld World { get; private set; }
        public DuelUnit Player { get; private set; }
        public DuelUnit Enemy { get; private set; }
        public DuelUnit Shiva { get; private set; }
        public DuelUnit Garuda { get; private set; }
        public DuelUnit Bahamut { get; private set; }   // only present when a Bahamut model is linked
        public string PlayerChampionId { get; private set; } = "wol";
        public string EnemyChampionId { get; private set; } = "garland";
        public DuelBot EnemyBot { get; private set; }
        public DuelDemoUI Ui { get; private set; }
        public bool HasModelFor(string id) => models != null && models.Find(id) != null;
        public Camera ViewCamera => viewCamera != null ? viewCamera : Camera.main;

        private sealed class UnitView
        {
            public DuelUnit Unit;
            public Transform Root;
            public GameObject Shield;
            public GameObject Mark;
            public Animator Animator;
            public DuelCharacterAnimator Anim;   // procedural champion animation (idle, run, attacks, spells, death, teleport)
            public DuelSecondaryMotion Cloth;    // champions: capes, skirts and tabards (the rips' "nuno" bone chains) trailing as they run
            public DuelSummonGuard Guard;        // summons: stand guard, face the nearest champion, rear up, recoil
            public bool WasAlive = true;
            public bool WasChanneling;
            public DVec2 LastPos;
            public HashSet<string> Params;
            public bool HasModel;
            public Transform Holder;      // parent of the model: gets the yaw offset and the procedural motion
            public float BaseYaw;         // the library's Yaw Offset
            public float PunchUntil;      // procedural attack cue for models without animations
            public string ModelId;
        }

        private struct Flash
        {
            public GameObject Go;
            public float DieAt;
        }

        private readonly Dictionary<int, UnitView> _views = new Dictionary<int, UnitView>();
        private readonly List<Flash> _flashes = new List<Flash>();
        private float _accumulator;
        private Vector2 _stick;
        private bool _attackHeld;
        private bool _warnedMaterial;
        private SummonerLoadout _playerLoadout;
        private int _level = 5;

        // ------------------------------------------------------------------ setup

        private void Start()
        {
            if (models == null) models = Resources.Load<DuelModelLibrary>("DuelModelLibrary");   // scene field is optional; the library also lives in Resources
            if (cameraRig != null) cameraRig.SetZoom(PlayerPrefs.GetFloat("pd_zoom", DefaultZoom));   // the demo camera sits closer than the old greybox one
            BuildArena();
            StartDuel(PlayerChampionId, EnemyChampionId);

            Ui = gameObject.AddComponent<DuelDemoUI>();
            Ui.Init(this);
        }

        /// <summary>(Re)builds the whole duel with the chosen champions. Keeps your summoner spells and level.</summary>
        public void StartDuel(string playerChampionId, string enemyChampionId)
        {
            if (Player != null)
            {
                _playerLoadout = Player.Loadout;
                _level = Player.Level;
            }

            foreach (var kv in _views) if (kv.Value.Root != null) Destroy(kv.Value.Root.gameObject);
            _views.Clear();
            for (int i = 0; i < _flashes.Count; i++) if (_flashes[i].Go != null) Destroy(_flashes[i].Go);
            _flashes.Clear();
            _accumulator = 0f;
            _stick = Vector2.zero;
            _attackHeld = false;

            PlayerChampionId = playerChampionId;
            EnemyChampionId = enemyChampionId;
            World = new DuelWorld { Mode = GameMode.Practice };

            var mine = _playerLoadout != null ? _playerLoadout.Clone() : SummonerLoadout.Default();   // Cure + Warp by default
            Player = World.AddChampion(DuelRoster.ById(playerChampionId), 0, new DVec2(-6f, -6f), _level, mine);

            var theirs = SummonerLoadout.Default();
            theirs.TrySet(0, SummonerSpellId.Esuna, GameMode.Practice);
            theirs.TrySet(1, SummonerSpellId.Cure, GameMode.Practice);
            Enemy = World.AddChampion(DuelRoster.ById(enemyChampionId), 1, new DVec2(6f, 6f), _level, theirs);

            // The summons stand in as neutral training targets (Ultima, Hunter's Resource). They do not fight back yet.
            Shiva = World.AddMonster("Shiva", new DVec2(-6f, 8f), 1800f);
            Shiva.ModelId = "shiva";
            Garuda = World.AddMonster("Garuda", new DVec2(8f, -7f), 1800f);
            Garuda.ModelId = "garuda";
            Bahamut = null;
            if (HasModelFor("bahamut"))
            {
                Bahamut = World.AddMonster("Bahamut", new DVec2(1f, 11f), 3000f);
                Bahamut.ModelId = "bahamut";
            }
            World.AddWard(0, new DVec2(11f, -11f));

            EnemyBot = new DuelBot(Enemy.Id);
            World.AddBot(EnemyBot);

            for (int i = 0; i < World.Units.Count; i++) CreateView(World.Units[i]);
            if (cameraRig != null && _views.TryGetValue(Player.Id, out var pv)) cameraRig.SetTarget(pv.Root);

            if (Ui != null) Ui.OnDuelRestarted();
        }

        private Material Pick(Material assigned, Color fallbackColor)
        {
            if (assigned != null) return assigned;
            if (!_warnedMaterial)
            {
                _warnedMaterial = true;
                Debug.LogWarning("[Project D] DuelDemo materials are not assigned. Run Tools > Project D > Build Duel Demo Scene so they are saved as assets (needed for phone builds).");
            }
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var m = new Material(shader);
            m.color = fallbackColor;
            return m;
        }

        private void BuildArena()
        {
            float size = DuelWorld.ArenaHalf * 2f;
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena_Ground";
            ground.transform.SetParent(transform, false);
            ground.transform.localScale = new Vector3(size / 10f, 1f, size / 10f);
            RemoveCollider(ground);
            ground.GetComponent<Renderer>().sharedMaterial = Pick(groundMat, new Color(0.20f, 0.23f, 0.28f));

            // Edge markers so the arena bounds are visible (shape cue, not just colour).
            float half = DuelWorld.ArenaHalf;
            MakeEdge(new Vector3(0f, 0.25f, half), new Vector3(size, 0.5f, 0.3f));
            MakeEdge(new Vector3(0f, 0.25f, -half), new Vector3(size, 0.5f, 0.3f));
            MakeEdge(new Vector3(half, 0.25f, 0f), new Vector3(0.3f, 0.5f, size));
            MakeEdge(new Vector3(-half, 0.25f, 0f), new Vector3(0.3f, 0.5f, size));
        }

        private void MakeEdge(Vector3 pos, Vector3 scale)
        {
            var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edge.name = "Arena_Edge";
            edge.transform.SetParent(transform, false);
            edge.transform.position = pos;
            edge.transform.localScale = scale;
            RemoveCollider(edge);
            edge.GetComponent<Renderer>().sharedMaterial = Pick(neutralMat, new Color(0.5f, 0.5f, 0.5f));
        }

        private static void RemoveCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
        }

        private void CreateView(DuelUnit u)
        {
            var root = new GameObject("View_" + u.Name);
            root.transform.SetParent(transform, false);
            var view = new UnitView { Unit = u, Root = root.transform };

            var entry = models != null ? models.Find(u.ModelId) : null;
            if (entry != null) view.HasModel = AttachModel(view, entry, u);
            if (!view.HasModel) AttachPlaceholder(view, u);
            if (!string.IsNullOrEmpty(u.ModelId))
                Debug.Log("[Project D] Model for '" + u.ModelId + "': " + (view.HasModel ? "using " + entry.prefab.name :
                    models == null ? "placeholder - no DuelModelLibrary found (run Tools > Project D > Link Duel Demo Models)" :
                    entry == null ? "placeholder - no prefab linked for this id (library has " + models.entries.Count + " entries; run Link Duel Demo Models)" :
                    "placeholder - the prefab has no renderers"));

            float shieldHeight = view.HasModel ? 2.4f : 0.9f;
            var shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.name = "ShieldBubble";
            RemoveCollider(shield);
            shield.transform.SetParent(root.transform, false);
            float shieldSize = view.HasModel ? 2.8f : 2.0f;
            shield.transform.localScale = new Vector3(shieldSize, shieldSize, shieldSize);
            shield.transform.localPosition = new Vector3(0f, shieldHeight, 0f);
            shield.GetComponent<Renderer>().sharedMaterial = Pick(flashMat, new Color(0.6f, 0.9f, 1f, 0.4f));
            shield.SetActive(false);

            var mark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mark.name = "WarpstrikeMark";
            RemoveCollider(mark);
            mark.transform.SetParent(root.transform, false);
            mark.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            mark.transform.localPosition = new Vector3(0f, view.HasModel ? 3.4f : 2.6f, 0f);
            mark.GetComponent<Renderer>().sharedMaterial = Pick(markMat, new Color(1f, 0.85f, 0.2f));
            mark.SetActive(false);

            view.Shield = shield;
            view.Mark = mark;
            _views[u.Id] = view;
        }

        /// <summary>Instantiates the artist's prefab, strips colliders, scales it to the target height and stands it on the ground.</summary>
        private bool AttachModel(UnitView view, DuelModelLibrary.Entry entry, DuelUnit u)
        {
            var holder = new GameObject("Model");
            holder.transform.SetParent(view.Root, false);
            view.Holder = holder.transform;
            view.BaseYaw = entry.yawOffset;
            view.ModelId = u.ModelId;
            var instance = Instantiate(entry.prefab, holder.transform);
            instance.name = entry.prefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) Destroy(colliders[i]);

            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning("[Project D] Model for '" + u.ModelId + "' has no renderers; using a placeholder shape.");
                Destroy(holder);
                return false;
            }

            if (u.Kind == UnitKind.Champion) TintUntextured(renderers, u.Team == 0 ? new Color(0.72f, 0.84f, 1f) : new Color(1f, 0.74f, 0.72f));

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            float height = entry.height > 0.1f ? entry.height : DuelModelLibrary.DefaultHeight(u.ModelId);
            if (bounds.size.y > 0.0001f) instance.transform.localScale *= height / bounds.size.y;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            instance.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
            holder.transform.localRotation = Quaternion.Euler(0f, entry.yawOffset, 0f);

            view.Animator = instance.GetComponentInChildren<Animator>();
            if (view.Animator != null)
            {
                view.Animator.applyRootMotion = false;
                view.Params = new HashSet<string>();
                var ps = view.Animator.parameters;   // empty if the prefab has no controller
                for (int i = 0; i < ps.Length; i++) view.Params.Add(ps[i].name);
            }

            bool ownClips = view.Animator != null && view.Animator.runtimeAnimatorController != null && view.Params.Count > 0;
            if (u.Kind == UnitKind.Champion && !ownClips) AttachCharacterAnimator(view, instance, holder.transform, u);
            else if (u.Kind == UnitKind.Monster)
            {
                view.Guard = holder.AddComponent<DuelSummonGuard>();
                view.Guard.Init(u.ModelId);
                view.Guard.Yaw = view.BaseYaw;
                AttachSummonEffects(view, instance, holder.transform, u, height);
            }
            return true;
        }

        /// <summary>Gown and hair in the wind (Shiva), wing beats (Garuda, Bahamut) and the particle effects around each summon.</summary>
        private void AttachSummonEffects(UnitView view, GameObject instance, Transform holder, DuelUnit u, float height)
        {
            var smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < smrs.Length; i++) smrs[i].updateWhenOffscreen = true;   // posed bones leave the imported bounds

            var kind = u.ModelId == "shiva" ? SecondaryKind.Cloth : SecondaryKind.Wings;
            var motion = DuelSecondaryMotion.Attach(instance, holder, kind, view.Guard, out string report);
            Debug.Log("[Project D] " + u.ModelId + ": " + (kind == SecondaryKind.Cloth ? "cloth" : "wing") + " motion " + (motion != null ? "on (" + report + ")" : "off (" + report + ")"));
            DuelSummonFx.Attach(view.Root, u.ModelId, height, fxMat);
        }

        /// <summary>Finds the bones and attaches the procedural animator. Without it the model just stands in its imported pose.</summary>
        private static void AttachCharacterAnimator(UnitView view, GameObject instance, Transform holder, DuelUnit u)
        {
            var rig = DuelRigFinder.Find(instance, holder, out string why);
            if (rig == null || !rig.Complete)
            {
                Debug.LogWarning("[Project D] " + u.ModelId + ": no animation rig found (" + why + "). It will use the simple bob-and-lean motion. " +
                                 "Run Tools > Project D > Save Model Material Report and send me Builds/ModelMaterialReport.txt.");
                return;
            }
            if (view.Animator != null) view.Animator.enabled = false;   // nothing else may move the bones
            var smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < smrs.Length; i++) smrs[i].updateWhenOffscreen = true;   // limbs leave the imported bounds when posed

            var anim = holder.gameObject.AddComponent<DuelCharacterAnimator>();
            anim.Yaw = view.BaseYaw;
            if (!anim.Init(rig, holder, DuelStyles.ForModel(u.ModelId)))
            {
                Debug.LogWarning("[Project D] " + u.ModelId + ": the rig's legs did not measure up (" + rig.Source + "); using the simple motion instead.");
                Destroy(anim);
                if (view.Animator != null) view.Animator.enabled = true;
                return;
            }
            view.Anim = anim;
            Debug.Log("[Project D] " + u.ModelId + ": procedural animation on (rig via " + rig.Source + ", style " + DuelStyles.ForModel(u.ModelId) + ")");
            view.Cloth = DuelSecondaryMotion.Attach(instance, holder, SecondaryKind.Cloth, null, out string clothReport);
            Debug.Log("[Project D] " + u.ModelId + ": cloth motion " + (view.Cloth != null ? "on (" + clothReport + ")" : "off (" + clothReport + ")"));
        }

        /// <summary>Materials with no texture would be plain white; give them a light team tint so the two sides can still be told apart.</summary>
        private static void TintUntextured(Renderer[] renderers, Color tint)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var mats = renderers[i].materials;   // instances: the shared assets stay untouched
                for (int k = 0; k < mats.Length; k++)
                {
                    var m = mats[k];
                    if (m == null) continue;
                    bool hasTex = (m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") != null)
                                  || (m.HasProperty("_MainTex") && m.GetTexture("_MainTex") != null);
                    if (hasTex) continue;
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", tint);
                    else if (m.HasProperty("_Color")) m.SetColor("_Color", tint);
                }
                renderers[i].materials = mats;
            }
        }

        private void AttachPlaceholder(UnitView view, DuelUnit u)
        {
            PrimitiveType prim = u.Kind == UnitKind.Champion ? PrimitiveType.Capsule
                : u.Kind == UnitKind.Monster ? PrimitiveType.Cube : PrimitiveType.Cylinder;

            var body = GameObject.CreatePrimitive(prim);
            body.name = "Body";
            RemoveCollider(body);
            body.transform.SetParent(view.Root, false);

            Material mat;
            if (u.Kind == UnitKind.Ward) mat = Pick(wardMat, new Color(0.9f, 0.85f, 0.4f));
            else if (u.Team == 0) mat = Pick(blueMat, new Color(0.25f, 0.5f, 0.95f));
            else if (u.Team == 1) mat = Pick(redMat, new Color(0.9f, 0.3f, 0.3f));
            else mat = Pick(neutralMat, new Color(0.6f, 0.6f, 0.6f));
            body.GetComponent<Renderer>().sharedMaterial = mat;

            if (u.Kind == UnitKind.Champion)
            {
                body.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);   // facing marker
                nose.name = "Facing";
                RemoveCollider(nose);
                nose.transform.SetParent(view.Root, false);
                nose.transform.localScale = new Vector3(0.25f, 0.25f, 0.6f);
                nose.transform.localPosition = new Vector3(0f, 1.2f, 0.55f);
                nose.GetComponent<Renderer>().sharedMaterial = Pick(wardMat, Color.white);
            }
            else if (u.Kind == UnitKind.Monster)
            {
                body.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
                body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            }
            else
            {
                body.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f);
                body.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            }
        }

        // ------------------------------------------------------------------ frame loop

        private void Update()
        {
            if (World == null) return;

            bool paused = Ui != null && Ui.PanelOpen;   // the spell picker freezes the duel
            if (!paused)
            {
                ReadKeyboard();
                var move = _stick.sqrMagnitude > 0.01f ? _stick : KeyboardMove();
                World.SetMoveInput(Player.Id, new DVec2(move.x, move.y));

                if (_attackHeld || Input.GetKey(KeyCode.Space)) World.TryBasicAttackNearest(Player.Id);

                _accumulator = Mathf.Min(_accumulator + Time.deltaTime, 0.25f);
                while (_accumulator >= Step)
                {
                    World.Tick(Step);
                    _accumulator -= Step;
                }
            }

            SyncViews();

            var events = World.DrainEvents();
            for (int i = 0; i < events.Count; i++)
                if (events[i].Kind == DuelEventKind.Area) SpawnFlash(events[i]);
            AnimateFromEvents(events);
            if (Ui != null) Ui.OnEvents(events);

            UpdateFlashes();
        }

        private static Vector3 ToWorld(DVec2 p) => new Vector3(p.X, 0f, p.Y);

        private const float GuardAlertRange = 9f;
        private const float GuardTurnSpeed = 150f;   // degrees per second

        private void SyncViews()
        {
            float now = World.Time;
            foreach (var kv in _views)
            {
                var v = kv.Value;
                var u = v.Unit;
                bool alive = u.Alive;
                bool show = alive || (v.Anim != null && v.Anim.DeathVisible);
                v.Root.gameObject.SetActive(show);
                if (alive)
                {
                    v.Root.position = ToWorld(u.Pos);
                    if (v.Guard != null) TurnGuard(v);
                    else if (u.Facing.Length > 0.01f) v.Root.rotation = Quaternion.LookRotation(ToWorld(u.Facing));
                }

                if (v.Anim != null)
                {
                    if (v.WasAlive && !alive) v.Anim.PlayDeath();
                    else if (!v.WasAlive && alive) v.Anim.PlayRise();
                    if (alive)
                    {
                        // Teleport: kneel and pray while channelling, beam out when it completes (the position jumps to the ward).
                        if (u.Channeling && !v.WasChanneling) v.Anim.BeginChannel(u.ChannelEndsAt - now);
                        else if (!u.Channeling && v.WasChanneling) v.Anim.EndChannel(DVec2.Distance(u.Pos, v.LastPos) > 1.5f);
                    }
                    v.Anim.Yaw = v.BaseYaw;
                    if (v.Cloth != null) v.Cloth.Drive = v.Anim.Walk01;
                }
                v.WasAlive = alive;
                v.WasChanneling = alive && u.Channeling;
                v.LastPos = u.Pos;
                if (!show) continue;

                if (!alive)
                {
                    v.Shield.SetActive(false);
                    v.Mark.SetActive(false);
                    continue;
                }
                v.Shield.SetActive(u.ShieldAmount(now) > 0f);
                v.Mark.SetActive(Player.MarkTargetId == u.Id && now < Player.MarkExpiresAt);
                bool moving = u.MoveInput.Length > 0.1f && !u.IsStunned(now);
                if (v.Animator != null && v.Params != null && v.Params.Contains("Speed"))
                    v.Animator.SetFloat("Speed", moving ? 1f : 0f);
                if (v.HasModel && v.Holder != null && v.Anim == null && v.Guard == null) ApplyProceduralMotion(v, moving);
            }
        }

        /// <summary>Summons keep an eye on the nearest champion and rear up when one enters their territory.</summary>
        private void TurnGuard(UnitView v)
        {
            DuelUnit nearest = null;
            float best = float.MaxValue;
            for (int i = 0; i < World.Units.Count; i++)
            {
                var c = World.Units[i];
                if (c.Kind != UnitKind.Champion || !c.Alive) continue;
                float d = DVec2.Distance(c.Pos, v.Unit.Pos);
                if (d < best) { best = d; nearest = c; }
            }
            if (nearest == null) { v.Guard.AlertTarget = 0f; return; }
            float dx = nearest.Pos.X - v.Unit.Pos.X, dz = nearest.Pos.Y - v.Unit.Pos.Y;
            if (dx * dx + dz * dz > 0.01f)
            {
                var want = Quaternion.Euler(0f, Mathf.Atan2(dx, dz) * Mathf.Rad2Deg, 0f);
                v.Root.rotation = Quaternion.RotateTowards(v.Root.rotation, want, GuardTurnSpeed * Time.deltaTime);
            }
            v.Guard.AlertTarget = best < GuardAlertRange ? 1f : 0f;
        }

        /// <summary>
        /// Fallback for models the animator could not rig: a small walk bob and lean, an attack lunge, and the yaw offset.
        /// Skipped for models whose Animator already has a controller.
        /// </summary>
        private void ApplyProceduralMotion(UnitView v, bool moving)
        {
            bool animated = v.Animator != null && v.Animator.runtimeAnimatorController != null && v.Params != null && v.Params.Count > 0;
            if (animated)
            {
                v.Holder.localRotation = Quaternion.Euler(0f, v.BaseYaw, 0f);
                return;
            }
            float bob = moving ? Mathf.Abs(Mathf.Sin(Time.time * 9f)) * 0.08f : 0f;
            float lean = moving ? 8f : 0f;
            float punch = Mathf.Clamp01((v.PunchUntil - Time.time) / 0.2f);
            v.Holder.localPosition = new Vector3(0f, bob, 0f);
            v.Holder.localRotation = Quaternion.Euler(lean + punch * 12f, v.BaseYaw, 0f);
            v.Holder.localScale = Vector3.one * (1f + punch * 0.06f);
        }

        private static readonly string[] AbilityTriggers = { "TrigQ", "TrigW", "TrigE", "TrigR" };

        private static void Trigger(UnitView v, string name)
        {
            if (v.Animator != null && v.Params != null && v.Params.Contains(name)) v.Animator.SetTrigger(name);
        }

        /// <summary>
        /// Routes duel events to the animators: basic attacks, ability casts (clip chosen from the ability's shape),
        /// summoner spells (by name), hits and recoils. Death, respawn and teleport are driven from unit state in SyncViews.
        /// Prefabs that ship their own Animator controller still get TrigAttack / TrigQ..R / TrigHit.
        /// </summary>
        private void AnimateFromEvents(List<DuelEvent> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (!_views.TryGetValue(e.UnitId, out var v)) continue;
                if (e.Kind == DuelEventKind.Attack)
                {
                    Trigger(v, "TrigAttack");
                    v.PunchUntil = Time.time + 0.2f;
                    if (v.Anim != null) v.Anim.PlayBasic();
                }
                else if (e.Kind == DuelEventKind.Damage && e.Text != "Bio")
                {
                    Trigger(v, "TrigHit");
                    if (v.Anim != null) v.Anim.PlayHit();
                    if (v.Guard != null) v.Guard.Flinch();
                }
                else if (e.Kind == DuelEventKind.Text && v.Unit.Kind == UnitKind.Champion)
                {
                    var abilities = v.Unit.Template.Abilities;
                    bool isAbility = false;
                    for (int k = 0; k < abilities.Length; k++)
                    {
                        if (abilities[k] == null || abilities[k].Name != e.Text) continue;
                        isAbility = true;
                        if (k < AbilityTriggers.Length) Trigger(v, AbilityTriggers[k]);
                        v.PunchUntil = Time.time + 0.2f;
                        if (v.Anim != null) v.Anim.PlayAbility(abilities[k].Shape, k);
                        break;
                    }
                    if (!isAbility && v.Anim != null) v.Anim.PlaySpell(e.Text);
                }
            }
        }

        private void SpawnFlash(DuelEvent e)
        {
            GameObject go;
            if (e.Shape == AreaShape.Circle)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.transform.position = ToWorld(e.Pos) + Vector3.up * 0.05f;
                go.transform.localScale = new Vector3(e.Width, 0.02f, e.Width);
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var mid = e.Pos + e.Dir * (e.Length * 0.5f);
                go.transform.position = ToWorld(mid) + Vector3.up * 0.05f;
                if (e.Dir.Length > 0.01f) go.transform.rotation = Quaternion.LookRotation(ToWorld(e.Dir));
                go.transform.localScale = new Vector3(e.Width, 0.02f, Mathf.Max(0.1f, e.Length));
            }
            RemoveCollider(go);
            go.name = "AbilityFlash";
            go.transform.SetParent(transform, true);
            go.GetComponent<Renderer>().sharedMaterial = Pick(flashMat, new Color(0.6f, 0.9f, 1f, 0.4f));
            _flashes.Add(new Flash { Go = go, DieAt = Time.time + 0.3f });
        }

        private void UpdateFlashes()
        {
            for (int i = _flashes.Count - 1; i >= 0; i--)
            {
                if (Time.time < _flashes[i].DieAt) continue;
                if (_flashes[i].Go != null) Destroy(_flashes[i].Go);
                _flashes.RemoveAt(i);
            }
        }

        // ------------------------------------------------------------------ input from the touch UI

        public void SetStick(Vector2 v) { _stick = v; }
        public void SetAttackHeld(bool held) { _attackHeld = held; }

        /// <summary>Direction to the nearest hostile unit, else the way the player is facing.</summary>
        public DVec2 AutoAim()
        {
            var t = World.Nearest(Player, 10f, u => DuelWorld.IsHittableBy(Player, u));
            if (t == null) return Player.Facing;
            var d = (t.Pos - Player.Pos).Normalized;
            return d.Length < 0.01f ? Player.Facing : d;
        }

        private DVec2 MoveOrFacing()
        {
            if (_stick.sqrMagnitude > 0.04f) return new DVec2(_stick.x, _stick.y).Normalized;
            var kb = KeyboardMove();
            if (kb.sqrMagnitude > 0.04f) return new DVec2(kb.x, kb.y).Normalized;
            return Player.Facing;
        }

        public CastOutcome CastAbility(int slot, Vector2? screenDirection)
        {
            var aim = screenDirection.HasValue ? new DVec2(screenDirection.Value.x, screenDirection.Value.y).Normalized : AutoAim();
            var outcome = World.TryCastAbility(Player.Id, slot, aim);
            Report(outcome);
            return outcome;
        }

        public CastOutcome CastSummoner(int slot, Vector2? screenDirection)
        {
            DVec2 aim;
            if (screenDirection.HasValue) aim = new DVec2(screenDirection.Value.x, screenDirection.Value.y).Normalized;
            else
            {
                var spell = Player.Spells != null ? Player.Spells.SpellIn(slot) : null;
                aim = spell.HasValue && spell.Value == SummonerSpellId.Warp ? MoveOrFacing() : AutoAim();
            }
            var outcome = World.TryCastSummoner(Player.Id, slot, aim);
            Report(outcome);
            return outcome;
        }

        private void Report(CastOutcome outcome)
        {
            if (outcome == CastOutcome.Ok || Ui == null) return;
            string msg;
            switch (outcome)
            {
                case CastOutcome.OnCooldown: msg = "Not ready yet"; break;
                case CastOutcome.NoMana: msg = "Not enough mana"; break;
                case CastOutcome.NoTarget: msg = "No valid target in range"; break;
                case CastOutcome.OutOfRange: msg = "Out of range"; break;
                case CastOutcome.Stunned: msg = "You are stunned"; break;
                case CastOutcome.Busy: msg = "Already channeling"; break;
                case CastOutcome.Dead: msg = "You are down"; break;
                case CastOutcome.EmptySlot: msg = "Empty slot"; break;
                default: msg = "Can't do that"; break;
            }
            Ui.Toast(msg);
        }

        // ------------------------------------------------------------------ editor / desktop keyboard

        private static Vector2 KeyboardMove()
        {
            float x = 0f, y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            return new Vector2(x, y);
        }

        private Vector2? MouseAim()
        {
            var cam = ViewCamera;
            if (cam == null || !Input.mousePresent) return null;
            var plane = new Plane(Vector3.up, Vector3.zero);
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!plane.Raycast(ray, out float dist)) return null;
            var hit = ray.GetPoint(dist);
            var d = new Vector2(hit.x - Player.Pos.X, hit.z - Player.Pos.Y);
            return d.sqrMagnitude < 0.01f ? (Vector2?)null : d.normalized;
        }

        private void ReadKeyboard()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) CastAbility(0, MouseAim());
            if (Input.GetKeyDown(KeyCode.Alpha2)) CastAbility(1, MouseAim());
            if (Input.GetKeyDown(KeyCode.Alpha3)) CastAbility(2, MouseAim());
            if (Input.GetKeyDown(KeyCode.Alpha4)) CastAbility(3, MouseAim());
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Z)) CastSummoner(0, null);
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.X)) CastSummoner(1, null);
        }

        // ------------------------------------------------------------------ test tools (Practice-style helpers)

        public void ToolResetCooldowns()
        {
            World.ResetCooldowns(Player.Id);
            World.ResetCooldowns(Enemy.Id);
            Player.Mana = Player.MaxMana;
            Ui.Toast("Cooldowns reset");
        }

        public void ToolResetEnemy() { World.ForceRespawn(Enemy.Id); Ui.Toast("Enemy reset"); }

        /// <summary>Turns a model by 90 degrees and remembers the result in the library (saved to the asset when played in the editor).</summary>
        public void ToolTurnModel(string modelId)
        {
            var entry = models != null ? models.Find(modelId) : null;
            if (entry == null) { Ui.Toast("No 3D model linked for " + modelId); return; }
            entry.yawOffset = Mathf.Repeat(entry.yawOffset + 90f, 360f);
            entry.yawTuned = true;
            models.PersistYaw(modelId, entry.yawOffset);
            var rebuild = new List<DuelUnit>();
            foreach (var kv in _views) if (kv.Value.ModelId == modelId) rebuild.Add(kv.Value.Unit);
            for (int i = 0; i < rebuild.Count; i++) RebuildView(rebuild[i]);
            Ui.Toast(modelId + ": yaw " + Mathf.RoundToInt(entry.yawOffset) + (Application.isEditor ? " saved" : " (this run only)"));
        }

        public void ToolRotateModel(bool enemy) { ToolTurnModel(enemy ? EnemyChampionId : PlayerChampionId); }

        private void RebuildView(DuelUnit u)
        {
            if (_views.TryGetValue(u.Id, out var old) && old.Root != null) Destroy(old.Root.gameObject);
            _views.Remove(u.Id);
            CreateView(u);
            if (u == Player && cameraRig != null && _views.TryGetValue(u.Id, out var nv)) cameraRig.SetTarget(nv.Root);
        }

        /// <summary>Moves the camera closer (factor under 1) or farther (over 1) and remembers it.</summary>
        public void ToolZoom(float factor)
        {
            if (cameraRig == null) { Ui.Toast("No camera rig"); return; }
            cameraRig.SetZoom(cameraRig.Zoom * factor);
            PlayerPrefs.SetFloat("pd_zoom", cameraRig.Zoom);
            Ui.Toast(factor < 1f ? "Camera closer" : "Camera farther");
        }

        public void ToolKillMe()
        {
            World.DealDamage(Enemy, Player, Player.MaxHp * 10f, DuelDamage.True, true, "Test damage");
            Ui.Toast("Down! (test the death and respawn animation)");
        }

        public void ToolRestoreSummons()
        {
            World.ForceRespawn(Shiva.Id);
            World.ForceRespawn(Garuda.Id);
            if (Bahamut != null) World.ForceRespawn(Bahamut.Id);
            Ui.Toast(Bahamut != null ? "Shiva, Garuda and Bahamut restored" : "Shiva and Garuda restored");
        }

        public void ToolPickFighters(string playerId, string enemyId)
        {
            StartDuel(playerId, enemyId);
            Ui.Toast(DuelRoster.ById(playerId).Name + " vs " + DuelRoster.ById(enemyId).Name);
        }

        public void ToolHealMe()
        {
            Player.Hp = Player.MaxHp;
            Player.Mana = Player.MaxMana;
            Ui.Toast("Fully healed");
        }

        public void ToolHurtMe()
        {
            World.DealDamage(Enemy, Player, Player.MaxHp * 0.5f, DuelDamage.True, true, "Test damage");
            Ui.Toast("Took 50% damage (test Cure)");
        }

        public void ToolStunMe()
        {
            World.AddStatus(Player, World.MakeStatus(StatusKind.Stun, 1f, 1f, 0f, 2f, Enemy.Id));
            Ui.Toast("Stunned for 2s (test Esuna)");
        }

        public void ToolSlowMe()
        {
            World.AddStatus(Player, World.MakeStatus(StatusKind.MoveSpeedSlow, 0.4f, 0.4f, 0f, 3f, Enemy.Id));
            Ui.Toast("Slowed for 3s (test Esuna)");
        }

        public void ToolLevel(int delta)
        {
            World.SetLevel(Player.Id, Player.Level + delta);
            World.SetLevel(Enemy.Id, Player.Level);
            Ui.Toast("Level " + Player.Level + " (both champions)");
        }

        public void ToolUnlockChampionUltima()
        {
            if (Player.Smite != null) Player.Smite.GrantConsumed(SmiteProgress.FirstUpgradeConsumed);
            Ui.Toast("Hunter's Resource upgrade: Ultima can hit champions");
        }

        public bool ToolToggleBot()
        {
            EnemyBot.Enabled = !EnemyBot.Enabled;
            Ui.Toast(EnemyBot.Enabled ? "Enemy bot ON" : "Enemy bot OFF (training dummy)");
            return EnemyBot.Enabled;
        }

        /// <summary>Puts a spell in slot 0 (D) or 1 (F). Choosing the spell already in the other slot swaps them. Resets spell cooldowns.</summary>
        public void SetPlayerSpell(int slot, SummonerSpellId id)
        {
            var next = Player.Loadout.Clone();
            var result = next.TrySet(slot, id, GameMode.Practice);
            if (result == LoadoutResult.DuplicateSpell)
            {
                next.Swap();
                result = LoadoutResult.Ok;
            }
            if (result != LoadoutResult.Ok) { Ui.Toast("Can't equip that here"); return; }
            World.SetLoadout(Player.Id, next);
            Ui.Toast(SummonerSpellCatalog.Get(id).DisplayName + " equipped (cooldowns restarted)");
        }
    }
}
