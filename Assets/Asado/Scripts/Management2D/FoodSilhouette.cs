using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asadito
{
    /// <summary>Small baked alpha masks; the cooking atlases remain GPU-only/compressed on mobile.</summary>
    public static class FoodSilhouette
    {
        [Serializable] sealed class Mask { public string FoodId; public string Bits; }
        [Serializable] sealed class Library { public int Width; public int Height; public Mask[] Masks; }
        static Dictionary<string,byte[]> masks;
        static int width,height;
        public static bool Contains(string id,Vector2 uv)
        {
            if (uv.x < 0 || uv.x >= 1 || uv.y < 0 || uv.y >= 1) return false;
            if (masks == null)
            {
                var asset = Resources.Load<TextAsset>("Definitions/FoodHitMasks");
                var data = asset != null ? JsonUtility.FromJson<Library>(asset.text) : null;
                masks = new Dictionary<string,byte[]>();
                if (data != null) { width = data.Width; height = data.Height; foreach (var m in data.Masks) masks[m.FoodId] = Convert.FromBase64String(m.Bits); }
            }
            // Missing content fails closed instead of buying through invisible rectangular corners.
            if (!masks.TryGetValue(id,out var bits)) return false;
            int index = Mathf.Min(height-1,(int)(uv.y*height))*width + Mathf.Min(width-1,(int)(uv.x*width));
            return (bits[index>>3] & (1<<(index&7))) != 0;
        }
    }
}
