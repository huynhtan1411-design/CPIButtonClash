// Dynamic Radial Masks <https://u3d.as/1w0H>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;


namespace AmazingAssets.DynamicRadialMasks.Editor
{
    public class DRMEditorWindow : EditorWindow
    {
        public class Enum
        {
           public enum FileExtension { cginc, shadersubgraph, asset };
        }


        DynamicRadialMasks.Enum.MaskShape maskShape;
        int maskCount;
        DynamicRadialMasks.Enum.MaskType maskType;
        DynamicRadialMasks.Enum.MaskBlendMode maskBlendMode;
        int maskID;
        DynamicRadialMasks.Enum.MaskScope maskScope;




        [MenuItem("Window/Amazing Assets/Dynamic Radial Masks", false, 1702)]
        static public void ShowWindowFromMainMenu()
        {
            DRMEditorWindow window = (DRMEditorWindow)EditorWindow.GetWindow(typeof(DRMEditorWindow));
            window.titleContent = new GUIContent(AssetInfo.assetName);

            window.minSize = new Vector2(400, 210);
            window.maxSize = new Vector2(400, 210);

            window.ShowUtility();
        }


        void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawMainWindow();
            EditorGUILayout.EndVertical();
        }


        void DrawMainWindow()
        {
            Rect drawRect = EditorGUILayout.GetControlRect();

            if (GUI.Button(new Rect(drawRect.xMax - drawRect.height, drawRect.yMin, drawRect.height, drawRect.height), UnityEditor.EditorGUIUtility.IconContent("_Popup@2x"), DRMEditorResources.GuiStyleIconButton))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Recompile CGINC Files"), false, CallbackRecompileCGINCFiles);

                if(EditorUtilities.GetCurrentRenderPipeline() == EditorUtilities.Enum.RenderPipeline.Builtin)
                {
                    if (Application.unityVersion.Contains("2019") == false &&
                        Application.unityVersion.Contains("2020") == false)
                        menu.AddItem(new GUIContent("Import Shader Compiler"), false, CallbackMenuImportShaderGenerator);
                }
                else
                {
                    if(Application.unityVersion.Contains("2019") == false)
                        menu.AddItem(new GUIContent("Import Shader Compiler"), false, CallbackMenuImportShaderGenerator);
                }


                menu.ShowAsContext();
            }
                       


            EditorGUILayout.LabelField("Shape");
            drawRect = GUILayoutUtility.GetLastRect();
            drawRect.xMin += UnityEditor.EditorGUIUtility.labelWidth;
            if (GUI.Button(drawRect, maskShape.ToString(), EditorStyles.popup))
            {
                PopupWindow.Show(drawRect, new DRMShapesEnumPopupWindow((int)maskShape, CallbackMenuShapesPopup));
            }


            maskCount = EditorGUILayout.IntSlider("Count", maskCount, 1, 200);
            maskType = (DynamicRadialMasks.Enum.MaskType)EditorGUILayout.EnumPopup("Type", maskType);
            maskBlendMode = (DynamicRadialMasks.Enum.MaskBlendMode)EditorGUILayout.EnumPopup("Blend Mode", maskBlendMode);
            maskID = EditorGUILayout.IntSlider("ID", maskID, 1, 32);
            maskScope = (DynamicRadialMasks.Enum.MaskScope)EditorGUILayout.EnumPopup("Scope", maskScope);


            GUILayout.Space(10);
            using (new EditorGUIHelper.GUIEnabled(false))
            {
                EditorGUILayout.LabelField(" ", " ", EditorStyles.textField, GUILayout.Height(16));

                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), "Required Instructions Count", EditorStyles.wordWrappedMiniLabel);

                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), " ", GetInstructionsCount(maskShape, maskCount, maskType, maskBlendMode).ToString("N0"), EditorStyles.wordWrappedMiniLabel);
            }


            GUILayout.Space(12);
            EditorGUILayout.BeginHorizontal("Toolbar");

            if (GUILayout.Button("Copy Path", EditorStyles.toolbarButton))
            {
                //Make sure main CGINC file exists
                string mainCGINCFilePath = CreateCGINCFile(maskShape, maskCount, maskType, maskBlendMode, maskID, maskScope);

                TextEditor te = new TextEditor();
                te.text = "\"" + mainCGINCFilePath + "\"";
                te.text = te.text.Replace(Path.DirectorySeparatorChar, '/');
                te.SelectAll();
                te.Copy();
            }

            if (GUILayout.Button("CGINC", EditorStyles.toolbarButton))
            {
                //Make sure main CGINC file exists
                string mainCGINCFilePath = CreateCGINCFile(maskShape, maskCount, maskType, maskBlendMode, maskID, maskScope);

                PingObject(mainCGINCFilePath);
            }

            using (new EditorGUIHelper.GUIEnabled(EditorUtilities.CanGenerateUnityShaderGrap()))
            {
                if (GUILayout.Button("Shader Graph", EditorStyles.toolbarButton))
                {
                    //Make sure main CGINC file exists
                    CreateCGINCFile(maskShape, maskCount, maskType, maskBlendMode, maskID, maskScope);


                    string filePath = GetDRMFilePath(Enum.FileExtension.shadersubgraph, maskShape, maskCount, maskType, maskBlendMode, maskID, maskScope);

                    if (File.Exists(filePath))
                        PingObject(filePath);
                    else
                    {
                        CreateSubGraphFile(maskShape, maskCount, maskType, maskBlendMode, maskID, maskScope, Enum.FileExtension.shadersubgraph);

                        AssetDatabase.Refresh();

                        PingObject(filePath);
                    }

                }
            }

            using (new EditorGUIHelper.GUIEnabled(EditorUtilities.CanGenerateAmplifyShaderFuntion()))
            {
                if (GUILayout.Button("Amplify Shader Editor", EditorStyles.toolbarButton))
                {
                    //Make sure main CGINC file exists
                    CreateCGINCFile(maskShape, maskCount, maskType, maskBlendMode, maskID, maskScope);


                    string filePath = GetDRMFilePath(Enum.FileExtension.asset, maskShape, maskCount, maskType, maskBlendMode, maskID, maskScope);

                    if (File.Exists(filePath))
                        PingObject(filePath);
                    else
                    {
                        CreateSubGraphFile(maskShape, maskCount, maskType, maskBlendMode, maskID, maskScope, Enum.FileExtension.asset);

                        AssetDatabase.Refresh();

                        PingObject(filePath);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

        }

        string GetTemplateFileLocation(Enum.FileExtension _Extention, string _MaskShape, string _MaskType, string _MaskBlendMode)
        {
            string fileID = string.Empty;
            switch (_Extention)
            {
                case Enum.FileExtension.cginc: fileID = _MaskShape; break;
                case Enum.FileExtension.shadersubgraph: fileID = "UnityShaderGraph"; break;
                case Enum.FileExtension.asset: fileID = "AmplifyShaderEditor"; break;
            }

            string fileName = string.Format("Template_{0}_{1}_{2}.txt", fileID, _MaskType, _MaskBlendMode);

            string path = Path.Combine(EditorUtilities.GetThisAssetProjectPath(), "Shaders");
            path = Path.Combine(path, "Templates");
            path = Path.Combine(path, fileName);

            return path;
        }
        string GetDRMFilePath(Enum.FileExtension _Extention, DynamicRadialMasks.Enum.MaskShape _MaskShape, int _ShapeCount, DynamicRadialMasks.Enum.MaskType _MaskType, DynamicRadialMasks.Enum.MaskBlendMode _MaskBlendMode, int _MaskID, DynamicRadialMasks.Enum.MaskScope _MaskScope)
        {
            string fileName = string.Format("DynamicRadialMasks_{0}_{1}_{2}_{3}_ID{4}_{5}.{6}", _MaskShape, _ShapeCount, _MaskType, _MaskBlendMode, _MaskID, _MaskScope, _Extention);

            string subFolderName = string.Empty;
            switch (_Extention)
            {
                case Enum.FileExtension.cginc: subFolderName = "CGINC"; break;
                case Enum.FileExtension.shadersubgraph: subFolderName = "Unity Shader Graph"; break;
                case Enum.FileExtension.asset: subFolderName = "Amplify Shader Editor"; break;
            }

            string path = Path.Combine(EditorUtilities.GetThisAssetProjectPath(), "Shaders");
            path = Path.Combine(path, subFolderName);
            path = Path.Combine(path, _MaskShape.ToString());
            path = Path.Combine(path, fileName);

            return path;
        }


        string CreateCGINCFile(DynamicRadialMasks.Enum.MaskShape _MaskShape, int _ShapeCount, DynamicRadialMasks.Enum.MaskType _MaskType, DynamicRadialMasks.Enum.MaskBlendMode _MaskBlendMode, int _MaskID, DynamicRadialMasks.Enum.MaskScope _MaskScope)
        {
            string filePath = GetDRMFilePath(Enum.FileExtension.cginc, _MaskShape, _ShapeCount, _MaskType, _MaskBlendMode, _MaskID, _MaskScope);
            if (File.Exists(filePath))
               return filePath;


            string[] cgincFile = GenerateCGINCFromTemplate(_MaskShape.ToString(), _ShapeCount.ToString(), _MaskType.ToString(), _MaskBlendMode.ToString(), _MaskID.ToString(), _MaskScope.ToString());


            string saveFolder = Path.GetDirectoryName(filePath);
            if (Directory.Exists(saveFolder) == false)
                Directory.CreateDirectory(saveFolder);

            File.WriteAllLines(filePath, cgincFile);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            return filePath;
        }
        void CreateSubGraphFile(DynamicRadialMasks.Enum.MaskShape _MaskShape, int _ShapeCount, DynamicRadialMasks.Enum.MaskType _MaskType, DynamicRadialMasks.Enum.MaskBlendMode _MaskBlendMode, int _MaskID, DynamicRadialMasks.Enum.MaskScope _MaskScope, Enum.FileExtension _Extention)
        {
            string templateFileLocation = GetTemplateFileLocation(_Extention, _MaskShape.ToString(), _MaskType.ToString(), _MaskBlendMode.ToString());

            string[] templateFileAllLines = ReadFileAllLines(templateFileLocation);
            if (templateFileAllLines == null || templateFileAllLines.Length == 0)
            {
                Debug.LogWarning("Template file not found: " + templateFileLocation);
                return;
            }

            string cgincFilePath = GetDRMFilePath(Enum.FileExtension.cginc, _MaskShape, _ShapeCount, _MaskType, _MaskBlendMode, _MaskID, _MaskScope);
            string cgincFileGUID = AssetDatabase.AssetPathToGUID(cgincFilePath);
            if (string.IsNullOrEmpty(cgincFileGUID))
            {
                Debug.LogWarning("CGINC file not found: " + cgincFilePath);
                return;
            }


            string[] subGraphFile = new string[templateFileAllLines.Length];

            for (int i = 0; i < templateFileAllLines.Length; i++)
            {
                subGraphFile[i] = templateFileAllLines[i].Replace("#SHAPE_BIG#", _MaskShape.ToString().ToUpper()).
                                                       Replace("#SHAPE_SMALL#", _MaskShape.ToString()).
                                                       Replace("#ARRAY_LENGTH#", _ShapeCount.ToString()).
                                                       Replace("#TYPE_BIG#", _MaskType.ToString().ToUpper()).
                                                       Replace("#TYPE_SMALL#", _MaskType.ToString()).
                                                       Replace("#BLEND_MODE_BIG#", _MaskBlendMode.ToString().ToUpper()).
                                                       Replace("#BLEND_MODE_SMALL#", _MaskBlendMode.ToString()).
                                                       Replace("#ID#", _MaskID.ToString()).
                                                       Replace("#SCOPE_BIG#", _MaskScope == DynamicRadialMasks.Enum.MaskScope.Local ? "LOCAL" : "GLOBAL").
                                                       Replace("#SCOPE_SMALL#", _MaskScope == DynamicRadialMasks.Enum.MaskScope.Local ? "Local" : "Global").
                                                       Replace("#CGINC_FILE_GUID#", cgincFileGUID);
            }



            string saveFolder = Path.Combine(EditorUtilities.GetThisAssetProjectPath(), "Shaders");
            saveFolder = Path.Combine(saveFolder, _Extention == Enum.FileExtension.asset ? "Amplify Shader Editor" : "Unity Shader Graph");
            saveFolder = Path.Combine(saveFolder, _MaskShape.ToString());

            if (Directory.Exists(saveFolder) == false)
                Directory.CreateDirectory(saveFolder);


            string saveLocalFileName = string.Format("DynamicRadialMasks_{0}_{1}_{2}_{3}_ID{4}_{5}.{6}", _MaskShape, _ShapeCount, _MaskType, _MaskBlendMode, _MaskID, _MaskScope, _Extention);
            saveLocalFileName = Path.Combine(saveFolder, saveLocalFileName);


            WriteFileAllLines(saveLocalFileName, subGraphFile);
        }

        string[] GenerateCGINCFromTemplate(string _MaskShape, string _ShapeCount, string _MaskType, string _MaskBlendMode, string _MaskID, string _MaskScope)
        {
            //Read template file
            string templateFileLocation = GetTemplateFileLocation(Enum.FileExtension.cginc, _MaskShape, _MaskType, _MaskBlendMode);

            string[] templateFileAllLines = ReadFileAllLines(templateFileLocation);
            if (templateFileAllLines == null || templateFileAllLines.Length == 0)
                return null;


            //Generate new cginc file
            string[] cgincFile = new string[templateFileAllLines.Length];

            for (int i = 0; i < templateFileAllLines.Length; i++)
            {
                if (templateFileAllLines[i].Contains("#FOR_LOOP#"))
                {
                    if (_ShapeCount != "1")
                        templateFileAllLines[i] = templateFileAllLines[i].Replace("#FOR_LOOP#", "[unroll]");
                    else
                        templateFileAllLines[i] = string.Empty;
                }

                cgincFile[i] = templateFileAllLines[i].Replace("#SHAPE_BIG#", _MaskShape.ToString().ToUpper()).
                                                       Replace("#SHAPE_SMALL#", _MaskShape.ToString()).
                                                       Replace("#ARRAY_LENGTH#", _ShapeCount.ToString()).
                                                       Replace("#TYPE_BIG#", _MaskType.ToString().ToUpper()).
                                                       Replace("#TYPE_SMALL#", _MaskType.ToString()).
                                                       Replace("#BLEND_MODE_BIG#", _MaskBlendMode.ToString().ToUpper()).
                                                       Replace("#BLEND_MODE_SMALL#", _MaskBlendMode.ToString()).
                                                       Replace("#ID#", _MaskID.ToString()).
                                                       Replace("#SCOPE_BIG#", _MaskScope == "Local" ? "LOCAL" : "GLOBAL").
                                                       Replace("#SCOPE_SMALL#", _MaskScope == "Local" ? "Local" : "Global").
                                                       Replace("#UNIFORM#", _MaskScope == "Local" ? string.Empty : "uniform ");
            }

            if (_MaskScope == "Global")
                cgincFile = cgincFile.Where(c => c.Contains("#REMOVE_FOR_GLOBAL#") == false).ToArray();
            else
                cgincFile = cgincFile.Select(c => c.Replace("#REMOVE_FOR_GLOBAL#", string.Empty)).ToArray();


            return cgincFile;
        }

        string[] ReadFileAllLines(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || File.Exists(filePath) == false)
                return null;

            return File.ReadAllLines(filePath);
        }
        void WriteFileAllLines(string filePath, string[] fileData)
        {
            try
            {
                File.WriteAllLines(filePath, fileData);
            }
            catch
            {
                Debug.LogWarning("Can not create file: " + Path.GetFileName(filePath) + "\nReason: Absolute file path length exceeds 259 character limit.\nSolution: Move project closer to the system root directory, making the path shorter.\n");
            }

        }
        void PingObject(string path)
        {
            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            if (obj == null)
            {
                //Try folder
                obj = AssetDatabase.LoadAssetAtPath(Path.GetDirectoryName(path), typeof(UnityEngine.Object));

                if (obj == null)
                    return;
            }


            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            UnityEditor.EditorGUIUtility.PingObject(obj);
        }


        void CallbackMenuShapesPopup(int value)
        {
            maskShape = (DynamicRadialMasks.Enum.MaskShape)value;

            Repaint();
        }
        void CallbackRecompileCGINCFiles()
        {
            string[] guids = AssetDatabase.FindAssets("Templates");
            foreach (var item in guids)
            {
                string templatesFolderPath = AssetDatabase.GUIDToAssetPath(item);
                if (templatesFolderPath.Contains("Amazing Assets") &&
                    templatesFolderPath.Contains("Dynamic Radial Masks") &&
                    templatesFolderPath.Contains("Shaders") &&
                    string.IsNullOrWhiteSpace(Path.GetExtension(templatesFolderPath)))
                {
                    string cgincFolder = Path.Combine(Path.GetDirectoryName(templatesFolderPath), "CGINC");

                    if (Directory.Exists(cgincFolder))
                    {
                        string[] cgincFiles = Directory.GetFiles(cgincFolder, "*.cginc", SearchOption.AllDirectories);

                        foreach (var drmFile in cgincFiles)
                        {
                            //          0          1   2     3        4     5    6
                            //DynamicRadialMasks_Torus_5_Advanced_Additive_ID1_Local.cginc
                            string[] data = Path.GetFileNameWithoutExtension(drmFile).Split('_');

                            string _MaskShape = data[1];
                            string _ShapeCount = data[2];
                            string _MaskType = data[3];
                            string _MaskBlendMode = data[4];
                            string _MaskID = data[5].Replace("ID", string.Empty);
                            string _MaskScope = data[6];


                            string[] cgincFile = GenerateCGINCFromTemplate(_MaskShape, _ShapeCount, _MaskType, _MaskBlendMode, _MaskID, _MaskScope);


                            File.WriteAllLines(drmFile, cgincFile);
                        }
                    }


                    AssetDatabase.Refresh();
                    break;
                }
            }

        }
        void CallbackMenuImportShaderGenerator()
        {
            string path = Path.Combine(EditorUtilities.GetThisAssetProjectPath(), "Editor", "Shader Compiler", "Shader Compiler.unitypackage");

            AssetDatabase.ImportPackage(path, false);
        }       

        int GetInstructionsCount(DynamicRadialMasks.Enum.MaskShape maskShape, int maskCount, DynamicRadialMasks.Enum.MaskType maskType, DynamicRadialMasks.Enum.MaskBlendMode maskBlendMode)
        {
            int iCount = 0;

            switch (maskShape)
            {
                case DynamicRadialMasks.Enum.MaskShape.Torus:
                    {
                        if(maskType == DynamicRadialMasks.Enum.MaskType.Advanced)
                        {
                            if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                                iCount = 14;
                            else
                                iCount = 16;
                        }
                        else
                        {
                            if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                                iCount = 11;
                            else
                                iCount = 13;
                        }
                    }
                    break;

                case DynamicRadialMasks.Enum.MaskShape.Tube:
                    if (maskType == DynamicRadialMasks.Enum.MaskType.Advanced)
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 12;
                        else
                            iCount = 14;
                    }
                    else
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 12;
                        else
                            iCount = 14;
                    }
                    break;

                case DynamicRadialMasks.Enum.MaskShape.HeightField:
                    if (maskType == DynamicRadialMasks.Enum.MaskType.Advanced)
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 13;
                        else
                            iCount = 15;
                    }
                    else
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 10;
                        else
                            iCount = 12;
                    }
                    break;

                case DynamicRadialMasks.Enum.MaskShape.Dot:
                    if (maskType == DynamicRadialMasks.Enum.MaskType.Advanced)
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 6;
                        else
                            iCount = 8;
                    }
                    else
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 6;
                        else
                            iCount = 8;
                    }
                    break;

                case DynamicRadialMasks.Enum.MaskShape.Shockwave:
                    if (maskType == DynamicRadialMasks.Enum.MaskType.Advanced)
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 13;
                        else
                            iCount = 15;
                    }
                    else
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 10;
                        else
                            iCount = 12;
                    }
                    break;

                case DynamicRadialMasks.Enum.MaskShape.Sonar:
                    if (maskType == DynamicRadialMasks.Enum.MaskType.Advanced)
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 15;
                        else
                            iCount = 17;
                    }
                    else
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 12;
                        else
                            iCount = 14;
                    }
                    break;

                case DynamicRadialMasks.Enum.MaskShape.Rings:
                    if (maskType == DynamicRadialMasks.Enum.MaskType.Advanced)
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 17;
                        else
                            iCount = 19;
                    }
                    else
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 14;
                        else
                            iCount = 16;
                    }
                    break;

                case DynamicRadialMasks.Enum.MaskShape.Ripple:
                    if (maskType == DynamicRadialMasks.Enum.MaskType.Advanced)
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 20;
                        else
                            iCount = 22;
                    }
                    else
                    {
                        if (maskBlendMode == DynamicRadialMasks.Enum.MaskBlendMode.Additive)
                            iCount = 17;
                        else
                            iCount = 19;
                    }
                    break;

                default:
                    break;
            }


            return iCount * maskCount;
        }
    }
}
