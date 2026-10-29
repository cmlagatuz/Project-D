// Project D | Codename: Zantetsuken — DuelDemoUIPanels.cs
// The duel demo's practice-style screens (second half of DuelDemoUI; same class, code-built uGUI, no prefabs):
//   score bar     : top centre, team kills, match clock, your K/D and gold
//   PRACTICE      : top-left tool panel (my champion / enemy champion / other), toggles show [ON]/[OFF] as text
//   INFO          : scoreboard, stats and skills tabs
//   SHOP          : original item catalog, category tabs, recipe credits, buy and sell (modal, pauses the duel)
//   CHAMPION SELECT: VS-style picker (modal)
// Colour is never the only cue: toggles say ON/OFF, tabs get a ">" marker, teams are written out (BLUE / RED).
// The layout follows the practice-tool flow of mobile MOBAs; every icon, colour, word and number here is original.
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Duel;
using ProjectD.SummonerSpells;

namespace ProjectD.DuelDemo
{
    public sealed partial class DuelDemoUI
    {
        private static readonly Color PanelBg = new Color(0.05f, 0.07f, 0.11f, 0.97f);
        private static readonly Color BtnOff = new Color(0.16f, 0.2f, 0.3f, 0.95f);
        private static readonly Color BtnOn = new Color(0.14f, 0.46f, 0.32f, 0.98f);
        private static readonly Color BtnDisabled = new Color(0.1f, 0.11f, 0.14f, 0.9f);
        private static readonly Color Accent = new Color(1f, 0.9f, 0.5f);
        private static readonly Color Soft = new Color(1f, 1f, 1f, 0.62f);

        private static string F(float v, string fmt = "0.#")
        {
            return v.ToString(fmt, System.Globalization.CultureInfo.InvariantCulture);
        }

        // ------------------------------------------------------------------ shared widgets

        /// <summary>A button whose label and highlight are recomputed every frame while it is visible (toggles, tabs, shop buttons).</summary>
        private sealed class LiveButton
        {
            public Button Btn;
            public Image Bg;
            public Text Label;
            public string Base;
            public System.Func<bool> State;      // true = highlighted (and "[ON]" when there is no LabelFn)
            public System.Func<string> LabelFn;  // full label, overrides the default
            public bool Disabled;                // not available yet: dark, labelled "(soon)", no click
        }

        private readonly List<LiveButton> _live = new List<LiveButton>();

        /// <summary>x and y are the top-left corner inside the parent, y counted downwards.</summary>
        private LiveButton AddLive(Transform parent, string label, float x, float y, float w, float h, int font,
            System.Action onClick, System.Func<bool> state = null, System.Func<string> labelFn = null, bool disabled = false)
        {
            var b = NewButton(parent, label, new Vector2(0f, 1f), new Vector2(x + w * 0.5f, -y - h * 0.5f), new Vector2(w, h), font,
                disabled ? null : onClick);
            var lb = new LiveButton
            {
                Btn = b, Bg = b.GetComponent<Image>(), Label = b.GetComponentInChildren<Text>(),
                Base = label, State = state, LabelFn = labelFn, Disabled = disabled
            };
            if (disabled) b.interactable = false;
            _live.Add(lb);
            RefreshLive(lb);
            return lb;
        }

        private static void RefreshLive(LiveButton lb)
        {
            if (lb.Disabled)
            {
                lb.Label.text = lb.Base + " (soon)";
                lb.Label.color = new Color(1f, 1f, 1f, 0.45f);
                lb.Bg.color = BtnDisabled;
                return;
            }
            bool on = lb.State != null && lb.State();
            if (lb.LabelFn != null) lb.Label.text = lb.LabelFn();
            else if (lb.State != null) lb.Label.text = lb.Base + (on ? "  [ON]" : "  [OFF]");
            if (lb.State != null) lb.Bg.color = on ? BtnOn : BtnOff;
        }

        /// <summary>A vertical scroll area. Returns the content rect; children are placed from its top-left corner.</summary>
        private RectTransform NewScroll(Transform parent, string name, float x, float y, float w, float h, float contentHeight)
        {
            var back = NewImage(parent, name, null, new Color(0f, 0f, 0f, 0.28f), true);
            PlaceTL(back.rectTransform, x, y, w, h);
            var viewport = NewRect("Viewport", back.transform);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = NewRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, contentHeight);

            var sr = back.gameObject.AddComponent<ScrollRect>();
            sr.viewport = viewport;
            sr.content = content;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 40f;
            return content;
        }

        private void Update()
        {
            if (_ctrl == null) return;
            if (Input.GetKeyDown(KeyCode.B)) ToggleShop();
            if (Input.GetKeyDown(KeyCode.Tab)) ToggleInfo();
            if (Input.GetKeyDown(KeyCode.P) && _practicePanel != null) _practicePanel.SetActive(!_practicePanel.activeSelf);
        }

        private float _nextInfoRefresh;

        private void UpdatePanels()
        {
            for (int i = 0; i < _live.Count; i++)
            {
                var lb = _live[i];
                if (lb.Btn != null && lb.Btn.gameObject.activeInHierarchy) RefreshLive(lb);
            }
            if (_infoPanel != null && _infoPanel.activeSelf && Time.unscaledTime >= _nextInfoRefresh)
            {
                _nextInfoRefresh = Time.unscaledTime + 0.12f;
                RefreshInfo();
            }
            if (_shopPanel != null && _shopPanel.activeSelf) RefreshShop();
        }

        // ------------------------------------------------------------------ score bar

        private Text _scoreBlue, _scoreRed, _scoreTimer, _kdaText;
        private int _lastSecs = -1, _lastBlue = -1, _lastRed = -1, _lastKills = -1, _lastDeaths = -1, _lastGold = -1, _lastLevel = -1;

        private void BuildScoreBar(Transform parent)
        {
            var bar = NewImage(parent, "ScoreBar", null, new Color(0f, 0f, 0f, 0.55f), false);
            var rt = bar.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -16f); rt.sizeDelta = new Vector2(500f, 64f);

            // team marks differ by shape as well as colour: BLUE = square, RED = circle
            var blueMark = NewImage(rt, "BlueMark", null, new Color(0.3f, 0.6f, 1f), false);
            PlaceTL(blueMark.rectTransform, 14f, -20f, 24f, 24f);
            _scoreBlue = NewText(rt, "Blue", "BLUE 0", 30, TextAnchor.MiddleLeft, Color.white);
            PlaceTL(_scoreBlue.rectTransform, 46f, -8f, 150f, 48f);

            _scoreTimer = NewText(rt, "Timer", "00:00", 34, TextAnchor.MiddleCenter, Accent);
            PlaceTL(_scoreTimer.rectTransform, 200f, -8f, 100f, 48f);

            _scoreRed = NewText(rt, "Red", "0 RED", 30, TextAnchor.MiddleRight, Color.white);
            PlaceTL(_scoreRed.rectTransform, 304f, -8f, 150f, 48f);
            var redMark = NewImage(rt, "RedMark", _circle, new Color(1f, 0.4f, 0.4f), false);
            PlaceTL(redMark.rectTransform, 462f, -20f, 24f, 24f);

            _kdaText = NewText(parent, "KdaStrip", "", 24, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.9f));
            var krt = _kdaText.rectTransform;
            krt.anchorMin = new Vector2(0.5f, 1f); krt.anchorMax = new Vector2(0.5f, 1f); krt.pivot = new Vector2(0.5f, 1f);
            krt.anchoredPosition = new Vector2(0f, -86f); krt.sizeDelta = new Vector2(640f, 34f);
        }

        private void UpdateScoreBar()
        {
            var w = _ctrl.World;
            var p = _ctrl.Player;
            int secs = Mathf.FloorToInt(w.Time);
            int blue = w.TeamKills(0), red = w.TeamKills(1);
            if (secs != _lastSecs)
            {
                _lastSecs = secs;
                _scoreTimer.text = (secs / 60).ToString("00") + ":" + (secs % 60).ToString("00");
            }
            if (blue != _lastBlue) { _lastBlue = blue; _scoreBlue.text = "BLUE " + blue; }
            if (red != _lastRed) { _lastRed = red; _scoreRed.text = red + " RED"; }
            if (p.Kills != _lastKills || p.Deaths != _lastDeaths || p.Gold != _lastGold || p.Level != _lastLevel)
            {
                _lastKills = p.Kills; _lastDeaths = p.Deaths; _lastGold = p.Gold; _lastLevel = p.Level;
                _kdaText.text = "YOU  Lv" + p.Level + "   K/D " + p.Kills + "/" + p.Deaths + "   Gold " + p.Gold;
            }
        }

        // ------------------------------------------------------------------ practice tool (non-modal)

        private GameObject _practicePanel;

        private void BuildPracticePanel(Transform parent)
        {
            var open = NewButton(parent, "PRACTICE", new Vector2(0f, 1f), new Vector2(30f + 115f, -20f - 28f), new Vector2(230f, 56f), 26,
                () => _practicePanel.SetActive(!_practicePanel.activeSelf));
            open.GetComponent<Image>().color = new Color(0.2f, 0.26f, 0.4f, 0.97f);

            var panel = NewImage(parent, "PracticePanel", null, PanelBg, true);
            _practicePanel = panel.gameObject;
            var rt = panel.rectTransform;
            PlaceTL(rt, 30f, -86f, 610f, 840f);

            var title = NewText(rt, "Title", "Practice tool", 32, TextAnchor.MiddleLeft, Color.white);
            PlaceTL(title.rectTransform, 20f, -8f, 400f, 44f);
            AddLive(rt, "CLOSE", 440f, 8f, 150f, 44f, 24, () => _practicePanel.SetActive(false));

            var content = NewScroll(rt, "PracticeScroll", 10f, -60f, 590f, 770f, 1600f);

            float y = 6f;
            int col = 0;

            void Header(string text)
            {
                if (col != 0) { col = 0; y += 70f; }
                var h = NewText(content, "H_" + text, text, 26, TextAnchor.MiddleLeft, Accent);
                PlaceTL(h.rectTransform, 10f, -y, 560f, 36f);
                y += 42f;
            }

            void Cell(string label, System.Action act, System.Func<bool> state = null, System.Func<string> labelFn = null, bool disabled = false)
            {
                AddLive(content, label, 8f + col * 282f, y, 272f, 60f, 22, act, state, labelFn, disabled);
                col++;
                if (col == 2) { col = 0; y += 70f; }
            }

            Header("MY CHAMPION");
            Cell("Refresh state", () => _ctrl.ToolRefresh(false));
            Cell("Level up", () => _ctrl.ToolLevelUnit(false, 1));
            Cell("Reset level", () => _ctrl.ToolResetLevel(false));
            Cell("Invincible", () => _ctrl.ToolToggleInvincible(false), () => _ctrl.PlayerInvincible);
            Cell("No cooldown", () => _ctrl.ToolToggleNoCooldown(false), () => _ctrl.PlayerNoCooldown);
            Cell("+10000 gold", () => _ctrl.ToolGold(false, 10000));
            Cell("Hurt me 50%", () => _ctrl.ToolHurtMe());
            Cell("Stun me 2s", () => _ctrl.ToolStunMe());
            Cell("Slow me 3s", () => _ctrl.ToolSlowMe());
            Cell("Kill me (death)", () => _ctrl.ToolKillMe());

            Header("ENEMY CHAMPION");
            Cell("Refresh state", () => _ctrl.ToolRefresh(true));
            Cell("Level up", () => _ctrl.ToolLevelUnit(true, 1));
            Cell("Reset level", () => _ctrl.ToolResetLevel(true));
            Cell("Computer (AI)", () => _ctrl.ToolToggleBot(), () => _ctrl.EnemyBot != null && _ctrl.EnemyBot.Enabled);
            Cell("Invincible", () => _ctrl.ToolToggleInvincible(true), () => _ctrl.EnemyInvincible);
            Cell("No cooldown", () => _ctrl.ToolToggleNoCooldown(true), () => _ctrl.EnemyNoCooldown);
            Cell("+1000 gold", () => _ctrl.ToolGold(true, 1000));
            Cell("Champion swap", () => { _practicePanel.SetActive(false); OpenFighterPanel(); });

            Header("OTHER");
            Cell("Enemy dummy", () => _ctrl.ToolSpawnDummy(true));
            Cell("Ally dummy", () => _ctrl.ToolSpawnDummy(false));
            Cell("Clear dummies", () => _ctrl.ToolClearDummies());
            Cell("Restore summons", () => _ctrl.ToolRestoreSummons());
            Cell("Minion waves", null, null, null, true);   // needs minions in the duel engine
            Cell("Fog of war", null, null, null, true);     // needs vision in the duel engine
            Cell("Game speed", () => _ctrl.ToolCycleSpeed(), null, () => "Game speed: " + F(_ctrl.GameSpeed) + "x");
            Cell("Unlock Ultima", () => _ctrl.ToolUnlockChampionUltima());

            Header("VIEW AND MODEL TUNING");
            Cell("Camera closer", () => _ctrl.ToolZoom(0.85f));
            Cell("Camera farther", () => _ctrl.ToolZoom(1.18f));
            Cell("Turn my model 90", () => _ctrl.ToolRotateModel(false));
            Cell("Turn enemy model 90", () => _ctrl.ToolRotateModel(true));
            Cell("Turn Shiva 90", () => _ctrl.ToolTurnModel("shiva"));
            Cell("Turn Garuda 90", () => _ctrl.ToolTurnModel("garuda"));
            Cell("Turn Bahamut 90", () => _ctrl.ToolTurnModel("bahamut"));

            if (col != 0) y += 70f;
            content.sizeDelta = new Vector2(0f, y + 20f);
            _practicePanel.SetActive(false);
        }

        // ------------------------------------------------------------------ info panel: scoreboard / stats / skills

        private GameObject _infoPanel;
        private int _infoTab;
        private readonly GameObject[] _infoBodies = new GameObject[3];
        private Text _scoreSummary;
        private readonly Text[][] _scoreCells = new Text[5][];
        private readonly Image[] _scoreRowBg = new Image[5];
        private Text _statLabels, _statYou, _statFoe, _statItems;
        private readonly Text[] _skillTexts = new Text[4];
        private Text _skillFooter;

        private static readonly float[] ScoreX = { 20f, 480f, 570f, 700f, 860f, 1010f };
        private static readonly float[] ScoreW = { 450f, 80f, 120f, 150f, 140f, 270f };
        private static readonly string[] ScoreHead = { "CHAMPION", "LV", "K / D", "GOLD", "ITEMS", "SUMMONER SPELLS" };

        private void ToggleInfo() { if (_infoPanel != null) _infoPanel.SetActive(!_infoPanel.activeSelf); }

        private void BuildInfoPanel(Transform parent)
        {
            var panel = NewImage(parent, "InfoPanel", null, PanelBg, true);
            _infoPanel = panel.gameObject;
            var rt = panel.rectTransform;
            PlaceCenter(rt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 720f));

            string[] tabs = { "SCOREBOARD", "STATS", "SKILLS" };
            for (int i = 0; i < tabs.Length; i++)
            {
                int k = i;
                AddLive(rt, tabs[i], 20f + i * 250f, 14f, 240f, 60f, 26, () => { _infoTab = k; ShowInfoTab(); }, () => _infoTab == k,
                    () => (_infoTab == k ? "> " : "") + tabs[k]);
            }
            AddLive(rt, "CLOSE", 1100f, 14f, 180f, 60f, 26, () => _infoPanel.SetActive(false));

            for (int i = 0; i < 3; i++)
            {
                var body = NewRect("Body" + i, rt);
                PlaceTL(body, 0f, -90f, 1300f, 620f);
                _infoBodies[i] = body.gameObject;
            }

            // ---- scoreboard
            var sb = (RectTransform)_infoBodies[0].transform;
            _scoreSummary = NewText(sb, "Summary", "", 28, TextAnchor.UpperLeft, Color.white);
            PlaceTL(_scoreSummary.rectTransform, 20f, -4f, 1260f, 90f);
            for (int r = 0; r < 5; r++)
            {
                float ry = 100f + r * 70f;
                if (r > 0)
                {
                    var bg = NewImage(sb, "RowBg" + r, null, new Color(1f, 1f, 1f, 0.07f), false);
                    PlaceTL(bg.rectTransform, 10f, -ry, 1280f, 64f);
                    _scoreRowBg[r] = bg;
                }
                var cells = new Text[6];
                for (int c = 0; c < 6; c++)
                {
                    cells[c] = NewText(sb, "Cell", r == 0 ? ScoreHead[c] : "", r == 0 ? 22 : 28, TextAnchor.MiddleLeft, r == 0 ? Accent : Color.white);
                    PlaceTL(cells[c].rectTransform, ScoreX[c], -ry, ScoreW[c], 64f);
                }
                _scoreCells[r] = cells;
            }

            // ---- stats
            var st = (RectTransform)_infoBodies[1].transform;
            var h1 = NewText(st, "HStat", "STAT", 24, TextAnchor.MiddleLeft, Accent);
            PlaceTL(h1.rectTransform, 40f, -4f, 360f, 40f);
            var h2 = NewText(st, "HYou", "YOU", 24, TextAnchor.MiddleLeft, Accent);
            PlaceTL(h2.rectTransform, 430f, -4f, 380f, 40f);
            var h3 = NewText(st, "HFoe", "ENEMY", 24, TextAnchor.MiddleLeft, Accent);
            PlaceTL(h3.rectTransform, 850f, -4f, 400f, 40f);
            _statLabels = NewText(st, "Labels", "", 27, TextAnchor.UpperLeft, Soft);
            PlaceTL(_statLabels.rectTransform, 40f, -48f, 380f, 500f);
            _statYou = NewText(st, "You", "", 27, TextAnchor.UpperLeft, Color.white);
            PlaceTL(_statYou.rectTransform, 430f, -48f, 400f, 500f);
            _statFoe = NewText(st, "Foe", "", 27, TextAnchor.UpperLeft, Color.white);
            PlaceTL(_statFoe.rectTransform, 850f, -48f, 420f, 500f);
            foreach (var t in new[] { _statLabels, _statYou, _statFoe }) { t.horizontalOverflow = HorizontalWrapMode.Overflow; t.lineSpacing = 1f; }
            _statItems = NewText(st, "Items", "", 24, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.85f));
            PlaceTL(_statItems.rectTransform, 40f, -552f, 1220f, 60f);

            // ---- skills
            var sk = (RectTransform)_infoBodies[2].transform;
            for (int i = 0; i < 4; i++)
            {
                _skillTexts[i] = NewText(sk, "Skill" + i, "", 25, TextAnchor.UpperLeft, Color.white);
                PlaceTL(_skillTexts[i].rectTransform, 30f, -(8f + i * 128f), 1240f, 124f);
            }
            _skillFooter = NewText(sk, "Footer", "", 24, TextAnchor.UpperLeft, Accent);
            PlaceTL(_skillFooter.rectTransform, 30f, -530f, 1240f, 80f);

            ShowInfoTab();
            _infoPanel.SetActive(false);
        }

        private void ShowInfoTab()
        {
            for (int i = 0; i < _infoBodies.Length; i++) _infoBodies[i].SetActive(i == _infoTab);
            _nextInfoRefresh = 0f;
        }

        private void RefreshInfo()
        {
            if (_infoTab == 0) RefreshScoreboard();
            else if (_infoTab == 1) RefreshStats();
            else RefreshSkills();
        }

        private string SpellNames(DuelUnit u)
        {
            var sb = new StringBuilder();
            for (int s = 0; s < 2; s++)
            {
                var cur = u.Loadout.Get(s);
                if (s > 0) sb.Append("   ");
                sb.Append(SummonerKeys[s]).Append(' ').Append(cur.HasValue ? SummonerSpellCatalog.Get(cur.Value).DisplayName : "-");
            }
            return sb.ToString();
        }

        private void RefreshScoreboard()
        {
            var w = _ctrl.World;
            _scoreSummary.text = "BLUE TEAM   " + w.TeamKills(0) + " kills   " + w.TeamGold(0) + " gold\n" +
                                 "RED TEAM    " + w.TeamKills(1) + " kills   " + w.TeamGold(1) + " gold";

            var rows = new List<DuelUnit>();
            for (int i = 0; i < w.Units.Count; i++) if (w.Units[i].Kind == UnitKind.Champion) rows.Add(w.Units[i]);
            rows.Sort((a, b) => a.Team != b.Team ? a.Team.CompareTo(b.Team) : a.Id.CompareTo(b.Id));

            for (int r = 1; r < 5; r++)
            {
                bool has = r - 1 < rows.Count;
                _scoreRowBg[r].gameObject.SetActive(has);
                for (int c = 0; c < 6; c++) _scoreCells[r][c].gameObject.SetActive(has);
                if (!has) continue;
                var u = rows[r - 1];
                var cells = _scoreCells[r];
                cells[0].text = (u == _ctrl.Player ? "YOU  " : "") + u.Name + "  [" + TeamTag(u) + "]" + (u.Alive ? "" : "  (down)");
                cells[1].text = u.Level.ToString();
                cells[2].text = u.Kills + " / " + u.Deaths;
                cells[3].text = u.Gold.ToString();
                cells[4].text = u.Items.Count + " / " + DuelItemCatalog.MaxSlots;
                cells[5].text = SpellNames(u);
                cells[5].fontSize = 22;
                _scoreRowBg[r].color = u == _ctrl.Player ? new Color(1f, 0.95f, 0.5f, 0.16f) : new Color(1f, 1f, 1f, 0.07f);
            }
        }

        private void RefreshStats()
        {
            float now = _ctrl.World.Time;
            var a = _ctrl.Player;
            var b = _ctrl.Enemy;
            var labels = new StringBuilder();
            var you = new StringBuilder();
            var foe = new StringBuilder();

            void Row(string label, string va, string vb)
            {
                labels.Append(label).Append('\n');
                you.Append(va).Append('\n');
                foe.Append(vb).Append('\n');
            }

            Row("Champion", a.Name, b.Name);
            Row("Level", a.Level.ToString(), b.Level.ToString());
            Row("Health", Mathf.CeilToInt(a.Hp) + " / " + Mathf.RoundToInt(a.MaxHp), Mathf.CeilToInt(b.Hp) + " / " + Mathf.RoundToInt(b.MaxHp));
            Row("Mana", Mathf.CeilToInt(a.Mana) + " / " + Mathf.RoundToInt(a.MaxMana), Mathf.CeilToInt(b.Mana) + " / " + Mathf.RoundToInt(b.MaxMana));
            Row("Attack damage", F(a.Ad, "0"), F(b.Ad, "0"));
            Row("Ability power", F(a.Ap, "0"), F(b.Ap, "0"));
            Row("Armor", F(a.Armor, "0"), F(b.Armor, "0"));
            Row("Magic resist", F(a.Mr, "0"), F(b.Mr, "0"));
            Row("Attack speed", F(a.AttackSpeed, "0.00") + " / s", F(b.AttackSpeed, "0.00") + " / s");
            Row("Attack range", F(a.AttackRange), F(b.AttackRange));
            Row("Move speed", F(a.MoveSpeed(now), "0.0"), F(b.MoveSpeed(now), "0.0"));
            Row("Ability haste", F(a.AbilityHaste, "0"), F(b.AbilityHaste, "0"));
            Row("Gold", a.Gold.ToString(), b.Gold.ToString());
            Row("Kills / deaths", a.Kills + " / " + a.Deaths, b.Kills + " / " + b.Deaths);
            Row("Items", a.Items.Count + " / " + DuelItemCatalog.MaxSlots, b.Items.Count + " / " + DuelItemCatalog.MaxSlots);

            _statLabels.text = labels.ToString();
            _statYou.text = you.ToString();
            _statFoe.text = foe.ToString();
            _statItems.text = "Your items: " + ItemNames(a);
        }

        private static string ItemNames(DuelUnit u)
        {
            if (u.Items.Count == 0) return "none (open the SHOP)";
            var sb = new StringBuilder();
            for (int i = 0; i < u.Items.Count; i++)
            {
                var it = DuelItemCatalog.Get(u.Items[i]);
                if (i > 0) sb.Append(", ");
                sb.Append(it != null ? it.Name : u.Items[i]);
            }
            return sb.ToString();
        }

        private void RefreshSkills()
        {
            var u = _ctrl.Player;
            for (int slot = 0; slot < 4; slot++) _skillTexts[slot].text = DescribeAbility(u, slot);
            _skillFooter.text = "Summoner spells:  " + SpellNames(u) + "\nAbility ranks rise with level (rank = 1 + (level - 1) / 3).";
        }

        private string DescribeAbility(DuelUnit u, int slot)
        {
            var ab = u.Template.Abilities[slot];
            if (ab == null) return AbilityKeys[slot] + "   (empty)";
            var w = _ctrl.World;
            int ranks = Mathf.Max(1, ab.Damage.Length);
            int rank = w.AbilityRank(u, ab);

            var sb = new StringBuilder();
            sb.Append(AbilityKeys[slot]).Append("    ").Append(ab.Name).Append("    rank ").Append(rank).Append(" / ").Append(ranks);
            if (rank < ranks) sb.Append("   (next rank at level ").Append(rank * 3 + 1).Append(')');
            sb.Append("\n      ").Append(ab.Shape);
            if (ab.Range > 0f) sb.Append(", range ").Append(F(ab.Range));
            if (ab.Radius > 0f) sb.Append(", radius ").Append(F(ab.Radius));
            if (ab.Damage.Length > 0)
                sb.Append("   |   about ").Append(Mathf.RoundToInt(w.AbilityDamage(u, ab))).Append(' ').Append(ab.Type == DuelDamage.Magical ? "magic" : "physical").Append(" damage");
            sb.Append("\n      cooldown ").Append(F(ab.Cooldown * 100f / (100f + u.AbilityHaste))).Append("s   |   ").Append(Mathf.RoundToInt(ab.Cost)).Append(" mana");
            if (ab.SlowPct > 0f) sb.Append("   |   slows ").Append(Mathf.RoundToInt(ab.SlowPct * 100f)).Append("% for ").Append(F(ab.SlowSeconds)).Append('s');
            if (ab.StunSeconds > 0f) sb.Append("   |   stuns ").Append(F(ab.StunSeconds)).Append('s');
            if (ab.ShieldAmount > 0f) sb.Append("   |   shield ").Append(Mathf.RoundToInt(ab.ShieldAmount)).Append(" for ").Append(F(ab.ShieldSeconds)).Append('s');
            if (ab.LifestealPct > 0f) sb.Append("   |   heals ").Append(Mathf.RoundToInt(ab.LifestealPct * 100f)).Append("% of damage");
            if (ab.Cleanse) sb.Append("   |   cleanses stun, slow and weaken");
            return sb.ToString();
        }

        // ------------------------------------------------------------------ shop (modal)

        private sealed class ShopRow
        {
            public DuelItem Item;
            public RectTransform Rt;
            public Image Bg;
            public Text Price;
            public Text Owned;
        }

        private GameObject _shopPanel;
        private string _shopSel;
        private int _shopSlot = -1;
        private int _shopCat = -1;      // -1 = all
        private RectTransform _shopContent;
        private readonly List<ShopRow> _shopRows = new List<ShopRow>();
        private Text _shopGold, _shopTitle, _shopDetail, _shopMsg;
        private readonly LiveButton[] _slotBtns = new LiveButton[DuelItemCatalog.MaxSlots];

        private static Color CategoryColor(ItemCategory c)
        {
            switch (c)
            {
                case ItemCategory.Attack: return new Color(0.85f, 0.35f, 0.3f);
                case ItemCategory.Magic: return new Color(0.6f, 0.4f, 0.9f);
                case ItemCategory.Defense: return new Color(0.35f, 0.55f, 0.85f);
                case ItemCategory.Move: return new Color(0.3f, 0.75f, 0.5f);
                default: return new Color(0.9f, 0.75f, 0.3f);
            }
        }

        private void ToggleShop()
        {
            if (_shopPanel == null) return;
            if (_shopPanel.activeSelf) { _shopPanel.SetActive(false); return; }
            if ((_spellPanel != null && _spellPanel.activeSelf) || (_fighterPanel != null && _fighterPanel.activeSelf)) return;
            _shopSlot = -1;
            ReflowShop();
            _shopPanel.SetActive(true);
            RefreshShop();
        }

        private void BuildShopPanel(Transform parent)
        {
            var dim = NewImage(parent, "ShopPanel", null, new Color(0f, 0f, 0f, 0.7f), true);
            _shopPanel = dim.gameObject;
            Stretch(dim.rectTransform);

            var box = NewImage(dim.transform, "Box", null, PanelBg, true);
            var rt = box.rectTransform;
            PlaceCenter(rt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 860f));

            var title = NewText(rt, "Title", "SHOP   (the duel is paused)", 34, TextAnchor.MiddleLeft, Color.white);
            PlaceTL(title.rectTransform, 20f, -12f, 800f, 56f);
            _shopGold = NewText(rt, "Gold", "", 34, TextAnchor.MiddleRight, Accent);
            PlaceTL(_shopGold.rectTransform, 1000f, -12f, 480f, 56f);

            string[] cats = { "ALL", "ATTACK", "MAGIC", "DEFENSE", "MOVE", "UTILITY" };
            for (int i = 0; i < cats.Length; i++)
            {
                int k = i;
                AddLive(rt, cats[i], 20f, 90f + i * 72f, 190f, 64f, 26, () => { _shopCat = k - 1; ReflowShop(); },
                    () => _shopCat == k - 1, () => (_shopCat == k - 1 ? "> " : "") + cats[k]);
            }

            int count = DuelItemCatalog.All.Count;
            _shopContent = NewScroll(rt, "ItemScroll", 230f, -90f, 720f, 560f, 16f + count * 104f);
            for (int i = 0; i < count; i++) BuildShopRow(DuelItemCatalog.All[i]);

            // detail panel
            var detail = NewImage(rt, "Detail", null, new Color(0f, 0f, 0f, 0.28f), true);
            PlaceTL(detail.rectTransform, 970f, -90f, 510f, 560f);
            _shopTitle = NewText(detail.transform, "DTitle", "", 34, TextAnchor.UpperLeft, Color.white);
            PlaceTL(_shopTitle.rectTransform, 20f, -14f, 470f, 50f);
            _shopDetail = NewText(detail.transform, "DBody", "", 24, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.9f));
            PlaceTL(_shopDetail.rectTransform, 20f, -70f, 470f, 380f);
            AddLive(detail.transform, "BUY", 20f, 470f, 470f, 70f, 30, () => { if (_shopSel != null) _ctrl.ToolBuy(_shopSel); },
                () => CanAffordSelected(), () => BuyLabel());

            // inventory
            var invLabel = NewText(rt, "InvLabel", "YOUR ITEMS (tap one to select it for selling)", 24, TextAnchor.MiddleLeft, Accent);
            PlaceTL(invLabel.rectTransform, 230f, -668f, 900f, 36f);
            for (int i = 0; i < DuelItemCatalog.MaxSlots; i++)
            {
                int k = i;
                _slotBtns[i] = AddLive(rt, "slot", 230f + i * 156f, 708f, 150f, 76f, 20, () => { _shopSlot = _shopSlot == k ? -1 : k; },
                    () => _shopSlot == k, () => SlotLabel(k));
            }
            AddLive(rt, "SELL", 1180f, 708f, 300f, 76f, 24, () =>
                {
                    if (_shopSlot >= 0 && _shopSlot < _ctrl.Player.Items.Count) { _ctrl.ToolSell(_shopSlot); _shopSlot = -1; }
                }, () => _shopSlot >= 0 && _shopSlot < _ctrl.Player.Items.Count, () => SellLabel());

            _shopMsg = NewText(rt, "Msg", "", 26, TextAnchor.MiddleLeft, Accent);
            PlaceTL(_shopMsg.rectTransform, 230f, -796f, 900f, 44f);
            AddLive(rt, "CLOSE", 20f, 780f, 190f, 64f, 26, () => _shopPanel.SetActive(false));

            _shopPanel.SetActive(false);
        }

        private void BuildShopRow(DuelItem item)
        {
            string id = item.Id;
            var b = NewButton(_shopContent, "", new Vector2(0f, 1f), Vector2.zero, new Vector2(700f, 96f), 20, () => { _shopSel = id; });
            var rt = b.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0f, 1f);
            var row = new ShopRow { Item = item, Rt = rt, Bg = b.GetComponent<Image>() };

            // a round monogram in the category colour (the category is also written out as text)
            var mono = NewImage(rt, "Mono", _circle, CategoryColor(item.Category), false);
            PlaceTL(mono.rectTransform, 12f, -16f, 64f, 64f);
            var letter = NewText(mono.transform, "Letter", item.Name.Substring(0, 1), 36, TextAnchor.MiddleCenter, Color.white);
            Stretch(letter.rectTransform);

            var nm = NewText(rt, "Name", item.Name, 28, TextAnchor.UpperLeft, Color.white);
            PlaceTL(nm.rectTransform, 90f, -8f, 400f, 36f);
            var tag = NewText(rt, "Tag", DuelItem.CategoryName(item.Category) + "  |  TIER " + item.Tier, 18, TextAnchor.UpperLeft, Soft);
            PlaceTL(tag.rectTransform, 90f, -42f, 400f, 24f);
            var stat = NewText(rt, "Stats", item.StatLine(), 20, TextAnchor.UpperLeft, new Color(0.8f, 0.92f, 1f));
            PlaceTL(stat.rectTransform, 90f, -64f, 480f, 28f);
            row.Price = NewText(rt, "Price", "", 30, TextAnchor.MiddleRight, Accent);
            PlaceTL(row.Price.rectTransform, 540f, -8f, 150f, 40f);
            row.Owned = NewText(rt, "Owned", "", 18, TextAnchor.UpperRight, Soft);
            PlaceTL(row.Owned.rectTransform, 470f, -52f, 220f, 40f);
            _shopRows.Add(row);
        }

        private void ReflowShop()
        {
            int idx = 0;
            for (int i = 0; i < _shopRows.Count; i++)
            {
                var row = _shopRows[i];
                bool show = _shopCat < 0 || (int)row.Item.Category == _shopCat;
                row.Rt.gameObject.SetActive(show);
                if (!show) continue;
                row.Rt.anchoredPosition = new Vector2(10f, -(8f + idx * 104f));
                idx++;
            }
            _shopContent.sizeDelta = new Vector2(0f, 16f + idx * 104f);
            _shopContent.anchoredPosition = Vector2.zero;
        }

        private bool CanAffordSelected()
        {
            var it = DuelItemCatalog.Get(_shopSel);
            return it != null && _ctrl.Player.Gold >= _ctrl.World.PriceFor(_ctrl.Player, it);
        }

        private string BuyLabel()
        {
            var it = DuelItemCatalog.Get(_shopSel);
            if (it == null) return "BUY (pick an item)";
            int price = _ctrl.World.PriceFor(_ctrl.Player, it);
            int gold = _ctrl.Player.Gold;
            return gold >= price ? "BUY  " + price : "NEED " + (price - gold) + " MORE";
        }

        private string SlotLabel(int i)
        {
            var items = _ctrl.Player.Items;
            if (i >= items.Count) return "(empty)";
            var it = DuelItemCatalog.Get(items[i]);
            return (_shopSlot == i ? "> " : "") + (it != null ? it.Name : items[i]);
        }

        private string SellLabel()
        {
            var items = _ctrl.Player.Items;
            if (_shopSlot < 0 || _shopSlot >= items.Count) return "SELL (select an item)";
            var it = DuelItemCatalog.Get(items[_shopSlot]);
            int refund = it != null ? (int)(it.Cost * DuelItemCatalog.SellRatio) : 0;
            return "SELL  +" + refund;
        }

        private void RefreshShop()
        {
            var p = _ctrl.Player;
            _shopGold.text = "Gold: " + p.Gold;
            if (_shopSlot >= p.Items.Count) _shopSlot = -1;

            for (int i = 0; i < _shopRows.Count; i++)
            {
                var row = _shopRows[i];
                if (!row.Rt.gameObject.activeSelf) continue;
                int price = _ctrl.World.PriceFor(p, row.Item);
                row.Price.text = price + "g";
                int credit = row.Item.Cost - price;
                row.Owned.text = credit > 0 ? "pieces owned: -" + credit + "g" : "";
                bool sel = row.Item.Id == _shopSel;
                row.Bg.color = sel ? new Color(0.2f, 0.42f, 0.6f, 0.98f) : BtnOff;
            }

            var it = DuelItemCatalog.Get(_shopSel);
            if (it == null)
            {
                _shopTitle.text = "Pick an item";
                _shopDetail.text = "Tap an item on the left.\n\nTier 2 items are built from pieces. Pieces you already own are credited against the price and used up when you buy.\n\nItems sell back for 70% of their price.\n\nYou hold up to " + DuelItemCatalog.MaxSlots + " items.";
                return;
            }

            _shopTitle.text = it.Name;
            var sb = new StringBuilder();
            sb.Append(DuelItem.CategoryName(it.Category)).Append("  |  TIER ").Append(it.Tier).Append("\n\n");
            sb.Append(it.StatLine()).Append("\n\n");
            if (it.BuildsFrom != null)
            {
                sb.Append("Built from: ");
                for (int i = 0; i < it.BuildsFrom.Length; i++)
                {
                    var c = DuelItemCatalog.Get(it.BuildsFrom[i]);
                    if (i > 0) sb.Append(" + ");
                    sb.Append(c != null ? c.Name : it.BuildsFrom[i]);
                }
                sb.Append("\n\n");
            }
            var users = new List<string>();
            for (int i = 0; i < DuelItemCatalog.All.Count; i++)
            {
                var other = DuelItemCatalog.All[i];
                if (other.BuildsFrom != null && System.Array.IndexOf(other.BuildsFrom, it.Id) >= 0) users.Add(other.Name);
            }
            if (users.Count > 0) sb.Append("Builds into: ").Append(string.Join(", ", users.ToArray())).Append("\n\n");
            int now = _ctrl.World.PriceFor(p, it);
            sb.Append("Price now: ").Append(now);
            if (now != it.Cost) sb.Append("  (full price ").Append(it.Cost).Append(')');
            _shopDetail.text = sb.ToString();
        }

        // ------------------------------------------------------------------ champion select (modal, VS layout)

        private int _pickSide;   // 0 = choosing for you, 1 = choosing for the enemy
        private readonly Dictionary<string, Text> _rosterTags = new Dictionary<string, Text>();
        private Text _vsYouName, _vsYouInfo, _vsFoeName, _vsFoeInfo;

        private void BuildFighterPanel(Transform parent)
        {
            var dim = NewImage(parent, "FighterPanel", null, new Color(0f, 0f, 0f, 0.7f), true);
            _fighterPanel = dim.gameObject;
            Stretch(dim.rectTransform);

            var box = NewImage(dim.transform, "Box", null, PanelBg, true);
            var rt = box.rectTransform;
            PlaceCenter(rt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 860f));

            var title = NewText(rt, "Title", "CHAMPION SELECT   (Shiva, Garuda and Bahamut always guard the arena)", 32, TextAnchor.MiddleLeft, Color.white);
            PlaceTL(title.rectTransform, 30f, -12f, 1440f, 52f);

            BuildVsCard(rt, "YOU", 40f, out _vsYouName, out _vsYouInfo);
            BuildVsCard(rt, "ENEMY", 900f, out _vsFoeName, out _vsFoeInfo);
            var vs = NewText(rt, "VS", "VS", 84, TextAnchor.MiddleCenter, Accent);
            PlaceTL(vs.rectTransform, 620f, -170f, 260f, 120f);

            AddLive(rt, "PICK FOR: YOU", 40f, 470f, 330f, 64f, 26, () => { _pickSide = 0; RefreshFighterPanel(); },
                () => _pickSide == 0, () => (_pickSide == 0 ? "> " : "") + "PICK FOR: YOU");
            AddLive(rt, "PICK FOR: ENEMY", 390f, 470f, 330f, 64f, 26, () => { _pickSide = 1; RefreshFighterPanel(); },
                () => _pickSide == 1, () => (_pickSide == 1 ? "> " : "") + "PICK FOR: ENEMY");

            int i = 0;
            foreach (var id in DuelRoster.AllIds)
            {
                string captured = id;
                var t = DuelRoster.ById(id);
                float x = 40f + i * 238f;
                var card = NewButton(rt, "", new Vector2(0f, 1f), new Vector2(x + 110f, -560f - 95f), new Vector2(220f, 190f), 20, () =>
                {
                    if (_pickSide == 0) _pendingPlayer = captured; else _pendingEnemy = captured;
                    RefreshFighterPanel();
                });
                var crt = card.GetComponent<RectTransform>();
                var nm = NewText(crt, "Name", t.Name, 26, TextAnchor.UpperLeft, Color.white);
                PlaceTL(nm.rectTransform, 12f, -10f, 196f, 64f);
                string side = DuelRoster.IsLight(id) ? "LIGHT" : "DARK";
                string look = _ctrl.HasModelFor(id) ? "3D model" : "placeholder shape";
                var info = NewText(crt, "Info", side + "\n" + look, 20, TextAnchor.UpperLeft, new Color(0.8f, 0.9f, 1f));
                PlaceTL(info.rectTransform, 12f, -78f, 196f, 56f);
                var tag = NewText(crt, "Tag", "", 22, TextAnchor.LowerLeft, Accent);
                PlaceTL(tag.rectTransform, 12f, -146f, 196f, 36f);
                _rosterTags[id] = tag;
                i++;
            }

            AddLive(rt, "FIGHT", 500f, 780f, 240f, 70f, 32, () =>
            {
                _fighterPanel.SetActive(false);
                _ctrl.ToolPickFighters(_pendingPlayer, _pendingEnemy);
            });
            AddLive(rt, "CANCEL", 760f, 780f, 240f, 70f, 32, () => _fighterPanel.SetActive(false));

            _fighterPanel.SetActive(false);
        }

        private void BuildVsCard(RectTransform box, string label, float x, out Text nameText, out Text infoText)
        {
            var card = NewImage(box, "Card" + label, null, new Color(1f, 1f, 1f, 0.06f), false);
            PlaceTL(card.rectTransform, x, -80f, 560f, 370f);
            var head = NewText(card.transform, "Head", label, 26, TextAnchor.MiddleLeft, Accent);
            PlaceTL(head.rectTransform, 20f, -10f, 520f, 36f);
            nameText = NewText(card.transform, "Name", "", 46, TextAnchor.MiddleLeft, Color.white);
            PlaceTL(nameText.rectTransform, 20f, -50f, 520f, 64f);
            infoText = NewText(card.transform, "Info", "", 25, TextAnchor.UpperLeft, new Color(0.85f, 0.93f, 1f));
            PlaceTL(infoText.rectTransform, 20f, -124f, 520f, 240f);
        }

        private void OpenFighterPanel()
        {
            _pendingPlayer = _ctrl.PlayerChampionId;
            _pendingEnemy = _ctrl.EnemyChampionId;
            _pickSide = 0;
            RefreshFighterPanel();
            _fighterPanel.SetActive(true);
        }

        private void RefreshFighterPanel()
        {
            foreach (var kv in _rosterTags)
            {
                bool you = kv.Key == _pendingPlayer, foe = kv.Key == _pendingEnemy;
                kv.Value.text = you && foe ? "YOU + ENEMY" : you ? "YOU" : foe ? "ENEMY" : "";
            }
            FillVsCard(_pendingPlayer, _vsYouName, _vsYouInfo);
            FillVsCard(_pendingEnemy, _vsFoeName, _vsFoeInfo);
        }

        private static void FillVsCard(string id, Text nameText, Text infoText)
        {
            var t = DuelRoster.ById(id);
            nameText.text = t.Name + "   (" + (DuelRoster.IsLight(id) ? "LIGHT" : "DARK") + ")";
            var sb = new StringBuilder();
            sb.Append("Health ").Append(F(t.Hp, "0")).Append("   Attack ").Append(F(t.Ad, "0")).Append("   Ability power ").Append(F(t.Ap, "0")).Append('\n');
            sb.Append("Armor ").Append(F(t.Armor, "0")).Append("   Magic resist ").Append(F(t.Mr, "0")).Append("   Range ").Append(F(t.AttackRange)).Append('\n');
            for (int s = 0; s < t.Abilities.Length; s++)
                if (t.Abilities[s] != null) sb.Append('\n').Append(AbilityKeys[s]).Append("  ").Append(t.Abilities[s].Name);
            infoText.text = sb.ToString();
        }
    }
}
