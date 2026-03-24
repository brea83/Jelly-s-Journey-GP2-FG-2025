
using NUnit.Framework.Interfaces;

namespace NGAME
{
    [System.Serializable]
    public class EdgeData
    {
        public string SourceNodeGuid = "";
        public string SourceSceneGuid = "";
        public string SourceSceneName = "";
        public string SourcePortName = "";


        public string DestinationNodeGuid = "";
        public string DestinationSceneGuid = "";
        public string DestinationSceneName = "";
        public string DestinationPortName = "";

        public EdgeData()
        { }

        public static EdgeData Invert(EdgeData otherEdge)
        {
            EdgeData result = new();

            result.SourceNodeGuid = otherEdge.DestinationNodeGuid;
            result.SourceSceneGuid = otherEdge.DestinationSceneGuid;
            result.SourceSceneName = otherEdge.DestinationSceneName;
            result.SourcePortName = otherEdge.DestinationPortName;

            result.DestinationNodeGuid = otherEdge.SourceNodeGuid;
            result.DestinationSceneGuid = otherEdge.SourceSceneGuid;
            result.DestinationSceneName = otherEdge.SourceSceneName;
            result.DestinationPortName = otherEdge.SourcePortName;

            return result;
        }
        public EdgeData(string sourcePortName, string sourceSceneGuid, string destinationNodeGuid, string destinationSceneGuid, string destinationPortName)
        {

            SourcePortName = sourcePortName;
            SourceSceneGuid = sourceSceneGuid;

            DestinationNodeGuid = destinationNodeGuid;
            DestinationSceneGuid = destinationSceneGuid;
            DestinationPortName = destinationPortName;
        }
    }
}
