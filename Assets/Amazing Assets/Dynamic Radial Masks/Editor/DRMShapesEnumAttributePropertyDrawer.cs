// Dynamic Radial Masks <https://u3d.as/1w0H>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;
using UnityEditor;


namespace AmazingAssets.DynamicRadialMasks.Editor
{
    [CustomPropertyDrawer(typeof(DRMShapesEnumAttribute))]
    public class DRMShapesEnumAttributePropertyDrawer : PropertyDrawer
    {
        static SerializedObject serializedObject;
        static SerializedProperty serializedProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label);

            Rect enumDrawRect = position;
            enumDrawRect.xMin += UnityEditor.EditorGUIUtility.labelWidth + 2;
            if(GUI.Button(enumDrawRect, DRMEditorResources.shapeNames[property.enumValueIndex], EditorStyles.popup))
            {
                serializedObject = property.serializedObject;
                serializedProperty = property;

                PopupWindow.Show(enumDrawRect, new DRMShapesEnumPopupWindow(property.enumValueIndex, Callback));
            }            
        }

        void Callback(int value)
        {
            serializedProperty.enumValueIndex = value;

            serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
