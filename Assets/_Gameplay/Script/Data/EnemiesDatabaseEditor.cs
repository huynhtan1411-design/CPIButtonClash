//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEngine;

//namespace CLHoma.Combat
//{
//    [CustomEditor(typeof(EnemiesDatabase))]
//    public class EnemiesDatabaseEditor : Editor
//    {
//        private EnemiesDatabase database;
//        private SerializedProperty enemiesProperty;

//        private void OnEnable()
//        {
//            database = (EnemiesDatabase)target;
//            enemiesProperty = serializedObject.FindProperty("enemies");
//        }

//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();

//            EditorGUILayout.LabelField("Enemies Database", EditorStyles.boldLabel);

//            if (GUILayout.Button("Add Enemy"))
//            {
//                AddEnemy();
//            }

//            EditorGUILayout.Space(5);

//            for (int i = 0; i < enemiesProperty.arraySize; i++)
//            {
//                SerializedProperty enemyProperty = enemiesProperty.GetArrayElementAtIndex(i);
//                SerializedProperty enemyType = enemyProperty.FindPropertyRelative("enemyType");
//                SerializedProperty enemyTier = enemyProperty.FindPropertyRelative("enemyTier");
//                SerializedProperty enemyElemental = enemyProperty.FindPropertyRelative("elementType");
//                SerializedProperty prefab = enemyProperty.FindPropertyRelative("prefab");
//                SerializedProperty stats = enemyProperty.FindPropertyRelative("stats");

//                EditorGUILayout.BeginVertical(GUI.skin.box);
//                EditorGUILayout.PropertyField(enemyType);
//                EditorGUILayout.PropertyField(enemyTier);
//                EditorGUILayout.PropertyField(prefab);
//                EditorGUILayout.PropertyField(stats);
//                EditorGUILayout.PropertyField(enemyElemental);

//                if (GUILayout.Button("Remove Enemy"))
//                {
//                    RemoveEnemy(i);
//                }

//                EditorGUILayout.EndVertical();
//                EditorGUILayout.Space(5);
//            }
//            serializedObject.ApplyModifiedProperties();
//        }

//        private void AddEnemy()
//        {
//            enemiesProperty.arraySize++;
//            serializedObject.ApplyModifiedProperties();
//        }

//        private void RemoveEnemy(int index)
//        {
//            enemiesProperty.DeleteArrayElementAtIndex(index);
//            serializedObject.ApplyModifiedProperties();
//        }
//    }
//}
//#endif
