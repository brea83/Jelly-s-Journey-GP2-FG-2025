using UnityEngine;
using NGAME;

public class DoorMarkerTest : MonoBehaviour, IEncounterRegionConnector
{

    [SerializeField]
    private RegionConnectionType m_ConnectionType = RegionConnectionType.TwoWay;

    [SerializeField] 
    private bool m_IsLockable = false;
    public RegionConnectionData GetRegionConnectionData()
    {
        RegionConnectionData result = new RegionConnectionData();

        result.ConnectionType = m_ConnectionType;
        result.Position = transform.position;
        result.IsLockable = m_IsLockable;

        return result;
    }
}
