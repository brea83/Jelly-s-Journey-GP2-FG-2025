using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
namespace NGAME.Editor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected;
        public RoomNode Node;
        public List<Port> InputPorts = new List<Port>();
        public List<Port> OutputPorts = new List<Port>();


        private DropdownField _roomSelect;
        private List<NGAME.SceneConnectionsData> _roomDataObjects;
        
        
        public NodeView(RoomNode node, List<NGAME.SceneConnectionsData> roomDataObjects = null) 
        {
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
        }

        private void UpdatePorts()
        { 
            if(Node.Room == null)
            {
                InputPorts.Clear();
                OutputPorts.Clear();
                return;
            }
            List<string> EntranceNames = Node.Room.Entrances.ConvertAll(entrance => entrance.Name);
            List<string> ExitNames = Node.Room.Exits.ConvertAll(entrance => entrance.Name);

            RemoveExcessPorts(InputPorts, inputContainer, EntranceNames);
            AddMissingPorts(InputPorts, inputContainer, EntranceNames);

            RemoveExcessPorts(OutputPorts, outputContainer, ExitNames);
            AddMissingPorts(OutputPorts, outputContainer, ExitNames, false);
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
                if (oldPorts.Contains(port))
                {
                    oldPorts.Remove(port);
                }
                port.RemoveFromHierarchy();
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
                newPort.portName = portName;
                portList.Add(newPort);
                portContainer.Add(newPort);
            }
            return newPort;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Node.Position.x = newPos.xMin;
            Node.Position.y = newPos.yMin;
        }

        public Port GetPortByName(string name, List<Port> portCollection)
        {
            return portCollection.Where(port =>
            port.portName == name
            ).First();
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
