using UnityEngine;

namespace NGAME
{
    public enum RegionConnectionType
    {
        TwoWay = 0,
        ExitOnly = 1,
        EntranceOnly = 2,
    }

    public class RegionConnectionData
    {
        public RegionConnectionType ConnectionType;
        public bool IsLockable;
        public Vector3 Position;
    }
    public interface IEncounterRegionConnector
    {
        public RegionConnectionData GetRegionConnectionData();
    }
}
