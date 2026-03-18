using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace NGAME.Editor
{
    public abstract class RoomGraphNode : ScriptableObject
    {
        [HideInInspector] public Vector2 Position;
    }
    [System.Serializable]
    public class RoomNode : RoomGraphNode
    {
        [HideInInspector] public string Guid;
        public NGAME.SceneConnectionsData Room = null;
        [HideInInspector] public int LastDropdownIndex;
        [HideInInspector] public List<EdgeData> OutgoingEdges = new List<EdgeData>();
        private bool _isStartNode = false;
        private int _exitCount;
        private int _entranceCount;
        public int ExitCount { get {return _exitCount; } }
        public int EntranceCount { get { return _entranceCount; } }

        public void SetAsStartRoom(bool isStartNode)
        {
            _isStartNode = isStartNode;
        }
        public void UpdateRoomData(NGAME.SceneConnectionsData room)
        {
            Room = room;
            _exitCount = Room.Exits.Count;
            _entranceCount = Room.Entrances.Count;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

    }
    [System.Serializable]
    public class EdgeData
    {
        public string sourcePortName;
        public string destinationGuid;
        public string destinationPortName;

        public EdgeData(string sourcePortName, string destinationGuid, string destinationPortName)
        {
            this.sourcePortName = sourcePortName;
            this.destinationGuid = destinationGuid;
            this.destinationPortName = destinationPortName;
        }
    }

}