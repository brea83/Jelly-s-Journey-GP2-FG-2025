using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace NGAME.Editor
{
    public interface IMapNode
    {
        public void AddEdge(RoomNode otherNode, Edge edge);
        public void RemoveEdge(RoomNode otherNode, Edge edge);
        public List<EdgeData> GetOutgoingEdges();
    }
}
