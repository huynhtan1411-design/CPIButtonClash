#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace WD
{
    public class BuildingConfigEditor : EditorWindow
    {
        private Vector2 scrollPosition;
        private BuildingData selectedConfig;
        private List<BuildingData> allConfigs = new List<BuildingData>();
        private string searchText = "";
        private bool showPreview = true;
        private float previewSize = 200f;
        private bool[] foldouts;

        [MenuItem("Tools/Button Clash/Building Config Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildingConfigEditor>("Building Config");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            RefreshConfigList();
        }

        private void RefreshConfigList()
        {
            allConfigs.Clear();
            string[] guids = AssetDatabase.FindAssets("t:BuildingData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BuildingData config = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
                if (config != null)
                {
                    allConfigs.Add(config);
                }
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space();

            if (allConfigs.Count == 0)
            {
                DrawNoConfigsMessage();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            
            // Left panel - Config List
            DrawConfigList();

            // Right panel - Config Editor
            if (selectedConfig != null)
            {
                DrawConfigEditor();
            }
            else
            {
                DrawNoSelectionMessage();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RefreshConfigList();
            }

            if (GUILayout.Button("Create New", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                CreateNewConfig();
            }

            if (selectedConfig != null && GUILayout.Button("Duplicate", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                DuplicateConfig();
            }

            GUILayout.FlexibleSpace();
            
            showPreview = EditorGUILayout.ToggleLeft("Show Preview", showPreview, GUILayout.Width(100));
            
            searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawConfigList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            EditorGUILayout.LabelField("Building Configs", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var config in allConfigs)
            {
                if (string.IsNullOrEmpty(searchText) || config.name.ToLower().Contains(searchText.ToLower()))
                {
                    bool isSelected = selectedConfig == config;
                    bool newSelected = EditorGUILayout.ToggleLeft(config.name, isSelected);
                    
                    if (newSelected != isSelected)
                    {
                        selectedConfig = newSelected ? config : null;
                        if (selectedConfig != null && selectedConfig.levelConfigs != null)
                        {
                            foldouts = new bool[selectedConfig.levelConfigs.Length];
                        }
                        GUI.FocusControl(null);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawConfigEditor()
        {
            EditorGUILayout.BeginVertical();
            
            // Preview section
            if (showPreview && selectedConfig.levelConfigs != null && selectedConfig.levelConfigs.Length > 0)
            {
                DrawPreview();
            }

            // Base Properties
            EditorGUILayout.LabelField("Base Properties", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var newNameBase = EditorGUILayout.TextField("Name Base", selectedConfig.nameBase);
            var newLevelIndexUnlock = EditorGUILayout.IntField("Level Index Unlock", selectedConfig.LevelIndexUnlock);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedConfig, "Change Base Properties");
                selectedConfig.nameBase = newNameBase;
                selectedConfig.LevelIndexUnlock = newLevelIndexUnlock;
                EditorUtility.SetDirty(selectedConfig);
            }

            EditorGUILayout.Space(10);

            // Base Stats
            EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var newHealthBase = EditorGUILayout.FloatField("Health Base", selectedConfig.HealthBase);
            var newAttackDamageBase = EditorGUILayout.FloatField("Attack Damage Base", selectedConfig.AttackDamageBase);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedConfig, "Change Base Stats");
                selectedConfig.HealthBase = newHealthBase;
                selectedConfig.AttackDamageBase = newAttackDamageBase;
                EditorUtility.SetDirty(selectedConfig);
            }

            EditorGUILayout.Space(10);

            // Type selection
            EditorGUI.BeginChangeCheck();
            var newType = (BuildingType)EditorGUILayout.EnumPopup("Building Type", selectedConfig.Type);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedConfig, "Change Building Type");
                selectedConfig.Type = newType;
                EditorUtility.SetDirty(selectedConfig);
            }

            // Prefab field
            EditorGUI.BeginChangeCheck();
            var newPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", selectedConfig.Prefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedConfig, "Change Building Prefab");
                selectedConfig.Prefab = newPrefab;
                EditorUtility.SetDirty(selectedConfig);
            }

            EditorGUILayout.Space(10);

            // Level Configurations
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level Configurations", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Level", GUILayout.Width(100)))
            {
                AddNewLevel();
            }
            EditorGUILayout.EndHorizontal();

            if (selectedConfig.levelConfigs == null)
            {
                selectedConfig.levelConfigs = new BuildingLevelData[0];
            }

            for (int i = 0; i < selectedConfig.levelConfigs.Length; i++)
            {
                DrawLevelConfig(i);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawLevelConfig(int index)
        {
            if (foldouts == null || foldouts.Length <= index)
            {
                System.Array.Resize(ref foldouts, Mathf.Max(index + 1, selectedConfig.levelConfigs.Length));
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Header with level number and delete button
            EditorGUILayout.BeginHorizontal();
            foldouts[index] = EditorGUILayout.Foldout(foldouts[index], $"Level {index + 1}", true);
            
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete Level", 
                    $"Are you sure you want to delete Level {index + 1}?", "Yes", "No"))
                {
                    DeleteLevel(index);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (foldouts[index])
            {
                EditorGUI.indentLevel++;
                var levelData = selectedConfig.levelConfigs[index];

                // General Section
                EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                levelData.name = EditorGUILayout.TextField("Name", levelData.name);
                levelData.icon = (Sprite)EditorGUILayout.ObjectField("Icon", levelData.icon, typeof(Sprite), false);
                levelData.towerModel = (GameObject)EditorGUILayout.ObjectField("Tower Model", levelData.towerModel, typeof(GameObject), false);
                levelData.description = EditorGUILayout.TextArea(levelData.description, GUILayout.Height(60));
                EditorGUILayout.Space(5);

                // Stats Section
                EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
                levelData.percentAddHealth = EditorGUILayout.FloatField("PercentAddHealth", levelData.percentAddHealth);
                levelData.percentAddDamage = EditorGUILayout.FloatField("PercentAddDamage", levelData.percentAddDamage);
                levelData.attackRange = EditorGUILayout.FloatField("Attack Range", levelData.attackRange);
                levelData.attackSpeed = EditorGUILayout.FloatField("Attack Speed", levelData.attackSpeed);
                levelData.maxTargets = EditorGUILayout.IntField("Max target", levelData.maxTargets);

                EditorGUILayout.Space(5);

                // Upgrade & Unlock Section
                EditorGUILayout.LabelField("Upgrade & Unlock", EditorStyles.boldLabel);
                levelData.upgradeCost = EditorGUILayout.IntField("Upgrade Cost", levelData.upgradeCost);
                levelData.levelUnlockCondition = EditorGUILayout.IntField("Level Unlock Condition", levelData.levelUnlockCondition);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(selectedConfig);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawPreview()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            
            previewSize = EditorGUILayout.Slider("Preview Size", previewSize, 100f, 400f);
            EditorGUILayout.Space();

            var levelData = selectedConfig.levelConfigs[0]; // Preview first level
            if (levelData != null)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Icon preview
                if (levelData.icon != null)
                {
                    var iconRect = EditorGUILayout.GetControlRect(GUILayout.Width(previewSize), GUILayout.Height(previewSize));
                    EditorGUI.DrawPreviewTexture(iconRect, levelData.icon.texture);
                }
                
                // Model preview (if available)
                if (levelData.towerModel != null)
                {
                    var modelPreviewRect = EditorGUILayout.GetControlRect(GUILayout.Width(previewSize), GUILayout.Height(previewSize));
                    EditorGUI.ObjectField(modelPreviewRect, levelData.towerModel, typeof(GameObject), false);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawNoConfigsMessage()
        {
            EditorGUILayout.HelpBox("No Building Configs found in the project. Click 'Create New' to create one.", MessageType.Info);
        }

        private void DrawNoSelectionMessage()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("Select a Building Config from the list to edit.", MessageType.Info);
            EditorGUILayout.EndVertical();
        }

        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Building Config",
                "NewBuildingConfig",
                "asset",
                "Please enter a name for the new Building Config"
            );

            if (!string.IsNullOrEmpty(path))
            {
                BuildingData newConfig = CreateInstance<BuildingData>();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                RefreshConfigList();
                selectedConfig = newConfig;
            }
        }

        private void DuplicateConfig()
        {
            if (selectedConfig == null) return;

            string path = EditorUtility.SaveFilePanelInProject(
                "Duplicate Building Config",
                $"{selectedConfig.name}_Copy",
                "asset",
                "Please enter a name for the duplicated Building Config"
            );

            if (!string.IsNullOrEmpty(path))
            {
                BuildingData newConfig = Instantiate(selectedConfig);
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                RefreshConfigList();
                selectedConfig = newConfig;
            }
        }

        private void AddNewLevel()
        {
            Undo.RecordObject(selectedConfig, "Add Level");
            
            var newLevels = new BuildingLevelData[selectedConfig.levelConfigs.Length + 1];
            selectedConfig.levelConfigs.CopyTo(newLevels, 0);
            newLevels[newLevels.Length - 1] = new BuildingLevelData();
            
            selectedConfig.levelConfigs = newLevels;
            System.Array.Resize(ref foldouts, newLevels.Length);
            foldouts[newLevels.Length - 1] = true; // Auto-expand new level
            
            EditorUtility.SetDirty(selectedConfig);
        }

        private void DeleteLevel(int index)
        {
            Undo.RecordObject(selectedConfig, "Delete Level");
            
            var newLevels = new BuildingLevelData[selectedConfig.levelConfigs.Length - 1];
            for (int i = 0, j = 0; i < selectedConfig.levelConfigs.Length; i++)
            {
                if (i != index)
                {
                    newLevels[j] = selectedConfig.levelConfigs[i];
                    j++;
                }
            }
            
            selectedConfig.levelConfigs = newLevels;
            System.Array.Resize(ref foldouts, newLevels.Length);
            
            EditorUtility.SetDirty(selectedConfig);
        }
    }
} 
#endif