using System.ComponentModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NGAME.Editor
{
    [CustomPropertyDrawer(typeof(SOWaveData))]
    public class SOWaveDataPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            VisualElement container = new();

            var respawnField = new PropertyField(property.FindPropertyRelative("m_RespawnsOnBacktrack"));
            var secondsBtwnSpawns = new PropertyField(property.FindPropertyRelative("m_SecBtwnSpawns"));

            container.Add(respawnField);
            container.Add(secondsBtwnSpawns);
        }
    }
}
