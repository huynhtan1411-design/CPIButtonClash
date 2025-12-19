// Dynamic Radial Masks <https://u3d.as/1w0H>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;


namespace AmazingAssets.DynamicRadialMasks.Editor
{
    public static class EditorUtilities
    {
        public class Enum
        {
            public enum RenderPipeline { Unknown, Builtin, Universal, HighDefinition }
        }

        static public char[] invalidFileNameCharachters = Path.GetInvalidFileNameChars();

        static string thisAssetPath;
        static bool? packageShaderGraphIsInstalled;
        static bool? packageAmplfyShaderEditorIsInstalled;



        static public string GetThisAssetProjectPath()
        {
            if (string.IsNullOrEmpty(thisAssetPath))
            {
                string fileName = "AmazingAssets.DynamicRadialMasks.Editor";

                string[] assets = AssetDatabase.FindAssets(fileName, null);
                if (assets != null && assets.Length > 0)
                {
                    string currentFilePath = AssetDatabase.GUIDToAssetPath(assets[0]);
                    thisAssetPath = Path.GetDirectoryName(Path.GetDirectoryName(currentFilePath));
                }
                else
                {
                    Debug.LogError("Cannot find 'AmazingAssets.DynamicRadialMasks.Editor.asmdef' file.");
                }
            }
            return thisAssetPath;
        }

        static internal Texture2D LoadTexture(string resourceName, TextureWrapMode wrapMode, bool linear)
        {
            Texture2D texture = (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath(Path.Combine(GetThisAssetProjectPath(), "Editor", "Icons", resourceName + ".png"), typeof(Texture2D));


            if (texture != null)
                texture.wrapMode = wrapMode;

            return texture;
        }

        static public Enum.RenderPipeline GetCurrentRenderPipeline()
        {
#if UNITY_6000_0_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null && UnityEngine.QualitySettings.renderPipeline == null)
                return Enum.RenderPipeline.Builtin;
            else
            {
                string currentType = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null ? UnityEngine.QualitySettings.renderPipeline.GetType().ToString() :
                                                                                                      UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.GetType().ToString();

                string parentType = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null ? UnityEngine.QualitySettings.renderPipeline.GetType().GetTypeInfo().BaseType.ToString() :
                                                                                                           UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.GetType().GetTypeInfo().BaseType.ToString();
#else
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null && UnityEngine.QualitySettings.renderPipeline == null)
                return Enum.RenderPipeline.Builtin;
            else
            {
                string currentType = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null ? UnityEngine.QualitySettings.renderPipeline.GetType().ToString() :
                                                                                                    UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().ToString();

                string parentType = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null ? UnityEngine.QualitySettings.renderPipeline.GetType().GetTypeInfo().BaseType.ToString() :
                                                                                                         UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().GetTypeInfo().BaseType.ToString();
#endif

                if (currentType.Contains("UnityEngine.Rendering.Universal.") || parentType.Contains("UnityEngine.Rendering.Universal."))
                    return Enum.RenderPipeline.Universal;

                else if (currentType.Contains("UnityEngine.Rendering.HighDefinition.") || parentType.Contains("UnityEngine.Rendering.HighDefinition."))
                    return Enum.RenderPipeline.HighDefinition;


                Debug.LogError("Undefined Render Pipeline '" + currentType + "'");
                return Enum.RenderPipeline.Unknown;
            }
        }

        static public string RemoveInvalidCharacters(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            else
            {
                if (name.IndexOfAny(invalidFileNameCharachters) == -1)
                    return name;
                else
                    return string.Concat(name.Split(invalidFileNameCharachters, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        static public bool ContainsInvalidFileNameCharacters(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            else
                return name.IndexOfAny(invalidFileNameCharachters) >= 0;
        }

        static public bool CanGenerateUnityShaderGrap()
        {
            if (GetCurrentRenderPipeline() == Enum.RenderPipeline.Universal || GetCurrentRenderPipeline() == Enum.RenderPipeline.HighDefinition)
                return true;


#if UNITY_2021_3_OR_NEWER   //Built-in RP supports ShaderGraph from Unity 2021.3+
            if (packageShaderGraphIsInstalled.HasValue == false)
                packageShaderGraphIsInstalled = EditorUtilities.IsPackageInstalled("com.unity.shadergraph");

            return packageShaderGraphIsInstalled.Value;
#else
            return false;
#endif
        }
        static public bool CanGenerateAmplifyShaderFuntion()
        {
            if (packageAmplfyShaderEditorIsInstalled.HasValue == false)
                packageAmplfyShaderEditorIsInstalled = EditorUtilities.IsFileInProject("AmplifyShaderEditor", ".asmdef");

            return packageAmplfyShaderEditorIsInstalled.Value;
        }
        public static bool IsPackageInstalled(string packageId)
        {
            if (!File.Exists("Packages/manifest.json"))
                return false;

            string jsonText = File.ReadAllText("Packages/manifest.json");
            return jsonText.Contains(packageId);
        }
        public static bool IsFileInProject(string fileName, string extension)
        {
            return AssetDatabase.FindAssets(fileName, null).Select(c => AssetDatabase.GUIDToAssetPath(c)).Where(c => Path.GetExtension(c) == extension).Count() > 0;
        }
    }
}
