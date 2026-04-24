using UnityEngine;
using NGAME.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using NGAME;

[CustomPropertyDrawer(typeof(RegionConnectionData))]
public class DoorPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        UnityEngine.UIElements.PopupWindow popup = new();
        popup.text = property.FindPropertyRelative("Name").stringValue;

        PropertyField type = new(property.FindPropertyRelative("TypeName"));
        type.Bind(property.serializedObject);
        type.SetEnabled(false);

        //PropertyField name = new(property.FindPropertyRelative("Name"));
        //name.Bind(property.serializedObject);
        //name.SetEnabled(false);

        PropertyField position = new(property.FindPropertyRelative("Position"));
        position.Bind(property.serializedObject);
        position.SetEnabled(false);

        PropertyField connectionType = new(property.FindPropertyRelative("ConnectionType"));
        connectionType.Bind(property.serializedObject);

        PropertyField lockableToggle = new(property.FindPropertyRelative("IsLockable"));
        lockableToggle.Bind(property.serializedObject);

        popup.Add(type);
        //popup.Add(name);
        popup.Add(position);

        popup.Add(connectionType);
        popup.Add(lockableToggle);

        return popup;
    }
}
