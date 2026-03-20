using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.Text;

namespace NGAME.Editor
{
    [CreateAssetMenu]
    public class RoomGraph : ScriptableObject
    {
        public RoomNode rootNode;
        public List<RoomNode> nodes = new List<RoomNode>();
        //public List<Edge> Edges = new List<Edge>();
        
        
        public RoomNode CreateNode(System.Type type)
        {
            RoomNode node = ScriptableObject.CreateInstance(type) as RoomNode;
            node.name = type.Name;
            node.Guid  = GUID.Generate().ToString();
            nodes.Add(node);

            AssetDatabase.AddObjectToAsset(node, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            return node;
        }
        
        public void DeleteNode(RoomNode node)
        {
            nodes.Remove(node);
            AssetDatabase.RemoveObjectFromAsset(node);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        public void AddEdge(RoomNode sourceNode, RoomNode destinationNode, Edge edge)
        {
            //EdgeData newEdgeData = new EdgeData(edge.output.portName, child.Guid, edge.input.portName);
            //parent.OutgoingEdges.Add(newEdgeData);
            sourceNode.AddEdge(destinationNode, edge);
        }

        public void RemoveEdge(RoomNode sourceNode, RoomNode destinationNode, Edge edge)
        {
            //foreach(EdgeData edgeData in parent.OutgoingEdges)
            //{
            //    if(edgeData.sourcePortName == edge.output.portName && edgeData.destinationGuid == child.Guid && edgeData.destinationPortName == edge.input.portName)
            //    {
            //        parent.OutgoingEdges.Remove(edgeData);
            //    }
            //}
            sourceNode.RemoveEdge(destinationNode, edge);
        }

        public List<EdgeData> GetOutgoingEdges(RoomNode parent)
        {
            return parent.OutgoingEdges;
        }

        public void SetStartNode(RoomNode node)
        {
            if(rootNode != null)
            {
                rootNode.SetAsStartRoom(false);
            }
            rootNode = node;
            node.SetAsStartRoom(true);
        }

        
    }
}