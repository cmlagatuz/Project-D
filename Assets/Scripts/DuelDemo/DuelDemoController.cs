// Project D | Codename: Zantetsuken — DuelDemoController.cs
// Unity view for the 1v1 duel demo. Owns the engine-agnostic DuelWorld, ticks it at a fixed step, mirrors units into
// the scene (primitive placeholder shapes — original art only), flashes ability areas, and exposes the commands the
// touch UI and the editor keyboard call. All rules live in ProjectD.Duel; this file only draws and forwards input.
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

        [SerializeField] private Material groundMat;
        [SerializeField] private Material blueMat;
        [SerializeField] private Material redMat;
        [SerializeField] private Material neutralMat;
        [SerializeField] private Material wardMat;
        [SerializeField] private Material flashMat;
        [SerializeField] private Material markMat;
        [SerializeField] private ArenaCameraRig cameraRig;
        [SerializeField] private Camera viewCamera;

        public DuelWorld World { get; private set; }
        public DuelUnit Player { get; private set; }
        public DuelUnit Enemy { get; private set; }
        public DuelUnit Golem { get; private set; }
        public DuelBot EnemyBot { get; private set; }
        public DuelDemoUI Ui { get; private set; }
        public Camera ViewCamera => viewCamera != null ? viewCamera : Camera.main;

        private sealed class UnitView
        {
            public DuelUnit Unit;
            public Transform Root;
            public GameObject Shield;
            public GameObject Mark;
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

        // ------------------------------------------------------------------ setup

        private void Start()
        {
            World = new DuelWorld { Mode = GameMode.Practice };

            var mine = SummonerLoadout.Default();                                    // Cure + Warp
            Player = World.AddChampion(DuelRoster.WarriorOfLight(), 0, new DVec2(-6f, -6f), 5, mine);

            var theirs = SummonerLoadout.Default();
            theirs.TrySet(0, SummonerSpellId.Esuna, GameMode.Practice);
            theirs.TrySet(1, SummonerSpellId.Cure, GameMode.Practice);
            Enemy = World.AddChampion(DuelRoster.Garland(), 1, new DVec2(6f, 6f), 5, theirs);

            Golem = World.AddMonster("Training Golem", new DVec2(-6f, 8f), 1500f);
            World.AddWard(0, new DVec2(11f, -11f));

            EnemyBot = new DuelBot(Enemy.Id);
            World.AddBot(EnemyBot);

            BuildArena();
            for (int i = 0; i < World.Units.Count; i++) CreateView(World.Units[i]);

            if (cameraRig != null && _views.TryGetValue(Player.Id, out var pv)) cameraRig.SetTarget(pv.Root);

            Ui = gameObject.AddComponent<DuelDemoUI>();
            Ui.Init(this);
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
            PrimitiveType prim = u.Kind == UnitKind.Champion ? PrimitiveType.Capsule
                : u.Kind == UnitKind.Monster ? PrimitiveType.Cube : PrimitiveType.Cylinder;

            var root = new GameObject("View_" + u.Name);
            root.transform.SetParent(transform, false);

            var body = GameObject.CreatePrimitive(prim);
            body.name = "Body";
            RemoveCollider(body);
            body.transform.SetParent(root.transform, false);

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
                nose.transform.SetParent(root.transform, false);
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

            var shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.name = "ShieldBubble";
            RemoveCollider(shield);
            shield.transform.SetParent(root.transform, false);
            shield.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            shield.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            shield.GetComponent<Renderer>().sharedMaterial = Pick(flashMat, new Color(0.6f, 0.9f, 1f, 0.4f));
            shield.SetActive(false);

            var mark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mark.name = "WarpstrikeMark";
            RemoveCollider(mark);
            mark.transform.SetParent(root.transform, false);
            mark.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            mark.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            mark.GetComponent<Renderer>().sharedMaterial = Pick(markMat, new Color(1f, 0.85f, 0.2f));
            mark.SetActive(false);

            _views[u.Id] = new UnitView { Unit = u, Root = root.transform, Shield = shield, Mark = mark };
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
            if (Ui != null) Ui.OnEvents(events);

            UpdateFlashes();
        }

        private static Vector3 ToWorld(DVec2 p) => new Vector3(p.X, 0f, p.Y);

        private void SyncViews()
        {
            float now = World.Time;
            foreach (var kv in _views)
            {
                var v = kv.Value;
                var u = v.Unit;
                v.Root.gameObject.SetActive(u.Alive);
                if (!u.Alive) continue;
                v.Root.position = ToWorld(u.Pos);
                if (u.Facing.Length > 0.01f) v.Root.rotation = Quaternion.LookRotation(ToWorld(u.Facing));
                v.Shield.SetActive(u.ShieldAmount(now) > 0f);
                v.Mark.SetActive(Player.MarkTargetId == u.Id && now < Player.MarkExpiresAt);
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

        public void ToolReviveGolem() { World.ForceRespawn(Golem.Id); Ui.Toast("Golem restored"); }

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
