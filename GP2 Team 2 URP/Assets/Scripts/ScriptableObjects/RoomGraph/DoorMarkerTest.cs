using UnityEngine;
using NGAME;

public class DoorMarkerTest : MonoBehaviour, IEncounterRegionConnector
{

    [SerializeField]
    private RegionConnectionType m_ConnectionType = RegionConnectionType.ExitAndEntrance;

    [SerializeField] 
    private bool m_IsLockable = false;
    public RegionConnectionData GetRegionConnectionData()
    {
        RegionConnectionData result = new RegionConnectionData();

        result.TypeName = "DoorMarkerTest";
        result.ConnectionType = m_ConnectionType;
        result.Position = transform.position;
        result.IsLockable = m_IsLockable;

        return result;
    }
}
