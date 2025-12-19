// Dynamic Radial Masks <https://u3d.as/1w0H>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace AmazingAssets.DynamicRadialMasks.Editor
{
    [CustomEditor(typeof(DRMLiveObjectsPool))]
    public class DRMLiveObjectsPoolEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            DRMLiveObjectsPool t = (target as DRMLiveObjectsPool);

            //Label
            EditorGUILayout.LabelField("Objects in pool", EditorStyles.miniBoldLabel);



            Rect drawRect = EditorGUILayout.GetControlRect();
            drawRect.width -= 50;

            int objectsCount = 0;
            if (t == null || t.DRMController == null)
            {
                EditorGUI.ProgressBar(drawRect, 0, "0");
            }
            else
            {
                if (t.DRMLiveObjects == null)
                    t.DRMLiveObjects = new List<DRMLiveObject>();

                objectsCount = t.DRMLiveObjects.Count;

                float value = (float)objectsCount / t.DRMController.count;

                EditorGUI.ProgressBar(drawRect, value, objectsCount + "/" + t.DRMController.count);
            }

            

            using (new EditorGUIHelper.GUIEnabled(objectsCount > 0))
            {
                if (GUI.Button(new Rect(drawRect.xMax + 2, drawRect.yMin, 48, drawRect.height), "Clear"))
                {
                    t.ClearPool();
                }
            }
        }
    }
}
