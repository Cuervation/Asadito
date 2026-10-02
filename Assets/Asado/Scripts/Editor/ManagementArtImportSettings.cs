#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Asadito.Editor
{
    /// <summary>
    /// Imports the future management-art library as explicit, lightweight sprites.
    /// These assets intentionally live outside Resources until their systems are implemented.
    /// </summary>
    public sealed class ManagementArtImportSettings : AssetPostprocessor
    {
        private const string ManagementRoot = "/Art/Management/";

        private void OnPreprocessTexture()
        {
            if (assetPath.IndexOf(ManagementRoot, System.StringComparison.Ordinal) < 0) return;

            var importer = (TextureImporter)assetImporter;
            bool hasAlpha = PngHasAlphaChannel(assetPath);
            bool isBackground = assetPath.EndsWith("/Patio_Management_Base.png") ||
                                assetPath.EndsWith("/ButcherShop_Background.png") ||
                                assetPath.EndsWith("/ButcherShop_CounterV2.png") ||
                                assetPath.EndsWith("/Fridge_Hybrid_OpenEmptyV2.png");
            int maxSize = isBackground ? 2048 : 512;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            var spriteSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(spriteSettings);
            spriteSettings.spriteMeshType = isBackground ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
            spriteSettings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(spriteSettings);
            importer.spritePixelsPerUnit = 100f;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = hasAlpha;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.anisoLevel = 1;
            importer.maxTextureSize = maxSize;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 85;

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.name = "Android";
            android.overridden = true;
            android.maxTextureSize = maxSize;
            android.format = hasAlpha ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGB4;
            android.textureCompression = TextureImporterCompression.CompressedHQ;
            android.compressionQuality = 85;
            importer.SetPlatformTextureSettings(android);
        }

        private static bool PngHasAlphaChannel(string path)
        {
            // PNG color type is byte 25: 4 = gray+alpha, 6 = RGBA. Reading only the header
            // avoids loading full-resolution image data while still selecting the right ETC2 format.
            byte[] header = new byte[26];
            using (FileStream stream = File.OpenRead(path))
            {
                int read = stream.Read(header, 0, header.Length);
                if (read < header.Length || header[0] != 137 || header[1] != 80 ||
                    header[2] != 78 || header[3] != 71) return true;
            }

            return header[25] == 4 || header[25] == 6;
        }
    }
}
#endif
