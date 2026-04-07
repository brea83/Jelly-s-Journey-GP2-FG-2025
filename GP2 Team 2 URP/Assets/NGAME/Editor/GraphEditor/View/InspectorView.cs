using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
namespace NGAME.Editor
{
    [UxmlElement]
    public partial class InspectorView : VisualElement
    {
        public InspectorView() { }
        private UnityEditor.Editor _editor;
        public void UpdateSelection(NodeView nodeView)
        {
            Clear();
            Object.DestroyImmediate(_editor);
             _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            
            var container = _editor.CreateInspectorGUI();

            Add(container);


            EditorApplication.delayCall += BindWaveObjectFieldChanges;

        }

        public void Repaint()
        {
            if(_editor != null)
            {
                OnWavesPropertyChanged();
                this.MarkDirtyRepaint();
            }
        }

        private void BindWaveObjectFieldChanges()
        {
            EditorApplication.delayCall -= BindWaveObjectFieldChanges;
            PropertyField wavesField = this.Q<PropertyField>("WavesField");
            if (wavesField == null)
                return;
            ListView wavesList = wavesField.Q<ListView>();

            if (wavesList == null)
                return;
            wavesList.itemsSourceChanged += OnWavesPropertyChanged;
            

            //foreach (VisualElement child in )
            //{
            //    PropertyField waveDataObjectInputField = child.Q<PropertyField>("WaveDataObjectField");
            //    if (waveDataObjectInputField != null)
            //    {
            //        waveDataObjectInputField.RegisterValueChangeCallback(OnWavesPropertyChanged);
            //    }
            //}
        }

        private void OnWavesPropertyChanged(SerializedPropertyChangeEvent evt)
        {
            OnWavesPropertyChanged();
        }

        private void OnWavesPropertyChanged()
        {
            
            SerializedProperty wavesList = _editor.serializedObject.FindProperty("Waves");
            PropertyField oldField = this.Q<PropertyField>("WavesField");
            VisualElement parentPanel = oldField.parent;
            oldField.RemoveFromHierarchy();

            PropertyField waves = new PropertyField(wavesList);
            waves.Bind(_editor.serializedObject);
            waves.name = "WavesField";

            parentPanel.Add(waves);

            BindWaveObjectFieldChanges();
        }
    }
}