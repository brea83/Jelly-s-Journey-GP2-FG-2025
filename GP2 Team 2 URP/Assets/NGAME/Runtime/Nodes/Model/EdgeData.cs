
namespace NGAME
{
    [System.Serializable]
    public class EdgeData
    {
        public string SourceSceneGuid;
        public string SourcePortName;


        public string DestinationNodeGuid;
        public string DestinationSceneGuid;
        public string DestinationPortName;

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
