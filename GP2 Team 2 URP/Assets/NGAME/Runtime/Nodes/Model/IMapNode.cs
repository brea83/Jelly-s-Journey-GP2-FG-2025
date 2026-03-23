using System.Collections.Generic;
using UnityEngine;

namespace NGAME
{
    public interface IMapNode
    {
        public void AddEdge(IMapNode otherNode, EdgeData edge);
        public void RemoveEdge(IMapNode otherNode, EdgeData edge);
        public List<EdgeData> GetOutgoingEdges();

        [HideInInspector] public Vector2 Position { get; set; }
        [HideInInspector] public string Guid { get; set; }

    }


}
