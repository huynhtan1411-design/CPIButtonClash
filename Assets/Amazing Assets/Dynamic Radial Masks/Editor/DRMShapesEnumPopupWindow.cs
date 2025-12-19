// Dynamic Radial Masks <https://u3d.as/1w0H>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;
using UnityEditor;


namespace AmazingAssets.DynamicRadialMasks.Editor
{
    internal class DRMShapesEnumPopupWindow : PopupWindowContent
    {
        public delegate void Callback(int value);
        Callback callbakcMethod;


        Vector2 drawRectSize = new Vector2(68, 86); 
        int textureDrawSize = 64;
        int currentSelecion = 0;
        int initialSelection = 0;


        public DRMShapesEnumPopupWindow(int selecion, Callback callbakc)
        {
            this.currentSelecion = selecion;
            this.initialSelection = selecion;
            this.callbakcMethod = callbakc;
        }

        public override void OnGUI(Rect rect)
        {
            for (int i = 0; i < DRMEditorResources.shapeNames.Length; i++)
            {
                Rect drawRect = new Rect(i * drawRectSize.x, 2, drawRectSize.x, drawRectSize.y);                              

                if (currentSelecion == i)
                    EditorGUI.DrawRect(drawRect, GUI.skin.settings.selectionColor);


                EditorGUI.LabelField(new Rect(drawRect.xMin, drawRect.yMin, drawRect.width, 18), DRMEditorResources.shapeNames[i], currentSelecion == i ? DRMEditorResources.LabelMiniCenterBold : DRMEditorResources.LabelMiniCenter);


                drawRect = new Rect(drawRect.xMin + 2, drawRect.yMax - textureDrawSize - 4, textureDrawSize, textureDrawSize);
                GUI.DrawTexture(drawRect, DRMEditorResources.IconShapeTexture(i));
                if (drawRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    currentSelecion = i;

                    if (callbakcMethod != null)
                        callbakcMethod(currentSelecion);

                    if (Event.current.clickCount > 1)
                    {
                        editorWindow.Close();
                    }
                    else
                    {
                        editorWindow.Repaint();
                    }
                }
            }


            if(Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.LeftArrow)
                    currentSelecion -= 1;

                if (Event.current.keyCode == KeyCode.RightArrow)
                    currentSelecion += 1;

                if (Event.current.keyCode == KeyCode.Return)
                    editorWindow.Close();

                if (Event.current.keyCode == KeyCode.Escape)
                {
                    currentSelecion = initialSelection;
                    editorWindow.Close();
                }



                currentSelecion = Mathf.Clamp(currentSelecion, 0, DRMEditorResources.shapeNames.Length - 1);

                if (callbakcMethod != null)
                    callbakcMethod(currentSelecion);

                editorWindow.Repaint();
            }
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(drawRectSize.x * 8, drawRectSize.y);
        }
    }
}
