using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NGAME
{
    public enum RegionConnectionType
    {
        EntranceOnly = 0,
        ExitOnly = 1,
        ExitAndEntrance = 2,
    }

    [Serializable]
    public class RegionConnectionData
    {
        public string TypeName = "IEncounterRegionConnector";
        public string Name = "Object Name";
        public RegionConnectionType ConnectionType;
        public bool IsLockable;
        public Vector3 Position;
    }
    [Serializable]
    public class SceneConnectionsData
    {
        public string SceneName;
        public string SceneGuid;
        public List<RegionConnectionData> Entrances;
        public List<RegionConnectionData> Exits;
        //public List<string> SpawnPositions;
        public Vector2 MinPoint = Vector3.zero;
        public Vector2 MaxPoint = Vector3.zero;

        public void UpdateBounds()
        {
            Vector3 min = CalculateMinBounds();
            Vector3 max = CalculateMaxBounds();
            MinPoint.x = min.x;
            MinPoint.y = min.z;

            MaxPoint.x = max.x;
            MaxPoint.y = max.z;

            MinPoint.Normalize();
            MaxPoint.Normalize();
        }
        public Vector3 CalculateMinBounds()
        {
            Vector3 min = new Vector2(float.MaxValue, float.MaxValue);

            foreach(NGAME.RegionConnectionData entrance in Entrances)
            {
                min = Vector3.Min(min, entrance.Position);
            }

            foreach(NGAME.RegionConnectionData exit in Exits)
            {
                min = Vector3.Min(min, exit.Position);
            }

            return min;
        }

        public Vector3 CalculateMaxBounds()
        {
            Vector3 max = new Vector2(float.MinValue, float.MinValue);

            foreach (NGAME.RegionConnectionData entrance in Entrances)
            {
                max = Vector3.Max(max, entrance.Position);
            }

            foreach (NGAME.RegionConnectionData exit in Exits)
            {
                max = Vector3.Max(max, exit.Position);
            }

            return max;
        }
    }
    public interface IEncounterRegionConnector
    {
        public RegionConnectionData GetRegionConnectionData();
        public void SetDestination(EdgeData edge);

        public UnityEvent<EdgeData> ConnectorActivated { get; }
    }
}
