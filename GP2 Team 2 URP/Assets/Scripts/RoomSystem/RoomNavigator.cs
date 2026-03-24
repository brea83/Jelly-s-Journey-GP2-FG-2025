using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using FMODUnity;
using UnityEngine.Windows;
using System.Collections;

namespace RoomSystem {
    public class RoomNavigator : MonoBehaviour
    {
        public static RoomNavigator Instance { get; private set; }
        public UnityEvent RoomLoadStart;
        public UnityEvent<Room> RoomLoadComplete;
        public Vector3 NoDoorsFallbackEntrance = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private List<Room> _rooms = new List<Room>();
        public int RoomCount { get { return _rooms.Count; } }
        private Queue<Room> _roomQueue = new Queue<Room>();

        private Room _currentRoom;
        [HideInInspector] public bool IsCurrentRoomBacktracking = false;
        private Room _nextRoom;
        private Room _previousRoom;
        private DoorData _mostRecentExit;

        //private Scene _previousScene;
        GameObject _player;
        PlayerInputMapSwapper _playerInputMapSwapper;
        [Header("Room Load Effects")]
        public CircleWipeControler CircleWipe;

        [Header("Debug stuff")]
        public bool EnableNavigation = true;
        public bool PrintDebugLogs = false;
        private bool _hasDisabled = false;
        private bool _navigationStarted = false;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            //Initialize();
        }
        private void Start()
        {
            GameManager.Instance.StateEnter.AddListener(OnStateEnter);
            _player = GameManager.Instance.player;
            _playerInputMapSwapper = _player.GetComponent<PlayerInputMapSwapper>();

        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            if(_hasDisabled) GameManager.Instance.StateEnter.AddListener(OnStateEnter);
        }
        private void OnDisable()
        {
            GameManager.Instance.StateEnter.RemoveListener(OnStateEnter);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _hasDisabled = true;
        }
        public void Initialize()
        {
            _navigationStarted = false;
            _roomQueue.Clear();
            _currentRoom = null;
            _nextRoom = null;
            _previousRoom = null;
            _mostRecentExit = null;
            foreach (Room room in _rooms)
            {
                room.Initialize();
                _roomQueue.Enqueue(room);
            }
        }
        private void OnStateEnter(GameState state)
        {
            if (!EnableNavigation || RoomCount < 1) return;

            GameState previousState = GameManager.Instance.previousState;
            if (state is PlayingState 
                && !_navigationStarted
                && previousState != null
                && (previousState is StartState//.GetType() == typeof(StartState)
                    || previousState is RestartState//previousState.GetType() == typeof(RestartState)
                    || previousState is EndState))//previousState.GetType() == typeof(EndState)))
            {
                Initialize();
                LoadFirstRoom();
                _navigationStarted = true;
            } 
           
        }
        public Room GetNextRoom()
        {
            Room next = null;
            if (_roomQueue.Count > 0)
            {
                next = _roomQueue.Dequeue();
                if (PrintDebugLogs) Debug.Log($"dequed {next.SceneName} from the room queue");
            }
            //_roomQueue.Enqueue(next);
            return next;
        }
        public void TravelThroughDoor(Door exit, GameObject player)
        {
            if (PrintDebugLogs) Debug.Log($"-------------------USING A DOOR-------------------");
            if (exit.Destination != "")
            {
                if (PrintDebugLogs) Debug.Log($"using door ({exit}) from room {_currentRoom.SceneName}, next room is {_nextRoom?.SceneName}, but door has a destinaton of {exit.Destination}");
                // I think I need to put the next room back in queue
                if (_nextRoom != null)
                {
                    Queue<Room> tempQueue = new Queue<Room>();
                    //tempQueue.Enqueue(_currentRoom);
                    tempQueue.Enqueue(_nextRoom);
                    foreach (Room room in _roomQueue)
                    {
                        tempQueue.Enqueue(room);
                    }
                    _roomQueue.Clear();
                    _roomQueue = tempQueue;
                    if (PrintDebugLogs) Debug.Log("Replacing next room to start of room queue");
                }
                _nextRoom = GetRoomBySceneName(exit.Destination);
                if (PrintDebugLogs) Debug.Log($"Setting next room to exit destination ({exit.Destination})");
            }

            if (_nextRoom == null) return;
            _previousRoom = LeaveRoom(exit, player);
            if(CircleWipe != null)
            {
                StartCoroutine(LoadAfterSeconds(CircleWipe.FadeSeconds));
            }
            else
            {
                if (RoomLoadStart != null) RoomLoadStart.Invoke();
                RuntimeManager.PlayOneShot("event:/sfx/RoomTransition");
                SceneManager.LoadScene(_nextRoom.SceneName, LoadSceneMode.Single);
            }
        }

        private IEnumerator LoadAfterSeconds(float seconds)
        {
            if (RoomLoadStart != null) RoomLoadStart.Invoke();
            yield return new WaitWhile(() =>
            {
                return CircleWipe.DoingFadeOut == true;
            });
            RuntimeManager.PlayOneShot("event:/sfx/RoomTransition");
            SceneManager.LoadScene(_nextRoom.SceneName, LoadSceneMode.Single);
        }
        private Room LeaveRoom(Door exit, GameObject player)
        {
            //_previousScene = SceneManager.GetActiveScene();

           // if (!exit.UsedState)
            //{
            if(PrintDebugLogs) Debug.Log($"leaving room {_currentRoom.SceneName}, current exit is {exit}, next room is {_nextRoom.SceneName}");
            foreach (DoorData data in _currentRoom.doors) 
            {
                if (exit.Data.IsEquivalent(data))
                {
                    if(data.DestinationSceneName == "") data.DestinationSceneName = _nextRoom.SceneName;

                    _mostRecentExit = data;
                    _mostRecentExit.UsedOnce = true;
                    if (PrintDebugLogs) Debug.Log("Found the doordata that matches our current exit and updated our most recent exit, and it's used state to true");
                    break;
                }
            }
            //}

            //PlayerInputMapSwapper _playerInputMapSwapper = _player.GetComponent<PlayerInputMapSwapper>();//<PlayerInput>();
            if (_playerInputMapSwapper != null)
            {
                if(PrintDebugLogs) Debug.Log($" room navigator is trying to disable  current actionmap {_playerInputMapSwapper.Input.currentActionMap?.name}");
                _playerInputMapSwapper.ToggleInputsEnabled(false);
            }
            _player.GetComponent<PlayerController>().ChangeDash();
            _player.SetActive(false);
            if (PrintDebugLogs) Debug.Log($"disabling player to leave {_currentRoom.SceneName}");
            _currentRoom.Visited = true;
            return _currentRoom;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (PrintDebugLogs) Debug.Log($"-------------------LOADING SCENE {scene.name}-------------------");
            if (_nextRoom != null
                && scene.name == _nextRoom.SceneName)
            {
                if (PrintDebugLogs) Debug.Log("just loaded scene is our next room");
                _currentRoom = EnterRoom(_nextRoom, _mostRecentExit);
                _nextRoom = GetNextRoom();
                if (RoomLoadComplete != null) RoomLoadComplete.Invoke(_currentRoom);
                if (PrintDebugLogs) Debug.Log($"finished entering {_currentRoom.SceneName}, now next room is {_nextRoom?.SceneName}");
            }
        }

        private Room EnterRoom(Room nextRoom, DoorData mostRecentExit) 
        {
            //if (_currentRoom == null) _currentRoom = nextRoom;
        
            UpdateRoomDoors(nextRoom, mostRecentExit);

            IsCurrentRoomBacktracking = nextRoom.Visited;
            DoorData currentEntrance = nextRoom.GetCurrentEntrance();
            if (currentEntrance != null && _previousRoom != null)
            {
                currentEntrance.UsedOnce = true;
                currentEntrance.DestinationSceneName = _previousRoom.SceneName;
            }
            if (PrintDebugLogs) Debug.Log($"entering room: {nextRoom.SceneName} at {nextRoom.GetCurrentEntrance().Name}, From previous door {mostRecentExit} in room {mostRecentExit?.ParentRoom}. current room has been visited is = {nextRoom.Visited}");
            _player.transform.SetPositionAndRotation(nextRoom.GetEntrancePosition(NoDoorsFallbackEntrance), nextRoom.GetEntranceRotation());
            _player.SetActive(true);
            //PlayerInputMapSwapper _playerInputMapSwapper = _player.GetComponent<PlayerInputMapSwapper>();//<PlayerInput>();
            if (_playerInputMapSwapper != null)
            {
                if (PrintDebugLogs) Debug.Log($"room navigator is trying to enable current actionmap {_playerInputMapSwapper.Input.currentActionMap?.name}");
                _playerInputMapSwapper.ToggleInputsEnabled();
            }

            return nextRoom;
        }

        private void UpdateRoomDoors(Room room, DoorData previousRoomExit)
        {
            UnityEngine.Object[] possibleDoors = FindObjectsByType(typeof(Door), FindObjectsSortMode.None);
            List<Door> doors = new List<Door>();
            foreach(var item in possibleDoors)
            {
                if(item is Door)
                {
                    Door door = (Door)item;
                    doors.Add(door);
                }
            }
            if (PrintDebugLogs) Debug.Log($"Found {doors.Count} doors");
                room.UpdateDoors(doors, previousRoomExit);

            if (PrintDebugLogs)
            {
                foreach(DoorData door in room.doors)
                {
                    Debug.Log($"{door.Name}, direction type {door.Direction}, in room {door.ParentRoom}, with destination: {door.DestinationSceneName}");
                }
            }
        }

        private Room GetRoomBySceneName(string searchName)
        {
            foreach (Room room in _rooms)
            {
                if(room.SceneName == searchName)
                {
                    return room;
                }
            }
            return _currentRoom;
        }
        
        public List<string> GetListOfRoomNames()
        {
            List<string> list = new List<string>();
            foreach (Room room in _rooms)
            {
                list.Add(room.SceneName);
            }
            return list;
        }
        public void LoadFirstRoom()
        {
            _nextRoom = GetNextRoom();
            
            //PlayerInputMapSwapper _playerInputMapSwapper = _player.GetComponent<PlayerInputMapSwapper>();//<PlayerInput>();
            if(_playerInputMapSwapper != null )
            {
                if (PrintDebugLogs) Debug.Log($" room navigator is loading first room and trying to disable  current actionmap {_playerInputMapSwapper.Input.currentActionMap?.name}");
                _playerInputMapSwapper.ToggleInputsEnabled(false);
            }
            _player.SetActive(false);

            if (CircleWipe != null)
            {
                StartCoroutine(LoadAfterSeconds(CircleWipe.FadeSeconds));
            }
            else
            {
                if (RoomLoadStart != null) RoomLoadStart.Invoke();
                RuntimeManager.PlayOneShot("event:/sfx/RoomTransition");
                SceneManager.LoadScene(_nextRoom.SceneName, LoadSceneMode.Single);
            }
            //SceneManager.LoadScene(_nextRoom.SceneName, LoadSceneMode.Single);
        }


       
    }
}