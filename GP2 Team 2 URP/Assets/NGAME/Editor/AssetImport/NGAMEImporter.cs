using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;

namespace NGAME.Editor
{
    public class NGAMEImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            Dictionary<string, RuntimeNode> guidToNode = new Dictionary<string, RuntimeNode>();


        }

        

        static List<RuntimeNode> TranslateNodeModelToRuntimeNodes(IMapNode rootMapNode)
        {
           
            return new List<RuntimeNode>();
        }

        //static T GetInputPortValue<T>(Port port)
        //{
        //    return new T();
        //}


    }
}
