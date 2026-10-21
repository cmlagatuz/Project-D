// Project D  |  Codename: Zantetsuken
// TouchHudLayoutStore.cs  (Touch HUD customization — local layout persistence)
//
// Stores each HUD element's position/scale/opacity so a player's custom control
// layout survives between sessions. Dependency-free (no JSON library assumed) —
// one PlayerPrefs string, pipe/semicolon-delimited, matching the project's existing
// preference for small hand-rolled formats over pulling in a JSON dependency for
// something this simple.

using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ProjectD.UI.HudEditor
{
    [System.Serializable]
    public struct HudElementLayout
    {
        public string ElementId;
        public float X, Y;   // anchored position
        public float Scale;  // uniform scale multiplier
        public float Alpha;  // 0..1 opacity
    }

    public static class TouchHudLayoutStore
    {
        private const string Key = "ProjectD.TouchHudLayout.v1";

        public static void Save(IEnumerable<HudElementLayout> layouts)
        {
            var rows = new List<string>();
            foreach (var l in layouts)
            {
                rows.Add(string.Join("~", new[]
                {
                    l.ElementId,
                    l.X.ToString(CultureInfo.InvariantCulture),
                    l.Y.ToString(CultureInfo.InvariantCulture),
                    l.Scale.ToString(CultureInfo.InvariantCulture),
                    l.Alpha.ToString(CultureInfo.InvariantCulture),
                }));
            }
            PlayerPrefs.SetString(Key, string.Join(";", rows));
            PlayerPrefs.Save();
        }

        public static Dictionary<string, HudElementLayout> Load()
        {
            var result = new Dictionary<string, HudElementLayout>();
            var raw = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(raw)) return result;

            foreach (var row in raw.Split(';'))
            {
                var parts = row.Split('~');
                if (parts.Length != 5) continue;
                if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) continue;
                if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) continue;
                if (!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var s)) continue;
                if (!float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var a)) continue;
                result[parts[0]] = new HudElementLayout { ElementId = parts[0], X = x, Y = y, Scale = s, Alpha = a };
            }
            return result;
        }

        public static bool HasSavedLayout() => PlayerPrefs.HasKey(Key);
        public static void ClearSaved() { PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); }
    }
}
