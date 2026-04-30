using NGAME;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(EC_NoCombat))]
public class EC_NoCombat_Drawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        Label result = new(property.FindPropertyRelative("Name").stringValue);
        result.tooltip = property.FindPropertyRelative("Description").stringValue;
        return result;
    }
}
