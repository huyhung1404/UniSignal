using Newtonsoft.Json;
using UnityEditor;

namespace UniCore.Editor.QuickAccess
{
    internal static class QuickAccessStorage
    {
        public const string KEY = "QA_DB";
        private static QuickAccessDB db;

        public static QuickAccessDB Database()
        {
            db ??= Load();
            return db;
        }

        private static QuickAccessDB Load()
        {
            var json = EditorPrefs.GetString(KEY, "");
            return string.IsNullOrEmpty(json) ? new QuickAccessDB() : JsonConvert.DeserializeObject<QuickAccessDB>(json);
        }

        public static void Save(QuickAccessDB database)
        {
            EditorPrefs.SetString(KEY, JsonConvert.SerializeObject(database));
            db = database;
        }
    }
}