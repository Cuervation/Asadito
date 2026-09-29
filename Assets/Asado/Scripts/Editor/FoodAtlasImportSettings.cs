#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Asadito.Editor
{
    /// <summary>Mobile-safe import settings for the generated six-frame food state atlases.</summary>
    public sealed class FoodAtlasImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/Resources/Art/Foods/States/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.spriteImportMode = SpriteImportMode.None;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.filterMode = FilterMode.Bilinear;
            // The six vertical frames must remain exactly divisible after import.
            // 1024x1536 source atlases become 512x768 at this cap, unlike 512x512.
            importer.maxTextureSize = 768;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 82;

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.name = "Android";
            android.overridden = true;
            android.maxTextureSize = 768;
            android.format = TextureImporterFormat.ETC2_RGBA8;
            android.textureCompression = TextureImporterCompression.CompressedHQ;
            android.compressionQuality = 82;
            importer.SetPlatformTextureSettings(android);
        }
    }

    /// <summary>Keep the twelve full-bleed level cards lightweight at phone-card display size.</summary>
    public sealed class LevelCardImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/Resources/Art/LevelCards/Nivel")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.spriteImportMode = SpriteImportMode.None;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 82;

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.name = "Android";
            android.overridden = true;
            android.maxTextureSize = 512;
            android.format = TextureImporterFormat.ETC2_RGBA8;
            android.textureCompression = TextureImporterCompression.CompressedHQ;
            android.compressionQuality = 82;
            importer.SetPlatformTextureSettings(android);
        }
    }
}
#endif
