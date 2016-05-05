using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

    public GameObject pooledObject;
    public int poolSize = 100;

    private Queue<GameObject> pool;
    private List<GameObject> auxList; //Used to maintain a permanent reference to every pooled object

	// Use this for initialization
	void Awake ()
    {
        Transform trans = transform;
        GameObject aux;
        pool = new Queue<GameObject>(poolSize);
        auxList = new List<GameObject>();
        for (int i = 0; i < poolSize; ++i)
        {
            aux = Instantiate(pooledObject);
            aux.SetActive(false);
            aux.transform.SetParent(trans);
            pool.Enqueue(aux);
            auxList.Add(aux);
        }
        Debug.Log("Object Pool created");
    }

    void Start()
    {
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, RecallObjects);
    }

    void OnDestroy()
    {
        if(rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, RecallObjects);
        }
        Debug.Log("Object Pool destroyed");
    }

    private void RecallObjects(EventInfo eventInfo)
    {
        foreach(GameObject obj in auxList)
        {
            if(obj.activeSelf)
            {
                AddObject(obj);
            }
        }
    }
    
    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject pooledObject = pool.Dequeue();
            pooledObject.SetActive(true);
            return pooledObject;
        }
        else
            return null;
    }

    public void AddObject(GameObject pooledObject)
    {
        pooledObject.SetActive(false);
        pool.Enqueue(pooledObject);
    }

    public int GetPoolCount()
    {
        return pool.Count;
    }
}
