using EncounterSystem;
using NGAME;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EnemyPool))]
public class NewEncounterManager : MonoBehaviour, IGameStateMachineListener
{
    public UnityEvent OnEncounterStart;
    public UnityEvent<int> OnWaveStart;
    public UnityEvent<int> OnWaveEnd;
    public UnityEvent OnEncounterEnd;

    private PlayingState m_UpdateState;
    [SerializeField]
    private MapGraphRuntime m_Graph;

    [SerializeField]
    private float m_SecondsBeforeEncounterStart = 0.0f;
    private float m_SecondsSinceLoadComplete = 0.0f;

    private bool m_IsBacktracking = false;
    private List<SOWaveData> m_CurrentEncounter = new();
    private List<Dictionary<SO_SpawnTypeTag, int>> m_ListOfSpawnsPerTypeLookup = new();
    private List<ISpawnPoint> m_SpawnPoints = new();

    private int m_CurrentWave = 0;
    private SOWaveData m_CurrentWaveData = null;

    private bool m_WaveIsSpawning = false;
    private float m_TimeSinceSpawn = 0.0f;
    private bool m_WaveComplete = false;

    private bool m_EncounterStarted = false;
    private bool m_EncounterComplete = false;

    private EnemyPool m_CurrentEnemyPool;

    [Header("DEBUG LOGS")]
    public bool PrintDebugLogText = false;
    [Header("DEBUG 'BUTTONS'")]
    [SerializeField] private bool m_KillAll = false;
    [SerializeField] private bool m_ResetWaves = false;

    private void Awake()
    {
        if(m_Graph != null)
        {
            m_Graph.RoomLoadComplete.AddListener(OnRoomLoadComplete);
            m_Graph.RoomLoadStart.AddListener(OnRoomLoadStart);
        }

        m_CurrentEnemyPool = GetComponent<EnemyPool>();
        //SubscribeToPlayState();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void SubscribeToPlayState(GameManager manager)
    {
        m_UpdateState = manager.GetState<PlayingState>();
        if (m_UpdateState == null && PrintDebugLogText)
        {
            Debug.Log("tried to add listener but myUpdateState == null");
            return;
        }

        m_UpdateState.StateUpdate.AddListener(ManagedUpdate);
        m_UpdateState.StateFixedUpdate.AddListener(ManagedFixedUpdate);
    }
    private void ResetValues()
    {
        m_CurrentWave = 0;
        m_TimeSinceSpawn = 0.0f;
        m_WaveIsSpawning = false;
        m_WaveComplete = false;
        m_EncounterStarted = false;
        m_EncounterComplete = false;
        m_SecondsSinceLoadComplete = 0.0f;
    }

    private void ManagedUpdate()
    {
        if(m_CurrentEncounter == null || m_CurrentEncounter.Count <= 0 || m_CurrentEnemyPool == null || m_SpawnPoints == null || m_SpawnPoints.Count == 0)
        {
            return;
        }

        m_CurrentEnemyPool.PrintDebugLogText = PrintDebugLogText;

        bool debugControllPressed = false;
        if (m_KillAll)
        {
            StopAllCoroutines();
            m_CurrentEnemyPool.KillAll();
            m_KillAll = false;
            debugControllPressed = true;
        }

        if (m_ResetWaves)
        {
            StopAllCoroutines();
            m_CurrentEnemyPool.ResetPool();
            m_ResetWaves = false;
            ResetValues();
            debugControllPressed = true;
        }

        if (debugControllPressed)
        {
            return; 
            // do spawning stuff next frame if a kill all or reset waves was hit
        }
        if (m_EncounterComplete)
        {
            return;
        }
        
        if (!m_EncounterStarted)
        {
            if(m_SecondsSinceLoadComplete >= m_SecondsBeforeEncounterStart)
            {
                StartEncounter();
                return;
            }
            m_SecondsSinceLoadComplete += Time.deltaTime;
            return; // just start the encounter this frame
        }

        if (TryCompleteWave())
        {
            m_CurrentWave++;
            return;
        }

        if(m_CurrentWave >= m_CurrentEncounter.Count)
        {
            EndEncounter();
            return;
        }

        if(m_CurrentWaveData == null || m_WaveComplete)
        {
            BeginNextWave();
        }
        else
        {
            m_TimeSinceSpawn += Time.deltaTime;
        }
       
    }

    private bool TryCompleteWave()
    {
        if (m_WaveComplete)
        {
            // return false if already completed
            return false;
        }

        m_WaveComplete = !m_WaveIsSpawning 
            && m_CurrentWaveData != null
            && m_CurrentEnemyPool.SpawnCount <= m_CurrentWaveData.EnemiesRemainingTrigger
            && m_TimeSinceSpawn >= m_CurrentWaveData.MinSecondsBeforeNextWave;

        if(m_WaveComplete && OnWaveEnd != null)
        {
            OnWaveEnd.Invoke(m_CurrentWave);
            if (PrintDebugLogText) Debug.Log($"Invoked OnWaveEnd for wave {m_CurrentWave}");
        }
        return m_WaveComplete;
    }

    private void ManagedFixedUpdate()
    {

    }

    private void StartEncounter()
    {
        m_EncounterStarted = true;
        if (OnEncounterStart != null) OnEncounterStart.Invoke();
        if (PrintDebugLogText) Debug.Log("Invoked OnEncounterStart");
    }
    private void EndEncounter()
    {
        m_EncounterComplete = true;
        if(OnEncounterEnd != null) OnEncounterEnd.Invoke();
        if (PrintDebugLogText) Debug.Log("Invoked OnEncounterEnd");
    }

    private void BeginNextWave()
    {
        m_WaveIsSpawning = true;
        m_WaveComplete = false;
        m_CurrentWaveData = m_CurrentEncounter[m_CurrentWave];

        if (OnWaveStart != null)
        {
            OnWaveStart.Invoke(m_CurrentWave);
            if (PrintDebugLogText) Debug.Log($"Invoked OnWaveStart for wave {m_CurrentWave}");
        }

        if (PrintDebugLogText)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"------ BEGINNING WAVE SPAWN ------ SpawnCount is: {m_CurrentEnemyPool.SpawnCount}, ");
            sb.Append($"current wave is: {m_CurrentWave}");
            Debug.Log(sb.ToString());
        }

        StartCoroutine(SpawnWave());
    }

    public void OnRoomLoadStart()
    {
        StopAllCoroutines();
        ResetValues();
        m_CurrentEncounter.Clear();
        m_CurrentWaveData = null;
        m_CurrentEnemyPool.ResetPool();
    }

    public void OnRoomLoadComplete (IEncounterRegionConnector connector)
    {
        ResetValues();
        m_ListOfSpawnsPerTypeLookup.Clear();

        m_IsBacktracking = m_Graph.IsCurrentRoomBacktracking();

        m_SpawnPoints = m_Graph.CurrentSpawnPoints;

        m_CurrentEncounter.Clear();
        Dictionary<int, GameObject> prefabsNeededByEncounter = new();

        foreach (SOWaveData wave in m_Graph.CurrentRoom.Waves)
        {
            if(wave == null) continue;

            if (!m_IsBacktracking || wave.RespawnsOnBacktrack == true)
            {
                m_CurrentEncounter.Add(wave);
                Dictionary<SO_SpawnTypeTag, int> newSpawnCountByTypeLookUp = wave.NumToSpawnByType;
                m_ListOfSpawnsPerTypeLookup.Add(newSpawnCountByTypeLookUp);
                foreach (GameObject prefab in wave.PrefabsNeeded)
                {
                    int id = prefab.GetInstanceID();
                    if (prefabsNeededByEncounter.ContainsKey(id))
                    {
                        continue;
                    }
                    prefabsNeededByEncounter.Add(id, prefab);
                }
            }
        }
        if(prefabsNeededByEncounter.Count > 0)
        {
            m_CurrentEnemyPool.InitializePool(prefabsNeededByEncounter.Values.ToList());
        }

    }

    private IEnumerator SpawnWave()
    {
        if (PrintDebugLogText) Debug.Log($"----------- SPAWN WAVE COROUTINE FIRST LINE -----------");
        
        int enemiesPerWave = m_CurrentWaveData.NumToSpawn;
        List<SO_SpawnTypeTag> possibleSpawnTypes = m_ListOfSpawnsPerTypeLookup[m_CurrentWave].Keys.ToList();
        int meleeCount = 0;
        int maxMelee = 0;

        int rangedCount = 0;
        int maxRanged = 0;

        int loopcount = 1;

        if(possibleSpawnTypes.Count > 0 && !m_CurrentWaveData.UseSpawnByType) 
        {
            SO_SpawnTypeTag allTag = possibleSpawnTypes.FirstOrDefault((SO_SpawnTypeTag tag) => tag.Tag == "Any" || tag.Tag == "Helpless");
            SO_SpawnTypeTag meleeTag = possibleSpawnTypes.FirstOrDefault((SO_SpawnTypeTag tag) => tag.Tag == "Melee");
            SO_SpawnTypeTag rangedTag = possibleSpawnTypes.FirstOrDefault((SO_SpawnTypeTag tag) => tag.Tag == "Ranged");
            if(allTag != null || (meleeTag != null && rangedTag != null))
            {
                // for now mixed waves will have a minimum of one of each type, just to make sure everything is able to spawn
                // later may choose to change this behavior
                maxMelee = UnityEngine.Random.Range(1, enemiesPerWave - 1);

                maxRanged = enemiesPerWave - maxMelee;
            }
            else if(rangedTag != null)
            {
                maxRanged = enemiesPerWave;
            }
            else if(meleeTag != null)
            {
                maxMelee = enemiesPerWave;
            }
        }

        while ((meleeCount + rangedCount) < enemiesPerWave)
        {
            if (PrintDebugLogText) Debug.Log($"----------- PASS #{loopcount} OF SPAWN LOOP FOR WAVE #{m_CurrentWave} -----------");
            if (PrintDebugLogText) Debug.Log($"NewEncounterManager.SpawnWave(): ranged enemies spawned {rangedCount}, melee enemies spawned{meleeCount}");
           
            foreach (SpawnPoint point in m_SpawnPoints)
            {
                List<SO_SpawnTypeTag> allowedTypes = point.AllowedSpawnableTypes;
                SO_SpawnTypeTag spawnerAnyTag = allowedTypes.FirstOrDefault((SO_SpawnTypeTag tag) => tag.Tag == "Any" || tag.Tag == "Helpless");
                SO_SpawnTypeTag spawnerMeleeTag = allowedTypes.FirstOrDefault((SO_SpawnTypeTag tag) => tag.Tag == "Melee");
                SO_SpawnTypeTag spawnerRangedTag = allowedTypes.FirstOrDefault((SO_SpawnTypeTag tag) => tag.Tag == "Ranged");

                AllowedEnemyType spawnType = AllowedEnemyType.None;
                bool allowsMelee = spawnerMeleeTag != null || spawnerAnyTag != null;
                bool allowsRanged = spawnerRangedTag != null || spawnerAnyTag != null;
                //foreach(SO_SpawnTypeTag tag in allowedTypes)
                //{
                //    if(tag.Tag == "Any")
                //    {
                //        allowsMelee = true;
                //        allowsRanged = true;
                //        break;
                //    }

                //    if(tag.Tag == "Melee")
                //    {
                //        allowsMelee = true;
                //        spawnType = AllowedEnemyType.Melee;
                //        continue;
                //    }

                //    if(tag.Tag == "Ranged")
                //    {
                //        allowsRanged = true;
                //        spawnType = AllowedEnemyType.Ranged;
                //        continue;
                //    }
                //}

                if(allowsMelee && allowsRanged)
                {
                    spawnType = AllowedEnemyType.Any;
                }
                else if(allowsMelee)
                {
                    spawnType = AllowedEnemyType.Melee;
                }
                else if (allowsRanged)
                {
                    spawnType = AllowedEnemyType.Ranged;
                }

                bool spawnSuccess = false;
                switch (spawnType)
                {
                    case AllowedEnemyType.Any:
                        bool coinFlip = UnityEngine.Random.Range(0, 100) >= 50;
                        if (meleeCount < maxMelee
                            && (rangedCount == maxRanged || rangedCount > meleeCount))
                        {
                            spawnSuccess = SpawnByType(point, typeof(MeleeEnemy), maxMelee, meleeCount);
                            if (spawnSuccess) meleeCount++;
                        }
                        else if (rangedCount < maxRanged
                            && (meleeCount == maxMelee || meleeCount > rangedCount))
                        {
                            spawnSuccess = SpawnByType(point, typeof(RangedEnemy), maxRanged, rangedCount);
                            if (spawnSuccess) rangedCount++;
                        }
                        else if (coinFlip)
                        {
                            spawnSuccess = SpawnByType(point, typeof(MeleeEnemy), maxMelee, meleeCount);
                            if (spawnSuccess) meleeCount++;
                        }
                        else
                        {
                            spawnSuccess = SpawnByType(point, typeof(RangedEnemy), maxRanged, rangedCount);
                            if (spawnSuccess) rangedCount++;
                        }
                        break;

                    case AllowedEnemyType.Melee:
                        if(possibleSpawnTypes.FirstOrDefault((SO_SpawnTypeTag tag) => tag.Tag == "Any" || tag.Tag == "Melee") != null)
                        {
                            spawnSuccess = SpawnByType(point, typeof(MeleeEnemy), maxMelee, meleeCount);
                            if (spawnSuccess) meleeCount++;
                        }
                        break;

                    case AllowedEnemyType.Ranged:
                        if (possibleSpawnTypes.FirstOrDefault((SO_SpawnTypeTag tag) => tag.Tag == "Any" || tag.Tag == "Ranged") != null)
                        {
                            spawnSuccess = SpawnByType(point, typeof(RangedEnemy), maxRanged, rangedCount);
                            if (spawnSuccess) rangedCount++;
                        }
                        break;

                    default:
                        break;
                }

                yield return new WaitForSeconds(m_CurrentWaveData.SecondsBtwnSpawns);
            }
            loopcount++;
        }

        if (PrintDebugLogText) Debug.Log($"----------------------- Completed spawning wave #{m_CurrentWave} -----------------------");
        
        m_WaveIsSpawning = false;
    }

    private bool SpawnByType(SpawnPoint point, System.Type type, int maxToSpawn, int typeSpawnedCount)
    {
        if(typeSpawnedCount < maxToSpawn)
        {
            if (PrintDebugLogText)
            {
                Debug.Log($"SpawnManager is asking to spawn {type} on a spawnpoint with allowed types of {point.AllowedSpawnableTypes.ToString()}.");
            }

            PooledObject instance = m_CurrentEnemyPool.SpawnInstance(point.GetPosition(), type, point);
            m_TimeSinceSpawn = 0.0f;
            return true;
        }
        else
        {
            if (PrintDebugLogText)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"will not ask to spawn {type} on a spawnpoint with allowed types of {point.AllowedSpawnableTypes.ToString()}, ");
                sb.Append($"because max number of {type} already are spawned");
                Debug.Log(sb.ToString());
            }
            return false;
        }
    }

    public void OnGameStateMachineInitialized(GameManager manager)
    {
        SubscribeToPlayState(manager);
    }
}
