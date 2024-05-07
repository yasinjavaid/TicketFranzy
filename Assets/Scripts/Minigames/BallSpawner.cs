using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public List<Pool> pools;


    public static BallSpawner sharedInstance;


    void Awake()
    {

        if (sharedInstance == null)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

        private void Start()
        {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab,transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);

        }

    }

    public GameObject SpawnFromPool (string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.Log("Pool with tag doesn't exist");
            return null;
        }

        if(poolDictionary[tag].Count > 0)
        {
            GameObject objectToSpawn = poolDictionary[tag].Dequeue();
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            return objectToSpawn;
        }
        else
        {
            Debug.Log("Pool is empty");
            return null;
        }

        //        poolDictionary[tag].Enqueue(objectToSpawn);
    }

    public bool isPoolContain(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return false;
        }

        if (poolDictionary[tag].Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public void ReturnToPool(string tag, GameObject obj)
    {
        obj.transform.parent = transform;
        poolDictionary[tag].Enqueue(obj);
        obj.SetActive(false);

    }

}
