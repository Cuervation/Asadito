using System;
using UnityEngine;

namespace Asadito.Runtime
{
    /// <summary>Creates six aligned, tightly cropped sprites from each lazily loaded vertical cooking atlas.</summary>
    public static class FoodSpriteLibrary
    {
        public const int StateCount = 6;
        public static readonly string[] StateNames = { "raw", "warming", "browning", "ideal", "overcooked", "burnt" };

        public static Sprite[] CreateStateSprites(FoodDefinition definition, Texture2D atlas)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (atlas == null) throw new ArgumentNullException(nameof(atlas));
            if (atlas.width <= 0 || atlas.height < StateCount)
                throw new ArgumentException("Food atlas must have enough pixels for six vertical state rows.", nameof(atlas));
            if (definition.SpriteCrop == null || !definition.SpriteCrop.IsValid)
                throw new ArgumentException("Food definition has invalid normalized sprite bounds.", nameof(definition));

            float cellHeight = atlas.height / (float)StateCount;
            float xMin = definition.SpriteCrop.XMin * atlas.width;
            float xMax = definition.SpriteCrop.XMax * atlas.width;
            var sprites = new Sprite[StateCount];
            for (int stage = 0; stage < StateCount; stage++)
            {
                int rowFromBottom = StateCount - 1 - stage;
                float yMin = rowFromBottom * cellHeight + (1f - definition.SpriteCrop.YMax) * cellHeight;
                float yMax = rowFromBottom * cellHeight + (1f - definition.SpriteCrop.YMin) * cellHeight;
                sprites[stage] = Sprite.Create(atlas, new Rect(xMin, yMin, xMax - xMin, yMax - yMin), new Vector2(.5f, .5f), 100f);
                sprites[stage].name = definition.Id + "_" + StateNames[stage];
            }
            return sprites;
        }
    }
}
