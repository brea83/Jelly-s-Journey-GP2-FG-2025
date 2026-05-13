using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using NGAME;
using FMODUnity;
using RoomSystem;

public class NewRoomNavigator : MonoBehaviour
{
    public UnityEvent RoomLoadStart;
    public UnityEvent<IEncounterRegionConnector> RoomLoadComplete;

    [SerializeField]
    private MapGraphRuntime m_Graph;
    [Header("Room Load Effects")]
    [SerializeField]
    private CircleWipeControler m_CircleWipe;
    private bool m_ThisLoadUsesCircleWipe = false;
    [SerializeField]
    private string m_RoomTransitionFmodEvent = "event:/sfx/RoomTransition";

    [Header("Debug stuff")]
    public bool NavigationEnabled = true;
    public bool PrintDebugLogs = false;

    private bool m_LoadFromGraphNeeded;

    private void Start()
    {
        StringBuilder sb = new StringBuilder();

        //m_Graph.PlaymodeStartedFromGraph.AddListener(OnGraphInitiatedPlaymode);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (m_LoadFromGraphNeeded)
        {
            m_LoadFromGraphNeeded = false;
            LoadScene(m_Graph.CurrentRoom.SceneData.Name, false);
        }
    }
    public void OnGraphInitiatedPlaymode(MapGraphRuntime runtimeGraph)
    {
        m_Graph = runtimeGraph;
        m_LoadFromGraphNeeded = true;

    }
    public void EnterFirstRoom()
    {
        RoomNode firstRoom = m_Graph.TryEnterFirstRoom();

        if (firstRoom == null)
        {
            Debug.LogError("No RoomNode found, make sure Graph runtime is assigned, and has rooms");
            return;
        }
        LoadScene(firstRoom.SceneData.Name, false);

    }
    private IEnumerator LoadAfterSeconds(float seconds, string sceneName, bool bIsCircleWipe = false)
    {
        if (RoomLoadStart != null) RoomLoadStart.Invoke();
        if (bIsCircleWipe)
        {
            m_CircleWipe.OnRoomLoadStart();
            yield return new WaitWhile(() =>
            {
                return m_CircleWipe.DoingFadeOut == true;
            });
        }
        else
        {
            yield return new WaitForSeconds(seconds);
        }
        RuntimeManager.PlayOneShot(m_RoomTransitionFmodEvent);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    protected void LoadScene(string sceneName, bool bUseCircleWipe = true)
    {
        if (bUseCircleWipe && m_CircleWipe != null)
        {
            m_ThisLoadUsesCircleWipe = true;
            StartCoroutine(LoadAfterSeconds(m_CircleWipe.FadeSeconds, sceneName, true));
        }
        else
        {
            m_ThisLoadUsesCircleWipe = false;
            if (RoomLoadStart != null) RoomLoadStart.Invoke();
            RuntimeManager.PlayOneShot(m_RoomTransitionFmodEvent);
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        //if (RoomLoadStart != null) RoomLoadStart.Invoke();
        //SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
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
                ConnectExit(connector);
                Door thisDoor = connector as Door;
                if (thisDoor != null && thisDoor.name == arrivalName)
                {
                    arrivalObject = connector;
                }
            }

            if (m_ThisLoadUsesCircleWipe)
            {
                m_CircleWipe.OnRoomLoadComplete(arrivalObject);
            }
            RoomLoadComplete.Invoke(arrivalObject);
        }
    }

    public void ConnectExit(IEncounterRegionConnector exit)
    {
        exit.ConnectorActivated.AddListener(OnConnectorActivated);

        RegionConnectionData liveSceneData = exit.GetRegionConnectionData();

        EdgeData edge = m_Graph.CurrentExits.FirstOrDefault((EdgeData e) => e.SourcePortName == liveSceneData.Name);
        RegionConnectionData graphConnectionData = m_Graph.CurrentRoom.OverridenConnectionData.FirstOrDefault(
            (RegionConnectionData data) => data.Name == liveSceneData.Name);

        exit.InitializeFromGraphData(graphConnectionData, edge);
    }

    protected void OnConnectorActivated(EdgeData edge)
    {
        if (PrintDebugLogs) Debug.Log("acting on connector activated event");

        if (m_Graph.TryEnterRoom(edge))
        {
            LoadScene(m_Graph.CurrentRoom.SceneData.Name);
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
