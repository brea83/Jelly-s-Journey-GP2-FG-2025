using System.Collections.Generic;
using UnityEngine;
namespace NGAME
{
    [System.Serializable]
    public class RoomNode : ScriptableObject, IMapNode
    {
        // public properties and geter/setters
        public string Guid { get => m_Guid; set => m_Guid = value; }
        public Vector2 Position { get => m_Position; set => m_Position = value; }

        public SceneConnectionsData SceneData = null;
        
        public List<EdgeData> OutgoingEdges = new List<EdgeData>();


        // private properties
        private string m_Guid;
        private Vector2 m_Position;
        private bool _isStartNode = false;
        private int _exitCount;
        private int _entranceCount;



        public void AddEdge(IMapNode otherNode, EdgeData edge)
        {
            if (otherNode is RoomNode)
            {
                AddRoomEdge(otherNode as RoomNode, edge);
            }
        }

        public void RemoveEdge(IMapNode otherNode, EdgeData edge)
        {
            if(otherNode is RoomNode)
            {
                RemoveRoomEdge(otherNode as RoomNode, edge);
            }
        }

        private void AddRoomEdge(RoomNode otherNode, EdgeData edge)
        {
            //EdgeRuntimeData newEdgeData = new EdgeRuntimeData(edge.output.portName, Room.SceneGuid, otherNode.m_Guid, otherNode.Room.SceneGuid, edge.input.portName);
            OutgoingEdges.Add(edge);
            //EditorUtility.SetDirty(this);
        }

        private void RemoveRoomEdge(RoomNode otherNode, EdgeData edge)
        {
            List<int> indexesToRemove = new List<int>();

            for (int i = 0; i < OutgoingEdges.Count; i++)
            {
                EdgeData edgeData = OutgoingEdges[i];

                if (edgeData.SourcePortName == edge.SourcePortName && edgeData.DestinationNodeGuid == otherNode.Guid && edgeData.DestinationPortName == edge.DestinationPortName)
                {
                    indexesToRemove.Add(i);
                    //OutgoingEdges.Remove(edgeData);
                }
            }

            indexesToRemove.Sort();
            for (int i = indexesToRemove.Count - 1; i >= 0; i--)
            {
                OutgoingEdges.RemoveAt(indexesToRemove[i]);
            }
            //EditorUtility.SetDirty(this);
        }

        public void SetAsStartRoom(bool isStartNode)
        {
            _isStartNode = isStartNode;
        }
        public void UpdateRoomData(NGAME.SceneConnectionsData room)
        {
            SceneData = room;
            if (SceneData == null)
            {
                _exitCount = 0;
                _entranceCount = 0;
            }
            else
            {
                _exitCount = SceneData.Exits.Count;
                _entranceCount = SceneData.Entrances.Count;
            }
            //EditorUtility.SetDirty(this);
        }

        public List<EdgeData> GetOutgoingEdges()
        {
            return OutgoingEdges;
        }
    }
   

}