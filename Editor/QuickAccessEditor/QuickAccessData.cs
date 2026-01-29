using System;
using System.Collections.Generic;

namespace UniCore.Editor.QuickAccess
{
    [Serializable]
    public class QuickAccessDB
    {
        public int favoriteLimit = 8;
        public List<GroupData> groups = new();
        public List<FavoriteStat> stats = new();
    }

    [Serializable]
    public class GroupData
    {
        public string groupName;
        public bool groupExpand = true;
        public List<AssetAddress> assets = new();
    }

    [Serializable]
    public class AssetAddress
    {
        public string name;
        public string guidAsset;
    }

    [Serializable]
    public class FavoriteStat
    {
        public string guid;
        public int score;
        public long lastUseTicks;
    }
}