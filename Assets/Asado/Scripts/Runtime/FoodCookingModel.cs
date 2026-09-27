using System;
using UnityEngine;

namespace Asadito.Runtime
{
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

    [Serializable]
    public sealed class FoodCookProfile
    {
        public string FoodId;
        [Min(0f)] public float CoreTransferRate = .08f;
        [Min(0f)] public float SurfaceTransferRate = .9f;
        [Min(0f)] public float CoolingRate = .025f;
        [Min(0f)] public float MoistureLossRate = .008f;
        [Min(0f)] public float MaillardRate = .025f;
        [Min(0f)] public float CharRate = .006f;
        [Min(0f)] public float FatRenderRate = .012f;
        [Min(0f)] public float MaillardStartsAtC = 120f;
        [Min(0f)] public float CharStartsAtC = 190f;
        public DonenessBand[] DonenessBands =
        {
            new DonenessBand(Doneness.Jugoso, 50f, 54f),
            new DonenessBand(Doneness.A_Punto, 55f, 59f),
            new DonenessBand(Doneness.A_PuntoMas, 60f, 64f),
            new DonenessBand(Doneness.Cocido, 65f, 69f),
            new DonenessBand(Doneness.Bien_Cocido, 70f, 76f)
        };
    }

    /// <summary>Lightweight, tunable gameplay thermal model; not a food safety or scientific cooking model.</summary>
    public static class FoodCookingModel
    {
        public static void Step(FoodState food, FoodCookProfile profile, float localHeat, float deltaMinutes, bool onGrill)
        {
            if (food == null || profile == null || deltaMinutes <= 0f) return;

            float heat = onGrill ? Mathf.Max(0f, localHeat) : 20f;
            FoodFaceState face = food.CurrentFace;
            float ambientTarget = onGrill ? heat : 20f;
            float surfaceRate = onGrill ? profile.SurfaceTransferRate : profile.CoolingRate;
            float surfaceAlpha = 1f - Mathf.Exp(-Mathf.Max(0f, surfaceRate) * deltaMinutes);
            face.SurfaceTemperatureC = Mathf.Lerp(face.SurfaceTemperatureC, ambientTarget, surfaceAlpha);
            float coreAlpha = 1f - Mathf.Exp(-Mathf.Max(0f, profile.CoreTransferRate) * deltaMinutes);
            food.CoreTemperatureC = Mathf.Lerp(food.CoreTemperatureC, face.SurfaceTemperatureC, coreAlpha);

            if (onGrill)
            {
                float heatFactor = Mathf.Clamp01(heat / 220f);
                food.Moisture = Mathf.Clamp01(food.Moisture - profile.MoistureLossRate * heatFactor * deltaMinutes);
                if (face.SurfaceTemperatureC >= profile.MaillardStartsAtC)
                    face.Maillard = Mathf.Clamp01(face.Maillard + profile.MaillardRate * heatFactor * deltaMinutes);
                if (face.SurfaceTemperatureC >= profile.CharStartsAtC)
                    face.Char = Mathf.Clamp01(face.Char + profile.CharRate * heatFactor * deltaMinutes);
                food.FatRendered = Mathf.Clamp01(food.FatRendered + profile.FatRenderRate * heatFactor * deltaMinutes);
            }

            food.SetCurrentFace(face);
        }

        public static Doneness GetDoneness(FoodState food, FoodCookProfile profile)
        {
            if (food == null || profile?.DonenessBands == null || profile.DonenessBands.Length == 0)
                return Doneness.A_Punto;
            DonenessBand nearest = profile.DonenessBands[0];
            float nearestDistance = float.PositiveInfinity;
            foreach (DonenessBand band in profile.DonenessBands)
            {
                float distance = food.CoreTemperatureC < band.MinimumCoreC
                    ? band.MinimumCoreC - food.CoreTemperatureC
                    : food.CoreTemperatureC > band.MaximumCoreC ? food.CoreTemperatureC - band.MaximumCoreC : 0f;
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
                float edgeDistance = food.CoreTemperatureC < band.MinimumCoreC
                    ? band.MinimumCoreC - food.CoreTemperatureC
                    : food.CoreTemperatureC - band.MaximumCoreC;
                return Mathf.Clamp(100f - edgeDistance * 8f, 0f, 100f);
            }
            return 0f;
        }
    }
}
