using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Properties;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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

        public List<Port> WavePorts = new();

        private DropdownField m_RoomSelectDropdown;
        private int m_LastDropDownIndex = 0;
        private List<NGAME.SceneConnectionsData> _roomDataObjects;
        private Color m_ValidPortColor = new();

        // container that input and output containers are in is called  topContainer on the parent class
        private SceneSpawnData m_CurrentSceneSpawnData;
        private VisualElement m_EncountersContainer;
        private ScrollView m_SpawningScrollView;

        private VisualElement m_WavesContainer;
        private List<VisualElement> m_WaveItems = new List<VisualElement>();
        
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

            Label entranceLabel = new();
            entranceLabel.text = "Entrances";
            inputContainer.Add( entranceLabel );

            Label exitLabel = new();
            exitLabel.text = "Exits";
            outputContainer.Add( exitLabel );


            m_EncountersContainer = new VisualElement();
            m_EncountersContainer.style.minHeight = 50;
            //m_EncountersContainer.style.maxHeight = 200;
            m_EncountersContainer.AddToClassList("nodeExtension");

            Label encountersLabel = new();
            encountersLabel.text = "Encounters";
            encountersLabel.AddToClassList("header2");
            m_EncountersContainer.Add(encountersLabel);

            Foldout spawnDataLabel = new();
            spawnDataLabel.text = "Count of Spawners by Allowed Type";
            spawnDataLabel.AddToClassList("header3");
            m_EncountersContainer.Add(spawnDataLabel);


            m_SpawningScrollView = new ScrollView();
            spawnDataLabel.Add(m_SpawningScrollView);

            m_WavesContainer = new VisualElement();
            VisualElement headerPanel = new();
            headerPanel.style.flexDirection = FlexDirection.Row;
            headerPanel.AddToClassList("header3");

            Label wavesLabel = new();
            wavesLabel.text = "Waves";
            wavesLabel.AddToClassList("header3");

            Button addWaveButton = new();
            addWaveButton.text = "+";
            addWaveButton.clicked += AddWave;

            //Button removeWaveButton = new();
            //removeWaveButton.text = "-";
            //removeWaveButton.clicked += RemoveWave;

            headerPanel.Add(wavesLabel);
            headerPanel.Add(addWaveButton);
            //headerPanel.Add(removeWaveButton);
            m_WavesContainer.Add(headerPanel);
            m_EncountersContainer.Add(m_WavesContainer);


            extensionContainer.Add(m_EncountersContainer);
            extensionContainer.style.flexGrow = 1.0f;
            RefreshExpandedState();
            

            CreateInputPorts();
            CreateOutputPorts();
            //CreateWavePorts();
            UpdateCurrentSceneSpawnData();
            PopulateEncounterContainer();
        }

        private void UpdateCurrentSceneSpawnData()
        {
            m_CurrentSceneSpawnData = m_RoomGraphView.SpawnersByScene.FirstOrDefault((SceneSpawnData e) => e.SceneGUID == Node.SceneData.SceneGuid);
        }
        private void CreateRoomSelector(List<NGAME.SceneConnectionsData> roomDataObjects)
        {
            List<string> choices = new List<string>();
            choices.Add("None Selected");
            int defaultIndex = 0;
            for (int i = 0; i < roomDataObjects.Count(); i++ )
            {
                SceneConnectionsData room = roomDataObjects[i];

                choices.Add(room.SceneName);
                if (room.MinPoint == Vector2.zero && room.MaxPoint == Vector2.zero)
                {
                    room.UpdateBounds();
                }

                if(Node.SceneData != null && Node.SceneData.SceneName == room.SceneName)
                {
                    defaultIndex = i + 1; // plus 1 because we have the default none at index 0 of the list before this loop starts
                }
            }

            //if(Node.SceneData != null)
            //{
            //    string sceneGuid = Node.SceneData.SceneGuid;


            //    defaultIndex = Node.LastDropdownIndex;
            //}

            m_RoomSelectDropdown = new DropdownField(choices, defaultIndex);
            titleContainer.Add(m_RoomSelectDropdown);
            
            m_RoomSelectDropdown.RegisterValueChangedCallback(OnValueChanged);
        }
        
        private void PopulateEncounterContainer()
        {
            m_SpawningScrollView.Clear();
            if(Node.SceneData == null)
            {
                return;
            }

            //Validate Connections?

            if(m_CurrentSceneSpawnData != null)
            {

                Dictionary<string, int> spawnerCountLookup = m_CurrentSceneSpawnData.CountSpawnersWithMatchingTypes();
                foreach(string spawnerType in spawnerCountLookup.Keys)
                {
                    int spawnerCount = spawnerCountLookup[spawnerType];
                    Label label = new Label();
                    label.AddToClassList("ListItem");
                    label.text = spawnerType + ": " + spawnerCount.ToString();
                    m_SpawningScrollView.Add(label);
                }
            }

            if(Node.Waves.Count > 0)
            {
                for(int i = 0; i < Node.Waves.Count; i++)
                {
                    CreateWaveItem(Node.Waves[i], i);
                }
            }
        }


        private void OnValueChanged(ChangeEvent<string> change) 
        {
            if (change.newValue == change.previousValue) return;

            m_LastDropDownIndex = m_RoomSelectDropdown.index;
            //Node.LastDropdownIndex = m_RoomSelectDropdown.index;

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

            UpdateCurrentSceneSpawnData();
            PopulateEncounterContainer();
        }

        //private void UpdateWavePorts()
        //{
        //    List<string> WaveNames = new();
        //    for (int i = 0; i < Node.NumberOfWaves; i++)
        //    {
        //        string portName = "Wave " + (i + 1).ToString();
        //        WaveNames.Add(portName);
        //    }

        //    RemoveExcessPorts(WavePorts, m_WavesContainer, WaveNames);
        //    AddMissingPorts(WavePorts, m_WavesContainer, WaveNames);
        //}

        private void UpdatePorts()
        {

            List<string> EntranceNames;
            List<string> ExitNames;
            //List<string> WaveNames = new();
            if (Node.SceneData == null)
            {
                EntranceNames = new();
                ExitNames = new();
            }
            else
            {
                EntranceNames = Node.SceneData.Entrances.ConvertAll(entrance => entrance.Name);
                ExitNames = Node.SceneData.Exits.ConvertAll(entrance => entrance.Name);

                //for (int i = 0; i < Node.NumberOfWaves; i++)
                //{
                //    string portName = "Wave " + (i + 1).ToString();
                //    WaveNames.Add(portName);
                //}
            }
            
            TryReconnectOldEdges(EntranceNames);

            RemoveExcessPorts(InputPorts, inputContainer, EntranceNames);
            AddMissingPorts(InputPorts, inputContainer, EntranceNames);

            RemoveExcessPorts(OutputPorts, outputContainer, ExitNames);
            AddMissingPorts(OutputPorts, outputContainer, ExitNames, false);

            //RemoveExcessPorts(WavePorts, m_WavesContainer, WaveNames);
            //AddMissingPorts(WavePorts, m_WavesContainer, WaveNames);
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
                if (portContainer != m_WavesContainer && port.direction == Direction.Input)
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
            if (Node.SceneData == null || Node.SceneData.SceneGuid == null) return;
            foreach(var exit in Node.SceneData.Exits)
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
            if (Node.SceneData == null || Node.SceneData.SceneGuid == null) return;
            foreach (var entrance in Node.SceneData.Entrances)
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

        //private void CreateWavePorts()
        //{
        //    if (Node.SceneData == null || Node.SceneData.SceneGuid == null) return;
            
        //    for(int i = 0; i < Node.NumberOfWaves; i++)
        //    {
        //        string portName = "Wave " + (i + 1).ToString();
        //        Port newPort = CreatePort(WavePorts, m_WavesContainer, portName, typeof(int));
        //    }
        //}

        private void CreateWaveItem(SOWaveData wave, int index) 
        { 
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.AddToClassList("ListItem");

            string waveName = "Wave " + m_WaveItems.Count.ToString();

            Foldout header = new Foldout();
            header.text = waveName;
            //header.contentContainer.style.flexDirection = FlexDirection.Row;

            ObjectField field = new ObjectField();
            field.objectType = typeof(SOWaveData);
            if(wave != null )
            {
                field.value = wave;
            }
            field.RegisterValueChangedCallback(
                evt => PatchWaveData(evt, index));
            //field.RegisterCallback<ChangeEvent<ObjectField>, int>(PatchWaveData, index);
            //field.dataSource = Node;
            //field.SetBinding("value", new DataBinding
            //{
            //    dataSourcePath = new PropertyPath(nameof(RoomNode.Waves[index]))
            //});


            Button removeMe = new Button();
            removeMe.style.flexGrow = 0;
            removeMe.text = "Remove Wave";
            removeMe.clicked += () =>
            {
                RemoveWave(row, index);
            };

            header.Add(removeMe);
            header.Add(field);

            row.Add(header);

            m_WavesContainer.Add(row);
            m_WaveItems.Add(row);
        }

        private void PatchWaveData(ChangeEvent<UnityEngine.Object> evt, int index)
        {
            Node.PatchWaveData(evt.newValue as SOWaveData, index);
        }
        private void AddWave()
        {
            SOWaveData wave = ScriptableObject.CreateInstance(typeof(SOWaveData)) as SOWaveData;
            Node.AddWave(wave);
            CreateWaveItem(wave, Node.Waves.Count -1);
            //UpdateWavePorts();
        }

        private void RemoveWave(VisualElement waveItem, int waveIndex)
        {
            m_WaveItems.Remove(waveItem);
            waveItem.RemoveFromHierarchy();
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
                SetUsedPortsOtherDirectionEnabled(port, false);
                //Port matchingPort = null;
                //if(port.direction == Direction.Input )
                //{
                //    matchingPort = GetPortByName(port.portName, OutputPorts);
                //}
                //else
                //{
                //    matchingPort = GetPortByName(port.portName, InputPorts);
                //}

                //if(matchingPort != null)
                //{
                //    matchingPort.SetEnabled(false);
                //}
            }
        }

        private static void SetUsedPortsOtherDirectionEnabled(Port port, bool value)
        {
            Port matchingPort = null;
            NodeView view = port.node as NodeView;
            if (port.direction == Direction.Input)
            {
                matchingPort = view.GetPortByName(port.portName, view.OutputPorts);
            }
            else
            {
                matchingPort = view.GetPortByName(port.portName, view.InputPorts);
            }

            if (matchingPort != null)
            {
                matchingPort.SetEnabled(value);
            }
        }

        private void OnPortDisconnected(Port port, Edge edge)
        {
            if(port != null)
            {
                MarkPortConnectionError(port, edge, "", false);
                SetUsedPortsOtherDirectionEnabled(port, true);
                
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

            // runtime node updates
             
            if(sourceNode != null && destinationNode != null)
            {
                EdgeData newEdgeData = new EdgeData(edge.output.portName, sourceNode.Node.SceneData.SceneGuid, destinationNode.Node.Guid, destinationNode.Node.SceneData.SceneGuid, edge.input.portName);
                newEdgeData.SourceSceneGuid = sourceNode.Node.Guid;
                newEdgeData.SourceSceneName = sourceNode.Node.SceneData.SceneName;
                newEdgeData.DestinationSceneName = destinationNode.Node.SceneData.SceneName;

                sourceNode.Node.RemoveEdge(destinationNode.Node, newEdgeData);
                destinationNode.Node.RemoveEdge(sourceNode.Node, newEdgeData);
                EditorUtility.SetDirty(sourceNode.Node);
                EditorUtility.SetDirty(destinationNode.Node);
            } 
        }

        public static void AddEdge(Edge edge)
        {
            // editor view node updates
            NodeView sourceNode = edge.output.node as NodeView;
            sourceNode.OnPortConnected(edge.output);

            NodeView destinationNode = edge.input.node as NodeView;
            destinationNode.OnPortConnected(edge.input);

            // runtime node updates

            EdgeData newEdgeData = new EdgeData(edge.output.portName, sourceNode.Node.SceneData.SceneGuid, destinationNode.Node.Guid, destinationNode.Node.SceneData.SceneGuid, edge.input.portName);
            newEdgeData.SourceNodeGuid = sourceNode.Node.Guid;
            newEdgeData.SourceSceneName = sourceNode.Node.SceneData.SceneName;
            newEdgeData.DestinationSceneName = destinationNode.Node.SceneData.SceneName;

            sourceNode.Node.AddEdge(destinationNode.Node, newEdgeData);
            destinationNode.Node.AddEdge(sourceNode.Node, newEdgeData);
            EditorUtility.SetDirty(sourceNode.Node);
            EditorUtility.SetDirty(destinationNode.Node);
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

            if (Node.SceneData == null)
            {
                return;
            }
            SceneConnectionsData matchingScene = mostRecentlyFetchedSceneData.FirstOrDefault((SceneConnectionsData e) => e.SceneGuid == Node.SceneData.SceneGuid);
            if (matchingScene == null)
            {
                StringBuilder sb = new();
                sb.Append("Map Graph has a node not included in the valid scenes. ");
                sb.Append("If you wish to remove these nodes use menu option Remove Missing Rooms (NOT IMPLEMENTED).\n");
                sb.Append("Possible reasons for this include: \n");
                sb.Append("You may have unselected the scene in the NGAME settings window \n");
                sb.Append("Or the scene no longer includes NGAME compatible interfaces (Logs for filtering based on that to be added soon).\n");
                Debug.LogWarning(sb.ToString());

                MarkMissingSceneError("Scene named " + Node.SceneData.SceneName + ", not valid.");
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
                        SetUsedPortsOtherDirectionEnabled(sourcePort, false);
                        SetUsedPortsOtherDirectionEnabled(destinationPort, false);
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
                        SetUsedPortsOtherDirectionEnabled(sourcePort, false);
                        SetUsedPortsOtherDirectionEnabled(destinationPort, false);
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

            Node.Position = new Vector2(newPos.xMin, newPos.yMin);
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
