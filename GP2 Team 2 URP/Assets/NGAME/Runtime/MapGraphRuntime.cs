using System.Collections.Generic;
using UnityEngine;

namespace NGAME
{
    public class MapGraphRuntime : ScriptableObject
    {
        [SerializeReference]
        public List<RoomRuntimeNode> Nodes = new();
    }
}
