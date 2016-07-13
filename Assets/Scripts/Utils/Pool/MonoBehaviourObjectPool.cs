using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonoBehaviourObjectPool<T> where T : MonoBehaviour
{
    public GameObject prefabWhereBehaviourIsIn;
    public int poolSize = 100;
    public bool grow = false; //Use with extreme caution

    private GameObject objectsParent;
    private Queue<T> pool;
    private List<T> auxList; //Used to maintain a permanent reference to every pooled object

    public MonoBehaviourObjectPool()
    {
        //Debug.Log("MonoBehaviour Object Pool created");
    }

    ~MonoBehaviourObjectPool()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, RecallObjects);
        }
        //Debug.Log("MonoBehaviour Object Pool destroyed");
    }

    public void Init(GameObject poolParent, GameObject poolContainer)
    {
        objectsParent = GameObject.Instantiate(poolContainer) as GameObject;

        objectsParent.transform.SetParent(poolParent.transform);
        objectsParent.name = prefabWhereBehaviourIsIn.name + "Pool";

        Transform trans = objectsParent.transform;
        GameObject aux;

        pool = new Queue<T>(poolSize);
        auxList = new List<T>();
        for (int i = 0; i < poolSize; ++i)
        {
            aux = GameObject.Instantiate(prefabWhereBehaviourIsIn) as GameObject;
            aux.SetActive(false);
            aux.transform.SetParent(trans);
            T comp = aux.GetComponent<T>();
            pool.Enqueue(comp);
            auxList.Add(comp);
        }
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, RecallObjects);
        //Debug.Log("MonoBehaviour Object Pool initialized");
    }

    private void RecallObjects(EventInfo eventInfo)
    {
        foreach (T obj in auxList)
        {
            //if (obj.gameObject.activeSelf)
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
            GameObject aux = GameObject.Instantiate(prefabWhereBehaviourIsIn) as GameObject;
            aux.SetActive(false);
            aux.transform.SetParent(objectsParent.transform);
            T comp = aux.GetComponent<T>();
            auxList.Add(comp);
            return comp;
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
