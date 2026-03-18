using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGAME
{
    [Serializable]
    public class RuntimeRoomNode : RuntimeNode
    {
        public string SceneGuid;

        public List<RegionConnectionData> ConnectedEntrances;
        public List<RegionConnectionData> ConnectedExits;

        public List<string> EntranceSourceSceneGuids;
        public List<string> ExitDestinationSceneGuids;
    }
}
