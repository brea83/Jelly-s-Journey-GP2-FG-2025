using EncounterSystem;
using System;
using UnityEngine;
using UnityEngine.Events;
using NGAME;
using System.Xml.Serialization;

namespace RoomSystem
{
    public enum RoomConnectionDirection { North, East, South, West }
    public enum ConnectionType { Entrance, Exit, ExitAndEntrance }
    

    public class Door : MonoBehaviour, IEncounterRegionConnector
    {
        public UnityEvent DoorUnlocked;
        public UnityEvent DoorLocked;
        public UnityEvent<EdgeData> DoorUsed;
        public EdgeData OutgoingEdge;

        public bool IsLockable = true;
        public bool StartsLocked = false;
        public bool LocksDurringCombat = true;
        public bool isDefaultEntrance = false;
        public bool UseOldBarrierAnimation = true;
        
        GameState _updateState;
        [SerializeField] private DoorData _data = new DoorData();//GUID.Generate().ToString(), ConnectionType.Exit, transform.position);
        //private RoomDoorData _connectedRoomEntrance = null;
        public RoomConnectionDirection Direction {  get { return _data.Direction; } }
        public string Destination {  get { return _data.DestinationSceneName; } }
        public bool Locked { get { return _data.Locked; } }
        //public bool useRealDirectionFlow = true;
        public bool UsedState { get { return _data.UsedOnce; } }
        private bool _inUse = false;
        public DoorData Data { get { return _data; } }

        public UnityEvent<EdgeData> ConnectorActivated => DoorUsed;


        private GameObject _barrier;

        [Header("Debug tools")]
        public Material UnlockedMaterial;
        public Material LockedMaterial;
        public Vector3 UnlockedOffset;
        private Vector3 _barrierLockedPosition;

        public float TempUnlockSpeed = 1f;
        private float _timer = 0f;
        private bool _isUnlocking;
        private bool _hasDisabled = false;
        
        private void OnEnable()
        {
            _data.ExitPosition = transform.position;
            EntrancePoint entrance = GetComponentInChildren<EntrancePoint>();
            if (entrance != null)
            {
                _data.EntrancePosition = entrance.transform.position;
                _data.EntranceRotation = entrance.transform.rotation;
            }
            _data.Name = name;
            
            if (_hasDisabled)
            {
                SubscribeToPlayState();
                if (LocksDurringCombat)
                {
                    //SpawnManager spawnManager = FindFirstObjectByType<SpawnManager>();
                    //if (spawnManager != null)
                    //{
                    //    spawnManager.OnEncounterStart.AddListener(OnEncounterStart);
                    //    spawnManager.OnEncounterEnd.AddListener(OnEncounterEnd);
                    //}

                    NewEncounterManager encounterManager = GameManager.Instance.EncounterManager;
                    if( encounterManager != null )
                    {
                        encounterManager.OnEncounterStart.AddListener(OnEncounterStart);
                        encounterManager.OnEncounterEnd.AddListener(OnEncounterEnd);
                    }
                }
            }
        }
            
        private void Start()
        {
            Transform childTransform = this.gameObject.FindComponentInChildWithTag<Transform>("Barrier");//this.transform.Find("Barrier");
            
            if (childTransform != null)
            {
                _barrier = childTransform.gameObject;
                _barrierLockedPosition = _barrier.transform.localPosition;
                //Debug.Log($"found {_barrier.name} in {this.name}");
            }
            if (!Locked)
            {
                UnlockDoor();
            }
            SubscribeToPlayState();

        }
        private void OnDisable()
        {
            UnsubscribeToPlayState();
            if(LocksDurringCombat)
            {
                //SpawnManager spawnManager = FindFirstObjectByType<SpawnManager>();
                //if (spawnManager != null)
                //{
                //    spawnManager.OnEncounterStart.RemoveListener(OnEncounterStart);
                //    spawnManager.OnEncounterEnd.RemoveListener(OnEncounterEnd);
                //}

                NewEncounterManager encounterManager = GameManager.Instance.EncounterManager;
                if (encounterManager != null)
                {
                    encounterManager.OnEncounterStart.RemoveListener(OnEncounterStart);
                    encounterManager.OnEncounterEnd.RemoveListener(OnEncounterEnd);
                }
            }
            _hasDisabled = true;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!_inUse
                && !Locked
                && other.GetComponent<PlayerController>() != null
                && other.CompareTag("Player"))
            {
                //player stepped into the exit
                _inUse = true;

                //RoomNavigator.Instance.TravelThroughDoor(this, other.gameObject);

                if (DoorUsed != null) DoorUsed.Invoke(OutgoingEdge);
            }
        }

        public void OnEncounterStart()
        {
            if (LocksDurringCombat)
            {
                LockDoor();
            }
        }

        public void OnEncounterEnd()
        {
            if (LocksDurringCombat)
            {
                UnlockDoor();
            }
        }

        public void LockDoor()
        {
            Data.Locked = true;
            if (_barrier == null) { return; }
            if(UseOldBarrierAnimation)
            {
                _isUnlocking = false;
                _barrier.GetComponent<BoxCollider>().enabled = true;
                //temp animation, remove this when animation is calling this function
                _barrier.transform.localPosition = _barrierLockedPosition;
                MeshRenderer barrierMesh = _barrier.GetComponent<MeshRenderer>();
                if(barrierMesh != null )
                {
                    barrierMesh.material = LockedMaterial;
                }
            }
            else
            {
                _barrier.SetActive(true);
            }
            DoorLocked?.Invoke();
        }

        public void UnlockDoor()
        {
            Data.Locked = false;
            if (_barrier == null) {  return; }
            if(UseOldBarrierAnimation)
            {
                _barrier.GetComponent<BoxCollider>().enabled = false;
                //temp "animation", remove this when animation is calling this function
                _isUnlocking = true;
                _timer = 0f;
                _barrier.transform.localPosition = UnlockedOffset;
                MeshRenderer barrierMesh = _barrier.GetComponent<MeshRenderer>();
                if (barrierMesh != null)
                {
                    barrierMesh.material = UnlockedMaterial;
                }
            }
            else
            {
                _barrier.SetActive(false);
            }
            DoorUnlocked?.Invoke();
                
        }
        private void ManagedUpdate()
        {

        }
        private void ManagedFixedUpdate()
        {
            if (_isUnlocking)
            {
                _barrier.transform.localPosition = Vector3.Lerp(_barrier.transform.localPosition, UnlockedOffset, _timer);
                if(_timer >= TempUnlockSpeed)
                {
                    _barrier.transform.localPosition = UnlockedOffset;
                    _timer = 0f;
                    _isUnlocking = false;
                }
                _timer += Time.fixedDeltaTime;
            }
        }
        private void SubscribeToPlayState()
        {
            _updateState = GameManager.Instance.GetState<PlayingState>();
            if (_updateState != null)
            {
                _updateState.StateUpdate.AddListener(ManagedUpdate);
                _updateState.StateFixedUpdate.AddListener(ManagedFixedUpdate);
            }
            else
            {
                Debug.Log("tried to add listener but myUpdateState == null");
            }
        }
        private void UnsubscribeToPlayState()
        {
            if (_updateState != null)
            {
                _updateState.StateUpdate.RemoveListener(ManagedUpdate);
                _updateState.StateFixedUpdate.RemoveListener(ManagedFixedUpdate);
            }
            else
            {
                Debug.Log("tried to remove listeners but myUpdateState == null");
            }
        }

        public RegionConnectionData GetRegionConnectionData() 
        {
            RegionConnectionData data = new RegionConnectionData();
            data.TypeName = this.GetType().FullName;
            data.Name = name;
            data.ConnectionType = _data.Type;

            data.IsLockable = IsLockable;
            data.StartsLocked = StartsLocked;
            data.IsLockedDurringCombat = LocksDurringCombat;
            
            data.Position = transform.position;
            return data;
        }

        public void SetDestination(EdgeData edge)
        {
            _data.DestinationSceneName = edge.DestinationSceneName;
            _data.DestinationPointName = edge.DestinationPortName;

            OutgoingEdge = edge;
        }

        public void InitializeFromGraphData(RegionConnectionData connectionData, EdgeData edge)
        {
            if(edge == null || string.IsNullOrEmpty(connectionData.TypeName))
            {
                LockDoor();
                return;
            }
            
            LocksDurringCombat = connectionData.IsLockedDurringCombat;

            StartsLocked = connectionData.StartsLocked;
            if ( StartsLocked && !Locked )
                LockDoor();
            else if( !StartsLocked && Locked )
                UnlockDoor();

            if (LocksDurringCombat)
            {
                NewEncounterManager encounterManager = GameManager.Instance.EncounterManager;
                if (encounterManager != null)
                {
                    encounterManager.OnEncounterStart.AddListener(OnEncounterStart);
                    encounterManager.OnEncounterEnd.AddListener(OnEncounterEnd);
                }
            }
                
            SetDestination(edge);
            
        }
    }
        
    [Serializable]
    public class DoorData 
    {
        [HideInInspector] public string Name;
        public RoomConnectionDirection Direction;
        //[HideInInspector] 
        public string ParentRoom;
        public string DestinationSceneName;
        public string DestinationPointName;
        [HideInInspector] public bool UsedOnce = false;
        public bool Locked = false;
        public bool UseRealDirectionFlow = true;
        //public ConnectionType Type;
        public RegionConnectionType Type;
        [HideInInspector] public Vector3 ExitPosition;
        [HideInInspector] public Vector3 EntrancePosition;
        [HideInInspector] public Quaternion EntranceRotation;

        public bool IsEquivalent(DoorData other)
        {
            bool printDebug = RoomNavigator.Instance.PrintDebugLogs;
            if (printDebug) 
            {
                Debug.Log("---COMPARING DOOR DATA ---");
                Debug.Log($"current name: {Name} compared to: {other.Name}");
                Debug.Log($"current ParentRoom: {ParentRoom} compared to: {other.ParentRoom}");
                Debug.Log($"current Direction: {Direction} compared to: {other.Direction}");
                Debug.Log($"current directionflow bool: {UseRealDirectionFlow} compared to: {other.UseRealDirectionFlow}");
            }
            if (other.ParentRoom == ParentRoom
                && other.Direction == Direction
                && other.UseRealDirectionFlow == UseRealDirectionFlow
                && other.Name == Name)
            {
                return true;
            }
            return false;
        }
        public bool IsArrivalFlowValid(Door arrivalDoor)
        {
            DoorData exitDoor = this;
            RegionConnectionType arrivalType = arrivalDoor.Data.Type;
            if ((exitDoor.Type == RegionConnectionType.ExitAndEntrance || exitDoor.Type == RegionConnectionType.ExitOnly)
                && (arrivalType == RegionConnectionType.ExitAndEntrance || arrivalType == RegionConnectionType.EntranceOnly))
            {
                if (UseRealDirectionFlow)// || arrivalDoor.Data.UseRealDirectionFlow)
                {
                    RoomConnectionDirection validArrivalDirecton = RealisticDirectionOpposite();
                    return arrivalDoor.Direction == validArrivalDirecton;
                }
                else if (arrivalDoor.Destination == exitDoor.ParentRoom
                    || arrivalDoor.isDefaultEntrance)
                {
                    
                    return true;
                }
            }
            return false;
        }
        private RoomConnectionDirection RealisticDirectionOpposite()
        {
            switch (Direction)
            {
                case RoomConnectionDirection.North:
                    return RoomConnectionDirection.South;
                case RoomConnectionDirection.East:
                    return RoomConnectionDirection.West;
                case RoomConnectionDirection.South:
                    return RoomConnectionDirection.North;
                case RoomConnectionDirection.West:
                    return RoomConnectionDirection.East;
                default:
                    Debug.LogWarning("tried to get the arrival direction for a door that doesn't have a direction assigned. returning north as default");
                    return RoomConnectionDirection.North;
            }
        }
    }
}