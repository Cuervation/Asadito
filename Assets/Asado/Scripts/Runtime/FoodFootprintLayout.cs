using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asadito.Runtime
{
    /// <summary>Aspect-preserving size and non-overlapping grill placement shared by every food.</summary>
    public static class FoodFootprintLayout
    {
        private const float Epsilon = .001f;

        public static Vector2 FitAspectToBounds(float spriteAspect, Vector2 bounds)
        {
            if (!IsFinitePositive(spriteAspect) || !IsFinitePositive(bounds.x) || !IsFinitePositive(bounds.y))
                return Vector2.zero;
            float width = Mathf.Min(bounds.x, bounds.y * spriteAspect);
            return new Vector2(width, width / spriteAspect);
        }

        /// <summary>
        /// Applies a chorizo-relative occupied area while deriving width/height from each atlas crop.
        /// The returned rectangle has the exact source-sprite aspect, so Image.preserveAspect never
        /// letterboxes or distorts it. Area multiplier 1 preserves the legacy chorizo bounds exactly.
        /// </summary>
        public static Vector2 CalculateVisualSize(FoodVisualReference reference, float referenceAspect,
            float spriteAspect, float footprintAreaMultiplier)
        {
            if (reference == null || !IsFinitePositive(reference.LegacyRectWidth) ||
                !IsFinitePositive(reference.LegacyRectHeight) || !IsFinitePositive(reference.LegacyDisplayScale) ||
                !IsFinitePositive(referenceAspect) || !IsFinitePositive(spriteAspect) ||
                !IsFinitePositive(footprintAreaMultiplier)) return Vector2.zero;

            Vector2 legacyBounds = new Vector2(reference.LegacyRectWidth, reference.LegacyRectHeight) * reference.LegacyDisplayScale;
            Vector2 referenceSize = FitAspectToBounds(referenceAspect, legacyBounds);
            float desiredArea = referenceSize.x * referenceSize.y * footprintAreaMultiplier;
            float width = Mathf.Sqrt(desiredArea * spriteAspect);
            return new Vector2(width, desiredArea / width);
        }

        public static Vector2 GetTouchTargetSize(Vector2 visualSize)
        {
            // Slight finger clearance without the old oversized fixed 196×188 targets.
            return new Vector2(Mathf.Max(128f, visualSize.x * 1.08f), Mathf.Max(128f, visualSize.y * 1.08f));
        }

        public static bool FitsInside(Vector2 area, Vector2 itemSize, Vector2 center)
        {
            if (!IsFinitePositive(area.x) || !IsFinitePositive(area.y) ||
                !IsFinitePositive(itemSize.x) || !IsFinitePositive(itemSize.y)) return false;
            return center.x - itemSize.x * .5f >= -area.x * .5f - Epsilon &&
                   center.x + itemSize.x * .5f <= area.x * .5f + Epsilon &&
                   center.y - itemSize.y * .5f >= -area.y * .5f - Epsilon &&
                   center.y + itemSize.y * .5f <= area.y * .5f + Epsilon;
        }

        public static bool Overlaps(Vector2 centerA, Vector2 sizeA, Vector2 centerB, Vector2 sizeB, float gap = 0f)
        {
            return Mathf.Abs(centerA.x - centerB.x) < (sizeA.x + sizeB.x) * .5f + gap - Epsilon &&
                   Mathf.Abs(centerA.y - centerB.y) < (sizeA.y + sizeB.y) * .5f + gap - Epsilon;
        }

        /// <summary>Places fixed-orientation rectangles with a best-short-side-fit MaxRects pass.</summary>
        public static bool TryPack(Vector2 area, Vector2[] itemSizes, float gap, out Vector2[] normalizedCenters)
        {
            normalizedCenters = Array.Empty<Vector2>();
            if (!IsFinitePositive(area.x) || !IsFinitePositive(area.y) || itemSizes == null || gap < 0f) return false;
            normalizedCenters = new Vector2[itemSizes.Length];
            if (itemSizes.Length == 0) return true;

            var ordered = new int[itemSizes.Length];
            for (int i = 0; i < ordered.Length; i++)
            {
                if (!IsFinitePositive(itemSizes[i].x) || !IsFinitePositive(itemSizes[i].y)) return false;
                ordered[i] = i;
            }
            Array.Sort(ordered, (a, b) =>
            {
                float areaA = itemSizes[a].x * itemSizes[a].y;
                float areaB = itemSizes[b].x * itemSizes[b].y;
                int byArea = areaB.CompareTo(areaA);
                return byArea != 0 ? byArea : a.CompareTo(b);
            });

            var free = new List<Rect>
            {
                new Rect(-area.x * .5f + gap * .5f, -area.y * .5f + gap * .5f,
                    area.x - gap, area.y - gap)
            };
            foreach (int index in ordered)
            {
                Vector2 paddedSize = itemSizes[index] + Vector2.one * gap;
                int best = -1;
                float bestShort = float.MaxValue;
                float bestLong = float.MaxValue;
                for (int i = 0; i < free.Count; i++)
                {
                    Rect candidate = free[i];
                    float leftoverX = candidate.width - paddedSize.x;
                    float leftoverY = candidate.height - paddedSize.y;
                    if (leftoverX < -Epsilon || leftoverY < -Epsilon) continue;
                    float shortSide = Mathf.Min(leftoverX, leftoverY);
                    float longSide = Mathf.Max(leftoverX, leftoverY);
                    if (shortSide < bestShort || (Mathf.Approximately(shortSide, bestShort) && longSide < bestLong))
                    {
                        best = i;
                        bestShort = shortSide;
                        bestLong = longSide;
                    }
                }

                if (best < 0)
                {
                    normalizedCenters = Array.Empty<Vector2>();
                    return false;
                }

                Rect slot = free[best];
                Rect used = new Rect(slot.xMin, slot.yMin, paddedSize.x, paddedSize.y);
                Vector2 center = new Vector2(used.xMin + gap * .5f + itemSizes[index].x * .5f,
                    used.yMin + gap * .5f + itemSizes[index].y * .5f);
                normalizedCenters[index] = new Vector2(center.x / area.x + .5f, center.y / area.y + .5f);
                SplitFreeRectangles(free, used);
            }
            return true;
        }

        private static void SplitFreeRectangles(List<Rect> free, Rect used)
        {
            for (int i = free.Count - 1; i >= 0; i--)
            {
                Rect region = free[i];
                if (!region.Overlaps(used)) continue;
                free.RemoveAt(i);
                if (used.xMin < region.xMax && used.xMax > region.xMin)
                {
                    AddIfPositive(free, new Rect(region.xMin, region.yMin, region.width, used.yMin - region.yMin));
                    AddIfPositive(free, new Rect(region.xMin, used.yMax, region.width, region.yMax - used.yMax));
                }
                if (used.yMin < region.yMax && used.yMax > region.yMin)
                {
                    AddIfPositive(free, new Rect(region.xMin, region.yMin, used.xMin - region.xMin, region.height));
                    AddIfPositive(free, new Rect(used.xMax, region.yMin, region.xMax - used.xMax, region.height));
                }
            }

            for (int i = 0; i < free.Count; i++)
            {
                for (int j = i + 1; j < free.Count; j++)
                {
                    if (Contains(free[i], free[j])) { free.RemoveAt(j--); continue; }
                    if (Contains(free[j], free[i])) { free.RemoveAt(i--); break; }
                }
            }
        }

        private static void AddIfPositive(List<Rect> rects, Rect rect)
        {
            if (rect.width > Epsilon && rect.height > Epsilon) rects.Add(rect);
        }

        private static bool Contains(Rect outer, Rect inner) => inner.xMin >= outer.xMin - Epsilon &&
            inner.yMin >= outer.yMin - Epsilon && inner.xMax <= outer.xMax + Epsilon && inner.yMax <= outer.yMax + Epsilon;

        private static bool IsFinitePositive(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
