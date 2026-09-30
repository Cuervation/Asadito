using UnityEngine;

namespace Asadito.Runtime
{
    /// <summary>Always-on, uniform cooking heat for the MVP parrilla.</summary>
    [System.Serializable]
    public sealed class GrillHeatModel
    {
        public const float DefaultTemperatureC = 210f;

        [Range(100f, 300f)] public float TemperatureC = DefaultTemperatureC;

        public float GetTemperatureC()
        {
            return Mathf.Clamp(TemperatureC, 100f, 300f);
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
