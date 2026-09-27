using Asadito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Asadito.Editor
{
    /// <summary>One-time, reproducible scene wiring for the first playable slice.</summary>
    [InitializeOnLoad]
    internal static class MvpSceneSetup
    {
        static MvpSceneSetup()
        {
            EditorApplication.delayCall += BuildOnce;
        }

        private static void BuildOnce()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += BuildOnce;
                return;
            }

            PrepareSprite("Assets/Asado/Resources/Art/PatioParrilla.png");
            PrepareSprite("Assets/Asado/Resources/Art/TiraAsadoCruda.png");
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path)) return;
            if (GameObject.Find("Asadito MVP") != null) return;

            var game = new GameObject("Asadito MVP");
            game.AddComponent<AsaditoGame>();
            Undo.RegisterCreatedObjectUndo(game, "Build Asadito MVP");

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.backgroundColor = new Color32(31, 34, 29, 255);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Asadito First Playable listo: parrilla, dos comensales y bandeja.");
        }

        private static void PrepareSprite(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null || importer.textureType == TextureImporterType.Sprite) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }
}
