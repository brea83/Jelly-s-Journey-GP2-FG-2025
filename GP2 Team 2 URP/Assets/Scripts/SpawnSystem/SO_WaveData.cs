using NGAME;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[CreateAssetMenu(fileName = "SO_WaveData", menuName = "NGAME/New Wave Data")]
public class SO_WaveData : ScriptableObject, NGAME.IWaveData
{
    public bool RespawnsOnBacktrack { get => m_RespawnsOnBacktrack; set => m_RespawnsOnBacktrack = value; }
    public List<ISpawnable> PossibleSpawns { get => ExtractSpawnablesFromPrefabs(); }
    public float SecondsBtwnSpawns { get => m_SecBtwnSpawns; set => m_SecBtwnSpawns = value; }
    public int NumToSpawn { get => m_NumToSpawn; set => m_NumToSpawn = value; }
    public bool UseSpawnByType { get => m_UseSpawnByType; set => m_UseSpawnByType = value; }
    public Dictionary<SO_SpawnTypeTag, int> NumToSpawnByType { get => UpdateSpawnTypesList(); }
    public float MinSecondsBeforeNextWave { get => m_MinSecondsBeforeNextWave; set => m_MinSecondsBeforeNextWave = value; }
    public int EnemiesRemainingTrigger { get => m_EnemiesReamainingTrigger; set => m_EnemiesReamainingTrigger = value; }

    [SerializeField]
    private bool m_RespawnsOnBacktrack = false;
    [SerializeField, TypeConstraint(typeof(ISpawnable))]
    private List<GameObject> m_PossibleEnemies = new List<GameObject>();
    [SerializeField, HideInInspector]
    private bool m_EnemyListDirty = false;
    [SerializeField]
    private float m_SecBtwnSpawns = 0.5f;
    [SerializeField]
    private int m_NumToSpawn = 1;
    [SerializeField, HideInInspector]
    private bool m_UseSpawnByType = false;
    [SerializeField]
    private float m_MinSecondsBeforeNextWave = 2.0f;
    [SerializeField]
    private int m_EnemiesReamainingTrigger = 0;

    // store the dictionary of spawn count by type as two lists
    [SerializeField, HideInInspector]
    private List<SO_SpawnTypeTag> m_SpawnTypes = new List<SO_SpawnTypeTag>();
    [SerializeField, HideInInspector]
    private List<int> m_SpawnCountPerType = new List<int>();

    SO_WaveData()
    {

    }

    

    public Dictionary<SO_SpawnTypeTag, int> UpdateSpawnTypesList()
    {
        Dictionary<SO_SpawnTypeTag, int> result = new();
        if(m_SpawnTypes.Count > 0 && m_SpawnTypes.Count >= m_SpawnCountPerType.Count)
        {
            for (int i = 0;i < m_SpawnTypes.Count;i++)
            {
                if(m_SpawnCountPerType.Count > i)
                {
                    if (!result.ContainsKey(m_SpawnTypes[i]))
                    {
                        result.Add(m_SpawnTypes[i], m_SpawnCountPerType[i]);
                    }
                    else
                    {
                        result[m_SpawnTypes[i]] += m_SpawnCountPerType[i];
                    }
                }
                else
                {
                    if (!result.ContainsKey(m_SpawnTypes[i]))
                    {
                        result.Add(m_SpawnTypes[i], 0);
                    }
                }
            }
        }

        if(m_EnemyListDirty && m_PossibleEnemies.Count > 0)
        {
            for (int i = 0; i < m_PossibleEnemies.Count; i++)
            {
                GameObject prefabObject = m_PossibleEnemies[i];

                ISpawnable enemy = prefabObject.GetComponent<ISpawnable>();
                if (enemy == null)
                {
                    continue;
                }
                SO_SpawnTypeTag tag = enemy.SpawnTypeTag;

                if (result.ContainsKey(tag))
                {
                    continue;
                }
                else
                {
                    result.Add(tag, 0);
                }
            }
        }

        m_SpawnTypes = result.Keys.ToList();
        m_SpawnCountPerType = result.Values.ToList();

        return result;
    }

    private List<ISpawnable> ExtractSpawnablesFromPrefabs()
    {
        List<ISpawnable> result = new();
        foreach (GameObject prefabObject in m_PossibleEnemies)
        {
            ISpawnable enemy = prefabObject.GetComponent<ISpawnable>();
            if (enemy == null)
            {
                continue;
            }
            result.Add(enemy);
        }

        return result;
    }

}
