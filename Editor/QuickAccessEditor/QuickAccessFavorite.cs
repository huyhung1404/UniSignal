using System;
using System.Linq;
using UnityEditor;

namespace UniCore.Editor.QuickAccess
{
    internal static class QuickAccessFavorite
    {
        public static void RegisterUse(string guid)
        {
            var stat = QuickAccessStorage.Database().stats.FirstOrDefault(s => s.guid == guid);
            if (stat == null)
            {
                stat = new FavoriteStat { guid = guid };
                QuickAccessStorage.Database().stats.Add(stat);
            }

            stat.score++;
            stat.lastUseTicks = DateTime.Now.Ticks;

            QuickAccessStorage.Database().stats.RemoveAll(s => string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(s.guid)));

            QuickAccessStorage.Save(QuickAccessStorage.Database());
        }

        public static string[] GetFavorites()
        {
            return QuickAccessStorage.Database().stats
                .OrderByDescending(GetWeight)
                .Take(QuickAccessStorage.Database().favoriteLimit)
                .Select(s => s.guid)
                .ToArray();
        }

        private static double GetWeight(FavoriteStat s)
        {
            var days = (DateTime.Now - new DateTime(s.lastUseTicks)).TotalDays;
            return s.score * Math.Exp(-days * 0.15);
        }
    }
}