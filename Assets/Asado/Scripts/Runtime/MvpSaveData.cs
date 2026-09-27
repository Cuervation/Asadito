using System;
using UnityEngine;

namespace Asadito.Runtime
{
    [Serializable]
    public sealed class MvpSettings
    {
        [Range(20f, 40f)] public float SimulationTimeScale = 30f;
        [Range(0f, 1f)] public float SfxVolume = .8f;
        public bool HapticsEnabled = true;
        public bool TutorialCompleted;
    }

    [Serializable]
    public sealed class MvpSaveData
    {
        public const int CurrentVersion = 1;
        public int Version = CurrentVersion;
        public int MaxUnlockedLevel = 1;
        public int[] StarsByLevel = new int[5];
        public int[] BestScoreByLevel = new int[5];
        public MvpSettings Settings = new MvpSettings();

        public void RecordLevelResult(int levelNumber, int score, int stars)
        {
            int index = Mathf.Clamp(levelNumber - 1, 0, 4);
            if (StarsByLevel == null || StarsByLevel.Length != 5) Array.Resize(ref StarsByLevel, 5);
            if (BestScoreByLevel == null || BestScoreByLevel.Length != 5) Array.Resize(ref BestScoreByLevel, 5);
            StarsByLevel[index] = Mathf.Max(StarsByLevel[index], Mathf.Clamp(stars, 0, 3));
            BestScoreByLevel[index] = Mathf.Max(BestScoreByLevel[index], score);
            if (stars >= 1 && levelNumber >= MaxUnlockedLevel && levelNumber < 5)
                MaxUnlockedLevel = Mathf.Max(MaxUnlockedLevel, levelNumber + 1);
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
            if (!PlayerPrefs.HasKey(Key)) return cached = new MvpSaveData();
            try
            {
                MvpSaveData loaded = JsonUtility.FromJson<MvpSaveData>(PlayerPrefs.GetString(Key));
                if (loaded == null || loaded.Version != MvpSaveData.CurrentVersion)
                    return cached = new MvpSaveData();
                loaded.MaxUnlockedLevel = Mathf.Clamp(loaded.MaxUnlockedLevel, 1, 5);
                if (loaded.StarsByLevel == null || loaded.StarsByLevel.Length != 5) loaded.StarsByLevel = new int[5];
                if (loaded.BestScoreByLevel == null || loaded.BestScoreByLevel.Length != 5) loaded.BestScoreByLevel = new int[5];
                if (loaded.Settings == null) loaded.Settings = new MvpSettings();
                loaded.Settings.SimulationTimeScale = Mathf.Clamp(loaded.Settings.SimulationTimeScale, 20f, 40f);
                return cached = loaded;
            }
            catch (ArgumentException)
            {
                return cached = new MvpSaveData();
            }
        }

        public static void Save(MvpSaveData data)
        {
            if (data == null) return;
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
