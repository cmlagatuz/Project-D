// Project D | Codename: Zantetsuken — DuelDemoUI.cs
// The duel demo's touch HUD, built entirely in code (no prefabs, no TextMeshPro) so it works in any scene:
//   left thumb  : floating movement stick
//   right thumb : hold-to-attack button, Q/W/E/R arc, and the two summoner spell buttons (D / F)
//   top         : your and the enemy's HP / mana / status line, SHOP, INFO and SPELLS buttons, PRACTICE tool (see DuelDemoUIPanels.cs)
// Colour is never the only cue: cooldowns show numbers, statuses show text tags, damage/heal show signs.
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ProjectD.Duel;
using ProjectD.Modes;
using ProjectD.SummonerSpells;

namespace ProjectD.DuelDemo
{
    public sealed partial class DuelDemoUI : MonoBehaviour
    {
        private static readonly string[] AbilityKeys = { "Q", "W", "E", "R" };
        private static readonly string[] SummonerKeys = { "D", "F" };

        private sealed class ButtonView
        {
            public RectTransform Rt;
            public Image Bg;
            public Image Icon;
            public Image Cd;
            public Text CdText;
            public Text Key;
            public Text Info;
            public Text Charges;
        }

        private sealed class UnitPanel
        {
            public Text Name;
            public RectTransform HpFill;
            public Text HpText;
            public RectTransform ManaFill;
            public Text Status;
        }

        private sealed class WorldBar
        {
            public DuelUnit Unit;
            public RectTransform Root;
            public GameObject BarGroup;
            public RectTransform Fill;
            public Text Label;
        }

        private sealed class Floater
        {
            public Text Text;
            public Vector3 World;
            public float Born;
        }

        private DuelDemoController _ctrl;
        private Font _font;
        private Sprite _circle;
        private Canvas _canvas;
        private RectTransform _worldLayer;
        private RectTransform _safe;
        private Rect _lastSafe;

        private UnitPanel _playerPanel;
        private UnitPanel _enemyPanel;
        private readonly ButtonView[] _ability = new ButtonView[4];
        private readonly ButtonView[] _summoner = new ButtonView[2];
        private readonly List<WorldBar> _bars = new List<WorldBar>();
        private readonly List<Floater> _floaters = new List<Floater>();
        private readonly List<string> _logLines = new List<string>();

        private Text _toast;
        private float _toastUntil;
        private Text _log;
        private GameObject _spellPanel;
        private GameObject _fighterPanel;
        private string _pendingPlayer;
        private string _pendingEnemy;
        private int _pickSlot;
        private readonly Text[] _pickSlotLabels = new Text[2];
        private readonly Dictionary<SummonerSpellId, Text> _cardTags = new Dictionary<SummonerSpellId, Text>();

        /// <summary>True while a modal panel (spells, fighters, shop) is open. The world is paused meanwhile.</summary>
        public bool PanelOpen => (_spellPanel != null && _spellPanel.activeSelf) || (_fighterPanel != null && _fighterPanel.activeSelf)
                                 || (_shopPanel != null && _shopPanel.activeSelf);

        // ------------------------------------------------------------------ build

        public void Init(DuelDemoController ctrl)
        {
            _ctrl = ctrl;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _circle = MakeCircleSprite(128);

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.transform.SetParent(transform, false);
            }

            var go = new GameObject("DuelUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            _canvas = go.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            _worldLayer = NewRect("WorldLayer", go.transform);
            Stretch(_worldLayer);
            _worldLayer.pivot = new Vector2(0.5f, 0.5f);

            _safe = NewRect("SafeArea", go.transform);
            ApplySafeArea();

            BuildTopPanels(_safe);
            BuildStick(_safe);
            BuildRightControls(_safe);
            BuildLogAndToast(_safe);
            BuildScoreBar(_safe);
            BuildTopButtons(_safe);
            BuildPracticePanel(_safe);
            BuildInfoPanel(_safe);
            BuildShopPanel(_safe);      // modal panels last so they sit on top
            BuildSpellPanel(_safe);
            BuildFighterPanel(_safe);
            BuildWorldBars();

            if (!Application.isMobilePlatform)
            {
                var help = NewText(_safe, "KeyHelp", "WASD move | Space attack | 1-4 = Q/W/E/R | Z,X summoners | B shop | Tab info | P practice | mouse aims",
                    22, TextAnchor.LowerCenter, new Color(1f, 1f, 1f, 0.7f));
                help.rectTransform.anchorMin = new Vector2(0.2f, 0f);
                help.rectTransform.anchorMax = new Vector2(0.8f, 0f);
                help.rectTransform.pivot = new Vector2(0.5f, 0f);
                help.rectTransform.anchoredPosition = new Vector2(0f, 8f);
                help.rectTransform.sizeDelta = new Vector2(0f, 34f);
            }

            Toast("1v1 duel demo: test the summoner spells");
        }

        private void ApplySafeArea()
        {
            var sa = Screen.safeArea;
            _lastSafe = sa;
            if (Screen.width <= 0 || Screen.height <= 0) { Stretch(_safe); return; }
            _safe.anchorMin = new Vector2(sa.xMin / Screen.width, sa.yMin / Screen.height);
            _safe.anchorMax = new Vector2(sa.xMax / Screen.width, sa.yMax / Screen.height);
            _safe.offsetMin = Vector2.zero;
            _safe.offsetMax = Vector2.zero;
        }

        private void BuildTopPanels(Transform parent)
        {
            // You: top-left under the PRACTICE button. Enemy: right of the score bar. The centre of the screen stays free.
            _playerPanel = BuildUnitPanel(parent, "PlayerPanel", new Vector2(0f, 1f), new Vector2(30f, -86f), 520f);
            _enemyPanel = BuildUnitPanel(parent, "EnemyPanel", new Vector2(0.5f, 1f), new Vector2(270f, -16f), 420f);
        }

        private UnitPanel BuildUnitPanel(Transform parent, string name, Vector2 anchor, Vector2 pos, float width)
        {
            var root = NewImage(parent, name, null, new Color(0f, 0f, 0f, 0.45f), false);
            var rt = root.rectTransform;
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(width, 132f);
            float inner = width - 20f;

            var p = new UnitPanel();
            p.Name = NewText(rt, "Name", "", 26, TextAnchor.MiddleLeft, Color.white);
            PlaceTL(p.Name.rectTransform, 10f, -4f, inner, 34f);

            p.HpFill = NewBar(rt, "HpBar", new Color(0.25f, 0.8f, 0.35f), out var hpBg);
            PlaceTL(hpBg, 10f, -42f, inner, 28f);
            p.HpText = NewText(hpBg, "HpText", "", 22, TextAnchor.MiddleCenter, Color.white);
            Stretch(p.HpText.rectTransform);

            p.ManaFill = NewBar(rt, "ManaBar", new Color(0.3f, 0.5f, 1f), out var mpBg);
            PlaceTL(mpBg, 10f, -74f, inner, 14f);

            p.Status = NewText(rt, "Status", "", 22, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.5f));
            PlaceTL(p.Status.rectTransform, 10f, -94f, inner, 36f);
            return p;
        }

        private void BuildStick(Transform parent)
        {
            var area = NewImage(parent, "StickArea", null, new Color(0f, 0f, 0f, 0.01f), true);
            var art = area.rectTransform;
            art.anchorMin = new Vector2(0f, 0f);
            art.anchorMax = new Vector2(0.45f, 0.7f);
            art.pivot = new Vector2(0f, 0f);
            art.offsetMin = Vector2.zero;
            art.offsetMax = Vector2.zero;

            var ring = NewImage(art, "Ring", _circle, new Color(1f, 1f, 1f, 0.18f), false);
            var knob = NewImage(art, "Knob", _circle, new Color(1f, 1f, 1f, 0.45f), false);
            foreach (var r in new[] { ring.rectTransform, knob.rectTransform })
            {
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.zero; r.pivot = new Vector2(0.5f, 0.5f);
            }
            ring.rectTransform.sizeDelta = new Vector2(240f, 240f);
            knob.rectTransform.sizeDelta = new Vector2(100f, 100f);
            ring.gameObject.SetActive(false);
            knob.gameObject.SetActive(false);

            var stick = area.gameObject.AddComponent<DuelTouchStick>();
            stick.Area = art;
            stick.Ring = ring.rectTransform;
            stick.Knob = knob.rectTransform;
            stick.Radius = 110f;
            stick.Changed = v => _ctrl.SetStick(v);
        }

        private void BuildRightControls(Transform parent)
        {
            var anchor = new Vector2(1f, 0f);

            // Basic attack: big, hold to repeat.
            var attack = BuildRoundButton(parent, "Attack", anchor, new Vector2(-190f, 190f), 200f, new Color(0.35f, 0.12f, 0.12f, 0.9f));
            attack.Key.text = "ATTACK";
            attack.Key.fontSize = 34;
            var atkBtn = attack.Bg.gameObject.AddComponent<DuelAimButton>();
            atkBtn.HoldMode = true;
            atkBtn.Held = held => _ctrl.SetAttackHeld(held);

            // Q W E R arc, up and left of the attack button.
            Vector2[] arc =
            {
                new Vector2(-440f, 170f), new Vector2(-435f, 300f), new Vector2(-365f, 405f), new Vector2(-190f, 440f)
            };
            for (int i = 0; i < 4; i++)
            {
                int slot = i;
                var v = BuildRoundButton(parent, "Ability" + AbilityKeys[i], anchor, arc[i], 120f, new Color(0.12f, 0.14f, 0.22f, 0.9f));
                v.Key.text = AbilityKeys[i];
                v.Key.fontSize = 50;
                var b = v.Bg.gameObject.AddComponent<DuelAimButton>();
                b.Released = dir => _ctrl.CastAbility(slot, dir);
                _ability[i] = v;
            }

            // Summoner spells D and F, below the arc.
            Vector2[] sums = { new Vector2(-365f, 62f), new Vector2(-490f, 62f) };
            for (int i = 0; i < 2; i++)
            {
                int slot = i;
                var v = BuildRoundButton(parent, "Summoner" + SummonerKeys[i], anchor, sums[i], 104f, new Color(0.14f, 0.12f, 0.2f, 0.95f));
                v.Icon = NewImage(v.Bg.transform, "Icon", null, Color.white, false);
                v.Icon.preserveAspect = true;
                var irt = v.Icon.rectTransform;
                irt.anchorMin = new Vector2(0.5f, 0.5f); irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f); irt.sizeDelta = new Vector2(92f, 92f);
                v.Icon.transform.SetSiblingIndex(0);
                v.Key.text = SummonerKeys[i];
                v.Key.fontSize = 26;
                v.Key.alignment = TextAnchor.UpperLeft;
                Stretch(v.Key.rectTransform);
                v.Key.rectTransform.offsetMin = new Vector2(8f, 0f);
                v.Key.rectTransform.offsetMax = new Vector2(0f, -4f);
                var b = v.Bg.gameObject.AddComponent<DuelAimButton>();
                b.Released = dir => _ctrl.CastSummoner(slot, dir);
                _summoner[i] = v;
            }
        }

        private ButtonView BuildRoundButton(Transform parent, string name, Vector2 anchor, Vector2 pos, float size, Color tint)
        {
            var rim = NewImage(parent, name + "_Rim", _circle, new Color(0.85f, 0.9f, 1f, 0.9f), false);
            PlaceCenter(rim.rectTransform, anchor, pos, new Vector2(size + 8f, size + 8f));

            var v = new ButtonView();
            v.Bg = NewImage(parent, name, _circle, tint, true);
            v.Rt = v.Bg.rectTransform;
            PlaceCenter(v.Rt, anchor, pos, new Vector2(size, size));

            v.Cd = NewImage(v.Rt, "Cooldown", _circle, new Color(0f, 0f, 0f, 0.7f), false);
            Stretch(v.Cd.rectTransform);
            v.Cd.type = Image.Type.Filled;
            v.Cd.fillMethod = Image.FillMethod.Radial360;
            v.Cd.fillOrigin = 2;
            v.Cd.fillClockwise = false;
            v.Cd.fillAmount = 0f;

            v.Key = NewText(v.Rt, "Key", "", 40, TextAnchor.MiddleCenter, Color.white);
            Stretch(v.Key.rectTransform);

            v.CdText = NewText(v.Rt, "CdText", "", 44, TextAnchor.MiddleCenter, Color.white);
            Stretch(v.CdText.rectTransform);

            v.Info = NewText(v.Rt, "Info", "", 18, TextAnchor.LowerCenter, new Color(0.8f, 0.9f, 1f));
            Stretch(v.Info.rectTransform);
            v.Info.rectTransform.offsetMin = new Vector2(0f, 8f);

            v.Charges = NewText(v.Rt, "Charges", "", 26, TextAnchor.LowerRight, new Color(1f, 0.95f, 0.5f));
            Stretch(v.Charges.rectTransform);
            v.Charges.rectTransform.offsetMax = new Vector2(-2f, 0f);
            v.Charges.rectTransform.offsetMin = new Vector2(0f, 4f);
            return v;
        }

        private void BuildLogAndToast(Transform parent)
        {
            _log = NewText(parent, "Log", "", 24, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.9f));
            PlaceTL(_log.rectTransform, 30f, -228f, 760f, 250f);

            _toast = NewText(parent, "Toast", "", 36, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.6f));
            var rt = _toast.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -150f); rt.sizeDelta = new Vector2(1100f, 60f);
        }

        /// <summary>Right-edge column: SHOP, INFO (scoreboard / stats / skills) and SPELLS. PRACTICE lives top-left.</summary>
        private void BuildTopButtons(Transform parent)
        {
            string[] names = { "SHOP", "INFO", "SPELLS" };
            System.Action[] actions = { ToggleShop, ToggleInfo, () => { _practicePanel.SetActive(false); OpenSpellPanel(); } };
            for (int i = 0; i < names.Length; i++)
            {
                var b = NewButton(parent, names[i], new Vector2(1f, 1f), new Vector2(-30f, -20f - i * 76f), new Vector2(190f, 66f), 30, actions[i]);
                b.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
            }
        }

        // ------------------------------------------------------------------ spell picker (modal)

        private void BuildSpellPanel(Transform parent)
        {
            var dim = NewImage(parent, "SpellPanel", null, new Color(0f, 0f, 0f, 0.65f), true);
            _spellPanel = dim.gameObject;
            Stretch(dim.rectTransform);

            var box = NewImage(dim.transform, "Box", null, new Color(0.08f, 0.1f, 0.15f, 0.98f), true);
            PlaceCenter(box.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 900f));

            var title = NewText(box.transform, "Title", "Summoner spells (Practice: all 11 are available)", 36, TextAnchor.MiddleLeft, Color.white);
            PlaceTL(title.rectTransform, 30f, -16f, 1140f, 50f);

            for (int s = 0; s < 2; s++)
            {
                int slot = s;
                var b = NewButton(box.transform, "", new Vector2(0f, 1f), new Vector2(30f + s * 300f, -80f), new Vector2(280f, 64f), 28,
                    () => { _pickSlot = slot; RefreshSpellPanel(); });
                b.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
                _pickSlotLabels[s] = b.GetComponentInChildren<Text>();
            }
            var hint = NewText(box.transform, "Hint", "Pick a slot, then tap a spell. Changing a spell restarts its cooldown.", 24,
                TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.7f));
            PlaceTL(hint.rectTransform, 650f, -80f, 520f, 64f);

            var list = SummonerSpellCatalog.AvailableFor(GameMode.Practice);
            for (int i = 0; i < list.Count; i++)
            {
                var def = list[i];
                var id = def.Id;
                int col = i % 4;
                int row = i / 4;
                var card = NewButton(box.transform, "", new Vector2(0f, 1f), new Vector2(30f + col * 290f, -165f - row * 210f),
                    new Vector2(270f, 190f), 20, () => { _ctrl.SetPlayerSpell(_pickSlot, id); RefreshSpellPanel(); });
                var crt = card.GetComponent<RectTransform>();
                crt.pivot = new Vector2(0f, 1f);
                var label = card.GetComponentInChildren<Text>();
                label.text = "";

                var icon = NewImage(crt, "Icon", SummonerSpellIcons.Load(id), Color.white, false);
                icon.preserveAspect = true;
                PlaceTL(icon.rectTransform, 12f, -12f, 96f, 96f);

                var name = NewText(crt, "Name", def.DisplayName, 28, TextAnchor.UpperLeft, Color.white);
                PlaceTL(name.rectTransform, 116f, -12f, 150f, 40f);

                var tag = NewText(crt, "Tag", "", 30, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.4f));
                PlaceTL(tag.rectTransform, 116f, -54f, 150f, 40f);
                _cardTags[id] = tag;

                var facts = NewText(crt, "Facts", "Cooldown " + Mathf.RoundToInt(def.CooldownSeconds) + "s"
                    + (def.MaxCharges > 1 ? " | " + def.MaxCharges + " charges" : ""), 22, TextAnchor.UpperLeft, new Color(0.8f, 0.9f, 1f));
                PlaceTL(facts.rectTransform, 12f, -120f, 250f, 30f);
                var kind = NewText(crt, "Kind", def.Archetype, 20, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.6f));
                PlaceTL(kind.rectTransform, 12f, -150f, 250f, 30f);
            }

            var close = NewButton(box.transform, "CLOSE", new Vector2(0f, 1f), new Vector2(480f, -810f), new Vector2(240f, 64f), 30,
                () => _spellPanel.SetActive(false));
            close.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            _spellPanel.SetActive(false);
        }

        private void OpenSpellPanel()
        {
            RefreshSpellPanel();
            _spellPanel.SetActive(true);
        }

        private void RefreshSpellPanel()
        {
            var loadout = _ctrl.Player.Loadout;
            for (int s = 0; s < 2; s++)
            {
                var cur = loadout.Get(s);
                string nm = cur.HasValue ? SummonerSpellCatalog.Get(cur.Value).DisplayName : "-";
                _pickSlotLabels[s].text = (_pickSlot == s ? ">> " : "") + SummonerKeys[s] + ": " + nm + (_pickSlot == s ? " <<" : "");
            }
            foreach (var kv in _cardTags)
            {
                string tag = "";
                for (int s = 0; s < 2; s++)
                {
                    var cur = loadout.Get(s);
                    if (cur.HasValue && cur.Value == kv.Key) tag += "[" + SummonerKeys[s] + "] ";
                }
                kv.Value.text = tag;
            }
        }

        /// <summary>The controller rebuilt the duel: drop the old over-head bars and floating text and make new ones.</summary>
        public void OnDuelRestarted()
        {
            for (int i = 0; i < _floaters.Count; i++) if (_floaters[i].Text != null) Destroy(_floaters[i].Text.gameObject);
            _floaters.Clear();
            _logLines.Clear();
            if (_log != null) _log.text = "";
            RebuildWorldBars();
            if (_spellPanel != null && _spellPanel.activeSelf) RefreshSpellPanel();
        }

        /// <summary>Drops and recreates the over-head bars (after units were added or removed).</summary>
        public void RebuildWorldBars()
        {
            for (int i = 0; i < _bars.Count; i++) if (_bars[i].Root != null) Destroy(_bars[i].Root.gameObject);
            _bars.Clear();
            BuildWorldBars();
        }

        // ------------------------------------------------------------------ over-head bars and floating text

        private void BuildWorldBars()
        {
            var units = _ctrl.World.Units;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                var root = NewRect("Bar_" + u.Name, _worldLayer);
                root.anchorMin = new Vector2(0.5f, 0.5f); root.anchorMax = new Vector2(0.5f, 0.5f);
                root.pivot = new Vector2(0.5f, 0f);
                root.sizeDelta = new Vector2(150f, 44f);

                var label = NewText(root, "Label", "", 22, TextAnchor.LowerCenter, Color.white);
                label.rectTransform.anchorMin = new Vector2(0f, 0.4f);
                label.rectTransform.anchorMax = new Vector2(1f, 1f);
                label.rectTransform.offsetMin = Vector2.zero;
                label.rectTransform.offsetMax = Vector2.zero;
                label.horizontalOverflow = HorizontalWrapMode.Overflow;

                var fill = NewBar(root, "Hp", u.Team == 0 ? new Color(0.3f, 0.6f, 1f) : u.Team == 1 ? new Color(1f, 0.35f, 0.35f) : new Color(0.8f, 0.8f, 0.8f), out var bg);
                bg.anchorMin = new Vector2(0f, 0f); bg.anchorMax = new Vector2(1f, 0.35f);
                bg.offsetMin = Vector2.zero; bg.offsetMax = Vector2.zero;

                var bar = new WorldBar { Unit = u, Root = root, BarGroup = bg.gameObject, Fill = fill, Label = label };
                if (u.Kind == UnitKind.Ward) bar.BarGroup.SetActive(false);
                _bars.Add(bar);
            }
        }

        private static string TeamTag(DuelUnit u) => u.Team == 0 ? "BLUE" : u.Team == 1 ? "RED" : "NEUTRAL";

        private void UpdateWorldBars()
        {
            var cam = _ctrl.ViewCamera;
            if (cam == null) return;
            for (int i = 0; i < _bars.Count; i++)
            {
                var b = _bars[i];
                var u = b.Unit;
                if (!u.Alive) { b.Root.gameObject.SetActive(false); continue; }
                var sp = cam.WorldToScreenPoint(new Vector3(u.Pos.X, 2.3f, u.Pos.Y));
                if (sp.z <= 0f) { b.Root.gameObject.SetActive(false); continue; }
                b.Root.gameObject.SetActive(true);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_worldLayer, sp, null, out var local))
                    b.Root.anchoredPosition = local;

                if (u.Kind == UnitKind.Ward) b.Label.text = "Ward (Teleport target)";
                else
                {
                    b.Label.text = u.Name + (u.Kind == UnitKind.Champion ? " Lv" + u.Level : "") + " [" + TeamTag(u) + "]";
                    SetFill(b.Fill, u.MaxHp > 0f ? u.Hp / u.MaxHp : 0f);
                }
            }
        }

        private void AddFloater(DuelUnit u, string text, Color color, int size = 34)
        {
            if (u == null) return;
            var t = NewText(_worldLayer, "Floater", text, size, TextAnchor.MiddleCenter, color);
            t.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            t.rectTransform.sizeDelta = new Vector2(500f, 50f);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            _floaters.Add(new Floater { Text = t, World = new Vector3(u.Pos.X, 2.6f, u.Pos.Y), Born = Time.time });
        }

        private void UpdateFloaters()
        {
            var cam = _ctrl.ViewCamera;
            for (int i = _floaters.Count - 1; i >= 0; i--)
            {
                var f = _floaters[i];
                float age = Time.time - f.Born;
                if (age > 1.2f || cam == null)
                {
                    Destroy(f.Text.gameObject);
                    _floaters.RemoveAt(i);
                    continue;
                }
                var sp = cam.WorldToScreenPoint(f.World);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_worldLayer, sp, null, out var local))
                    f.Text.rectTransform.anchoredPosition = local + new Vector2(0f, age * 70f);
                var c = f.Text.color;
                c.a = Mathf.Clamp01(1f - (age - 0.6f) / 0.6f);
                f.Text.color = c;
            }
        }

        // ------------------------------------------------------------------ events from the world

        public void OnEvents(List<DuelEvent> events)
        {
            if (_ctrl == null) return;
            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                var unit = _ctrl.World.Get(e.UnitId);
                string who = unit != null ? unit.Name : "?";
                string stamp = "[" + e.Time.ToString("0.0") + "s] ";
                switch (e.Kind)
                {
                    case DuelEventKind.Damage:
                        AddFloater(unit, "-" + Mathf.RoundToInt(e.Amount) + (string.IsNullOrEmpty(e.Text) ? "" : " " + e.Text),
                            unit != null && unit.Team == 0 ? new Color(1f, 0.45f, 0.45f) : Color.white);
                        AddLog(stamp + who + " takes " + Mathf.RoundToInt(e.Amount) + (string.IsNullOrEmpty(e.Text) ? "" : " (" + e.Text + ")"));
                        break;
                    case DuelEventKind.Heal:
                        AddFloater(unit, "+" + Mathf.RoundToInt(e.Amount) + (string.IsNullOrEmpty(e.Text) ? "" : " " + e.Text), new Color(0.5f, 1f, 0.6f));
                        AddLog(stamp + who + " heals " + Mathf.RoundToInt(e.Amount) + (string.IsNullOrEmpty(e.Text) ? "" : " (" + e.Text + ")"));
                        break;
                    case DuelEventKind.Shield:
                        AddFloater(unit, "SHIELD " + Mathf.RoundToInt(e.Amount), new Color(0.6f, 0.95f, 1f));
                        AddLog(stamp + who + " gains a " + Mathf.RoundToInt(e.Amount) + " shield");
                        break;
                    case DuelEventKind.Kill:
                        AddFloater(unit, "DEFEATED", new Color(1f, 0.8f, 0.3f), 40);
                        AddLog(stamp + e.Text);
                        break;
                    case DuelEventKind.Respawn:
                        AddLog(stamp + who + " respawned");
                        break;
                    case DuelEventKind.Gold:
                        AddFloater(unit, e.Text, new Color(1f, 0.9f, 0.3f));
                        AddLog(stamp + who + ": " + e.Text);
                        break;
                    case DuelEventKind.Text:
                        AddFloater(unit, e.Text, new Color(1f, 0.95f, 0.7f), 30);
                        AddLog(stamp + who + ": " + e.Text);
                        break;
                }
            }
        }

        private void AddLog(string line)
        {
            _logLines.Add(line);
            while (_logLines.Count > 8) _logLines.RemoveAt(0);
            _log.text = string.Join("\n", _logLines.ToArray());
        }

        public void Toast(string message)
        {
            if (_toast == null) return;
            _toast.text = message;
            _toastUntil = Time.time + 1.8f;
            if (_shopMsg != null) _shopMsg.text = message;   // the shop sits on top of the toast, so it shows the last message itself
        }

        // ------------------------------------------------------------------ per-frame HUD refresh

        private void LateUpdate()
        {
            if (_ctrl == null || _ctrl.World == null) return;
            if (Screen.safeArea != _lastSafe) ApplySafeArea();

            float now = _ctrl.World.Time;
            var p = _ctrl.Player;
            var en = _ctrl.Enemy;

            UpdatePanel(_playerPanel, p, now);
            UpdatePanel(_enemyPanel, en, now);

            for (int i = 0; i < 4; i++) UpdateAbilityButton(i, _ability[i], p, now);
            for (int i = 0; i < 2; i++) UpdateSummonerButton(i, _summoner[i], p, now);

            UpdateWorldBars();
            UpdateFloaters();
            UpdateScoreBar();
            UpdatePanels();

            if (_toast != null)
            {
                float left = _toastUntil - Time.time;
                var c = _toast.color;
                c.a = Mathf.Clamp01(left / 0.4f);
                _toast.color = c;
            }
        }

        private void UpdatePanel(UnitPanel panel, DuelUnit u, float now)
        {
            panel.Name.text = u.Name + "  Lv" + u.Level + "  [" + TeamTag(u) + "]";
            SetFill(panel.HpFill, u.MaxHp > 0f ? u.Hp / u.MaxHp : 0f);
            SetFill(panel.ManaFill, u.MaxMana > 0f ? u.Mana / u.MaxMana : 0f);
            panel.HpText.text = Mathf.CeilToInt(u.Hp) + " / " + Mathf.CeilToInt(u.MaxHp);
            panel.Status.text = u.Alive ? Describe(u, now) : "DEFEATED - respawn in " + Mathf.CeilToInt(Mathf.Max(0f, u.RespawnAt - now)) + "s";
        }

        private static string Describe(DuelUnit u, float now)
        {
            var sb = new StringBuilder();
            if (u.IsStunned(now)) sb.Append("[STUN] ");
            float slow = u.MaxValue(StatusKind.MoveSpeedSlow, now);
            if (slow > 0.01f) sb.Append("[SLOW -").Append(Mathf.RoundToInt(slow * 100f)).Append("%] ");
            float weak = u.MaxValue(StatusKind.DamageDealtReduction, now);
            if (weak > 0.01f) sb.Append("[WEAKENED -").Append(Mathf.RoundToInt(weak * 100f)).Append("%] ");
            float speed = u.Haste.SpeedBonus(now) + u.SumValue(StatusKind.MoveSpeedBuff, now);
            if (speed > 0.01f) sb.Append("[SPEED +").Append(Mathf.RoundToInt(speed * 100f)).Append("%] ");
            if (u.Haste.IsActive(now)) sb.Append("[HASTE ").Append(u.Haste.Remaining(now).ToString("0.0")).Append("s] ");
            float shield = u.ShieldAmount(now);
            if (shield > 0.5f) sb.Append("[SHIELD ").Append(Mathf.RoundToInt(shield)).Append("] ");
            if (u.IsActive(StatusKind.DamageOverTime, now)) sb.Append("[BIO] ");
            float wounds = u.MaxValue(StatusKind.HealingReduction, now);
            if (wounds > 0.01f) sb.Append("[WOUNDS -").Append(Mathf.RoundToInt(wounds * 100f)).Append("%] ");
            if (u.IsActive(StatusKind.DisableImmunity, now)) sb.Append("[IMMUNE] ");
            if (u.Channeling) sb.Append("[TELEPORT ").Append(Mathf.Max(0f, u.ChannelEndsAt - now).ToString("0.0")).Append("s] ");
            if (u.MarkTargetId >= 0 && now < u.MarkExpiresAt) sb.Append("[MARK ").Append(Mathf.Max(0f, u.MarkExpiresAt - now).ToString("0.0")).Append("s] ");
            return sb.ToString();
        }

        private void UpdateAbilityButton(int slot, ButtonView v, DuelUnit p, float now)
        {
            var ab = p.Template.Abilities[slot];
            float rem = _ctrl.World.AbilityCooldownRemaining(p, slot);
            bool onCd = rem > 0.05f;
            bool noMana = p.Mana < ab.Cost;
            v.Cd.fillAmount = ab.Cooldown > 0f ? Mathf.Clamp01(rem / ab.Cooldown) : 0f;
            v.CdText.text = onCd ? Mathf.CeilToInt(rem).ToString() : "";
            v.Key.enabled = !onCd;
            v.Info.text = noMana ? "NO MP" : Mathf.RoundToInt(ab.Cost) + " MP";
            v.Info.color = noMana ? new Color(1f, 0.6f, 0.5f) : new Color(0.8f, 0.9f, 1f);
            v.Bg.color = onCd || noMana || !p.Alive ? new Color(0.08f, 0.09f, 0.13f, 0.9f) : new Color(0.16f, 0.2f, 0.34f, 0.95f);
        }

        private void UpdateSummonerButton(int slot, ButtonView v, DuelUnit p, float now)
        {
            var spell = p.Spells != null ? p.Spells.SpellIn(slot) : null;
            if (!spell.HasValue)
            {
                v.Icon.sprite = null;
                v.Icon.enabled = false;
                v.Info.text = "-";
                v.Cd.fillAmount = 0f;
                v.CdText.text = "";
                v.Charges.text = "";
                return;
            }

            var def = SummonerSpellCatalog.Get(spell.Value);
            v.Icon.enabled = true;
            var sprite = SummonerSpellIcons.Load(spell.Value);
            if (v.Icon.sprite != sprite) v.Icon.sprite = sprite;
            v.Info.text = def.DisplayName;

            bool recast = spell.Value == SummonerSpellId.Warpstrike && p.MarkTargetId >= 0 && now < p.MarkExpiresAt;
            bool ready = p.Spells.IsReady(slot, now);
            float rem = p.Spells.Remaining(slot, now);

            v.Cd.fillAmount = recast ? 0f : p.Spells.Remaining01(slot, now);
            if (recast) v.CdText.text = "AGAIN";
            else v.CdText.text = !ready ? Mathf.CeilToInt(rem).ToString() : "";
            v.CdText.fontSize = recast ? 22 : 40;

            v.Charges.text = def.MaxCharges > 1 ? "x" + p.Spells.Charges(slot, now) : "";
            v.Bg.color = recast ? new Color(0.55f, 0.45f, 0.1f, 0.95f) : new Color(0.14f, 0.12f, 0.2f, 0.95f);
        }

        // ------------------------------------------------------------------ uGUI helpers

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void PlaceTL(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void PlaceCenter(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static void SetFill(RectTransform fill, float fraction)
        {
            fill.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
        }

        private Image NewImage(Transform parent, string name, Sprite sprite, Color color, bool raycast)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        private Text NewText(Transform parent, string name, string value, int size, TextAnchor align, Color color)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            t.text = value;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var sh = rt.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.85f);
            sh.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
        }

        /// <summary>A solid bar. Returns the fill rect; SetFill() changes its width. bg is the bar's background rect.</summary>
        private RectTransform NewBar(Transform parent, string name, Color fillColor, out RectTransform bg)
        {
            var back = NewImage(parent, name, null, new Color(0.04f, 0.05f, 0.08f, 0.9f), false);
            bg = back.rectTransform;
            var fill = NewImage(back.transform, "Fill", null, fillColor, false).rectTransform;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            return fill;
        }

        private Button NewButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize, System.Action onClick)
        {
            var img = NewImage(parent, "Btn_" + label, null, new Color(0.16f, 0.2f, 0.3f, 0.95f), true);
            var rt = img.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var text = NewText(rt, "Label", label, fontSize, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform);
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        private static Sprite MakeCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - r;
                    float dy = y + 0.5f - r;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(r - d);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }
    }
}
