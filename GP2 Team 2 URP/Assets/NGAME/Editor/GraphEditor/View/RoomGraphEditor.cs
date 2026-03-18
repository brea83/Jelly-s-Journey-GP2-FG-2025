using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace NGAME.Editor
{
    public class RoomGraphEditor : EditorWindow
    {
        private RoomGraph _graph;
        private RoomGraphView _graphView;
        private InspectorView _inspectorView;

        //toolbar buttons
        private UnityEngine.UIElements.Button _newGraphButton;
        private UnityEngine.UIElements.Button _saveGraphButton;
        //[SerializeField]
        //private VisualTreeAsset _VisualTreeAsset = default;



        [MenuItem("NGAME/Editor")]
        public static void OpenWindow()
        {
            RoomGraphEditor window = GetWindow<RoomGraphEditor>();
            window.titleContent = new GUIContent("RoomGraphEditor");
            //window.saveChangesMessage = "This Graph has unsaved changes. Would you like to save?";
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
            }
            //StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Toolkit/Styles/Editor/RoomGraphEditor.uss");

            
            _graphView = root.Q<RoomGraphView>();
            _inspectorView = root.Q<InspectorView>();

            _graphView.OnNodeSelected = OnNodeSelectionChanged;
            OnSelectionChange();

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