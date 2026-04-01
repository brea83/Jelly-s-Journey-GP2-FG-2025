using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
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

        }
    }
}