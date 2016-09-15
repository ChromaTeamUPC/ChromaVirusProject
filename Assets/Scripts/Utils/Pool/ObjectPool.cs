using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class ObjectPool {

    public GameObject pooledPrefab;
    public int poolSize = 100;
    public bool grow = false; //Use with extreme caution

    private GameObject objectsParent;
    private Queue<GameObject> pool;
    private List<GameObject> auxList; //Used to maintain a permanent reference to every pooled object

    public ObjectPool()
    {
        //Debug.Log("Object Pool created");
    }

    ~ObjectPool()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, RecallObjects);
        }
        //Debug.Log("Object Pool destroyed");
    }

    public void Init(GameObject poolParent, GameObject poolContainer)
    {
        objectsParent = GameObject.Instantiate(poolContainer) as GameObject;

        objectsParent.transform.SetParent(poolParent.transform);
        objectsParent.name = pooledPrefab.name + "Pool";

        Transform trans = objectsParent.transform;
        GameObject aux;

        pool = new Queue<GameObject>(poolSize);
        auxList = new List<GameObject>();
        for (int i = 0; i < poolSize; ++i)
        {
            aux = GameObject.Instantiate(pooledPrefab) as GameObject;
            aux.SetActive(false);
            aux.transform.SetParent(trans);
            pool.Enqueue(aux);
            auxList.Add(aux);
        }
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, RecallObjects);
        //Debug.Log("Object Pool initialized");
    }

    private void RecallObjects(EventInfo eventInfo)
    {
        pool.Clear();

        foreach(GameObject obj in auxList)
        {
            //if(obj.activeSelf)
            {
                AddObject(obj);
            }
        }

        //Debug.Log(objectsParent.name + " pool count: " + pool.Count);
    }

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject pooledObject = pool.Dequeue();
            pooledObject.SetActive(true);
            return pooledObject;
        }
        else if (grow)
        {
            //Grow will happen not now but when all the objects will be enqueued again and capacity will be full
            GameObject aux = GameObject.Instantiate(pooledPrefab) as GameObject;
            aux.SetActive(false);
            aux.transform.SetParent(objectsParent.transform);
            auxList.Add(aux);
            return aux;
        }
        else
            return null;
    }

    public void AddObject(GameObject pooledObject)
    {
        pooledObject.SetActive(false);
        pooledObject.transform.SetParent(objectsParent.transform);
        pool.Enqueue(pooledObject);
    }

    public int GetPoolCount()
    {
        return pool.Count;
    }
}
