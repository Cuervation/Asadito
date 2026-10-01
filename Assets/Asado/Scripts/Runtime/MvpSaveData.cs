using System;
using UnityEngine;

namespace Asadito.Runtime
{
    [Serializable]
    public sealed class MvpSettings
    {
        [Range(20f, 40f)] public float SimulationTimeScale = 20f;
        [Range(0f, 1f)] public float SfxVolume = .8f;
        public bool HapticsEnabled = true;
        public bool TutorialCompleted;
    }

    [Serializable]
    public sealed class MvpSaveData
    {
        public const int CurrentVersion = 5;
        public int Version = CurrentVersion;
        public int MaxUnlockedLevel = 1;
        public int[] StarsByLevel = new int[MvpLevelCatalog.Count];
        public int[] BestScoreByLevel = new int[MvpLevelCatalog.Count];
        public MvpSettings Settings = new MvpSettings();
        public ManagementState Management;

        public void RecordLevelResult(int levelNumber, int score, int stars)
        {
            if (levelNumber < 1 || levelNumber > MvpLevelCatalog.Count) return;
            int index = levelNumber - 1;
            if (StarsByLevel == null || StarsByLevel.Length != MvpLevelCatalog.Count) Array.Resize(ref StarsByLevel, MvpLevelCatalog.Count);
            if (BestScoreByLevel == null || BestScoreByLevel.Length != MvpLevelCatalog.Count) Array.Resize(ref BestScoreByLevel, MvpLevelCatalog.Count);
            StarsByLevel[index] = Mathf.Max(StarsByLevel[index], Mathf.Clamp(stars, 0, 3));
            BestScoreByLevel[index] = Mathf.Max(BestScoreByLevel[index], score);
            if (stars >= 1 && levelNumber >= MaxUnlockedLevel && levelNumber < MvpLevelCatalog.Count)
                MaxUnlockedLevel = Mathf.Max(MaxUnlockedLevel, levelNumber + 1);
        }

        public static MvpSaveData Migrate(MvpSaveData data)
        {
            if (data == null || data.Version < 1 || data.Version > CurrentVersion) data = new MvpSaveData();
            if (data.StarsByLevel == null) data.StarsByLevel = new int[0];
            if (data.BestScoreByLevel == null) data.BestScoreByLevel = new int[0];
            Array.Resize(ref data.StarsByLevel, MvpLevelCatalog.Count);
            Array.Resize(ref data.BestScoreByLevel, MvpLevelCatalog.Count);
            data.MaxUnlockedLevel = Mathf.Clamp(data.MaxUnlockedLevel, 1, MvpLevelCatalog.Count);
            if (data.Settings == null) data.Settings = new MvpSettings();
            else if (data.Version < 3 && Mathf.Approximately(data.Settings.SimulationTimeScale, 30f))
                data.Settings.SimulationTimeScale = 20f;
            data.Settings.SimulationTimeScale = Mathf.Clamp(data.Settings.SimulationTimeScale, 20f, 40f);
            if (data.Management == null) data.Management = ManagementState.New(ManagementConfig.Load());
            data.Management.Balance = Mathf.Max(0, data.Management.Balance);
            if (data.Management.Inventory == null) data.Management.Inventory = new System.Collections.Generic.List<InventoryUnit>();
            if (data.Management.Purchases == null) data.Management.Purchases = new System.Collections.Generic.List<PurchaseRecord>();
            data.Management.NextUnitId = Mathf.Max(1, data.Management.NextUnitId);
            foreach (var unit in data.Management.Inventory) data.Management.NextUnitId = Mathf.Max(data.Management.NextUnitId, unit.Id + 1);
            // Existing management players already know the purchase/preparation loop.
            // Legacy cooking-only TutorialCompleted does not skip the new L1 buying guide.
            if (data.Version < 5 && data.Management.Cycle > 0)
                data.Management.ManagementTutorialCompleted = true;
            if (data.Management.ActiveRun != null && data.Management.ActiveRun.Units != null)
                foreach (var unit in data.Management.ActiveRun.Units)
                    data.Management.NextUnitId = Mathf.Max(data.Management.NextUnitId, unit.Id + 1);
            data.Version = CurrentVersion;
            return data;
        }
    }

    /// <summary>Local versioned MVP progress/settings storage, limited to the fields required by the product spec.</summary>
    public static class MvpSave
    {
        private const string Key = "asadito.mvp.save";
        private static MvpSaveData cached;

        public static MvpSaveData Load()
        {
            if (cached != null) return cached;
            if (!PlayerPrefs.HasKey(Key)) return cached = MvpSaveData.Migrate(new MvpSaveData());
            try
            {
                MvpSaveData loaded = JsonUtility.FromJson<MvpSaveData>(PlayerPrefs.GetString(Key));
                return cached = MvpSaveData.Migrate(loaded);
            }
            catch (ArgumentException)
            {
                return cached = MvpSaveData.Migrate(new MvpSaveData());
            }
        }

        public static void Save(MvpSaveData data)
        {
            if (data == null) return;
            data = MvpSaveData.Migrate(data);
            data.Version = MvpSaveData.CurrentVersion;
            cached = data;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static void RecordLevelResult(int levelNumber, int score, int stars)
        {
            MvpSaveData data = Load();
            data.RecordLevelResult(levelNumber, score, stars);
            Save(data);
        }
    }

    [Serializable]
    public sealed class StarThresholds
    {
        [Min(0)] public int OneStar = 40;
        [Min(0)] public int TwoStars = 120;
        [Min(0)] public int ThreeStars = 180;

        public int Evaluate(int score)
        {
            if (score >= ThreeStars) return 3;
            if (score >= TwoStars) return 2;
            return score >= OneStar ? 1 : 0;
        }
    }
}
