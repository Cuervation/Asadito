using UnityEngine;

namespace Asadito.Runtime
{
    /// <summary>Read-only gameplay gauge, not a timer, scoring rule or food-safety indicator.</summary>
    public static class FoodCookingProgress
    {
        public const float GreenStart = .45f;
        public const float GreenEnd = .60f;
        public static readonly Color Red = new Color32(241, 73, 56, 255);
        public static readonly Color Yellow = new Color32(255, 204, 48, 255);
        public static readonly Color Green = new Color32(72, 214, 109, 255);

        public static float Value(FoodState food, FoodCookProfile profile)
        {
            if (food == null || profile == null) return 0f;
            float min = 55f, max = 59f, burntCore = 88f;
            if (profile.DonenessBands != null)
                foreach (var band in profile.DonenessBands)
                {
                    if (band.Doneness == Doneness.A_Punto) { min = band.MinimumCoreC; max = band.MaximumCoreC; }
                    burntCore = Mathf.Max(burntCore, band.MaximumCoreC + 12f);
                }
            // Cheese readiness includes browning, not just its ordinary doneness bands.
            if (profile.UsesCheeseStages) { min = 54f; max = 66f; burntCore = 85f; }
            float core = food.CoreTemperatureC;
            float value = core < min ? GreenStart * Mathf.InverseLerp(20f, min, core) :
                core <= max ? Mathf.Lerp(GreenStart, GreenEnd, Mathf.InverseLerp(min, max, core)) :
                Mathf.Lerp(GreenEnd, 1f, Mathf.InverseLerp(max, burntCore, core));
            if (profile.UsesCheeseStages)
            {
                var stage = FoodCookingModel.GetProvoletaStage(food);
                if (stage == ProvoletaCookingStage.Burnt) return 1f;
                if (stage == ProvoletaCookingStage.Failed) value = Mathf.Max(.78f, value);
                else if (stage != ProvoletaCookingStage.Ideal && core <= max) value = Mathf.Min(GreenStart - .02f, value);
            }
            // A charred/dried piece cannot appear green just because its core is at the target.
            float charAmount = Mathf.Max(food.Char, food.CurrentFace.Char);
            if (charAmount >= .72f) return 1f;
            if (charAmount > .18f)
                value = Mathf.Max(value, Mathf.Lerp(GreenEnd, 1f, Mathf.InverseLerp(.18f, .72f, charAmount)));
            if (food.Moisture < .42f)
                value = Mathf.Max(value, Mathf.Lerp(GreenEnd, 1f, Mathf.InverseLerp(.42f, .05f, food.Moisture)));
            return Mathf.Clamp01(value);
        }

        public static Color ColorAt(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (progress < .25f) return Color.Lerp(Red, Yellow, progress / .25f);
            if (progress < GreenStart) return Color.Lerp(Yellow, Green, Mathf.InverseLerp(.25f, GreenStart, progress));
            if (progress <= GreenEnd) return Green;
            if (progress < .78f) return Color.Lerp(Green, Yellow, Mathf.InverseLerp(GreenEnd, .78f, progress));
            return Color.Lerp(Yellow, Red, Mathf.InverseLerp(.78f, 1f, progress));
        }
    }
}
