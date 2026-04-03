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
        public List<SceneData> IncludedScenes = new List<SceneData>();
        public List<SceneConnectionsData> ValidScenes = new List<SceneConnectionsData>();
        public List<SceneSpawnData> SpawnersByScene = new List<SceneSpawnData>();
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
                    //RoomNode roomNode = node as RoomNode;
                    CreateNodeView(node);
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
                List<Edge> invalidEdges = new();
                foreach(Edge edge in viewChange.edgesToCreate)
                {
                    if(!edge.output.enabledSelf || !edge.input.enabledSelf)
                    {
                        //this.RemoveElement(edge);
                        invalidEdges.Add(edge);
                    }
                    else
                    {
                        NodeView sourceNode = edge.output.node as NodeView;
                        NodeView destinationNode = edge.input.node as NodeView;
                        NodeView.AddEdge(edge);
                        //_graph.AddEdge(sourceNode.Node, destinationNode.Node, edge);
                        Debug.Log("Edge created between " + edge.input.portName + ", and " + edge.output.portName);
                    }
                }

                foreach(Edge edge in invalidEdges)
                {
                    viewChange.edgesToCreate.Remove(edge);
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
            Vector2 position = evt.mousePosition;
            foreach(var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => OnContextMenuCreateNode(a, type));
            }
        }
        protected void OnContextMenuCreateNode(DropdownMenuAction a, System.Type type)
        {
            
            Vector2 screenMousePosition = a.eventInfo.localMousePosition;
            CreateNode(type, screenMousePosition);
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

            SO_Settings settings = AssetDatabase.LoadAssetAtPath<SO_Settings>(AssetDatabase.GUIDToAssetPath(settingsGuid[0]));

            if (settings == null || settings.Scenes.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < settings.Scenes.Count; i++)
            {
                SceneData data = settings.Scenes[i];

                if(data.FilePath == "" || !data.IncludeInGraphTool)
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

                (SceneConnectionsData connections, SceneSpawnData spawners) componentData = GetScenesRegionConnectionData(aScene, sceneGuid);
                if (componentData.Item1 != null)
                {
                    
                    ValidScenes.Add(componentData.Item1);
                }

                if (componentData.Item2 != null) 
                {
                    SpawnersByScene.Add(componentData.Item2);
                }


                EditorSceneManager.ClosePreviewScene(aScene);
            }

        }

        private (SceneConnectionsData connections, SceneSpawnData spawners) GetScenesRegionConnectionData(Scene aScene, string sceneGuid)
        {
            //short hands for comparisons later
            RegionConnectionType twoWay = RegionConnectionType.ExitAndEntrance;
            RegionConnectionType entranceOnly = RegionConnectionType.EntranceOnly;
            RegionConnectionType exitOnly = RegionConnectionType.ExitOnly;

            List<RegionConnectionData> entrances = new List<RegionConnectionData>();
            List<RegionConnectionData> exits = new List<RegionConnectionData>();

            List<SpawnerData> spawners = new();


            bool bConnectionsFound = false;
            bool bSpawnersFound = false;

            GameObject[] rootObjects = aScene.GetRootGameObjects();


            foreach (GameObject obj in rootObjects)
            {
                // connection data
                IEncounterRegionConnector[] connectorComponent = obj.GetComponentsInChildren<IEncounterRegionConnector>();
                if (connectorComponent.Length > 0) bConnectionsFound = true;

                foreach (IEncounterRegionConnector connection in connectorComponent)
                {
                    //connections.Add(component.GetRegionConnectionData());
                    RegionConnectionData data = connection.GetRegionConnectionData();
                    if (data.ConnectionType == twoWay || data.ConnectionType == entranceOnly)
                    {
                        entrances.Add(data);
                    }
                    if(data.ConnectionType == twoWay || data.ConnectionType == exitOnly)
                    {
                        exits.Add(data);
                    }
                }

                // spawner data

                ISpawnPoint[] spawnerComponents = obj.GetComponentsInChildren<ISpawnPoint>();
                if(spawnerComponents.Length > 0) bSpawnersFound = true;

                foreach (ISpawnPoint spawner in spawnerComponents)
                {
                    spawners.Add(spawner.GetSpawnerData());
                }

            }

            if (!bConnectionsFound && !bSpawnersFound)
            {
                Debug.Log("No IEncounterRegionConnector or ISpawnPoint components found in scene: " + aScene.name);
            }
            else
            {
                Debug.Log("Scene: " + aScene.name + " contains target data types");
            }

            SceneConnectionsData connectionData = null;
            if (bConnectionsFound) 
            {
                connectionData = new SceneConnectionsData();
                connectionData.SceneName = aScene.name;
                connectionData.SceneGuid = sceneGuid;
                connectionData.Entrances = entrances;
                connectionData.Exits = exits;
            }
            

            SceneSpawnData spawnData = null;
            if (bSpawnersFound) 
            {
                spawnData = new SceneSpawnData();
                spawnData.SpawnPoints = spawners;
                spawnData.SceneGUID = sceneGuid;
            }
            
            return (connectionData, spawnData);
        }
    
        
    }
}
