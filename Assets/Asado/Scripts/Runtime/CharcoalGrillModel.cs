using UnityEngine;

namespace Asadito.Runtime
{
    /// <summary>Small tunable charcoal field. Heat is local and ember energy is conserved when raked.</summary>
    [System.Serializable]
    public sealed class CharcoalGrillModel
    {
        public HeatGrid Grid = new HeatGrid(8, 6);
        [Min(0f)] public float FuelEnergy = 1f;
        [Min(0f)] public float HeatWhenLitC = 210f;
        [Min(0f)] public float FuelBurnPerMinute = .002f;
        public bool IsLit { get; private set; }

        public void Reset()
        {
            Grid = new HeatGrid(8, 6);
            FuelEnergy = 1f;
            IsLit = false;
            FillGrid();
        }

        public void Ignite()
        {
            IsLit = true;
            FuelEnergy = Mathf.Max(.1f, FuelEnergy);
            FillGrid();
        }

        public void Step(float deltaMinutes)
        {
            if (!IsLit || deltaMinutes <= 0f) return;
            FuelEnergy = Mathf.Max(0f, FuelEnergy - FuelBurnPerMinute * deltaMinutes);
            bool extinguished = FuelEnergy <= .001f;
            if (extinguished) IsLit = false;
            for (int y = 0; y < Grid.Height; y++)
            for (int x = 0; x < Grid.Width; x++)
            {
                HeatCell cell = Grid.GetCell(x, y);
                cell.EmberEnergy = extinguished ? 0f : Mathf.Max(0f, cell.EmberEnergy - FuelBurnPerMinute * deltaMinutes);
                cell.Heat = Mathf.Lerp(20f, HeatWhenLitC, Mathf.Clamp01(cell.EmberEnergy));
                Grid.SetCell(x, y, cell);
            }
        }

        public Vector2 Sample(Vector2 normalizedPosition, Vector2 normalizedSize)
        {
            if (!IsLit) return new Vector2(20f, 0f);
            Rect region = new Rect(normalizedPosition - normalizedSize * .5f, normalizedSize);
            return Grid.SampleRegion(region);
        }

        public bool MoveEmbers(int fromX, int fromY, int toX, int toY)
        {
            if (!IsLit || (fromX == toX && fromY == toY)) return false;
            HeatCell from = Grid.GetCell(fromX, fromY);
            HeatCell to = Grid.GetCell(toX, toY);
            float capacity = Mathf.Max(0f, 1f - to.EmberEnergy);
            float moved = Mathf.Min(from.EmberEnergy * .35f, Mathf.Min(.24f, capacity));
            if (moved <= .001f) return false;
            from.EmberEnergy -= moved;
            to.EmberEnergy += moved;
            Grid.SetCell(fromX, fromY, from);
            Grid.SetCell(toX, toY, to);
            RefreshHeat(fromX, fromY);
            RefreshHeat(toX, toY);
            return true;
        }

        private void FillGrid()
        {
            if (Grid == null) Grid = new HeatGrid(8, 6);
            for (int y = 0; y < Grid.Height; y++)
            for (int x = 0; x < Grid.Width; x++)
            {
                float edgeFactor = .78f + .22f * Mathf.Sin((x + 1f) / (Grid.Width + 1f) * Mathf.PI);
                float ember = IsLit ? FuelEnergy * edgeFactor : 0f;
                Grid.SetCell(x, y, new HeatCell(Mathf.Lerp(20f, HeatWhenLitC, ember), ember));
            }
        }

        private void RefreshHeat(int x, int y)
        {
            HeatCell cell = Grid.GetCell(x, y);
            cell.Heat = Mathf.Lerp(20f, HeatWhenLitC, Mathf.Clamp01(cell.EmberEnergy));
            Grid.SetCell(x, y, cell);
        }
    }

    public static class DonenessLabels
    {
        public static string ToSpanish(this Doneness doneness)
        {
            switch (doneness)
            {
                case Doneness.Jugoso: return "JUGOSO";
                case Doneness.A_Punto: return "A PUNTO";
                case Doneness.A_PuntoMas: return "A PUNTO+";
                case Doneness.Cocido: return "COCIDO";
                default: return "BIEN COCIDO";
            }
        }
    }

}
