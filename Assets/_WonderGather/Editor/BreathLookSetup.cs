using System.IO;
using UnityEditor;
using UnityEngine;

namespace WonderGather.Editor
{
    // Makes the look of breath in cold air: a material of the shader "Wonder Gather/Breath", kept where the game
    // can find it by name (Resources), so that the shader is in the build without any scene being touched.
    public static class BreathLookSetup
    {
        public const string Folder = "Assets/_WonderGather/Resources", MaterialPath = Folder + "/BreathInAir.mat";

        [MenuItem("Wonder Gather/Make the breath-in-air material")]
        public static void Make()
        {
            var shader = Shader.Find("Wonder Gather/Breath");
            if (shader == null) throw new System.InvalidOperationException("The shader \"Wonder Gather/Breath\" was not found.");
            if (!AssetDatabase.IsValidFolder(Folder)) { Directory.CreateDirectory(Folder); AssetDatabase.Refresh(); }
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "BreathInAir" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else material.shader = shader;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            Debug.Log("BREATHLOOK made " + MaterialPath);
        }
    }
}
