using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace NGAME.Editor
{
    [UxmlElement]
    public partial class RoomGraphView : GraphView
    {
        public Action<NodeView> OnNodeSelected;
        public List<NGAME.Editor.SceneData> IncludedScenes = new List<NGAME.Editor.SceneData>();
        public List<NGAME.SceneConnectionsData> ValidScenes = new List<NGAME.SceneConnectionsData>();
        //public StyleSheet Style;
        
        private RoomGraph _graph;

        public RoomGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GetRoomDataObjects();

            //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Toolkit/Styles/Editor/RoomGraphEditor.uss");
            //styleSheets.Add(styleSheet);
        }

        internal void PopulateView(RoomGraph roomGraph)
        {
            this._graph = roomGraph;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            foreach(RoomNode node in _graph.nodes)
            {
                if(node is RoomNode)
                {
                    RoomNode roomNode = node as RoomNode;
                    CreateNodeView(roomNode);
                }
            }

            foreach(RoomNode node in _graph.nodes)
            {
                NodeView currentView = FindNodeView(node);

                currentView.ValidateNode(ValidScenes);
            }
        }

        private NodeView FindNodeView(IMapNode roomNode)
        {
            return GetNodeByGuid(roomNode.Guid) as NodeView;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange viewChange) 
        {
            if (viewChange.elementsToRemove != null)
            {
                foreach (GraphElement element in viewChange.elementsToRemove)
                {
                    NodeView nodeView =  element as NodeView;
                    if(nodeView != null)
                    {
                        _graph.DeleteNode(nodeView.Node);

                        AssetDatabase.RemoveObjectFromAsset(nodeView.Node);
                        EditorUtility.SetDirty(_graph);
                        AssetDatabase.SaveAssetIfDirty(_graph);
                    }

                    Edge edge = element as Edge;
                    if(edge != null)
                    {
                        NodeView sourceNode = edge.output.node as NodeView;
                        NodeView destinationNode = edge.input.node as NodeView;
                        NodeView.RemoveEdge(edge);
                        //_graph.RemoveEdge(sourceNode.Node, destinationNode.Node, edge);
                    }
                }
            }

            if(viewChange.edgesToCreate != null)
            {
                foreach(Edge edge in viewChange.edgesToCreate)
                {
                    NodeView sourceNode = edge.output.node as NodeView;
                    NodeView destinationNode = edge.input.node as NodeView;
                    NodeView.AddEdge(edge);
                    //_graph.AddEdge(sourceNode.Node, destinationNode.Node, edge);
                    Debug.Log("Edge created between " + edge.input.portName + ", and " + edge.output.portName);
                }
            }

            if(viewChange.movedElements != null)
            {
                foreach (GraphElement element in viewChange.movedElements)
                {
                    Edge edge = element as Edge;
                    if( edge != null )
                    {
                        Debug.Log("Edge between " + edge.input.portName + ", and " + edge.output.portName + ". MOVED");

                    }
                }
            }
            return viewChange;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var types = TypeCache.GetTypesDerivedFrom<IMapNode>();
            Vector2 position = evt.localMousePosition;
            foreach(var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type, position));
            }
        }

        void CreateNode(System.Type type, Vector2 position)
        {
            string newGuid = GUID.Generate().ToString();
            RoomNode node = _graph.CreateNode(type, newGuid);
            node.Position = position;

            AssetDatabase.AddObjectToAsset(node, _graph);
            EditorUtility.SetDirty(_graph);
            EditorUtility.SetDirty(node);
            AssetDatabase.SaveAssetIfDirty(_graph);
            AssetDatabase.SaveAssetIfDirty(node);
            CreateNodeView(node);
        }
        void CreateNodeView(RoomNode roomNode)
        {
            NodeView nodeView = new NodeView(this, roomNode, ValidScenes);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            //return base.GetCompatiblePorts(startPort, nodeAdapter);
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node
            ).ToList();
        }

        private void GetRoomDataObjects()
        {
            string[] settingsGuid = AssetDatabase.FindAssets("t:SO_Settings");

            if(settingsGuid.Length <= 0)
            {
                return;
            }

            NGAME.Editor.SO_Settings settings = AssetDatabase.LoadAssetAtPath<NGAME.Editor.SO_Settings>(AssetDatabase.GUIDToAssetPath(settingsGuid[0]));

            if (settings == null || settings.Scenes.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < settings.Scenes.Count; i++)
            {
                NGAME.Editor.SceneData data = settings.Scenes[i];

                if(data.FilePath == "" || !data.IncludeInGraphTool)
                {
                    continue;
                }

                NGAME.SceneConnectionsData connections = GetScenesRegionConnectionData(data.FilePath);
                if (connections != null)
                {
                    connections.SceneName = data.Name;
                    connections.SceneGuid = settings.Guids[i];
                    ValidScenes.Add(connections);
                }
            }
  
        }

        private SceneConnectionsData GetScenesRegionConnectionData(string filePath)
        {
            //List<RegionConnectionData> connections = new List<RegionConnectionData>();

            //short hands for comparisons later
            RegionConnectionType twoWay = RegionConnectionType.ExitAndEntrance;
            RegionConnectionType entranceOnly = RegionConnectionType.EntranceOnly;
            RegionConnectionType exitOnly = RegionConnectionType.ExitOnly;

            List<RegionConnectionData> entrances = new List<RegionConnectionData>();
            List<RegionConnectionData> exits = new List<RegionConnectionData>();

            Scene aScene = EditorSceneManager.OpenPreviewScene(filePath);
            if (!aScene.IsValid())
            {
                Debug.Log("Graph tried to include an invalid scene from filepath: " + filePath);
                EditorSceneManager.ClosePreviewScene(aScene);
                return null;
            }

            bool bComponentsFound = false;

            GameObject[] rootObjects = aScene.GetRootGameObjects();


            foreach (GameObject obj in rootObjects)
            {
                IEncounterRegionConnector[] components = obj.GetComponentsInChildren<IEncounterRegionConnector>();

                if (components.Length > 0)
                {
                    bComponentsFound = true;
                    foreach (IEncounterRegionConnector component in components)
                    {
                        //connections.Add(component.GetRegionConnectionData());
                        RegionConnectionData data = component.GetRegionConnectionData();
                        if (data.ConnectionType == twoWay || data.ConnectionType == entranceOnly)
                        {
                            entrances.Add(data);
                        }
                        if(data.ConnectionType == twoWay || data.ConnectionType == exitOnly)
                        {
                            exits.Add(data);
                        }
                    }
                }
            }

            if (!bComponentsFound)
            {
                Debug.Log("No IEncounterRegionConnector components found in scene: " + aScene.name);
                
                EditorSceneManager.ClosePreviewScene(aScene);
                return null;
            }

            Debug.Log("Scene: " + aScene.name + " contains target data types");


            NGAME.SceneConnectionsData result = new NGAME.SceneConnectionsData();
            result.SceneName = aScene.name;
            result.Entrances = entrances;
            result.Exits = exits;

            EditorSceneManager.ClosePreviewScene(aScene);
            return result;
        }
    }
}
