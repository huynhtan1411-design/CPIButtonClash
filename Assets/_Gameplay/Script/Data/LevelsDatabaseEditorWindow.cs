#if UNITY_EDITOR

using CLHoma.LevelSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LevelsDatabaseEditorWindow : EditorWindow
{
    private LevelsDatabase database;
    private SerializedObject serializedObject;
    private int selectedLevelIndex = 0;
    private const string gameplayScenePath = "Assets/_Gameplay/Scenes/Loading.unity";
    [MenuItem("Tools/Levels Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelsDatabaseEditorWindow>("Levels Database Editor");
    }

    private void OnGUI()
    {
        database = (LevelsDatabase)EditorGUILayout.ObjectField("Database", database, typeof(LevelsDatabase), false);
        if (database == null) return;

        if (serializedObject == null || serializedObject.targetObject != database)
        {
            serializedObject = new SerializedObject(database);
        }

        serializedObject.Update();

        SerializedProperty levelsProp = serializedObject.FindProperty("levels");

        if (levelsProp.arraySize > 0)
        {
            selectedLevelIndex = EditorGUILayout.Popup("Select Level", selectedLevelIndex, GetLevelNames(levelsProp.arraySize));
            if (selectedLevelIndex >= levelsProp.arraySize) selectedLevelIndex = levelsProp.arraySize - 1;

            SerializedProperty levelProp = levelsProp.GetArrayElementAtIndex(selectedLevelIndex);

            SerializedProperty totalLevelTimeProp = levelProp.FindPropertyRelative("totalLevelTime");
            EditorGUILayout.PropertyField(totalLevelTimeProp, new GUIContent("Total Level Time"));



            SerializedProperty intervalEachEnemy = levelProp.FindPropertyRelative("intervalEachEnemy");
            EditorGUILayout.PropertyField(intervalEachEnemy, new GUIContent("Interval Each Enemy"));



            SerializedProperty intervalEachRound = levelProp.FindPropertyRelative("intervalEachRound");
            EditorGUILayout.PropertyField(intervalEachRound, new GUIContent("Interval Each Round"));

            SerializedProperty roundDatasProp = levelProp.FindPropertyRelative("roundDatas");
            EditorGUILayout.PropertyField(roundDatasProp, new GUIContent("Round Data"), true);
        }

        if (GUILayout.Button("Add Level"))
        {
            levelsProp.arraySize++;
            selectedLevelIndex = levelsProp.arraySize - 1;
        }
        if (GUILayout.Button("Test level"))
        {
            PlayerPrefs.SetInt("CLHoma_Level", selectedLevelIndex);
            try
            {
                EditorSceneManager.OpenScene(gameplayScenePath);
                EditorApplication.isPlaying = true;
                Debug.Log($"Running Gameplay Scene at level {selectedLevelIndex + 1}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception: {e}");
            }
        }
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(database);
    }

    private string[] GetLevelNames(int count)
    {
        string[] names = new string[count];
        for (int i = 0; i < count; i++)
        {
            names[i] = "Level " + (i + 1);
        }
        return names;
    }
}
#endif
