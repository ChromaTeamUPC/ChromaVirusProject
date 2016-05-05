using UnityEngine;
using System.Collections;

public class VoxelController : MonoBehaviour {

    public float minDuration;
    public float maxDuration;
    public float duration = 0;

    public int spawnLevels = 0;
    [Range(0,100)]
    public int spawnChildrenChance = 0;
    public int spawnChildrenCount = 9;

    private float realDuration;

    [HideInInspector]
    public Transform trans;

    void Awake()
    {
        trans = gameObject.transform;
    }

    void OnEnable()
    {
        if (duration != 0)
            realDuration = duration;
        else
            realDuration = Random.Range(minDuration, maxDuration);
    }

    // Update is called once per frame
    void Update()
    {
        realDuration -= Time.deltaTime;
        if (realDuration <= 0.0f)
        {
            Vector3 scale = gameObject.transform.localScale / 2;
            Vector3 position = gameObject.transform.position;
            if (spawnLevels > 0)
            {
                if (Random.Range(0, 101) <= spawnChildrenChance)
                {
                    VoxelController child;
                    for (int i = 0; i < spawnChildrenCount; ++i)
                    {
                        child = rsc.poolMng.voxelPool.GetObject();
                        if (child != null)
                        {
                            child.transform.position = position;
                            child.transform.localScale = scale;
                            child.transform.rotation = Random.rotation;
                            child.gameObject.SetActive(true);
                            child.GetComponent<Renderer>().material = GetComponent<Renderer>().material;
                            child.spawnLevels = spawnLevels - 1;
                        }
                    }
                }
            }
            rsc.poolMng.voxelPool.AddObject(this);
        }
    }
}
