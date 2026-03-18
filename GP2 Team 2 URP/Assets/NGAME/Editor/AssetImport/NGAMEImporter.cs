using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace NGAME.Editor
{
    public class NGAMEImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            //throw new System.NotImplementedException();
        }

        //static List<RoomGraphNode> GetNexNodes(RoomGraphNode node)
        //{
        //    List<EdgeData> outgoingEdges = node.GetOutgoingEdges();
        //    foreach (EdgeData edge in outgoingEdges) 
        //    {
        //        edge.sourcePortName
        //    }
        //}
    }
}
