using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Overlays;

namespace NGAME.Editor
{
    public class RoomGraphEditor : GraphViewEditorWindow
    {
        public override IEnumerable<GraphView> graphViews 
        { 
            get 
            { 
                List<GraphView> graphs = new()
                {
                    _graphView
                }; 
                return graphs; 
            } 
        }
        private RoomGraph _graph;
        private RoomGraphView _graphView;
        private NodeInspectorWindow _inspectorView;

        private GraphViewBlackboardWindow _blackboardWindow;
        private GraphViewMinimapWindow _minimapWindow;

        //toolbar buttons
        private UnityEngine.UIElements.Button _newGraphButton;
        private UnityEngine.UIElements.Button _saveGraphButton;
        //[SerializeField]
        //private VisualTreeAsset _VisualTreeAsset = default;

        private StyleSheet m_Style;


        [MenuItem("NGAME/Editor")]
        public static void OpenWindow()
        {
            //RoomGraphEditor window = GetWindow<RoomGraphEditor>();
            //window.titleContent = new GUIContent("RoomGraphEditor");
            //window.saveChangesMessage = "This Graph has unsaved changes. Would you like to save?";
            List<EditorWindow> windows = ShowGraphViewWindowWithTools<RoomGraphEditor>();
            RoomGraphEditor editor = windows[0] as RoomGraphEditor;
            editor._blackboardWindow = windows[1] as GraphViewBlackboardWindow;
            editor._minimapWindow = windows[2] as GraphViewMinimapWindow;

            System.Type[] dockNextToType = new System.Type[1] { typeof(GraphViewBlackboardWindow) };

            editor._inspectorView = GetWindow<NodeInspectorWindow>("Node Inspector", dockNextToType);
            
            if(editor._graphView != null)
            {
                editor._inspectorView.SelectGraphViewFromWindow(editor, editor._graphView);
            }
            editor._inspectorView.ShowTab();

        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            //Import UXML
            VisualTreeAsset visualTree;
            string[] treeGuids = AssetDatabase.FindAssets("NGAMEEditor");
            if (treeGuids.Length > 0)
            {
                visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(treeGuids[0]));
                visualTree.CloneTree(root);
            }

            //VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/Panels/Editor/GraphView Tools/RoomGraphEditor.uxml");



            StyleSheet styleSheet;
            string[] guids = AssetDatabase.FindAssets("NGAMEEditorStyle  t:StyleSheet");
            if (guids.Length > 0)
            {
                styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]));
                root.styleSheets.Add(styleSheet);
                m_Style = styleSheet;
            }
            //StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Toolkit/Styles/Editor/RoomGraphEditor.uss");

            
            _graphView = root.Q<RoomGraphView>();
            

            _graphView.OnNodeSelected = OnNodeSelectionChanged;
            _graphView.OnNodeValuesChanged = OnNodeValuesChanged;
            OnSelectionChange();

            if(m_Style != null)
            {
                _graphView.styleSheets.Add(m_Style);
                //_inspectorView.styleSheets.Add(m_Style);
            }

            _newGraphButton = root.Q<Button>("NewGraphButton");
            _newGraphButton.clicked += OnNewGraphClicked;
            _saveGraphButton = root.Q<Button>("SaveGraphButton");
            _saveGraphButton.clicked += OnSaveGraphClicked;
        }

        private void OnSelectionChange()
        {
            RoomGraph roomGraph = Selection.activeObject as RoomGraph;
            if(roomGraph == null)
            {
                return;
            }

            if (_graph != null)
            {
                AssetDatabase.SaveAssetIfDirty(_graph);
            }
            _graph = roomGraph;
            _graphView.PopulateView(roomGraph);
        }

        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            _inspectorView.UpdateSelection(nodeView);
        }

        private void OnNodeValuesChanged(NodeView nodeView)
        {
            _inspectorView.Repaint(nodeView);
        }

        private void OnNewGraphClicked()
        {
            _graph = CreateInstance< RoomGraph>();
            _graphView.PopulateView(_graph);
            string path = EditorUtility.SaveFilePanelInProject("New Graph Asset", "NewGraph", "asset",
            "Please enter a file name");
            if (path.Length != 0)
            {
                AssetDatabase.CreateAsset(_graph, path);
            }
        }

        private void OnSaveGraphClicked()
        {
            AssetDatabase.SaveAssets();
        }
    }
}