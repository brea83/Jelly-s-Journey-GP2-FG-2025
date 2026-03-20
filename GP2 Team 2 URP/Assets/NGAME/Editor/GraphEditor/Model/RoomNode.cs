using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace NGAME.Editor
{
    public abstract class RoomGraphNode : ScriptableObject
    {
        [HideInInspector] public Vector2 Position;
        public abstract List<EdgeData> GetOutgoingEdges();
    }
    [System.Serializable]
    public class RoomNode : RoomGraphNode, IMapNode
    {
        [HideInInspector] public string Guid;
        public SceneConnectionsData Room = null;
        [HideInInspector] public int LastDropdownIndex;
        [HideInInspector] public List<EdgeData> OutgoingEdges = new List<EdgeData>();
        private bool _isStartNode = false;
        private int _exitCount;
        private int _entranceCount;
        public int ExitCount { get {return _exitCount; } }
        public int EntranceCount { get { return _entranceCount; } }

        public void AddEdge(RoomNode otherNode, Edge edge)
        {
            EdgeData newEdgeData = new EdgeData(edge.output.portName, otherNode.Guid, edge.input.portName);
            OutgoingEdges.Add(newEdgeData);
            EditorUtility.SetDirty(this);
        }

        public override List<EdgeData> GetOutgoingEdges() { return OutgoingEdges; }

        public void RemoveEdge(RoomNode otherNode, Edge edge)
        {
            List<int> indexesToRemove = new List<int>();

            for (int i = 0; i < OutgoingEdges.Count; i++)
            {
                EdgeData edgeData = OutgoingEdges[i];

                if (edgeData.SourcePortName == edge.output.portName && edgeData.DestinationNodeGuid == otherNode.Guid && edgeData.DestinationPortName == edge.input.portName)
                {
                    indexesToRemove.Add(i);
                    //OutgoingEdges.Remove(edgeData);
                }
            }

            indexesToRemove.Sort();
            for (int i = indexesToRemove.Count -1; i >= 0; i--)
            {
                OutgoingEdges.RemoveAt(indexesToRemove[i]);
            }
            EditorUtility.SetDirty(this);
        }

        public void SetAsStartRoom(bool isStartNode)
        {
            _isStartNode = isStartNode;
        }
        public void UpdateRoomData(NGAME.SceneConnectionsData room)
        {
            Room = room;
            if (Room == null)
            {
                _exitCount = 0;
                _entranceCount = 0;
            }
            else
            {
                _exitCount = Room.Exits.Count;
                _entranceCount = Room.Entrances.Count;
            }
            EditorUtility.SetDirty(this);
        }

    }
    [System.Serializable]
    public class EdgeData
    {
        public string SourcePortName;
        public string DestinationNodeGuid;
        public string DestinationPortName;

        public EdgeData(string sourcePortName, string destinationNodeGuid, string destinationPortName)
        {
            SourcePortName = sourcePortName;
            DestinationNodeGuid = destinationNodeGuid;
            DestinationPortName = destinationPortName;
        }
    }

}