#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Asadito.Editor
{
    /// <summary>Explicit authoring, never runs on import. Shared production meshes are native Unity assets.</summary>
    public static class ManagementModelBake
    {
        [MenuItem("Asadito/Bake Management 3D Food Models")]
        public static void Bake()
        {
            const string folder="Assets/Asado/Resources/Management3D";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Asado/Resources","Management3D");
            Write(folder+"/chorizo.asset",ManagementMeshes.Chorizo());Write(folder+"/tira.asset",ManagementMeshes.Tira());
            AssetDatabase.SaveAssets();Debug.Log("Management 3D production meshes baked: chorizo/tira; original sprites untouched.");
        }
        static void Write(string path,Mesh mesh)
        {
            var previous=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(previous==null)AssetDatabase.CreateAsset(mesh,path);
            else{EditorUtility.CopySerialized(mesh,previous);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(previous);}
        }
    }
}
#endif
