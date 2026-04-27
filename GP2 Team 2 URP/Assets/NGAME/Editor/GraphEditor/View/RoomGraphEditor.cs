using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
        private List<SceneData> m_sceneData = new List<SceneData>();
        private Dictionary<string, Texture2D> m_ScenePreviewLookup = new();

        private RoomGraphView _graphView;
        private NodeInspectorView _inspectorView;
        private MiniMap _minimapWindow;

        private Button m_ToggleMinimapButton;
        private Button m_ToggleInspectorButton;

        //toolbar buttons
        private UnityEditor.UIElements.ToolbarMenu m_FileMenu;

        private StyleSheet m_Style;

        private EdgeData m_PlaymodeEntranceRequest;
        private string m_PlaymodeGraphPath;
        private bool m_PlaymodeRequestSent;


        [MenuItem("NGAME/Editor")]
        public static void OpenWindow()
        {
            RoomGraphEditor window = GetWindow<RoomGraphEditor>();
            window.titleContent = new GUIContent("RoomGraphEditor");
            window.saveChangesMessage = "This Graph has unsaved changes. Would you like to save?";
        }

        private void OnEnable()
        {
            //Debug.Log("NGAME Window OnEnable");
            //EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            //SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            //EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
           // SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            //EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            //SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
        {
            //StringBuilder message = new();
            //message.Append("SceneManager.activeSceneChanged: curent scene (" + currentScene.name + "), ");
            //message.Append("next scene (" + nextScene.name + ")");
            //Debug.Log(message.ToString());

            if(m_PlaymodeEntranceRequest != null && nextScene.name == m_PlaymodeEntranceRequest.DestinationSceneName)
                InitRuntimeGraph(nextScene);
        }
        //private void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        //{
        //    //Debug.Log("SceneManager.sceneLoaded: " + loadedScene.name);
        //}
        //private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        //{
        //    //Debug.Log("PlayMode State Change: " + stateChange.ToString());
        //    //Debug.Log("NGAME Editor.m_PlaymodeEntranceRequest: " + m_PlaymodeEntranceRequest);
        //}

        public void RegisterPlayModeRequest(SceneAsset requestedScene, EdgeData entrance)
        {
            m_PlaymodeEntranceRequest = entrance;
            m_PlaymodeGraphPath = AssetDatabase.GetAssetPath(_graph);
            m_PlaymodeRequestSent = false;
            Debug.Log("NGAME Editor recieved request to enter: " + entrance);

            if (requestedScene == null)
            {
                Debug.Log("Could not start Playmode from graph because could not find Scene " + entrance.DestinationSceneName);
                return;
            }

            EditorSceneManager.playModeStartScene = requestedScene;

            EditorApplication.EnterPlaymode();
        }

        protected void InitRuntimeGraph(Scene loadedScene)
        {
            GameObject[] rootObjects = loadedScene.GetRootGameObjects();
            MapGraphRuntime runtimeGraph = FindFirstObjectByType<MapGraphRuntime>();

            if (runtimeGraph == null || m_PlaymodeEntranceRequest == null)
                return;
            
            if(!m_PlaymodeRequestSent)
            {
                m_PlaymodeRequestSent = true;
                EditorSceneManager.playModeStartScene = null;
                runtimeGraph.TryEnterRoomFromGraph(m_PlaymodeEntranceRequest, _graph);
                m_PlaymodeEntranceRequest = null;
            }
        }

        public void CreateGUI()
        {
            ReadCurrentSceneData();

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

            StyleSheet styleSheet;
            string[] guids = AssetDatabase.FindAssets("NGAMEEditorStyle  t:StyleSheet");
            if (guids.Length > 0)
            {
                styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]));
                root.styleSheets.Add(styleSheet);
                m_Style = styleSheet;
            }
            
            _graphView = root.Q<RoomGraphView>();

            _graphView.ScenePreviewsRequested = OnScenePreviewsRequested;
            _graphView.SceneDataRequested = OnSceneDataRequested;
            _graphView.OnNodeSelected = OnNodeSelectionChanged;
            _graphView.OnNodeValuesChanged = OnNodeValuesChanged;
            _graphView.RegisterPlayModeRequest = RegisterPlayModeRequest;
            _graphView.OnGraphChanged += OnGraphChanged;
            
            _inspectorView = _graphView.GetOrCreateNodeInspector();
            _minimapWindow = _graphView.GetOrCreateMiniMap();

            _graphView.UpdateDataObjects( m_sceneData, m_ScenePreviewLookup);

            OnSelectionChange();

            if(m_Style != null)
            {
                _graphView.styleSheets.Add(m_Style);
            }

            m_FileMenu = root.Q<ToolbarMenu>("FileMenu");
            m_FileMenu.menu.AppendAction("New ...", (a) => OnNewGraphClicked());
            m_FileMenu.menu.AppendAction("Load Graph...", (a) => { OnLoadGraph(); });

            m_FileMenu.menu.AppendSeparator();

            m_FileMenu.menu.AppendAction("Save", (a) => OnSaveGraphClicked());
            m_FileMenu.menu.AppendAction("Discard Changes", (a) => { DiscardChanges(); });

            Button refreshButton = new();
            refreshButton.text = "Refresh Scene Data";
            refreshButton.clicked += OnRefreshScenes;
            m_FileMenu.parent.Add(refreshButton);

            m_ToggleMinimapButton = new();
            m_ToggleMinimapButton.text = "MiniMap";
            m_ToggleMinimapButton.name = "ToggleMiniMap";
            m_ToggleMinimapButton.clicked += OnToggleMiniMap;
            m_FileMenu.parent.Add(m_ToggleMinimapButton);
            SetClassOnVisiblity(_minimapWindow.visible, m_ToggleMinimapButton);

            m_ToggleInspectorButton = new();
            m_ToggleInspectorButton.text = "Node Inspector";
            m_ToggleInspectorButton.name = "ToggleNodeInspector";
            m_ToggleInspectorButton.clicked += OnToggleNodeInspector;
            m_FileMenu.parent.Add(m_ToggleInspectorButton);
            SetClassOnVisiblity(_inspectorView.visible, m_ToggleInspectorButton);
        }

        private void OnToggleMiniMap()
        {
            _minimapWindow.visible = !_minimapWindow.visible;
            SetClassOnVisiblity(_minimapWindow.visible, m_ToggleMinimapButton);
           
        }

        private void OnToggleNodeInspector()
        {
            _inspectorView.visible = !_inspectorView.visible;
            SetClassOnVisiblity(_inspectorView.visible, m_ToggleInspectorButton);
            
        }

        private void SetClassOnVisiblity(bool bIsVisible, VisualElement elementToStyle)
        {
            if (bIsVisible)
                elementToStyle.AddToClassList("visible");
            else
                elementToStyle.RemoveFromClassList("visible");
        }

        private void OnSelectionChange()
        {
            RoomGraph roomGraph = Selection.activeObject as RoomGraph;
            if(roomGraph == null)
            {
                return;
            }

            if (_graph != null && hasUnsavedChanges)
            {
                ShowSaveDialogue();
            }

            _graph = roomGraph;
            _graphView.PopulateView(roomGraph);
        }

        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            if(_inspectorView != null)
                _inspectorView.UpdateSelection(nodeView);
        }

        private void OnGraphChanged()
        {
            EditorUtility.SetDirty(_graph);
            hasUnsavedChanges = true;
        }
        private void OnNodeValuesChanged(NodeView nodeView)
        {
            if (nodeView != null) 
            {
                if (_inspectorView != null)
                    _inspectorView.Repaint(nodeView);
                if(nodeView.Node != null)
                    EditorUtility.SetDirty(nodeView.Node);
            } 

            if(_graph != null)
                EditorUtility.SetDirty(_graph);
            hasUnsavedChanges = true;
        }

        private void OnNewGraphClicked()
        {
            if (hasUnsavedChanges)
            {
                ShowSaveDialogue();
            }

            string path = EditorUtility.SaveFilePanelInProject("New Graph Asset", "NewGraph", "asset",
            "Please enter a file name");
            if (path.Length != 0)
            {
                _graph = CreateInstance< RoomGraph>();
                _graphView.PopulateView(_graph);
                AssetDatabase.CreateAsset(_graph, path);
            }
        }

        public override void SaveChanges()
        {
            if (_graphView != null)
                _graphView.SaveGraph();
            
            base.SaveChanges();
        }

        public override void DiscardChanges()
        {
            Debug.Log("discard changes clicked, will reload graph file");
            _graphView.DiscardChanges();

            base.DiscardChanges();
        }

        private void OnSaveGraphClicked()
        {
            //AssetDatabase.SaveAssets();
            SaveChanges();
        }

        private void OnRefreshScenes()
        {
            //Debug.Log("Refresh scene data clicked");
            ReadCurrentSceneData();
            _graphView.RefreshSceneData(m_sceneData, m_ScenePreviewLookup);
        }

        private void ShowSaveDialogue()
        {
                // EditorUtility.DisplayDialog returns true if ok/save is pressed
            if (EditorUtility.DisplayDialog("Unsaved Changes", this.saveChangesMessage, "Save", "Discard"))
            {
                SaveChanges();
            }
            else
            {
                DiscardChanges();
            }
        }

        private void OnLoadGraph()
        {
            Debug.Log("Load graph clicked");

            if (hasUnsavedChanges)
            {
                ShowSaveDialogue();
            }

            string path = EditorUtility.OpenFilePanelWithFilters("Open Graph", "Assets", new string[] { "Asset files", "asset" });
            path = path.Replace(Application.dataPath, "Assets");
            Debug.Log("Found path: " + path);

            if(path == "")
            {
                return;
            }

            _graph = AssetDatabase.LoadAssetAtPath<RoomGraph>(path);
            _graphView.PopulateView(_graph);
        }

        private List<SceneData> OnSceneDataRequested()
        { 
            return m_sceneData;
        }

        private Dictionary<string, Texture2D> OnScenePreviewsRequested() 
        {
            return m_ScenePreviewLookup;
        }

        private void ReadCurrentSceneData()
        {
            string[] settingsGuid = AssetDatabase.FindAssets("t:SO_Settings");

            if (settingsGuid.Length <= 0)
            {
                return;
            }

            SO_Settings settings = AssetDatabase.LoadAssetAtPath<SO_Settings>(AssetDatabase.GUIDToAssetPath(settingsGuid[0]));

            if (settings == null || settings.Scenes.Count <= 0)
            {
                return;
            }

            m_sceneData.Clear();
            m_ScenePreviewLookup.Clear();

            for (int i = 0; i < settings.Scenes.Count; i++)
            {
                SceneInclusionData data = settings.Scenes[i];

                if (data.FilePath == "" || !data.IncludeInGraphTool)
                {
                    continue;
                }

                // open the scene to collect data from it
                Scene aScene = EditorSceneManager.OpenPreviewScene(data.FilePath);
                if (!aScene.IsValid())
                {
                    Debug.Log("Graph tried to include an invalid scene from filepath: " + data.FilePath);
                    EditorSceneManager.ClosePreviewScene(aScene);
                    continue;
                }
                string sceneGuid = settings.Guids[i];

                SceneData sceneData = CreateSceneDataObject(aScene, sceneGuid, data.FilePath);

                Texture2D previewImage = ScenePreviewRenderer.WriteTexture(aScene, sceneData, 100);
                previewImage.filterMode = FilterMode.Point;
                if (m_ScenePreviewLookup.ContainsKey(sceneGuid))
                {
                    m_ScenePreviewLookup[sceneGuid] = previewImage;
                }
                else
                {
                    m_ScenePreviewLookup.Add(sceneGuid, previewImage);
                }

                EditorSceneManager.ClosePreviewScene(aScene);
                m_sceneData.Add(sceneData);
            }
        }

        private SceneData CreateSceneDataObject(Scene aScene, string sceneGuid, string filePath)
        {
            SceneData result = ScriptableObject.CreateInstance<SceneData>();
            result.Name = aScene.name;
            result.Guid = sceneGuid;
            result.FilePath = filePath;

            List<RegionConnectionData> conectionObjects = new();
            List<SpawnerData> spawners = new();

            GameObject[] rootObjects = aScene.GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                // connection data
                IEncounterRegionConnector[] connectorComponent = obj.GetComponentsInChildren<IEncounterRegionConnector>();

                foreach (IEncounterRegionConnector connection in connectorComponent)
                {
                    RegionConnectionData data = connection.GetRegionConnectionData();
                    conectionObjects.Add(data);
                }

                // spawner data

                ISpawnPoint[] spawnerComponents = obj.GetComponentsInChildren<ISpawnPoint>();

                foreach (ISpawnPoint spawner in spawnerComponents)
                {
                    spawners.Add(spawner.GetSpawnerData());
                }
            }

            result.UniqueConnectionObjects = conectionObjects;
            result.SpawnPoints = spawners;
            result.Bounds = new (conectionObjects, spawners);

            return result;
        }

    }
}