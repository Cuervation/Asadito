using System;
using UnityEngine;

namespace Asadito.Runtime
{
    public enum ProvoletaCookingStage { NotApplicable, Cold, Softening, Browning, Ideal, Failed, Burnt }
    public enum FoodCookVisualStage { Raw, Warming, Browning, Ideal, Overcooked, Burnt }

    [Serializable]
    public struct DonenessBand
    {
        public Doneness Doneness;
        [Min(0f)] public float MinimumCoreC;
        [Min(0f)] public float MaximumCoreC;

        public DonenessBand(Doneness doneness, float minimumCoreC, float maximumCoreC)
        {
            Doneness = doneness;
            MinimumCoreC = minimumCoreC;
            MaximumCoreC = maximumCoreC;
        }
    }

    /// <summary>Per-food tuning data. Stored inside FoodCatalog.json; this is not a food-safety model.</summary>
    [Serializable]
    public sealed class FoodCookProfile
    {
        public string FoodId;
        [Min(0f)] public float CoreTransferRate = .025f;
        [Min(0f)] public float SurfaceTransferRate = .9f;
        [Min(0f)] public float CoolingRate = .025f;
        [Min(0f)] public float MoistureLossRate = .008f;
        [Min(0f)] public float MaillardRate = .025f;
        [Min(0f)] public float CharRate = .006f;
        [Min(0f)] public float FatRenderRate = .012f;
        [Min(0f)] public float MaillardStartsAtC = 120f;
        [Min(0f)] public float CharStartsAtC = 190f;
        [Min(.25f)] public float ThermalMass = 1f;
        [Min(.1f)] public float ThicknessCm = 3f;
        [Min(100f)] public float PreferredHeatC = 190f;
        [Min(100f)] public float StrongHeatThresholdC = 210f;
        [Range(0f, 1f)] public float SplitRiskRate;
        [Range(0f, 1f)] public float FaceBalanceWeight = .7f;
        public bool UsesCheeseStages;
        public DonenessBand[] DonenessBands =
        {
            new DonenessBand(Doneness.Jugoso, 50f, 54f),
            new DonenessBand(Doneness.A_Punto, 55f, 59f),
            new DonenessBand(Doneness.A_PuntoMas, 60f, 64f),
            new DonenessBand(Doneness.Cocido, 65f, 69f),
            new DonenessBand(Doneness.Bien_Cocido, 70f, 76f)
        };

        public FoodCookProfile Clone()
        {
            return new FoodCookProfile
            {
                FoodId = FoodId,
                CoreTransferRate = CoreTransferRate,
                SurfaceTransferRate = SurfaceTransferRate,
                CoolingRate = CoolingRate,
                MoistureLossRate = MoistureLossRate,
                MaillardRate = MaillardRate,
                CharRate = CharRate,
                FatRenderRate = FatRenderRate,
                MaillardStartsAtC = MaillardStartsAtC,
                CharStartsAtC = CharStartsAtC,
                ThermalMass = ThermalMass,
                ThicknessCm = ThicknessCm,
                PreferredHeatC = PreferredHeatC,
                StrongHeatThresholdC = StrongHeatThresholdC,
                SplitRiskRate = SplitRiskRate,
                FaceBalanceWeight = FaceBalanceWeight,
                UsesCheeseStages = UsesCheeseStages,
                DonenessBands = DonenessBands == null ? Array.Empty<DonenessBand>() : (DonenessBand[])DonenessBands.Clone()
            };
        }
    }

    /// <summary>Lightweight, tunable gameplay thermal model; never use it to guide food safety.</summary>
    public static class FoodCookingModel
    {
        public static FoodCookProfile CreateProfile(string foodId)
        {
            FoodDefinition definition = FoodCatalog.Get(foodId);
            FoodCookProfile profile = definition.Profile;
            profile.FoodId = definition.Id;
            return profile;
        }

        public static bool IsProfileValid(FoodCookProfile profile, out string error)
        {
            error = null;
            if (profile == null) { error = "missing profile"; return false; }
            if (string.IsNullOrWhiteSpace(profile.FoodId)) { error = "missing food id"; return false; }
            if (!Positive(profile.CoreTransferRate) || !Positive(profile.SurfaceTransferRate) || !Positive(profile.CoolingRate) ||
                !Positive(profile.ThermalMass) || !Positive(profile.ThicknessCm) || !Positive(profile.PreferredHeatC) ||
                !Positive(profile.StrongHeatThresholdC) || profile.DonenessBands == null || profile.DonenessBands.Length < 5)
            { error = "rates, mass, thickness, heat targets, and five doneness bands are required"; return false; }
            for (int i = 0; i < profile.DonenessBands.Length; i++)
            {
                DonenessBand band = profile.DonenessBands[i];
                if (!Finite(band.MinimumCoreC) || !Finite(band.MaximumCoreC) || band.MaximumCoreC < band.MinimumCoreC)
                { error = "invalid temperature band at index " + i; return false; }
            }
            return true;
        }

        public static void Step(FoodState food, FoodCookProfile profile, float localHeat, float deltaMinutes, bool onGrill)
        {
            if (food == null || profile == null || deltaMinutes <= 0f) return;

            float heat = onGrill ? Mathf.Max(0f, localHeat) : 20f;
            FoodFaceState face = food.CurrentFace;
            float ambientTarget = onGrill ? heat : 20f;
            float surfaceRate = onGrill ? profile.SurfaceTransferRate : profile.CoolingRate;
            float surfaceAlpha = 1f - Mathf.Exp(-Mathf.Max(0f, surfaceRate) * deltaMinutes);
            face.SurfaceTemperatureC = Mathf.Lerp(face.SurfaceTemperatureC, ambientTarget, surfaceAlpha);
            float coreAlpha = 1f - Mathf.Exp(-Mathf.Max(0f, profile.CoreTransferRate) * deltaMinutes / Mathf.Max(.25f, profile.ThermalMass));
            food.CoreTemperatureC = Mathf.Lerp(food.CoreTemperatureC, face.SurfaceTemperatureC, coreAlpha);

            if (onGrill)
            {
                float heatFactor = Mathf.Clamp01(heat / Mathf.Max(120f, profile.PreferredHeatC));
                float strongHeat = Mathf.Max(0f, heat - profile.StrongHeatThresholdC);
                float overheatMultiplier = 1f + strongHeat / 120f;
                food.Moisture = Mathf.Clamp01(food.Moisture - profile.MoistureLossRate * heatFactor * overheatMultiplier * deltaMinutes);
                if (face.SurfaceTemperatureC >= profile.MaillardStartsAtC)
                    face.Maillard = Mathf.Clamp01(face.Maillard + profile.MaillardRate * heatFactor * deltaMinutes);
                if (face.SurfaceTemperatureC >= profile.CharStartsAtC)
                {
                    float charHeat = Mathf.Clamp01((face.SurfaceTemperatureC - profile.CharStartsAtC + 20f) /
                                                   Mathf.Max(35f, profile.PreferredHeatC - profile.CharStartsAtC + 30f));
                    face.Char = Mathf.Clamp01(face.Char + profile.CharRate * charHeat * deltaMinutes);
                }
                food.FatRendered = Mathf.Clamp01(food.FatRendered + profile.FatRenderRate * heatFactor * deltaMinutes);
                if (strongHeat > 0f && profile.SplitRiskRate > 0f)
                    food.SplitRisk = Mathf.Clamp01(food.SplitRisk + profile.SplitRiskRate * strongHeat / 100f * deltaMinutes);
            }

            food.SetCurrentFace(face);
        }

        public static FoodCookVisualStage GetVisualStage(FoodState food, FoodCookProfile profile)
        {
            if (food == null || profile == null) return FoodCookVisualStage.Raw;
            if (profile.UsesCheeseStages)
            {
                switch (GetProvoletaStage(food))
                {
                    case ProvoletaCookingStage.Cold: return FoodCookVisualStage.Raw;
                    case ProvoletaCookingStage.Softening: return FoodCookVisualStage.Warming;
                    case ProvoletaCookingStage.Browning: return FoodCookVisualStage.Browning;
                    case ProvoletaCookingStage.Ideal: return FoodCookVisualStage.Ideal;
                    case ProvoletaCookingStage.Burnt: return FoodCookVisualStage.Burnt;
                    case ProvoletaCookingStage.Failed: return FoodCookVisualStage.Overcooked;
                }
            }

            FoodFaceState face = food.CurrentFace;
            if (face.Char >= .72f) return FoodCookVisualStage.Burnt;
            if (face.Char >= .32f || food.Moisture <= .28f) return FoodCookVisualStage.Overcooked;
            if (face.Maillard >= .18f && food.CoreTemperatureC >= profile.DonenessBands[0].MinimumCoreC)
                return FoodCookVisualStage.Ideal;
            if (face.Maillard >= .04f) return FoodCookVisualStage.Browning;
            if (face.SurfaceTemperatureC >= 38f) return FoodCookVisualStage.Warming;
            return FoodCookVisualStage.Raw;
        }

        public static ProvoletaCookingStage GetProvoletaStage(FoodState food)
        {
            if (food == null) return ProvoletaCookingStage.NotApplicable;
            if (food.Char >= .72f) return ProvoletaCookingStage.Burnt;
            if (food.CoreTemperatureC > 69f || food.Moisture < .2f) return ProvoletaCookingStage.Failed;
            if (food.CoreTemperatureC < 30f) return ProvoletaCookingStage.Cold;
            if (food.CoreTemperatureC < 46f) return ProvoletaCookingStage.Softening;
            if (food.CoreTemperatureC >= 54f && food.CoreTemperatureC <= 66f && food.Maillard >= .12f)
                return ProvoletaCookingStage.Ideal;
            return food.Maillard > .015f ? ProvoletaCookingStage.Browning : ProvoletaCookingStage.Softening;
        }

        public static Doneness GetDoneness(FoodState food, FoodCookProfile profile)
        {
            if (food == null || profile?.DonenessBands == null || profile.DonenessBands.Length == 0) return Doneness.A_Punto;
            DonenessBand nearest = profile.DonenessBands[0];
            float nearestDistance = float.PositiveInfinity;
            foreach (DonenessBand band in profile.DonenessBands)
            {
                float distance = food.CoreTemperatureC < band.MinimumCoreC ? band.MinimumCoreC - food.CoreTemperatureC :
                    food.CoreTemperatureC > band.MaximumCoreC ? food.CoreTemperatureC - band.MaximumCoreC : 0f;
                if (distance < nearestDistance) { nearest = band; nearestDistance = distance; }
            }
            return nearest.Doneness;
        }

        public static float EvaluateDonenessMatch(FoodState food, FoodCookProfile profile, Doneness desired)
        {
            if (food == null || profile?.DonenessBands == null) return 0f;
            foreach (DonenessBand band in profile.DonenessBands)
            {
                if (band.Doneness != desired) continue;
                if (food.CoreTemperatureC >= band.MinimumCoreC && food.CoreTemperatureC <= band.MaximumCoreC) return 100f;
                float edgeDistance = food.CoreTemperatureC < band.MinimumCoreC ? band.MinimumCoreC - food.CoreTemperatureC : food.CoreTemperatureC - band.MaximumCoreC;
                return Mathf.Clamp(100f - edgeDistance * 8f, 0f, 100f);
            }
            return 0f;
        }

        private static bool Positive(float value) => Finite(value) && value > 0f;
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
