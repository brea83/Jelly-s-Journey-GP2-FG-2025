using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace GraphViewTools {
    public class RoomGraphEditor : EditorWindow
    {
        private RoomGraph _graph;
        private RoomGraphView _graphView;
        private InspectorView _inspectorView;
        //[SerializeField]
        //private VisualTreeAsset _VisualTreeAsset = default;

        

        [MenuItem("RoomGraphEditor/Editor")]
        public static void OpenWindow()
        {
            RoomGraphEditor window = GetWindow<RoomGraphEditor>();
            window.titleContent = new GUIContent("RoomGraphEditor");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            //Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/Panels/Editor/GraphView Tools/RoomGraphEditor.uxml");
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Toolkit/Styles/Editor/RoomGraphEditor.uss");
            root.styleSheets.Add(styleSheet);

            
            _graphView = root.Q<RoomGraphView>();
            _inspectorView = root.Q<InspectorView>();

            _graphView.OnNodeSelected = OnNodeSelectionChanged;
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            RoomGraph roomGraph = Selection.activeObject as RoomGraph;
            if (roomGraph != null)
            {
                _graph = roomGraph;
                _graphView.PopulateView(roomGraph);
            }
        }

        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            _inspectorView.UpdateSelection(nodeView);
        }
    }
}