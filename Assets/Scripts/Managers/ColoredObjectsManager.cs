using UnityEngine;
using System.Collections;


public class ColoredObjectsManager : MonoBehaviour
{
    //Platform 1 Materials

    //Player
    public Material[] player1Mats = new Material[4];
    private Material currentPlayer1;

    //Player Shot Light Color
    public Color[] playerShotLights = new Color[4];
    private Color currentPlayerShotLight;

    //Voxel Materials
    public Material[] voxelMats = new Material[4];
    private Material currentVoxel;

    //Spider Materials
    public Material[] spiderMats = new Material[4];
    private Material currentSpider;
    
    private ScriptObjectPool<PlayerShotController>[] playerShotPools = new ScriptObjectPool<PlayerShotController>[4];
    private ScriptObjectPool<PlayerShotController> currentPlayerShotPool;

    private ObjectPool spiderPool;
    private ScriptObjectPool<VoxelController> voxelPool;

    private ChromaColor currentColor;

	void Start ()
    {
        Debug.Log("Colored Objects Manager created");

        playerShotPools[0] = rsc.poolMng.playerShotRedPool;
        playerShotPools[1] = rsc.poolMng.playerShotGreenPool;
        playerShotPools[2] = rsc.poolMng.playerShotBluePool;
        playerShotPools[3] = rsc.poolMng.playerShotYellowPool;

        spiderPool = rsc.poolMng.spiderPool;
        voxelPool = rsc.poolMng.voxelPool;
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        currentColor = rsc.colorMng.CurrentColor;
        SetCurrentMaterials();
    }

    void OnDestroy()
    {
        if(rsc.eventMng != null)
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        Debug.Log("Colored Objects Manager destroyed");
    }

    void ColorChanged(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        currentColor = info.newColor;

        SetCurrentMaterials();
    }

    void SetCurrentMaterials()
    {
        int colorIndex = (int)currentColor;

        currentPlayer1 = player1Mats[colorIndex];
        currentPlayerShotPool = playerShotPools[colorIndex];
        currentPlayerShotLight = playerShotLights[colorIndex];
        currentVoxel = voxelMats[colorIndex];
        currentSpider = spiderMats[colorIndex];       
    }

    private Material GetMaterial(Material[] matArray, ChromaColor color)
    {
        return matArray[(int)color];
    }

    public Material GetPlayer1Material(ChromaColor color)
    {
        return GetMaterial(player1Mats, color);
    }

    //Player Shot methods
    public PlayerShotController GetPlayerShot()
    {
        return currentPlayerShotPool.GetObject();
    }

    public PlayerShotController GetPlayerShot(ChromaColor color)
    {
        return playerShotPools[(int)color].GetObject();
    }

    public Color GetPlayerShotLightColor()
    {
        return currentPlayerShotLight;
    }

    //Spider methods
    public SpiderAIBehaviour GetSpider()
    {
        //Get a spider from pool
        GameObject spider = spiderPool.GetObject();       

        if (spider != null)
        {
            SpiderAIBehaviour spiderAI = spider.GetComponent<SpiderAIBehaviour>();
            spiderAI.color = currentColor;
            Material[] mats = spiderAI.rend.materials;
            mats[1] = currentSpider;
            spiderAI.rend.materials = mats;

            return spiderAI;
        }

        return null;
    }

    public SpiderAIBehaviour GetSpiderRandomColor()
    {
        return GetSpider(ChromaColorInfo.Random);
    }

    public SpiderAIBehaviour GetSpider(int offset)
    {
        int colorIndex = ((int)currentColor + offset) % ChromaColorInfo.Count;

        ChromaColor color = (ChromaColor)colorIndex;

        return GetSpider(color);
    }

    public SpiderAIBehaviour GetSpider(ChromaColor color)
    {
        //Get a spider from pool
        GameObject spider = spiderPool.GetObject();

        if (spider != null)
        {
            SpiderAIBehaviour spiderAI = spider.GetComponent<SpiderAIBehaviour>();
            spiderAI.color = color;
            Material[] mats = spiderAI.rend.materials;
            mats[1] = GetMaterial(spiderMats, color);
            spiderAI.rend.materials = mats;

            return spiderAI;
        }

        return null;
    }

    public Material GetSpiderMaterial(ChromaColor color)
    {
        return GetMaterial(spiderMats, color);
    }

    //Voxel methods
    public VoxelController GetVoxel()
    {
        //Get a voxel from pool
        VoxelController voxel = voxelPool.GetObject();

        if (voxel != null)
        {
            voxel.GetComponent<Renderer>().material = currentVoxel;
        }

        return voxel;
    }

    public VoxelController GetVoxel(ChromaColor color)
    {
        //Get a voxel from pool
        VoxelController voxel = voxelPool.GetObject();

        if (voxel != null)
        {
            voxel.GetComponent<Renderer>().material = GetMaterial(voxelMats, color);       
        }

        return voxel;
    }

    public Material GetVoxelMaterial(ChromaColor color)
    {
        return GetMaterial(voxelMats, color);
    }

    public Material GetVoxelRandomMaterial()
    {
        return GetVoxelMaterial(ChromaColorInfo.Random);
    }
}
