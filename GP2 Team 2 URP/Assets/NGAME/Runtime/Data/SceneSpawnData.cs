using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGAME
{
    [Serializable]
    public class SceneSpawnData
    {
        public string SceneGUID;
        public List<SpawnerData> SpawnPoints;
    }
}
