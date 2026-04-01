using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NGAME.Editor
{
    public class EditorExtensions : EditorWindow
    {

        public static void DrawProperties(SerializedProperty property, bool drawChildren)
        {
            string lastPropPath = string.Empty;

            foreach (SerializedProperty p in property)
            {
                if(!p.isArray || p.propertyType != SerializedPropertyType.Generic)
                {
                    if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath))
                    {
                        continue;
                    }
                    lastPropPath = p.propertyPath;
                    EditorGUILayout.PropertyField(p, drawChildren);
                    continue; //early skip to next item in foreach
                }

                // draw fold out for array property
                EditorGUILayout.BeginHorizontal();
                p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                EditorGUILayout.EndHorizontal();

                if (p.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    // recursively draw nested children's properties in list
                    DrawProperties(p, drawChildren);
                    EditorGUI.indentLevel--;
                }
                
            }
        }
    }
}