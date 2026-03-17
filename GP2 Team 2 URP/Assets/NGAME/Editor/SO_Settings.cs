using System;
using System.Collections.Generic;
using UnityEngine;


namespace NGAME.Editor
{
    [Serializable]
    public class SceneData
    {
        public string Name = "default name";
        public string Guid;
        public string FilePath = "";
        public bool IncludeInGraphTool = false;
        public string Description = "";
        public bool IsMarkedForDelete = false;
    }

    [CreateAssetMenu(fileName = "SO_Settings", menuName = "Scriptable Objects/SO_Settings")]
    public class SO_Settings : ScriptableObject
    {
        public string Name = "Default Settings";
        public string Guid;
        //public Dictionary<string, SceneData> GuidToSceneData;
        public List<string> Guids;
        public List<SceneData> Scenes;
    }
}
