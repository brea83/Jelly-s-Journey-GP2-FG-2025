
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using static Unity.Cinemachine.CinemachineSplineRoll;

namespace RoomSystem
{
    [System.Serializable]
    public class Room 
    {
        [HideInInspector] public List<DoorData> doors = new List<DoorData>();

        public string SceneName;
        private DoorData _fallbackEntrance;
        private DoorData _currentEntrance;
        //public bool RespawnsOnBacktrack;
        [HideInInspector] public bool Visited = false;

        public void Initialize()
        {
            doors.Clear();
            _fallbackEntrance = null;
            _currentEntrance = null;
            Visited = false;
        }
        public void UpdateDoors(List<Door> newDoors, DoorData previousRoomExit)
        {
            bool printDebug = RoomNavigator.Instance.PrintDebugLogs;
            
            List<DoorData> updatedDoors = new List<DoorData>();
            _currentEntrance = null;
            if(doors.Count > 0)
            {
                if (printDebug) Debug.Log($"------- UPDATING {SceneName} DOORS -------");
                foreach (Door newDoor in newDoors)
                {
                    DoorData newData = newDoor.Data;
                    newData.ParentRoom = this.SceneName;
                    foreach (DoorData oldData in doors)
                    {
                        if (oldData.IsEquivalent(newData))
                        {
                            newData.DestinationSceneName = oldData.DestinationSceneName;
                            newData.UsedOnce = oldData.UsedOnce;

                            //if(_fallbackEntrance.Data == oldData)
                            //{ _fallbackEntrance = newDoor; }
                            updatedDoors.Add(newDoor.Data);
                            if (printDebug) Debug.Log($"updating new {newData.Name} with old {oldData.Name}'s info");
                        }
                    }
                    if (previousRoomExit != null 
                        && previousRoomExit.IsArrivalFlowValid(newDoor))
                    {
                        _currentEntrance = newDoor.Data;
                    }
                    else if(_fallbackEntrance == null && newDoor.isDefaultEntrance)
                    {
                        _fallbackEntrance = newDoor.Data;
                    }
                }
                
                doors.Clear();
                doors.AddRange(updatedDoors);
            }
            else
            {
                SetDoors(newDoors, previousRoomExit);
            }
        }

        public void SetDoors(List<Door> newDoors, DoorData previousRoomExit)
        {
            List<DoorData> newData = new List<DoorData>();
            if (newDoors.Count > 0)
            {
                foreach(Door door in newDoors) 
                {
                    door.Data.ParentRoom = this.SceneName;
                    newData.Add(door.Data);
                    if (_fallbackEntrance == null && door.isDefaultEntrance)
                    {
                        _fallbackEntrance = door.Data;
                    }

                    if (previousRoomExit != null
                        && previousRoomExit.IsArrivalFlowValid(door))
                    {
                        _currentEntrance = door.Data;
                    }

                }
                doors.Clear();
                doors.AddRange(newData);
                if(_fallbackEntrance == null)
                {
                    _fallbackEntrance = newDoors.First().Data;
                }
                //_currentEntrance = _fallbackEntrance;
            }
        }
        public Vector3 GetEntrancePosition(Vector3 noDoorsEntrance)
        {
            if (_currentEntrance != null)
            {
                return _currentEntrance.EntrancePosition;
            }
            else if(_fallbackEntrance != null)
            {
                return _fallbackEntrance.EntrancePosition;
            }
            else
            {
                return noDoorsEntrance;
            }
        }
        public Quaternion GetEntranceRotation()
        {
            if (_currentEntrance != null)
            {
                return _currentEntrance.EntranceRotation;
            }
            else if (_fallbackEntrance != null)
            {
                return _fallbackEntrance.EntranceRotation;
            }
            else
            {
                return Quaternion.identity;
            }
        }
        public DoorData GetCurrentEntrance() 
        {
            if (_currentEntrance != null)
            {
                return _currentEntrance;
            }
            else if (_fallbackEntrance != null)
            {
                //_currentEntrance = _fallbackEntrance;
                return _fallbackEntrance;
            }
            else
            {
                return null;
            }
        }


    }


}