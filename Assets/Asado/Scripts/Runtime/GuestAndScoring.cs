using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asadito.Runtime
{
    public enum Doneness { Jugoso, A_Punto, A_PuntoMas, Cocido, Bien_Cocido }

    [Serializable]
    public sealed class GuestProfile
    {
        public string Id;
        public string Name;
        [Min(0)] public int Age = 30;
        [Min(0f)] public float Weight = 70f;
        [Min(0f)] public float Appetite = 1f;
        public Doneness PreferredDoneness = Doneness.A_Punto;
        public List<string> FavoriteFoods = new List<string>();
        public List<string> LikedFoods = new List<string>();
        public List<string> DislikedFoods = new List<string>();
        [Min(0f)] public float TargetFoodAmount;

        public void CalculateTarget(GuestAmountTuning tuning)
        {
            TargetFoodAmount = tuning.GetAgeBase(Age) * tuning.GetBodySizeModifier(Weight) * Appetite;
        }
    }

    [Serializable]
    public sealed class GuestAmountTuning
    {
        [Min(0f)] public float ChildAgeBase = .12f;
        [Min(0f)] public float AdultAgeBase = .2f;
        [Min(0f)] public float SeniorAgeBase = .16f;
        [Min(0)] public int AdultStartsAt = 18;
        [Min(0)] public int SeniorStartsAt = 65;
        [Min(0f)] public float LightWeightBelowKg = 55f;
        [Min(0f)] public float HeavyWeightFromKg = 90f;
        [Min(0f)] public float LightBodyModifier = .85f;
        [Min(0f)] public float AverageBodyModifier = 1f;
        [Min(0f)] public float HeavyBodyModifier = 1.15f;

        public float GetAgeBase(int age) => age < AdultStartsAt ? ChildAgeBase : age < SeniorStartsAt ? AdultAgeBase : SeniorAgeBase;
        public float GetBodySizeModifier(float weight) => weight < LightWeightBelowKg ? LightBodyModifier : weight >= HeavyWeightFromKg ? HeavyBodyModifier : AverageBodyModifier;
    }

    [Serializable]
    public sealed class ScoreConfig
    {
        [Min(0f)] public float CookingQualityWeight = .4f;
        [Min(0f)] public float SatietyWeight = .3f;
        [Min(0f)] public float DonenessMatchWeight = .2f;
        [Min(0f)] public float FoodPreferenceWeight = .1f;
    }

    [Serializable]
    public struct ScoreBreakdown
    {
        [Range(0f, 100f)] public float CookingQuality;
        [Range(0f, 100f)] public float Satiety;
        [Range(0f, 100f)] public float DonenessMatch;
        [Range(0f, 100f)] public float FoodPreference;
        public float Total(ScoreConfig config)
        {
            float sum = config.CookingQualityWeight + config.SatietyWeight + config.DonenessMatchWeight + config.FoodPreferenceWeight;
            if (sum <= 0f) return 0f;
            return Mathf.Clamp((CookingQuality * config.CookingQualityWeight + Satiety * config.SatietyWeight + DonenessMatch * config.DonenessMatchWeight + FoodPreference * config.FoodPreferenceWeight) / sum, 0f, 100f);
        }
    }
}
