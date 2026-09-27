using System;
using UnityEngine;

namespace Asadito.Runtime
{
    [Serializable]
    public struct HeatCell
    {
        [Min(0f)] public float Heat;
        [Min(0f)] public float EmberEnergy;

        public HeatCell(float heat, float emberEnergy)
        {
            Heat = Mathf.Max(0f, heat);
            EmberEnergy = Mathf.Max(0f, emberEnergy);
        }
    }

    /// <summary>Configurable grill heat field. Coordinates and regions are normalized to 0..1.</summary>
    [Serializable]
    public sealed class HeatGrid
    {
        [Min(1)] public int Width = 8;
        [Min(1)] public int Height = 6;
        [SerializeField] private HeatCell[] cells;

        public HeatGrid() : this(8, 6) { }

        public HeatGrid(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            cells = new HeatCell[Width * Height];
        }

        public HeatCell GetCell(int x, int y)
        {
            EnsureStorage();
            return cells[Index(x, y)];
        }

        public void SetCell(int x, int y, HeatCell cell)
        {
            EnsureStorage();
            cells[Index(x, y)] = cell;
        }

        public Vector2 SampleRegion(Rect normalizedRegion)
        {
            EnsureStorage();
            float xMin = Mathf.Clamp01(normalizedRegion.xMin);
            float xMax = Mathf.Clamp01(normalizedRegion.xMax);
            float yMin = Mathf.Clamp01(normalizedRegion.yMin);
            float yMax = Mathf.Clamp01(normalizedRegion.yMax);
            int firstX = Mathf.Clamp(Mathf.FloorToInt(xMin * Width), 0, Width - 1);
            int lastX = Mathf.Clamp(Mathf.CeilToInt(xMax * Width) - 1, firstX, Width - 1);
            int firstY = Mathf.Clamp(Mathf.FloorToInt(yMin * Height), 0, Height - 1);
            int lastY = Mathf.Clamp(Mathf.CeilToInt(yMax * Height) - 1, firstY, Height - 1);
            float heat = 0f, embers = 0f;
            int count = 0;
            for (int y = firstY; y <= lastY; y++)
            for (int x = firstX; x <= lastX; x++)
            {
                HeatCell cell = cells[y * Width + x];
                heat += cell.Heat;
                embers += cell.EmberEnergy;
                count++;
            }
            return count == 0 ? Vector2.zero : new Vector2(heat / count, embers / count);
        }

        private int Index(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("Grid coordinate is outside the configured dimensions.");
            return y * Width + x;
        }

        private void EnsureStorage()
        {
            Width = Mathf.Max(1, Width);
            Height = Mathf.Max(1, Height);
            if (cells == null || cells.Length != Width * Height)
                Array.Resize(ref cells, Width * Height);
        }
    }
}
