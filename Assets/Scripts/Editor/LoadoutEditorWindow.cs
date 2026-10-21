// Project D  |  Codename: Zantetsuken
// LoadoutEditorWindow.cs  (Editor-only)
//
// A designer-facing window to author each champion's quick-buy build order:
//   - Pick a champion from a dropdown.
//   - See the ordered build as a reorderable list (move up/down, remove).
//   - Add items from a searchable, category-filtered palette (exclusives are
//     limited to the selected champion; only one boots at a time is encouraged).
//   - Live validation against the store slot rules (BuildValidator) with an
//     approximate build cost.
//   - Save all builds back to build_recommendations.seed.json.
//
//   Tools > Project D > Loadout Editor (Quick-Buy Builds)
//
// Reads ItemDefinition assets for the catalog and the seed JSON for existing builds.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ProjectD.Store;

namespace ProjectD.EditorTools
{
    public sealed class LoadoutEditorWindow : EditorWindow
    {
        private const string SeedPath = "Assets/GameData/Items/build_recommendations.seed.json";

        // Roster (id -> display). Kept here so the editor works even before other data loads.
        private static readonly (string id, string name, string role)[] Champions =
        {
            ("wol","Warrior of Light","Vanguard"), ("garland","Garland","Fighter"),
            ("firion","Firion","Marksman"), ("emperor","The Emperor","Mage"),
            ("onion_knight","Onion Knight","Assassin"), ("cloud_of_darkness","Cloud of Darkness","Mage"),
            ("cecil","Cecil Harvey","Vanguard"), ("kain","Kain Highwind","Assassin"),
            ("bartz","Bartz Klauser","Fighter"), ("exdeath","Exdeath","Vanguard"),
        };

        private CatalogAdapter _catalog;
        private Dictionary<string, ItemDefinition> _defs;
        private Dictionary<string, List<string>> _builds;   // championId -> ordered ids
        private Dictionary<string, string> _roles;          // championId -> role

        private int _champIndex;
        private ReorderableList _list;
        private Vector2 _paletteScroll, _issuesScroll;
        private string _search = "";
        private int _categoryFilter; // 0 = All, else ItemCategory
        private bool _dirty;

        [MenuItem("Tools/Project D/Loadout Editor (Quick-Buy Builds)", priority = 74)]
        public static void Open()
        {
            var w = GetWindow<LoadoutEditorWindow>(false, "Loadout Editor");
            w.minSize = new Vector2(900, 560);
            w.Reload();
        }

        private void OnEnable() { if (_builds == null) Reload(); }

        private void Reload()
        {
            LoadCatalog();
            LoadBuilds();
            RebuildList();
            _dirty = false;
        }

        // ---- data loading ---- //
        private void LoadCatalog()
        {
            _defs = new Dictionary<string, ItemDefinition>();
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/GameData/Items" }))
            {
                var d = AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (d != null && !string.IsNullOrEmpty(d.itemId)) _defs[d.itemId] = d;
            }
            _catalog = new CatalogAdapter(_defs);
        }

        private void LoadBuilds()
        {
            _builds = new Dictionary<string, List<string>>();
            _roles = new Dictionary<string, string>();
            foreach (var (id, _, role) in Champions) { _builds[id] = new List<string>(); _roles[id] = role; }

            if (File.Exists(SeedPath))
            {
                var root = MiniJSON.Deserialize(File.ReadAllText(SeedPath)) as Dictionary<string, object>;
                if (root != null && root.TryGetValue("builds", out var bo) && bo is List<object> builds)
                {
                    foreach (var b in builds)
                    {
                        var d = (Dictionary<string, object>)b;
                        string champ = Str(d, "championId");
                        if (string.IsNullOrEmpty(champ)) continue;
                        var order = new List<string>();
                        if (d.TryGetValue("order", out var oo) && oo is List<object> ol)
                            foreach (var it in ol) order.Add(it?.ToString());
                        _builds[champ] = order;
                        if (d.TryGetValue("role", out var r) && r != null) _roles[champ] = r.ToString();
                    }
                }
            }
        }

        private string CurrentChampId => Champions[_champIndex].id;
        private List<string> CurrentOrder => _builds[CurrentChampId];

        // ---- reorderable list ---- //
        private void RebuildList()
        {
            _list = new ReorderableList(CurrentOrder, typeof(string), true, true, false, true);
            _list.drawHeaderCallback = r => EditorGUI.LabelField(r, $"Build order — {Champions[_champIndex].name}  ({_roles[CurrentChampId]})");
            _list.elementHeight = 24f;
            _list.drawElementCallback = (rect, index, active, focused) =>
            {
                string id = CurrentOrder[index];
                var def = Def(id);
                rect.y += 3; rect.height = 18;

                var stepRect = new Rect(rect.x, rect.y, 24, rect.height);
                EditorGUI.LabelField(stepRect, (index + 1).ToString(), EditorStyles.miniBoldLabel);

                var tagRect = new Rect(rect.x + 26, rect.y, 92, rect.height);
                DrawCategoryTag(tagRect, def != null ? def.category : ItemCategory.Basic);

                var nameRect = new Rect(rect.x + 122, rect.y, rect.width - 122 - 70, rect.height);
                string label = def != null ? def.displayName : $"<missing: {id}>";
                var prev = GUI.color; if (def == null) GUI.color = new Color(1f, 0.5f, 0.5f);
                EditorGUI.LabelField(nameRect, label);
                GUI.color = prev;

                var costRect = new Rect(rect.xMax - 66, rect.y, 66, rect.height);
                EditorGUI.LabelField(costRect, def != null ? $"{def.totalCost}g" : "", RightAlign());
            };
            _list.onRemoveCallback = l => { RecordUndo("Remove item"); CurrentOrder.RemoveAt(l.index); MarkDirty(); };
            _list.onReorderCallback = l => MarkDirty();
        }

        // ---- GUI ---- //
        private void OnGUI()
        {
            if (_builds == null) Reload();
            DrawToolbar();

            using (new EditorGUILayout.HorizontalScope())
            {
                // Left: current build + validation
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.52f)))
                {
                    _list.DoLayoutList();
                    DrawValidation();
                }

                // Right: item palette
                using (new EditorGUILayout.VerticalScope())
                    DrawPalette();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                int idx = EditorGUILayout.Popup(_champIndex, Champions.Select(c => $"{c.name} ({c.role})").ToArray(),
                    EditorStyles.toolbarPopup, GUILayout.Width(240));
                if (EditorGUI.EndChangeCheck()) { _champIndex = idx; RebuildList(); }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Auto-Sort", EditorStyles.toolbarButton, GUILayout.Width(80))) AutoSort();
                if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(70))) { if (ConfirmDiscard()) Reload(); }
                using (new EditorGUI.DisabledScope(!_dirty))
                    if (GUILayout.Button(_dirty ? "Save*" : "Save", EditorStyles.toolbarButton, GUILayout.Width(70))) SaveAll();
            }
        }

        private void DrawValidation()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            var issues = BuildValidator.Validate(CurrentChampId, CurrentOrder, _catalog);
            int cost = BuildValidator.ApproxCost(CurrentOrder, _catalog, includeConsumables: false);

            using (var sv = new EditorGUILayout.ScrollViewScope(_issuesScroll, GUILayout.Height(120)))
            {
                _issuesScroll = sv.scrollPosition;
                foreach (var i in issues)
                {
                    var t = i.Severity switch
                    {
                        BuildIssue.Level.Error => MessageType.Error,
                        BuildIssue.Level.Warning => MessageType.Warning,
                        _ => MessageType.Info
                    };
                    EditorGUILayout.HelpBox(i.Message, t);
                }
            }
            EditorGUILayout.LabelField($"Approx. build cost (excl. consumables): {cost}g", EditorStyles.miniBoldLabel);
        }

        private void DrawPalette()
        {
            EditorGUILayout.LabelField("Item Palette — click to add", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
                string[] cats = new[] { "All", "Starter", "Consumable", "Trinket", "Boots", "Basic", "Epic", "Legendary", "Exclusive" };
                _categoryFilter = EditorGUILayout.Popup(_categoryFilter, cats, GUILayout.Width(110));
            }

            using (var sv = new EditorGUILayout.ScrollViewScope(_paletteScroll))
            {
                _paletteScroll = sv.scrollPosition;
                foreach (var def in FilteredPalette())
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        DrawCategoryTag(GUILayoutUtility.GetRect(90, 18, GUILayout.Width(90)), def.category);
                        EditorGUILayout.LabelField(def.displayName, GUILayout.MinWidth(140));
                        GUILayout.Label($"{def.totalCost}g", RightAlign(), GUILayout.Width(60));
                        bool already = CurrentOrder.Contains(def.itemId);
                        using (new EditorGUI.DisabledScope(already))
                            if (GUILayout.Button(already ? "Added" : "Add", GUILayout.Width(60)))
                            { RecordUndo("Add item"); CurrentOrder.Add(def.itemId); MarkDirty(); }
                    }
                }
            }
        }

        private IEnumerable<ItemDefinition> FilteredPalette()
        {
            string champ = CurrentChampId;
            string search = _search?.Trim().ToLowerInvariant() ?? "";
            var wantCat = CategoryFromFilter(_categoryFilter);

            return _defs.Values
                .Where(d => d != null)
                // Exclusives only for the selected champion.
                .Where(d => string.IsNullOrEmpty(d.exclusiveToChampionId) || d.exclusiveToChampionId == champ)
                .Where(d => !wantCat.HasValue || d.category == wantCat.Value)
                .Where(d => search.Length == 0 || d.displayName.ToLowerInvariant().Contains(search) || d.itemId.ToLowerInvariant().Contains(search))
                .OrderBy(d => (int)d.category).ThenBy(d => d.totalCost);
        }

        // ---- actions ---- //
        private void AutoSort()
        {
            RecordUndo("Auto-sort build");
            // Canonical order: Starter -> Boots -> ChampionExclusive -> Epic/Legendary (by cost) -> Basic -> Consumable/Trinket.
            int Rank(ItemCategory c) => c switch
            {
                ItemCategory.Starter => 0, ItemCategory.Boots => 1, ItemCategory.ChampionExclusive => 2,
                ItemCategory.Legendary => 3, ItemCategory.Epic => 4, ItemCategory.Basic => 5,
                ItemCategory.Trinket => 6, ItemCategory.Consumable => 7, _ => 8
            };
            CurrentOrder.Sort((a, b) =>
            {
                var da = Def(a); var db = Def(b);
                int ra = da != null ? Rank(da.category) : 9, rb = db != null ? Rank(db.category) : 9;
                if (ra != rb) return ra - rb;
                int ca = da?.totalCost ?? 0, cb = db?.totalCost ?? 0;
                return ca.CompareTo(cb);
            });
            MarkDirty();
        }

        private void SaveAll()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"_comment\": \"Per-champion recommended build order. Edited via Tools > Project D > Loadout Editor. Starter -> Boots -> core (incl. exclusive) -> situational.\",");
            sb.AppendLine("  \"builds\": [");
            for (int c = 0; c < Champions.Length; c++)
            {
                var champId = Champions[c].id;
                var order = _builds[champId];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"championId\": \"{champId}\",");
                sb.AppendLine($"      \"role\": \"{_roles[champId]}\",");
                sb.Append("      \"order\": [");
                sb.Append(string.Join(", ", order.Select(id => $"\"{id}\"")));
                sb.AppendLine("]");
                sb.Append("    }");
                sb.AppendLine(c < Champions.Length - 1 ? "," : "");
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            File.WriteAllText(SeedPath, sb.ToString());
            AssetDatabase.ImportAsset(SeedPath);
            _dirty = false;
            ShowNotification(new GUIContent("Builds saved"));
            Debug.Log($"[Project D] Saved quick-buy builds to {SeedPath}.");
        }

        // ---- helpers ---- //
        private ItemDefinition Def(string id) => id != null && _defs.TryGetValue(id, out var d) ? d : null;

        private void MarkDirty() { _dirty = true; Repaint(); }
        private void RecordUndo(string label) { /* list edits are immediate; kept for symmetry */ }
        private bool ConfirmDiscard() => !_dirty || EditorUtility.DisplayDialog("Loadout Editor", "Discard unsaved changes?", "Discard", "Cancel");

        private void OnDestroy()
        {
            if (_dirty && EditorUtility.DisplayDialog("Loadout Editor", "You have unsaved build changes. Save now?", "Save", "Discard"))
                SaveAll();
        }

        private static ItemCategory? CategoryFromFilter(int f) => f switch
        {
            1 => ItemCategory.Starter, 2 => ItemCategory.Consumable, 3 => ItemCategory.Trinket,
            4 => ItemCategory.Boots, 5 => ItemCategory.Basic, 6 => ItemCategory.Epic,
            7 => ItemCategory.Legendary, 8 => ItemCategory.ChampionExclusive, _ => null
        };

        private static void DrawCategoryTag(Rect r, ItemCategory c)
        {
            var col = c switch
            {
                ItemCategory.Starter => new Color(0.45f,0.55f,0.65f),
                ItemCategory.Consumable => new Color(0.45f,0.70f,0.50f),
                ItemCategory.Trinket => new Color(0.55f,0.60f,0.75f),
                ItemCategory.Boots => new Color(0.60f,0.50f,0.75f),
                ItemCategory.Basic => new Color(0.50f,0.55f,0.60f),
                ItemCategory.Epic => new Color(0.35f,0.60f,0.85f),
                ItemCategory.Legendary => new Color(0.85f,0.65f,0.30f),
                ItemCategory.ChampionExclusive => new Color(0.95f,0.80f,0.32f),
                _ => Color.gray
            };
            var prev = GUI.color; GUI.color = col;
            GUI.Box(r, GUIContent.none);
            GUI.color = Color.black;
            GUI.Label(new Rect(r.x + 4, r.y, r.width - 4, r.height), ShortCat(c), EditorStyles.miniBoldLabel);
            GUI.color = prev;
        }

        private static string ShortCat(ItemCategory c) => c switch
        {
            ItemCategory.ChampionExclusive => "EXCLUSIVE", ItemCategory.Consumable => "CONSUM.",
            _ => c.ToString().ToUpperInvariant()
        };

        private static GUIStyle RightAlign()
        {
            var s = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };
            return s;
        }

        private static string Str(Dictionary<string, object> d, string k)
            => d != null && d.TryGetValue(k, out var v) && v != null ? v.ToString() : "";

        // Adapter exposing ItemDefinition assets to the engine-agnostic BuildValidator.
        private sealed class CatalogAdapter : IBuildCatalog
        {
            private readonly Dictionary<string, ItemDefinition> _d;
            public CatalogAdapter(Dictionary<string, ItemDefinition> d) => _d = d;
            public bool Exists(string id) => id != null && _d.ContainsKey(id);
            public ItemCategory CategoryOf(string id) => _d.TryGetValue(id, out var x) ? x.category : ItemCategory.Basic;
            public string ExclusiveChampionOf(string id) => _d.TryGetValue(id, out var x) ? x.exclusiveToChampionId ?? "" : "";
            public int TotalCostOf(string id) => _d.TryGetValue(id, out var x) ? x.totalCost : 0;
            public string DisplayNameOf(string id) => _d.TryGetValue(id, out var x) ? x.displayName : id;
        }
    }
}
#endif
