using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    protected List<PooledObject> _activePool = new List<PooledObject>();
    protected List<PooledObject> _inactivePool = new List<PooledObject>();
    public  List<GameObject> Prefabs { get; protected set; }
    public  int PoolSize { get; protected set; }
    public int SpawnCount { get; protected set; }

    public virtual void UpdatePool()
    {
        if (SpawnCount > 0)
        {
            foreach (PooledObject instance in _activePool)
            {
                if (instance.gameObject.activeInHierarchy)
                {
                    instance.PoolUpdate();
                }
                
            }
        }
    }
    public virtual void InitializePool(List<GameObject> prefabs, int poolSize = 20)
    {
        DestroyInactiveSpawns();
        _activePool.Clear();
        _inactivePool.Clear();
        PoolSize = poolSize;
        Prefabs = prefabs;

        int evenSplit = poolSize / Prefabs.Count;

        foreach (GameObject prefab in Prefabs) {
            PooledObject poolComponent = prefab.GetComponent<PooledObject>();
            for (int i = 0; i < evenSplit; i++)
            {
                PooledObject instance = null;
                if (poolComponent == null)
                {
                     Debug.LogError($"prefab {prefab} has no pooledObject Component");
                }
                else
                {
                    instance = Instantiate(prefab, transform).GetComponent<PooledObject>();
                }
                
                _inactivePool.Add(instance);
                //Debug.Log($"added to inactive pool: {instance}, allowed type match is: {instance.gameObject.GetComponent<SpawnTags>().SpawnLocationType}");
                instance.gameObject.SetActive(false);
            }
        }
    }
        
    protected virtual void DestroyInactiveSpawns()
    {
        foreach(PooledObject prefab in _inactivePool)
        {
            Destroy(prefab);
        }
    }

    public virtual void ReturnToPool(PooledObject instance)
    {

        if (_activePool.Contains(instance))
        {
            //may need to disable colliders here if having weird physics with objects set to inactive
            instance.OnReturnToPool.RemoveAllListeners();
            instance.gameObject.SetActive(false);

            _activePool.Remove(instance);
            _inactivePool.Add(instance);

            SpawnCount--;
        }
        else
        {
            Debug.LogError(this.name + " was told to that " + instance + " needs to return to pool, but that object is not in this manager's pool.");
        }
    }

    public virtual PooledObject SpawnInstance(Vector3 spawnPosition)
    {

        PooledObject instance = null;
        if (_inactivePool.Count > 0)
        {
            instance = _inactivePool.First();
            //enable colider and physics stuff if that is put to sleep when returned to pool
        }
        else
        {
            //if inactive pool is empty make a new enemy
            //default just grabs the first prefab, need to make a randomizer or specific selector version
            instance = Instantiate(Prefabs.First(), spawnPosition, Quaternion.identity).GetComponent<PooledObject>();
        }

        instance.transform.position = spawnPosition;
        instance.gameObject.SetActive(true);
        instance.OnReturnToPool.AddListener(ReturnToPool);
        _inactivePool.Remove(instance);
        _activePool.Add(instance);

        SpawnCount++;

        return instance;
    }

    public virtual void ResetPool()
    {
        //SpawnCount = 0;
        List<PooledObject> objectsToReset = new List<PooledObject>();
        objectsToReset.AddRange(_activePool);
        foreach (PooledObject instance in objectsToReset)
        {
            instance.ReturnToPool();
        }
    }
}
