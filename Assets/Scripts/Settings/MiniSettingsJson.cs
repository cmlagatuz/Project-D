// Project D  |  Codename: Zantetsuken
// MiniSettingsJson.cs  (Settings — serialize/deserialize GameSettings)
//
// Uses Unity's JsonUtility when available (the [Serializable] classes map 1:1) but
// is wrapped so the model layer has a single, testable entry point. Kept minimal;
// swap for a hardened serializer before shipping if you need migration hooks.

namespace ProjectD.Settings
{
    public static class MiniSettingsJson
    {
        public static string Serialize(GameSettings s)
        {
#if UNITY_2018_1_OR_NEWER
            return UnityEngine.JsonUtility.ToJson(s, prettyPrint: true);
#else
            return null;
#endif
        }

        public static GameSettings Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
#if UNITY_2018_1_OR_NEWER
            try { return UnityEngine.JsonUtility.FromJson<GameSettings>(json); }
            catch { return null; }
#else
            return null;
#endif
        }
    }
}
