using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asadito.Runtime
{
    /// <summary>Aspect-preserving food sizing and footprint placement shared by grill and raw tray.</summary>
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

        /// <summary>
        /// Returns one shared scale that guarantees every full food footprint can fit inside
        /// one surface with the requested edge clearance. It does not try to fit the whole order;
        /// excess portions are still stacked by TryPackOrStack.
        /// </summary>
        public static float GetMaxUniformFitScale(Vector2 area, Vector2[] itemSizes, float edgeGap)
        {
            if (!IsFinitePositive(area.x) || !IsFinitePositive(area.y) || itemSizes == null ||
                edgeGap < 0f || area.x <= edgeGap * 2f || area.y <= edgeGap * 2f) return 0f;
            if (itemSizes.Length == 0) return 1f;

            float availableWidth = area.x - edgeGap * 2f;
            float availableHeight = area.y - edgeGap * 2f;
            float scale = 1f;
            for (int i = 0; i < itemSizes.Length; i++)
            {
                if (!IsFinitePositive(itemSizes[i].x) || !IsFinitePositive(itemSizes[i].y)) return 0f;
                scale = Mathf.Min(scale, Mathf.Min(availableWidth / itemSizes[i].x,
                    availableHeight / itemSizes[i].y));
            }
            return Mathf.Clamp01(scale);
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
            if (!TryPackInternal(area, itemSizes, gap, false, out normalizedCenters, out _, out int packedCount))
                return false;
            if (packedCount == itemSizes.Length) return true;
            normalizedCenters = Array.Empty<Vector2>();
            return false;
        }

        /// <summary>
        /// Finds the largest single scale that packs every item into an area. Applying one common
        /// scale preserves the relative size of different cuts instead of normalizing each to a cell.
        /// </summary>
        public static bool TryPackScaled(Vector2 area, Vector2[] itemSizes, float gap,
            out float uniformScale, out Vector2[] normalizedCenters)
        {
            uniformScale = 0f;
            normalizedCenters = Array.Empty<Vector2>();
            if (!IsFinitePositive(area.x) || !IsFinitePositive(area.y) || itemSizes == null || gap < 0f)
                return false;
            if (itemSizes.Length == 0)
            {
                uniformScale = 1f;
                return true;
            }
            if (GetAreaSortedIndices(itemSizes) == null) return false;

            var scaledSizes = new Vector2[itemSizes.Length];
            float low = 0f;
            float high = 1f;
            Vector2[] bestCenters = Array.Empty<Vector2>();
            const int SearchIterations = 22;
            for (int iteration = 0; iteration < SearchIterations; iteration++)
            {
                float candidateScale = (low + high) * .5f;
                for (int i = 0; i < itemSizes.Length; i++)
                    scaledSizes[i] = itemSizes[i] * candidateScale;
                if (TryPack(area, scaledSizes, gap, out Vector2[] candidateCenters))
                {
                    low = candidateScale;
                    bestCenters = candidateCenters;
                }
                else high = candidateScale;
            }

            if (low <= 0f || bestCenters.Length != itemSizes.Length) return false;
            uniformScale = low;
            normalizedCenters = bestCenters;
            return true;
        }

        /// <summary>
        /// Packs as many visual food footprints as possible without overlap. Only the portions that
        /// cannot fit are staggered into an intentional stack above the largest packed piece.
        /// Centers are normalized to the area and stacked[i] reports which portions had to stack.
        /// </summary>
        public static bool TryPackOrStack(Vector2 area, Vector2[] itemSizes, float gap,
            out Vector2[] normalizedCenters, out bool[] stacked)
        {
            normalizedCenters = Array.Empty<Vector2>();
            stacked = Array.Empty<bool>();
            if (!TryPackInternal(area, itemSizes, gap, true, out normalizedCenters, out bool[] packed, out int packedCount))
                return false;

            stacked = new bool[itemSizes.Length];
            if (itemSizes.Length == 0) return true;
            if (packedCount == itemSizes.Length)
            {
                RecenterPackedItems(area, itemSizes, packed, normalizedCenters);
                return true;
            }

            var ordered = GetAreaSortedIndices(itemSizes);
            if (packedCount == 0)
            {
                // Even when a single cut is larger than the usable tray, keep the first cut as the
                // visible base layer and stack the rest; its center remains inside the tray.
                packed[ordered[0]] = true;
                normalizedCenters[ordered[0]] = new Vector2(.5f, .5f);
                packedCount = 1;
            }

            RecenterPackedItems(area, itemSizes, packed, normalizedCenters);
            int anchor = ordered[0];
            for (int i = 0; i < ordered.Length; i++)
                if (packed[ordered[i]]) { anchor = ordered[i]; break; }

            Vector2 anchorCenter = NormalizedToLocal(area, normalizedCenters[anchor]);
            int stackOrder = 0;
            foreach (int index in ordered)
            {
                if (packed[index]) continue;
                stacked[index] = true;
                Vector2 offset = GetStackOffset(stackOrder++, itemSizes[index], itemSizes[anchor]);
                Vector2 center = ClampCenterToArea(area, itemSizes[index], anchorCenter + offset);
                normalizedCenters[index] = LocalToNormalized(area, center);
            }
            return true;
        }

        private static bool TryPackInternal(Vector2 area, Vector2[] itemSizes, float gap, bool allowUnplaced,
            out Vector2[] normalizedCenters, out bool[] packed, out int packedCount)
        {
            normalizedCenters = Array.Empty<Vector2>();
            packed = Array.Empty<bool>();
            packedCount = 0;
            if (!IsFinitePositive(area.x) || !IsFinitePositive(area.y) || itemSizes == null || gap < 0f) return false;

            normalizedCenters = new Vector2[itemSizes.Length];
            packed = new bool[itemSizes.Length];
            if (itemSizes.Length == 0) return true;

            int[] ordered = GetAreaSortedIndices(itemSizes);
            if (ordered == null)
            {
                normalizedCenters = Array.Empty<Vector2>();
                packed = Array.Empty<bool>();
                return false;
            }

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
                    if (allowUnplaced) continue;
                    normalizedCenters = Array.Empty<Vector2>();
                    packed = Array.Empty<bool>();
                    packedCount = 0;
                    return false;
                }

                Rect slot = free[best];
                Rect used = new Rect(slot.xMin, slot.yMin, paddedSize.x, paddedSize.y);
                Vector2 center = new Vector2(used.xMin + gap * .5f + itemSizes[index].x * .5f,
                    used.yMin + gap * .5f + itemSizes[index].y * .5f);
                normalizedCenters[index] = LocalToNormalized(area, center);
                packed[index] = true;
                packedCount++;
                SplitFreeRectangles(free, used);
            }
            return true;
        }

        private static int[] GetAreaSortedIndices(Vector2[] itemSizes)
        {
            if (itemSizes == null) return null;
            var ordered = new int[itemSizes.Length];
            for (int i = 0; i < ordered.Length; i++)
            {
                if (!IsFinitePositive(itemSizes[i].x) || !IsFinitePositive(itemSizes[i].y)) return null;
                ordered[i] = i;
            }
            Array.Sort(ordered, (a, b) =>
            {
                float areaA = itemSizes[a].x * itemSizes[a].y;
                float areaB = itemSizes[b].x * itemSizes[b].y;
                int byArea = areaB.CompareTo(areaA);
                return byArea != 0 ? byArea : a.CompareTo(b);
            });
            return ordered;
        }

        private static void RecenterPackedItems(Vector2 area, Vector2[] itemSizes, bool[] packed,
            Vector2[] normalizedCenters)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < packed.Length; i++)
            {
                if (!packed[i]) continue;
                Vector2 center = NormalizedToLocal(area, normalizedCenters[i]);
                minX = Mathf.Min(minX, center.x - itemSizes[i].x * .5f);
                minY = Mathf.Min(minY, center.y - itemSizes[i].y * .5f);
                maxX = Mathf.Max(maxX, center.x + itemSizes[i].x * .5f);
                maxY = Mathf.Max(maxY, center.y + itemSizes[i].y * .5f);
            }

            if (minX == float.MaxValue) return;
            Vector2 shift = new Vector2(-(minX + maxX) * .5f, -(minY + maxY) * .5f);
            Vector2 normalizedShift = new Vector2(shift.x / area.x, shift.y / area.y);
            for (int i = 0; i < packed.Length; i++)
                if (packed[i]) normalizedCenters[i] += normalizedShift;
        }

        private static Vector2 GetStackOffset(int stackOrder, Vector2 itemSize, Vector2 anchorSize)
        {
            // Make the stack layer visibly peek out from under its anchor while remaining subtle.
            float x = Mathf.Clamp(Mathf.Min(itemSize.x, anchorSize.x) * .28f, 24f, 72f);
            float y = Mathf.Clamp(Mathf.Min(itemSize.y, anchorSize.y) * .42f, 24f, 80f);
            switch (stackOrder % 8)
            {
                case 0: return new Vector2(x, y);
                case 1: return new Vector2(-x, y);
                case 2: return new Vector2(x, -y);
                case 3: return new Vector2(-x, -y);
                case 4: return new Vector2(0f, y * 2f);
                case 5: return new Vector2(0f, -y * 2f);
                case 6: return new Vector2(x * 2f, 0f);
                default: return new Vector2(-x * 2f, 0f);
            }
        }

        private static Vector2 ClampCenterToArea(Vector2 area, Vector2 itemSize, Vector2 center)
        {
            if (itemSize.x >= area.x) center.x = 0f;
            else center.x = Mathf.Clamp(center.x, (itemSize.x - area.x) * .5f, (area.x - itemSize.x) * .5f);
            if (itemSize.y >= area.y) center.y = 0f;
            else center.y = Mathf.Clamp(center.y, (itemSize.y - area.y) * .5f, (area.y - itemSize.y) * .5f);
            return center;
        }

        private static Vector2 NormalizedToLocal(Vector2 area, Vector2 normalized) =>
            new Vector2((normalized.x - .5f) * area.x, (normalized.y - .5f) * area.y);

        private static Vector2 LocalToNormalized(Vector2 area, Vector2 local) =>
            new Vector2(local.x / area.x + .5f, local.y / area.y + .5f);

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
