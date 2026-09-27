using System;
using UnityEngine;

namespace Asadito.Runtime
{
    [Serializable]
    public struct FoodFaceState
    {
        [Min(0f)] public float SurfaceTemperatureC;
        [Range(0f, 1f)] public float Maillard;
        [Range(0f, 1f)] public float Char;
    }

    /// <summary>Gameplay cooking state; values are tunable simulation signals, not a food-safety model.</summary>
    [Serializable]
    public sealed class FoodState
    {
        [Min(0f)] public float CoreTemperatureC;
        [Min(0f)] public float SurfaceTemperatureC;
        [Range(0f, 1f)] public float Moisture = 1f;
        [Range(0f, 1f)] public float Maillard;
        [Range(0f, 1f)] public float Char;
        [Range(0f, 1f)] public float FatRendered;
        public FoodFaceState[] Faces = { new FoodFaceState(), new FoodFaceState() };
        [SerializeField] private int exposedFace;

        public int ExposedFace => exposedFace;
        public FoodFaceState CurrentFace => Faces != null && Faces.Length > 0 ? Faces[Mathf.Clamp(exposedFace, 0, Faces.Length - 1)] : default;

        public void Flip()
        {
            EnsureFaces();
            exposedFace = (exposedFace + 1) % Faces.Length;
        }

        public void Reset()
        {
            CoreTemperatureC = 0f;
            SurfaceTemperatureC = 0f;
            Moisture = 1f;
            Maillard = Char = FatRendered = 0f;
            Faces = new[] { new FoodFaceState(), new FoodFaceState() };
            exposedFace = 0;
        }

        public void SetCurrentFace(FoodFaceState state)
        {
            EnsureFaces();
            Faces[exposedFace] = state;
            SurfaceTemperatureC = state.SurfaceTemperatureC;
            Maillard = Mathf.Max(Faces[0].Maillard, Faces[1].Maillard);
            Char = Mathf.Max(Faces[0].Char, Faces[1].Char);
        }

        private void EnsureFaces()
        {
            if (Faces == null || Faces.Length != 2) Faces = new FoodFaceState[2];
            exposedFace = Mathf.Clamp(exposedFace, 0, 1);
        }
    }
}
