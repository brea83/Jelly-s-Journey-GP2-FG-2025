using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

        [Header("Debug stuff")]
        public bool EnableNavigation = true;
        public bool PrintDebugLogs = false;

        [SerializeField]
        protected RoomGraph m_Graph;
        protected RoomNode m_CurrentRoom;
        protected EdgeData m_MostRecentlyTraversedEdge;
        protected List<EdgeData> m_CurrentRoomExits = new();
        protected List<IEncounterRegionConnector> m_CurrentConnectors = new();

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
                m_CurrentRoomExits = m_CurrentRoom.GetAllEdgesAsOutgoing();
                InitCurrentConnectors(SceneManager.GetActiveScene());
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
            InitCurrentConnectors(scene);
            if(RoomLoadComplete != null)
            {
                string arrivalName = m_MostRecentlyTraversedEdge.DestinationPortName;

                IEncounterRegionConnector arrivalObject = null;
                foreach(IEncounterRegionConnector connector in m_CurrentConnectors)
                {
                    if(connector.GetRegionConnectionData().Name == arrivalName)
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

        protected void InitCurrentConnectors(Scene loadedScene)
        {
            m_CurrentConnectors.Clear();
            if (!loadedScene.IsValid())
            {
                Debug.Log("MapGraph Runtime could not find current active scene ");
                return;
            }

            GameObject[] rootObjects = loadedScene.GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                IEncounterRegionConnector[] components = obj.GetComponentsInChildren<IEncounterRegionConnector>();

                if (components.Length > 0)
                {
                    foreach (IEncounterRegionConnector component in components)
                    {
                        m_CurrentConnectors.Add(component);

                        ConnectExit(component);
                    }
                }
            }
        }
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
    }
}
