using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asadito.Runtime
{
    [Serializable]
    public sealed class ServingPortion
    {
        public string Id;
        public string FoodId;
        [Min(0f)] public float Amount;
        public Doneness Doneness;
        [Range(0f, 100f)] public float CookingQuality = 100f;
    }

    [Serializable]
    public struct ServingAssignment
    {
        public string GuestId;
        public string PortionId;
        public ServingAssignment(string guestId, string portionId) { GuestId = guestId; PortionId = portionId; }
    }

    /// <summary>Deterministic greedy allocator prioritizing target coverage, then food preference, then doneness.</summary>
    public static class ServingAllocator
    {
        public static List<ServingAssignment> Allocate(IList<GuestProfile> guests, IList<ServingPortion> portions)
        {
            var result = new List<ServingAssignment>();
            if (guests == null || portions == null || guests.Count == 0) return result;
            var assigned = new float[guests.Count];
            var portionsByGuest = new int[guests.Count];
            var order = new List<int>(portions.Count);
            for (int i = 0; i < portions.Count; i++) order.Add(i);
            order.Sort((a, b) => string.CompareOrdinal(portions[a]?.Id, portions[b]?.Id));

            foreach (int portionIndex in order)
            {
                ServingPortion portion = portions[portionIndex];
                if (portion == null) continue;
                int bestGuest = -1;
                int bestServedCount = int.MaxValue;
                float bestFit = float.NegativeInfinity;
                var scoring = new ScoreConfig();
                for (int guestIndex = 0; guestIndex < guests.Count; guestIndex++)
                {
                    GuestProfile guest = guests[guestIndex];
                    if (guest == null) continue;
                    // Explicit diner requests are hard identities; soft likes cannot substitute another cut.
                    if (!string.IsNullOrEmpty(guest.RequestedFoodId) &&
                        !string.Equals(guest.RequestedFoodId,portion.FoodId,StringComparison.Ordinal)) continue;
                    float target = Mathf.Max(0f, guest.TargetFoodAmount);
                    float coverage = target <= 0f ? 0f : Mathf.Min(portion.Amount, Mathf.Max(0f, target - assigned[guestIndex]));
                    float satiety = target <= 0f ? 100f : Mathf.Clamp01(coverage / target) * 100f;
                    float fit = satiety * scoring.SatietyWeight
                        + GetFoodPreferenceScore(guest, portion.FoodId) * scoring.FoodPreferenceWeight
                        + (guest.PreferredDoneness == portion.Doneness ? 100f : 0f) * scoring.DonenessMatchWeight;
                    int servedCount = portionsByGuest[guestIndex];
                    if (servedCount < bestServedCount || (servedCount == bestServedCount && fit > bestFit))
                    {
                        bestGuest = guestIndex; bestServedCount = servedCount; bestFit = fit;
                    }
                }
                if (bestGuest < 0) continue;
                assigned[bestGuest] += Mathf.Max(0f, portion.Amount);
                portionsByGuest[bestGuest]++;
                result.Add(new ServingAssignment(guests[bestGuest].Id, portion.Id));
            }
            return result;
        }

        public static int GetFoodPreference(GuestProfile guest, string foodId)
        {
            if (guest == null || string.IsNullOrEmpty(foodId)) return 1;
            if (Contains(guest.DislikedFoods, foodId)) return 0;
            if (Contains(guest.FavoriteFoods, foodId)) return 3;
            if (Contains(guest.LikedFoods, foodId)) return 2;
            return 1;
        }

        /// <summary>Maps preference tiers to a player-readable 0–100 score; favorites can earn the full 100.</summary>
        public static float GetFoodPreferenceScore(GuestProfile guest, string foodId)
        {
            switch (GetFoodPreference(guest, foodId))
            {
                case 0: return 0f;
                case 1: return 40f;
                case 2: return 70f;
                default: return 100f;
            }
        }

        private static bool Contains(List<string> values, string value)
        {
            if (values == null) return false;
            for (int i = 0; i < values.Count; i++)
                if (string.Equals(values[i], value, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
