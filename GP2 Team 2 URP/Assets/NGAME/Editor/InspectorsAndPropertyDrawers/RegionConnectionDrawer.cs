using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NGAME.Editor
{
    [CustomPropertyDrawer(typeof(RegionConnectionData))]
    public class RegionConnectionDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            UnityEngine.UIElements.PopupWindow popup = new();
            popup.text = property.FindPropertyRelative("Name").stringValue;

            PropertyField type = new(property.FindPropertyRelative("TypeName"));
            type.Bind(property.serializedObject);
            type.SetEnabled(false);
            popup.Add(type);

            //PropertyField name = new(property.FindPropertyRelative("Name"));
            //name.Bind(property.serializedObject);
            //name.SetEnabled(false);

            PropertyField position = new(property.FindPropertyRelative("Position"));
            position.Bind(property.serializedObject);
            position.SetEnabled(false);
            popup.Add(position);

            PropertyField connectionType = new(property.FindPropertyRelative("ConnectionType"));
            connectionType.Bind(property.serializedObject);
            popup.Add(connectionType);

            SerializedProperty lockable = property.FindPropertyRelative("IsLockable");
            if (lockable.boolValue)
            {
                PropertyField startsLocked = new(property.FindPropertyRelative("StartsLocked"));
                startsLocked.Bind(property.serializedObject);
                popup.Add(startsLocked);

                PropertyField combatLocks = new(property.FindPropertyRelative("IsLockedDurringCombat"));
                combatLocks.Bind(property.serializedObject);
                popup.Add(combatLocks);

            }

            return popup;
        }
    }
}
