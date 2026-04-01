using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace NGAME
{
    public class MapGraphRuntime : MonoBehaviour
    {
        public UnityEvent RoomLoadStart;
        public UnityEvent<IEncounterRegionConnector> RoomLoadComplete;
        //[Header("Room Load Effects")]
        //public CircleWipeControler CircleWipe;

        public RoomNode CurrentRoom {get => m_CurrentRoom;}
        public EdgeData LastTraversedEdge { get => m_MostRecentlyTraversedEdge;}
        public List<ISpawnPoint> CurrentSpawnPoints { get => m_CurrentSpawnPoints;}

        [Header("Debug stuff")]
        public bool EnableNavigation = true;
        public bool PrintDebugLogs = false;

        [SerializeField]
        protected RoomGraph m_Graph;
        protected RoomNode m_CurrentRoom;
        protected EdgeData m_MostRecentlyTraversedEdge;
        protected List<EdgeData> m_CurrentRoomExits = new();
        protected List<IEncounterRegionConnector> m_CurrentConnectors = new();

        protected List<ISpawnPoint> m_CurrentSpawnPoints = new();

        protected Dictionary<string, int> m_RoomGuidVisitCounts = new();

        private void Start()
        {
            StringBuilder sb = new StringBuilder();

            if (PrintDebugLogs)
            {
                m_Graph.PrintGraph();
            }


            m_CurrentRoom = m_Graph.rootNode;

            if (!EnableNavigation)
            {
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            if( m_CurrentRoom.SceneData!= null && m_CurrentRoom.SceneData.SceneName != SceneManager.GetActiveScene().name)
            {
                StartCoroutine(LoadAfterSeconds(2, m_CurrentRoom.SceneData.SceneName));
            }
            else
            {
                //m_CurrentRoomExits = m_CurrentRoom.GetAllEdgesAsOutgoing();
                //InitConnectorsAndSpawners(SceneManager.GetActiveScene());
                OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            }
        }
        private IEnumerator LoadAfterSeconds(float seconds, string sceneName)
        {
            if (RoomLoadStart != null) RoomLoadStart.Invoke();
            yield return new WaitForSeconds(seconds);

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        protected void LoadScene(string sceneName)
        {
            //if (CircleWipe != null)
            //{
            //    StartCoroutine(LoadAfterSeconds(CircleWipe.FadeSeconds));
            //}
            //else
            //{
            //    if (RoomLoadStart != null) RoomLoadStart.Invoke();
            //    RuntimeManager.PlayOneShot("event:/sfx/RoomTransition");
            //    SceneManager.LoadScene(_nextRoom.SceneName, LoadSceneMode.Single);
            //}

            if (RoomLoadStart != null) RoomLoadStart.Invoke();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        protected void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            m_CurrentRoomExits = m_CurrentRoom.GetAllEdgesAsOutgoing();
            InitConnectorsAndSpawners(scene);
            if (!m_RoomGuidVisitCounts.ContainsKey(m_CurrentRoom.Guid))
            {
                m_RoomGuidVisitCounts.Add(m_CurrentRoom.Guid, 1);
            }
            else
            {
                m_RoomGuidVisitCounts[m_CurrentRoom.Guid] ++;
            }

            if (RoomLoadComplete != null)
            {

                string arrivalName = "";
                if(m_MostRecentlyTraversedEdge != null)
                {
                    arrivalName = m_MostRecentlyTraversedEdge.DestinationPortName;
                }

                IEncounterRegionConnector arrivalObject = null;
                foreach (IEncounterRegionConnector connector in m_CurrentConnectors)
                {
                    if (connector.GetRegionConnectionData().Name == arrivalName)
                    {
                        arrivalObject = connector;
                        break;
                    }
                }

                RoomLoadComplete.Invoke(arrivalObject);
            }
        }

        public void ConnectExit(IEncounterRegionConnector exit)
        {
            exit.ConnectorActivated.AddListener(OnConnectorActivated);

            RegionConnectionData data = exit.GetRegionConnectionData();

            EdgeData edge = m_CurrentRoomExits.FirstOrDefault((EdgeData e) => e.SourcePortName == data.Name);

            exit.InitializeFromGraphData(edge);
            //if(edge != null)
            //{
            //    exit.SetDestination(edge);
            //}
        }
        protected void InitConnectorsAndSpawners(Scene loadedScene)
        {
            m_CurrentConnectors.Clear();
            m_CurrentSpawnPoints.Clear();
            if (!loadedScene.IsValid())
            {
                Debug.Log("MapGraph Runtime could not find current active scene ");
                return;
            }

            GameObject[] rootObjects = loadedScene.GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                // get and init connections
                IEncounterRegionConnector[] components = obj.GetComponentsInChildren<IEncounterRegionConnector>();

                if (components.Length > 0)
                {
                    foreach (IEncounterRegionConnector component in components)
                    {
                        m_CurrentConnectors.Add(component);

                        ConnectExit(component);
                    }
                }

                // get and init spawners

                ISpawnPoint[] spawnComponents = obj.GetComponentsInChildren<ISpawnPoint>();

                if(spawnComponents.Length > 0)
                {
                    m_CurrentSpawnPoints.AddRange(spawnComponents.ToList());
                }
                
            }
        }
        //protected void InitCurrentConnectors(Scene loadedScene)
        //{
        //    m_CurrentConnectors.Clear();
        //    if (!loadedScene.IsValid())
        //    {
        //        Debug.Log("MapGraph Runtime could not find current active scene ");
        //        return;
        //    }

        //    GameObject[] rootObjects = loadedScene.GetRootGameObjects();

        //    foreach (GameObject obj in rootObjects)
        //    {
        //        IEncounterRegionConnector[] components = obj.GetComponentsInChildren<IEncounterRegionConnector>();

        //        if (components.Length > 0)
        //        {
        //            foreach (IEncounterRegionConnector component in components)
        //            {
        //                m_CurrentConnectors.Add(component);

        //                ConnectExit(component);
        //            }
        //        }
        //    }
        //}
        protected void OnConnectorActivated(EdgeData edge)
        {
            if(PrintDebugLogs) Debug.Log("acting on connector activated event");

            RoomNode nextRoom = m_Graph.GetNodeByGuid(edge.DestinationNodeGuid) as RoomNode;
            if(nextRoom != null)
            {
                m_CurrentRoom = nextRoom;
                m_MostRecentlyTraversedEdge = edge;
                LoadScene(edge.DestinationSceneName);
            }
            
        }
        //intended to be called BEFORE a room is entered a second time (currently visit is incremented on scene load)
        public bool IsRoomBacktracking(string roomNodeGuid) 
        {
            if (m_RoomGuidVisitCounts.ContainsKey(roomNodeGuid))
            {
                return m_RoomGuidVisitCounts[roomNodeGuid] >= 1;
            }
            return false;
        }

        //intended to be called AFTER a room is entered and the visit count has been incremented
        public bool IsCurrentRoomBacktracking()
        {
            if (m_RoomGuidVisitCounts.ContainsKey(m_CurrentRoom.Guid))
            {
                return m_RoomGuidVisitCounts[m_CurrentRoom.Guid] > 1;
            }
            return false;
        }
    }
}
