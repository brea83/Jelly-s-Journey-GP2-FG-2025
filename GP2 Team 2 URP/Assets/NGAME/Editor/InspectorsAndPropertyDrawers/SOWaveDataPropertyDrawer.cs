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
            if(property.objectReferenceValue == null)
            {
                //Debug.Log("skipping property drawer fro null SOWaveData");
                return;
            }

            SOWaveData wave = property.objectReferenceValue as SOWaveData;
            if (wave == null)
            {
                //Debug.Log("object refrence was not null, but was not convertable to SOWaveData, skipping property drawer");
                return;
            }

            var editor = UnityEditor.Editor.CreateEditor(wave);
            IMGUIContainer container = new IMGUIContainer(() => { editor.OnInspectorGUI(); });


        }


        //public override VisualElement CreatePropertyGUI(SerializedProperty property)
        //{
        //    var popup = new UnityEngine.UIElements.PopupWindow();

        //    popup.text = "Wave Data";

        //    //SOWaveData data = ScriptableObject.CreateInstance<SOWaveData>(property.objectReferenceValue);
        //    var editor = UnityEditor.Editor.CreateEditor(property.objectReferenceValue);
        //    //editor.OnInspectorGUI();
        //    IMGUIContainer container = new IMGUIContainer(() => { editor.OnInspectorGUI(); });
        //    //VisualElement container = editor.CreateInspectorGUI();


        //    //var respawnField = new PropertyField(property.FindPropertyRelative("m_RespawnsOnBacktrack"));
        //    //var secondsBtwnSpawns = new PropertyField(property.FindPropertyRelative("m_SecBtwnSpawns"));

        //    //container.Add(respawnField);
        //    //container.Add(secondsBtwnSpawns);

        //    popup.Add(container);
        //    return popup;
        //}
    }
}
