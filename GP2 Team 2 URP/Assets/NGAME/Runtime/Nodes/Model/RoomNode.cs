using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public List<EdgeData> IncomingEdges = new List<EdgeData>();


        // private properties
        [SerializeField]
        private string m_Guid;
        [SerializeField]
        private Vector2 m_Position;
        [SerializeField]
        private bool _isStartNode = false;

        [SerializeField]
        public int NumberOfWaves = 0;
        //private int _exitCount;
        //private int _entranceCount;



        public void AddEdge(IMapNode otherNode, EdgeData edge)
        {
            if (otherNode is RoomNode)
            {
                if (edge.DestinationNodeGuid == Guid) 
                {
                    IncomingEdges.Add(edge); 
                }
                else
                {
                    OutgoingEdges.Add(edge);
                }
            }
        }

        public void RemoveEdge(IMapNode otherNode, EdgeData edge)
        {
            if(otherNode is RoomNode)
            {
                if (edge.DestinationNodeGuid == Guid)
                {
                    RemoveIncomingRoomEdge(edge);
                }
                else
                {
                    RemoveOutgoingRoomEdge(edge);
                }
            }
        }

        private void RemoveOutgoingRoomEdge(EdgeData edge)
        {
            List<int> indexesToRemove = new List<int>();

            for (int i = 0; i < OutgoingEdges.Count; i++)
            {
                EdgeData edgeData = OutgoingEdges[i];

                if (edgeData.SourcePortName == edge.SourcePortName && edgeData.DestinationNodeGuid == edge.DestinationNodeGuid && edgeData.DestinationPortName == edge.DestinationPortName)
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
        }

        private void RemoveIncomingRoomEdge( EdgeData edge)
        {
            List<int> indexesToRemove = new List<int>();

            for (int i = 0; i < IncomingEdges.Count; i++)
            {
                EdgeData edgeData = IncomingEdges[i];

                if (edgeData.SourcePortName == edge.SourcePortName && edgeData.DestinationNodeGuid == edge.DestinationNodeGuid && edgeData.DestinationPortName == edge.DestinationPortName)
                {
                    indexesToRemove.Add(i);
                }
            }

            indexesToRemove.Sort();
            for (int i = indexesToRemove.Count - 1; i >= 0; i--)
            {
                IncomingEdges.RemoveAt(indexesToRemove[i]);
            }
        }

        public void SetAsStartRoom(bool isStartNode)
        {
            _isStartNode = isStartNode;
        }
        public void UpdateRoomData(NGAME.SceneConnectionsData room)
        {
            SceneData = room;
            //if (SceneData == null)
            //{
            //    _exitCount = 0;
            //    _entranceCount = 0;
            //}
            //else
            //{
            //    _exitCount = SceneData.Exits.Count;
            //    _entranceCount = SceneData.Entrances.Count;
            //}
            //EditorUtility.SetDirty(this);
        }

        public List<EdgeData> GetOutgoingEdges()
        {
            return OutgoingEdges;
        }

        public List<EdgeData> GetIncomingEdges()
        {
            return IncomingEdges;
        }

        public string PrintNode()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Room Node guid: " + Guid + "\n");
            if (this.SceneData != null) 
            {
                sb.Append("Scene: " + this.SceneData.SceneName + "\n");
            }
            else
            {
                sb.Append("No Scene Data \n");
            }

            foreach (EdgeData edgeData in OutgoingEdges)
            {
                sb.Append("Exit: " + edgeData.SourcePortName + ", connects to ");
                sb.Append(edgeData.DestinationPortName + ", in Node: " + edgeData.DestinationNodeGuid);
                sb.Append("\n");
            } 

            return sb.ToString();
        }

        public List<EdgeData> GetAllEdgesAsOutgoing()
        {
            List<EdgeData> results = new();

            results.AddRange(OutgoingEdges);

            foreach(EdgeData edge in IncomingEdges)
            {
                results.Add(EdgeData.Invert(edge));
            }

            return results;
        }
    }
   

}