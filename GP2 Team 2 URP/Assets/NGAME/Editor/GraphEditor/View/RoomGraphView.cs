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

        public void ValidateNode(RoomNode node, NodeView view, List<NGAME.SceneConnectionsData> mostRecentlyFetchedSceneData)
        {
           

            NGAME.SceneConnectionsData matchingScene = mostRecentlyFetchedSceneData.FirstOrDefault((NGAME.SceneConnectionsData e) => e.SceneGuid == node.Room.SceneGuid);
            if (matchingScene == null)
            {
                StringBuilder sb = new();
                sb.Append("Map Graph has a node not included in the valid scenes. ");
                sb.Append("If you wish to remove these nodes use menu option Remove Missing Rooms (NOT IMPLEMENTED).\n");
                sb.Append("Possible reasons for this include: \n");
                sb.Append("You may have unselected the scene in the NGAME settings window \n");
                sb.Append("Or the scene no longer includes NGAME compatible interfaces (Logs for filtering based on that to be added soon).\n");
                Debug.LogWarning(sb.ToString());
                view.titleContainer.AddToClassList("Error1");
                view.AddToClassList("Error1");
                return;
            }


        }

        internal void PopulateView(RoomGraph roomGraph)
        {
            this._graph = roomGraph;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            foreach(RoomNode node in _graph.nodes)
            {
                CreateNodeView(node);
            }

            foreach(RoomNode node in _graph.nodes)
            {
                NodeView currentView = FindNodeView(node);

                ValidateNode(node, currentView, ValidScenes);

                List<EdgeData> outgoingEdges = _graph.GetOutgoingEdges(node);

                List<int> indexOfInvalidEdges = new();

                for (int i = 0; i < outgoingEdges.Count; i++)
                {
                    EdgeData edge = outgoingEdges[i];
                    Port sourcePort = currentView.GetPortByName(edge.SourcePortName, currentView.OutputPorts);
                   
                    NodeView destinationView = GetNodeByGuid(edge.DestinationNodeGuid) as NodeView;
                    if (destinationView == null)
                    {
                        sourcePort.AddToClassList("Error1");
                        Debug.LogWarning("Node " + node.name + ", initialized with connection to a missing node with guid: " + edge.DestinationNodeGuid + ". Removing edge from node.");
                        indexOfInvalidEdges.Add(i);
                        continue;
                    }
                    
                    Port destinationPort = destinationView.GetPortByName(edge.DestinationPortName, destinationView.InputPorts);
                    if (sourcePort != null && destinationPort != null)
                    {
                        Edge newEdge = sourcePort.ConnectTo(destinationPort);
                        AddElement(newEdge);
                    }
                }

                foreach( int index  in indexOfInvalidEdges)
                {
                    outgoingEdges.RemoveAt(index);
                    EditorUtility.SetDirty(node);
                    EditorUtility.SetDirty(_graph);
                }
            }
        }

        private NodeView FindNodeView(RoomNode roomNode)
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
                    }

                    Edge edge = element as Edge;
                    if(edge != null)
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        _graph.RemoveEdge(parentView.Node, childView.Node, edge);
                    }
                }
            }

            if(viewChange.edgesToCreate != null)
            {
                foreach(Edge edge in viewChange.edgesToCreate)
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    _graph.AddEdge(parentView.Node, childView.Node, edge);
                }
            }
            return viewChange;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var types = TypeCache.GetTypesDerivedFrom<RoomGraphNode>();
            foreach(var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
            }
        }

        void CreateNode(System.Type type)
        {
            RoomNode node = _graph.CreateNode(type);
            CreateNodeView(node);
        }
        void CreateNodeView(RoomNode roomNode)
        {
            NodeView nodeView = new NodeView(roomNode, ValidScenes);
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
            //Debug.Log("SEARCHING FOR ROOM DATA SCRIPTABLE OBJECTS:");
            //string[] roomDataGuids = AssetDatabase.FindAssets("t:RoomData");
            //foreach (string roomDataGuid in roomDataGuids)
            //{
            //    Debug.Log(roomDataGuid);
            //    RoomData room = AssetDatabase.LoadAssetAtPath<RoomData>(AssetDatabase.GUIDToAssetPath(roomDataGuid));
            //    if(room != null && room.Guid == "")
            //    {
            //        room.Guid = GUID.Generate().ToString();
            //    }
            //    RoomDataObjects.Add(room);
                
            //}
            //Debug.Log("SEARCH FOR ROOM DATA COMPLETE");

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

        private NGAME.SceneConnectionsData GetScenesRegionConnectionData(string filePath)
        {
            //List<NGAME.RegionConnectionData> connections = new List<NGAME.RegionConnectionData>();

            //short hands for comparisons later
            NGAME.RegionConnectionType twoWay = NGAME.RegionConnectionType.ExitAndEntrance;
            NGAME.RegionConnectionType entranceOnly = NGAME.RegionConnectionType.EntranceOnly;
            NGAME.RegionConnectionType exitOnly = NGAME.RegionConnectionType.ExitOnly;

            List<NGAME.RegionConnectionData> entrances = new List<NGAME.RegionConnectionData>();
            List<NGAME.RegionConnectionData> exits = new List<NGAME.RegionConnectionData>();

            Scene aScene = EditorSceneManager.OpenPreviewScene(filePath);
            if (!aScene.IsValid())
            {
                Debug.Log("Graph tried to include an invalid scene from filepath: " + filePath);
                return null;
            }

            bool bComponentsFound = false;

            GameObject[] rootObjects = aScene.GetRootGameObjects();


            foreach (GameObject obj in rootObjects)
            {
                NGAME.IEncounterRegionConnector[] components = obj.GetComponentsInChildren<NGAME.IEncounterRegionConnector>();

                if (components.Length > 0)
                {
                    bComponentsFound = true;
                    foreach (NGAME.IEncounterRegionConnector component in components)
                    {
                        //connections.Add(component.GetRegionConnectionData());
                        NGAME.RegionConnectionData data = component.GetRegionConnectionData();
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
            EditorSceneManager.ClosePreviewScene(aScene);
            
            NGAME.SceneConnectionsData result = new NGAME.SceneConnectionsData();
            result.SceneName = aScene.name;
            result.Entrances = entrances;
            result.Exits = exits;

            //result.PreviewScene = aScene;

            return result;
        }
    }
}
