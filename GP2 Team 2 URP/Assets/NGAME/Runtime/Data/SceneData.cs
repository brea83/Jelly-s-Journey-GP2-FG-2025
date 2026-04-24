using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NGAME
{
    [System.Serializable]
    public class SceneData : ScriptableObject
    {
        public string Name;
        public string Guid;
        public string FilePath;
        public string Description;
        public SceneBounds Bounds;

        public List<RegionConnectionData> UniqueConnectionObjects;

        public List<SpawnerData> SpawnPoints;

        //public List<RegionConnectionData> GetEntrances();

    }
}
