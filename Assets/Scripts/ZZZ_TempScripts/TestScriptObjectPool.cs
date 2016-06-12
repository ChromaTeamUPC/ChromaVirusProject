using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TestScriptObjectPool<T> where T : MonoBehaviour
{
    public int poolSize = 100;

    [HideInInspector]
    public GameObject objectsParent;

    public GameObject objectWhereScriptIs;

    private Queue<T> pool;
    private List<T> auxList; //Used to maintain a permanent reference to every pooled object

    //Use with caution
    public bool grow = false;

    public TestScriptObjectPool()
    {
        Debug.Log("Script Object Pool created");
    }

    ~TestScriptObjectPool()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, RecallObjects);
        }
        Debug.Log("Script Object Pool destroyed");
    }

    public void Init(GameObject poolParent, GameObject poolContainer)
    {
        //objectsParent = GameObject.Instantiate(Resources.Load("Prefabs/Pool/PoolContainer")) as GameObject;
        objectsParent = GameObject.Instantiate(poolContainer) as GameObject;

        objectsParent.transform.SetParent(poolParent.transform);
        objectsParent.name = objectWhereScriptIs.name + "Pool";

        Transform trans = objectsParent.transform;
        GameObject aux;

        pool = new Queue<T>(poolSize);
        auxList = new List<T>();
        for (int i = 0; i < poolSize; ++i)
        {
            aux = GameObject.Instantiate(objectWhereScriptIs) as GameObject;
            aux.SetActive(false);
            aux.transform.SetParent(trans);
            T comp = aux.GetComponent<T>();
            pool.Enqueue(comp);
            auxList.Add(comp);
        }
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, RecallObjects);
        Debug.Log("Script Object Pool initialized");
    }

    private void RecallObjects(EventInfo eventInfo)
    {
        foreach (T obj in auxList)
        {
            if (obj.gameObject.activeSelf)
            {
                AddObject(obj);
            }
        }
    }

    public T GetObject()
    {
        if (pool.Count > 0)
        {
            T pooledObject = pool.Dequeue();
            pooledObject.gameObject.SetActive(true);
            return pooledObject;
        }
        else if (grow)
        {
            //Grow will happen not now but when all the objects will be enqueued again and capacity will be full
            GameObject aux = GameObject.Instantiate(objectWhereScriptIs) as GameObject;
            aux.SetActive(false);
            aux.transform.SetParent(objectsParent.transform);
            return aux.GetComponent<T>();
        }
        else
            return null;
    }

    public void AddObject(T pooledObject)
    {
        pooledObject.gameObject.SetActive(false);
        pooledObject.transform.SetParent(objectsParent.transform);
        pool.Enqueue(pooledObject);
    }

    public int GetPoolCount()
    {
        return pool.Count;
    }
}
