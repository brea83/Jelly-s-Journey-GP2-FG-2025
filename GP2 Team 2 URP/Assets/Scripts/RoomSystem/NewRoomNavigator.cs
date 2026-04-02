using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using NGAME;

public class NewRoomNavigator : MonoBehaviour
{
    public UnityEvent RoomLoadStart;
    public UnityEvent<IEncounterRegionConnector> RoomLoadComplete;
    //[Header("Room Load Effects")]
    //public CircleWipeControler CircleWipe;

    [SerializeField]
    private MapGraphRuntime m_Graph;

    [Header("Debug stuff")]
    public bool EnableNavigation = true;
    public bool PrintDebugLogs = false;

    private void Start()
    {
        StringBuilder sb = new StringBuilder();

        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    public void EnterFirstRoom()
    {
        RoomNode firstRoom = m_Graph.TryEnterFirstRoom();
        
        if (firstRoom == null)
        {
            Debug.LogError("No RoomNode found, make sure Graph runtime is assigned, and has rooms");
            return;
        }
        StartCoroutine(LoadAfterSeconds(2, firstRoom.SceneData.SceneName));

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
        m_Graph.OnSceneLoaded(scene, mode);

        if (RoomLoadComplete != null)
        {

            string arrivalName = "";
            if (m_Graph.LastTraversedEdge != null)
            {
                arrivalName = m_Graph.LastTraversedEdge.DestinationPortName;
            }

            IEncounterRegionConnector arrivalObject = null;
            foreach (IEncounterRegionConnector connector in m_Graph.CurrentConnectors)
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

        EdgeData edge = m_Graph.CurrentExits.FirstOrDefault((EdgeData e) => e.SourcePortName == data.Name);

        exit.InitializeFromGraphData(edge);
        //if(edge != null)
        //{
        //    exit.SetDestination(edge);
        //}
    }
    protected void LinkConnectorsToEdges(Scene loadedScene)
    {
        if (!loadedScene.IsValid())
        {
            Debug.Log("NewRoomNavigator could not find current active scene ");
            return;
        }


        foreach (IEncounterRegionConnector component in m_Graph.CurrentConnectors)
        {
            ConnectExit(component);
        }
       
    }

    protected void OnConnectorActivated(EdgeData edge)
    {
        if (PrintDebugLogs) Debug.Log("acting on connector activated event");

        if (m_Graph.TryEnterRoom(edge))
        {
            LoadScene(m_Graph.CurrentRoom.SceneData.SceneName);
        }
        //RoomNode nextRoom = m_Graph.GetRoomByGuid(edge.DestinationNodeGuid);
        //if (nextRoom != null)
        //{
        //    m_CurrentRoom = nextRoom;
        //    m_MostRecentlyTraversedEdge = edge;
        //    LoadScene(edge.DestinationSceneName);
        //}

    }
}
