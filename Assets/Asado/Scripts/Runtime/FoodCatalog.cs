using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asadito.Runtime
{
    [Serializable]
    public sealed class FoodCatalogData
    {
        public int SchemaVersion = 2;
        public FoodVisualReference VisualReference = new FoodVisualReference();
        public FoodDefinition[] Foods = Array.Empty<FoodDefinition>();
    }

    /// <summary>Historical chorizo sizing is frozen as the authored visual/physical scale unit.</summary>
    [Serializable]
    public sealed class FoodVisualReference
    {
        public string FoodId = "chorizo";
        public float LegacyRectWidth = 236f;
        public float LegacyRectHeight = 176f;
        public float LegacyDisplayScale = .92f;

        public FoodVisualReference Clone() => new FoodVisualReference
        {
            FoodId = FoodId,
            LegacyRectWidth = LegacyRectWidth,
            LegacyRectHeight = LegacyRectHeight,
            LegacyDisplayScale = LegacyDisplayScale
        };
    }

    /// <summary>Authored, serializable identity and presentation settings for a playable food.</summary>
    [Serializable]
    public sealed class FoodDefinition
    {
        public string Id;
        public string DisplayName;
        public string Category;
        [Min(.1f)] public float FootprintAreaMultiplier = 1f;
        public FoodSpriteCrop SpriteCrop = new FoodSpriteCrop();
        public FoodCookProfile Profile = new FoodCookProfile();

        public FoodDefinition Clone()
        {
            return new FoodDefinition
            {
                Id = Id,
                DisplayName = DisplayName,
                Category = Category,
                FootprintAreaMultiplier = FootprintAreaMultiplier,
                SpriteCrop = SpriteCrop == null ? new FoodSpriteCrop() : SpriteCrop.Clone(),
                Profile = Profile == null ? new FoodCookProfile() : Profile.Clone()
            };
        }
    }

    /// <summary>Normalized crop within each equal horizontal cell, with Y measured from cell top.</summary>
    [Serializable]
    public sealed class FoodSpriteCrop
    {
        [Range(0f, 1f)] public float XMin;
        [Range(0f, 1f)] public float YMin;
        [Range(0f, 1f)] public float XMax = 1f;
        [Range(0f, 1f)] public float YMax = 1f;

        public FoodSpriteCrop Clone() => new FoodSpriteCrop { XMin = XMin, YMin = YMin, XMax = XMax, YMax = YMax };
        public bool IsValid => XMin >= 0f && YMin >= 0f && XMax <= 1f && YMax <= 1f && XMax > XMin && YMax > YMin;
    }

    /// <summary>
    /// Loads the authored food data once and returns defensive copies so callers cannot mutate the catalog.
    /// Sprite atlases are loaded separately and lazily by the presentation layer.
    /// </summary>
    public static class FoodCatalog
    {
        private const string ResourcePath = "Definitions/FoodCatalog";
        private static FoodCatalogData cachedData;
        private static Dictionary<string, FoodDefinition> byId;

        public static int Count => EnsureLoaded().Foods.Length;

        public static FoodVisualReference GetVisualReference() => EnsureLoaded().VisualReference.Clone();

        public static FoodDefinition[] GetAll()
        {
            FoodDefinition[] source = EnsureLoaded().Foods;
            var result = new FoodDefinition[source.Length];
            for (int i = 0; i < source.Length; i++) result[i] = source[i].Clone();
            return result;
        }

        public static FoodDefinition Get(string foodId)
        {
            if (string.IsNullOrWhiteSpace(foodId) || !EnsureIndex().TryGetValue(foodId.Trim(), out FoodDefinition value))
                throw new ArgumentException("Unknown food id '" + foodId + "'. Check Resources/Definitions/FoodCatalog.json.", nameof(foodId));
            return value.Clone();
        }

        public static bool TryGet(string foodId, out FoodDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(foodId) || !EnsureIndex().TryGetValue(foodId.Trim(), out FoodDefinition value)) return false;
            definition = value.Clone();
            return true;
        }

        public static string[] Validate(out string[] errors)
        {
            var issues = new List<string>();
            FoodDefinition[] foods = EnsureLoaded().Foods;
            if (foods == null || foods.Length == 0) issues.Add("Food catalog must contain at least one definition.");
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (FoodDefinition food in foods ?? Array.Empty<FoodDefinition>())
            {
                if (food == null) { issues.Add("Food catalog contains a null definition."); continue; }
                if (string.IsNullOrWhiteSpace(food.Id)) issues.Add("A food definition has an empty id.");
                else if (!ids.Add(food.Id)) issues.Add("Duplicate food id: " + food.Id);
                if (string.IsNullOrWhiteSpace(food.DisplayName)) issues.Add((food.Id ?? "<empty>") + " has no display name.");
                if (string.IsNullOrWhiteSpace(food.Category)) issues.Add((food.Id ?? "<empty>") + " has no category.");
                if (float.IsNaN(food.FootprintAreaMultiplier) || float.IsInfinity(food.FootprintAreaMultiplier) || food.FootprintAreaMultiplier <= 0f)
                    issues.Add((food.Id ?? "<empty>") + " has an invalid chorizo-relative footprint area.");
                if (food.SpriteCrop == null || !food.SpriteCrop.IsValid) issues.Add((food.Id ?? "<empty>") + " has invalid atlas crop bounds.");
                if (food.Profile == null) issues.Add((food.Id ?? "<empty>") + " has no cook profile.");
                else
                {
                    if (food.Profile.DonenessBands == null || food.Profile.DonenessBands.Length < 5)
                        issues.Add((food.Id ?? "<empty>") + " requires five doneness bands.");
                    if (!FoodCookingModel.IsProfileValid(food.Profile, out string profileError))
                        issues.Add((food.Id ?? "<empty>") + " profile: " + profileError);
                }
            }
            if (EnsureLoaded().VisualReference == null ||
                !string.Equals(EnsureLoaded().VisualReference.FoodId, "chorizo", StringComparison.OrdinalIgnoreCase) ||
                EnsureLoaded().VisualReference.LegacyRectWidth <= 0f || EnsureLoaded().VisualReference.LegacyRectHeight <= 0f ||
                EnsureLoaded().VisualReference.LegacyDisplayScale <= 0f)
                issues.Add("Food visual reference must preserve the historical chorizo sizing.");
            if (!EnsureIndex().TryGetValue("chorizo", out FoodDefinition chorizo) || Mathf.Abs(chorizo.FootprintAreaMultiplier - 1f) > .001f)
                issues.Add("Chorizo must remain the 1.0 footprint-area reference.");
            errors = issues.ToArray();
            return errors;
        }

        private static FoodCatalogData EnsureLoaded()
        {
            if (cachedData != null) return cachedData;
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null) throw new InvalidOperationException("Missing Resources/" + ResourcePath + ".json.");
            cachedData = JsonUtility.FromJson<FoodCatalogData>(asset.text);
            if (cachedData == null || cachedData.SchemaVersion != 2 || cachedData.VisualReference == null || cachedData.Foods == null)
                throw new InvalidOperationException("Invalid food catalog schema at Resources/" + ResourcePath + ".json.");
            return cachedData;
        }

        private static Dictionary<string, FoodDefinition> EnsureIndex()
        {
            if (byId != null) return byId;
            byId = new Dictionary<string, FoodDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (FoodDefinition food in EnsureLoaded().Foods)
                if (food != null && !string.IsNullOrWhiteSpace(food.Id) && !byId.ContainsKey(food.Id)) byId.Add(food.Id, food);
            return byId;
        }
    }
}
