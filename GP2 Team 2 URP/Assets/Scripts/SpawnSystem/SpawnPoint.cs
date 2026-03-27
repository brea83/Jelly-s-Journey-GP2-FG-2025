using NGAME;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;
namespace EncounterSystem
{
    public enum AllowedEnemyType { Any, Melee, Ranged }
    public class SpawnPoint : MonoBehaviour, ISpawnPoint
    {
        [SerializeField]
        protected AllowedEnemyType _allowedEnemyType;
        public AllowedEnemyType AllowedEnemyType { get { return _allowedEnemyType; } }

        public List<SO_SpawnTypeTag> AllowedSpawnableTypes { get => m_AllowedSpawnableTypes; set => m_AllowedSpawnableTypes = value; }

        [SerializeField]
        protected List<SO_SpawnTypeTag> m_AllowedSpawnableTypes = new();
        //public UnityEvent<Vector3> OnEnemySpawn;

        void Awake()
        {
            SpawnManager spawnManager = FindAnyObjectByType<SpawnManager>();
            if (spawnManager != null )
            {
                spawnManager.RegisterSpawnPoint( this );
            }
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public SpawnerData GetSpawnerData()
        {
            SpawnerData spawnerData = new SpawnerData();
            spawnerData.ValidTypes = AllowedSpawnableTypes;
            spawnerData.Name = name;
            spawnerData.Position = transform.position;

            return spawnerData;
        }
    }
}