using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGAME
{
    [Serializable]
    public class SpawnerData
    {
        public string Name;
        public List<SO_SpawnTypeTag> ValidTypes = new();
        public Vector3 Position;
    }
}
