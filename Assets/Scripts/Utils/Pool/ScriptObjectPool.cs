using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptObjectPool<T> where T : MonoBehaviour
{
    //This defines which game object will be the parent of the pooled objects so they don't fill the hierarchy root
    public GameObject objectsParent;
    //This defines which kind of GameObject has the script attached to, so we know what to instantiate
    public GameObject objectWhereScriptIs; 
    
    public int poolSize = 100;

    //Use with caution
    public bool grow = false;

    private Queue<T> pool;
    private List<T> auxList;

    public ScriptObjectPool()
    {
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, RecallObjects);
        Debug.Log("Script Object Pool created");
    }

    ~ScriptObjectPool()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, RecallObjects);
        }
        Debug.Log("Script Object Pool destroyed");
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

    // Use this for initialization
    public void Init()
    {
        Transform trans = objectsParent.transform;
        GameObject aux;

        pool = new Queue<T>(poolSize);
        auxList = new List<T>();
        for (int i = 0; i < poolSize; ++i)
        {
            aux = GameObject.Instantiate(objectWhereScriptIs);
            aux.SetActive(false);
            aux.transform.SetParent(trans);
            T comp = aux.GetComponent<T>();
            pool.Enqueue(comp);
            auxList.Add(comp);
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
            GameObject aux = GameObject.Instantiate(objectWhereScriptIs);
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
        pool.Enqueue(pooledObject);
    }

    public int GetPoolCount()
    {
        return pool.Count;
    }
}
