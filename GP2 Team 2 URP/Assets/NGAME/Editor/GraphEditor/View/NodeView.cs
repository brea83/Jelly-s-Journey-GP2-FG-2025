using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace NGAME.Editor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public RoomGraphView m_RoomGraphView;
        public Action<NodeView> OnNodeSelected;
        public RoomNode Node;
        public List<Port> InputPorts = new List<Port>();
        public List<Port> OutputPorts = new List<Port>();
        public List<Port> OldConnectedPorts = new List<Port>();

        private DropdownField _roomSelect;
        private List<NGAME.SceneConnectionsData> _roomDataObjects;
        private Color m_ValidPortColor = new();
        
        public NodeView(RoomGraphView graph, RoomNode node, List<NGAME.SceneConnectionsData> roomDataObjects = null) 
        {
            this.m_RoomGraphView = graph;
            this.Node = node;
            this.title = node.name;
            this.viewDataKey = node.Guid;

            style.left = node.Position.x;
            style.top = node.Position.y;
            
            if (roomDataObjects != null )
            {
                _roomDataObjects = roomDataObjects;
                CreateRoomSelector( roomDataObjects );
            }
            
            CreateInputPorts();
            CreateOutputPorts();
        }

        private void CreateRoomSelector(List<NGAME.SceneConnectionsData> roomDataObjects)
        {
            List<string> choices = new List<string>();
            choices.Add("None Selected");
            foreach(NGAME.SceneConnectionsData room in roomDataObjects )
            {
                choices.Add(room.SceneName);
                if (room.MinPoint == Vector2.zero && room.MaxPoint == Vector2.zero)
                {
                    room.UpdateBounds();
                }
            }

            int defaultIndex = 0;
            if(Node.Room != null)
            {
                defaultIndex = Node.LastDropdownIndex;
            }

            _roomSelect = new DropdownField(choices, defaultIndex);
            titleContainer.Add(_roomSelect);
            
            _roomSelect.RegisterValueChangedCallback(OnValueChanged);
        }
        private void OnValueChanged(ChangeEvent<string> change) 
        {
            if (change.newValue == change.previousValue) return;

            Node.LastDropdownIndex = _roomSelect.index;

            NGAME.SceneConnectionsData newData = null;
            foreach(NGAME.SceneConnectionsData room in _roomDataObjects )
            {
                if( room.SceneName == change.newValue )
                {
                    newData = room;
                    break;
                }
            }
            Node.UpdateRoomData( newData );
            UpdatePorts();
            MarkMissingSceneError("", false);
        }

        private void UpdatePorts()
        {

            List<string> EntranceNames;
            List<string> ExitNames;  
            if (Node.Room == null)
            {
                EntranceNames = new();
                ExitNames = new();
            }
            else
            {
                EntranceNames = Node.Room.Entrances.ConvertAll(entrance => entrance.Name);
                ExitNames = Node.Room.Exits.ConvertAll(entrance => entrance.Name);
            }
            
            TryReconnectOldEdges(EntranceNames);

            RemoveExcessPorts(InputPorts, inputContainer, EntranceNames);
            AddMissingPorts(InputPorts, inputContainer, EntranceNames);

            RemoveExcessPorts(OutputPorts, outputContainer, ExitNames);
            AddMissingPorts(OutputPorts, outputContainer, ExitNames, false);

        }

        private void TryReconnectOldEdges(List<string> newPortNames)
        {
            List<int> indexesToRemove = new();

            for (int i = 0; i < OldConnectedPorts.Count; i++)
            {
                Port oldPort = OldConnectedPorts[i];
                if (newPortNames.Contains(oldPort.portName))
                {
                    InputPorts.Add(oldPort);
                    Edge oldEdge = null;
                    if (oldPort.connections.Count() >= 1 )
                    {
                        oldEdge = oldPort.connections.First();
                        MarkPortConnectionError(oldEdge.output, oldEdge, "", false);
                    }
                    MarkPortConnectionError(oldPort, oldEdge, "", false);
                    indexesToRemove.Add(i);
                }
            }

            indexesToRemove.Sort();

            for (int i = indexesToRemove.Count -1; i >= 0; i--)
            {
                OldConnectedPorts.RemoveAt(indexesToRemove[i]);
            }
        }


        private void AddMissingPorts(List<Port> oldPorts, VisualElement portContainer, List<string> newPortNames, bool isInputPort = true)
        {
            List<string> missingPorts = GetMissingPortNames(oldPorts, newPortNames);

            foreach(string name in missingPorts)
            {
                if (isInputPort)
                {
                    CreatePort(oldPorts, portContainer, name, typeof(bool));
                }
                else
                {
                    CreatePort(oldPorts, portContainer, name, typeof (bool), Direction.Output, Port.Capacity.Single);
                }
            }
        }
        private void RemoveExcessPorts(List<Port> oldPorts, VisualElement portContainer, List<string> newPortNames)
        {
            List<Port> excessPorts = GetExcessPorts(oldPorts, newPortNames);
            
            foreach(Port port in excessPorts )
            {

                bool bIsRetained = false;
                if (port.direction == Direction.Input)
                {
                    bIsRetained = TryRetainConnectedPorts(port);
                }

                if (!bIsRetained)
                {
                    oldPorts.Remove(port);
                    port.RemoveFromHierarchy();
                }
            }
        }

        private List<string> GetMissingPortNames(List<Port> ports, List<string> portNames)
        {
            List<string> missingPortNames = new List<string>();
            List<string> existingPortNames = new List<string>();
            foreach (Port port in ports)
            {
                existingPortNames.Add(port.portName);
            }

            foreach (string portName in portNames)
            {
                if (!existingPortNames.Contains(portName))
                {
                    missingPortNames.Add(portName);
                }
            }

            return missingPortNames;
        }

        private List<Port> GetExcessPorts(List<Port> ports, List<string> newPortNames)
        {
            List<Port> excessPorts = new List<Port>();

            foreach (Port port in ports)
            {
                if (!newPortNames.Contains(port.portName))
                {
                    excessPorts.Add(port);
                }
            }

            return excessPorts;
        }
        private void CreateOutputPorts()
        {
            if (Node.Room == null || Node.Room.SceneGuid == null) return;
            foreach(var exit in Node.Room.Exits)
            {
                Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                if(output != null)
                {
                    output.portName = exit.Name;
                    OutputPorts.Add(output);
                    outputContainer.Add(output);
                }
            }
        }

        private void CreateInputPorts()
        {
            if (Node.Room == null || Node.Room.SceneGuid == null) return;
            foreach (var entrance in Node.Room.Entrances)
            {
                Port newPort = CreatePort(InputPorts, /*contentContainer*/ inputContainer, entrance.Name, typeof(bool));

                //Vector2 topDownPosition = new Vector2(entrance.Position.x, entrance.Position.z);
                //topDownPosition.Normalize();
                //Vector2 relativePosition = topDownPosition - Node.Room.MinPoint;
                //newPort.style.position = Position.Absolute;
                //newPort.style.top = relativePosition.y;
                //newPort.style.left = relativePosition.x;
            }
        }

        private Port CreatePort(List<Port> portList, VisualElement portContainer, string portName, System.Type passedDataType, 
            Direction flowDirection = Direction.Input, Port.Capacity portCapacity = Port.Capacity.Multi, 
            Orientation orientation = Orientation.Horizontal)
        {
            Port newPort = InstantiatePort(orientation, flowDirection, portCapacity, passedDataType);
            if (newPort != null)
            {
                //newPort.conn
                m_ValidPortColor = newPort.portColor;
                newPort.portName = portName;
                portList.Add(newPort);
                portContainer.Add(newPort);
            }
            return newPort;
        }

        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            //Debug.Log("InstantiatePort called");
            return Port.Create<Edge>(orientation, direction, capacity, type);
        }

        protected bool TryRetainConnectedPorts(Port port)
        {
            //base.OnPortRemoved(port);
            //Debug.Log("On Port removed called for port: " + port.portName);
            if (port.connected)
            {
                string sourceTooltip = "Missing connection to port named " + port.portName;
                string destinationTooltip = "Connecetion named " + port.portName + ", doesn't exist in scene.";
                Port otherPort = null;
                Edge edge = null;
                List<Edge> edges = port.connections.ToList();
                foreach (Edge e in edges)
                {
                    edge = e;
                    otherPort = port.direction == Direction.Input ? e.output : e.input;
                    if(otherPort == null)
                    {
                        continue;
                    }
                }
                MarkPortConnectionError(otherPort, edge, sourceTooltip);
                MarkPortConnectionError(port, null, destinationTooltip);
                OldConnectedPorts.Add(port);
                InputPorts.Remove(port);
                return true;
            }
            return false;
        }

        private void OnPortConnected(Port port)
        {
            //string sceneName = Node.Room != null ? Node.Room.SceneName : "NULL";
            //Debug.Log("port connected event on port " + port.portName + ", in room node with scene " + sceneName);
            if(port != null)
            {
                MarkPortConnectionError(port, null, "", false);
                
            }
        }

        private void OnPortDisconnected(Port port, Edge edge)
        {
            if(port != null)
            {
                MarkPortConnectionError(port, edge, "", false);
                
            }

            if (OldConnectedPorts.Contains(port))
            {
                OldConnectedPorts.Remove(port);
                port.RemoveFromHierarchy();
            }
        }

        public static void RemoveEdge(Edge edge)
        {
            NodeView sourceNode = edge.output.node as NodeView;
            if(sourceNode != null)
            {
                sourceNode.OnPortDisconnected(edge.output, edge);
            }

            NodeView destinationNode = edge.input.node as NodeView;
            if(destinationNode != null)
            {
                destinationNode.OnPortDisconnected(edge.input, edge);
            }
        }

        public static void AddEdge(Edge edge)
        {
            NodeView sourceNode = edge.output.node as NodeView;
            sourceNode.OnPortConnected(edge.output);

            NodeView destinationNode = edge.input.node as NodeView;
            destinationNode.OnPortConnected(edge.input);
        }

        private void MarkPortConnectionError(Port port, Edge edge, string tooltip = "", bool bShowError = true)
        {
            if (bShowError)
            {
                if (port != null)
                {
                    port.AddToClassList("Error1");
                    port.tooltip = tooltip;
                    port.portColor = Color.red;
                }

                if (edge != null)
                {
                    edge.AddToClassList("Error1");
                    edge.tooltip = tooltip;
                    edge.input.portColor = Color.red;
                }
            }
            else
            {
                if (port != null)
                {
                    port.RemoveFromClassList("Error1");
                    port.tooltip = tooltip;
                    port.portColor = m_ValidPortColor;
                }

                if (edge != null)
                {
                    edge.RemoveFromClassList("Error1");
                    edge.tooltip = tooltip;
                    edge.input.portColor = m_ValidPortColor;
                }
            }
            
        }

        private void MarkMissingSceneError(string errorTooltip = "", bool bShowError = true)
        {
            if (bShowError)
            {
                AddToClassList("Error2");
                titleContainer.AddToClassList("Error1");
                titleContainer.tooltip = errorTooltip;
            }
            else
            {
                RemoveFromClassList("Error2");
                titleContainer.RemoveFromClassList("Error1");
                titleContainer.tooltip = "";
            }
        }

        public void ValidateNode(List<NGAME.SceneConnectionsData> mostRecentlyFetchedSceneData)
        {
            ValidateNodeScene(mostRecentlyFetchedSceneData);
            ValidateOutputEdges(mostRecentlyFetchedSceneData);
        }

        internal void ValidateNodeScene(List<NGAME.SceneConnectionsData> mostRecentlyFetchedSceneData)
        {

            if (Node.Room == null)
            {
                return;
            }
            SceneConnectionsData matchingScene = mostRecentlyFetchedSceneData.FirstOrDefault((SceneConnectionsData e) => e.SceneGuid == Node.Room.SceneGuid);
            if (matchingScene == null)
            {
                StringBuilder sb = new();
                sb.Append("Map Graph has a node not included in the valid scenes. ");
                sb.Append("If you wish to remove these nodes use menu option Remove Missing Rooms (NOT IMPLEMENTED).\n");
                sb.Append("Possible reasons for this include: \n");
                sb.Append("You may have unselected the scene in the NGAME settings window \n");
                sb.Append("Or the scene no longer includes NGAME compatible interfaces (Logs for filtering based on that to be added soon).\n");
                Debug.LogWarning(sb.ToString());

                MarkMissingSceneError("Scene named " + Node.Room.SceneName + ", not valid.");
                return;
            }


        }

        internal void ValidateOutputEdges(List<NGAME.SceneConnectionsData> mostRecentlyFetchedSceneData, bool bDeleteInvalidEdges = true)
        {
            List<int> indexOfInvalidEdges = new();

            for (int i = 0; i < Node.OutgoingEdges.Count; i++)
            {
                EdgeData serializedEdge = Node.OutgoingEdges[i];
                Port sourcePort = GetPortByName(serializedEdge.SourcePortName, OutputPorts);

                NodeView destinationView = m_RoomGraphView.GetNodeByGuid(serializedEdge.DestinationNodeGuid) as NodeView;
                if (destinationView == null)
                {
                    sourcePort.AddToClassList("Error1");
                    string errorTooltip = "Connected to missing node guid: " + serializedEdge.DestinationNodeGuid;
                    //Debug.LogWarning("Node " + Node.Room.SceneName + ", has a connection to a missing node with guid: " + edge.DestinationNodeGuid + ". Removing edge from node.");
                    MarkPortConnectionError(sourcePort, null, errorTooltip);

                    indexOfInvalidEdges.Add(i);
                    continue;
                }

                Port destinationPort = destinationView.GetPortByName(serializedEdge.DestinationPortName, destinationView.InputPorts);
                if (sourcePort != null)
                {
                    if (destinationPort != null)
                    {
                        Edge newEdge = sourcePort.ConnectTo(destinationPort);
                        m_RoomGraphView.AddElement(newEdge);

                    }
                    else
                    {
                        string errorTooltip = "Missing connection to port named " + serializedEdge.DestinationPortName;
                        string destinationTooltip = "Connecetion named " + serializedEdge.DestinationPortName + ", doesn't exist in scene.";
                        Port newDestination = destinationView.AddErrorInputPort(serializedEdge.DestinationPortName);

                        Edge newEdge = sourcePort.ConnectTo(newDestination);
                        m_RoomGraphView.AddElement(newEdge);
                        MarkPortConnectionError(sourcePort, newEdge, errorTooltip);
                        MarkPortConnectionError(newDestination, null, destinationTooltip);
                        //Debug.LogWarning("Node " + node.Room.SceneName + ", has a connection to a missing port named " + edge.DestinationPortName + ", this is probably because the node this port was connected to had its scene changed.");
                    }
                }
            }

            foreach (int index in indexOfInvalidEdges)
            {
                if (bDeleteInvalidEdges)
                {
                    Node.OutgoingEdges.RemoveAt(index);
                }
                EditorUtility.SetDirty(this.Node);
            }
        }

        protected Port AddErrorInputPort( string portName)
        {
            return CreatePort(OldConnectedPorts, inputContainer, portName, typeof(bool)) ;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Node.Position.x = newPos.xMin;
            Node.Position.y = newPos.yMin;
        }

        public Port GetPortByName(string name, List<Port> portCollection)
        {
            return portCollection.FirstOrDefault((Port e) => e.portName == name);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if(OnNodeSelected != null)
            {
                OnNodeSelected.Invoke(this);
            }
        }
    }
}

//DropdownField formatListItemCallback seems to fire whenever the whole list is displayed
// DropdownField formatSelectedFieldCallback seems to fire whenever you select a field, but before the ValueChangedCallback is fired
