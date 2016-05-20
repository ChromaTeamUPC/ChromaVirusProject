using UnityEngine;
using System.Collections;


public class ColoredObjectsManager : MonoBehaviour
{
    [SerializeField]
    private Material whiteMaterial;

    //Platform 1 Materials

    //Player
    [SerializeField]
    private Material[] player1Mats = new Material[4];
    private Material currentPlayer1;

    //Player Shot Light Color
    [SerializeField]
    private Color[] playerShotLights = new Color[4];
    private Color currentPlayerShotLight;

    //Voxel Materials
    [SerializeField]
    private Material[] voxelMats = new Material[4];
    private Material currentVoxel;

    //Spider Materials
    [SerializeField]
    private Material[] spiderMats = new Material[4];
    private Material currentSpider;
    
    private ScriptObjectPool<PlayerShotController>[] player1ShotPools = new ScriptObjectPool<PlayerShotController>[4];
    private ScriptObjectPool<PlayerShotController> currentPlayer1ShotPool;

    private ScriptObjectPool<MuzzleController>[] player1MuzzlePools = new ScriptObjectPool<MuzzleController>[4];
    private ScriptObjectPool<MuzzleController> currentPlayer1MuzzlePool;

    private ObjectPool spiderPool;
    private ScriptObjectPool<VoxelController> voxelPool;

    private ChromaColor currentColor;

	void Start ()
    {
        Debug.Log("Colored Objects Manager created");

        player1ShotPools[0] = rsc.poolMng.player1ShotRedPool;
        player1ShotPools[1] = rsc.poolMng.player1ShotGreenPool;
        player1ShotPools[2] = rsc.poolMng.player1ShotBluePool;
        player1ShotPools[3] = rsc.poolMng.player1ShotYellowPool;

        player1MuzzlePools[0] = rsc.poolMng.player1MuzzleRedPool;
        player1MuzzlePools[1] = rsc.poolMng.player1MuzzleGreenPool;
        player1MuzzlePools[2] = rsc.poolMng.player1MuzzleBluePool;
        player1MuzzlePools[3] = rsc.poolMng.player1MuzzleYellowPool;

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
        currentPlayer1ShotPool = player1ShotPools[colorIndex];
        currentPlayer1MuzzlePool = player1MuzzlePools[colorIndex];
        currentPlayerShotLight = playerShotLights[colorIndex];
        currentVoxel = voxelMats[colorIndex];
        currentSpider = spiderMats[colorIndex];       
    }

    private Material GetMaterial(Material[] matArray, ChromaColor color)
    {
        return matArray[(int)color];
    }

    public Material WhiteMaterial { get { return whiteMaterial; } }

    public Material GetPlayer1Material(ChromaColor color)
    {
        return GetMaterial(player1Mats, color);
    }

    //Player Shot methods
    public PlayerShotController GetPlayer1Shot()
    {
        return currentPlayer1ShotPool.GetObject();
    }

    public MuzzleController GetPlayer1Muzzle(ChromaColor color)
    {
        return player1MuzzlePools[(int)color].GetObject();
    }

    public MuzzleController GetPlayer1Muzzle()
    {
        return currentPlayer1MuzzlePool.GetObject();
    }

    public PlayerShotController GetPlayer1Shot(ChromaColor color)
    {
        return player1ShotPools[(int)color].GetObject();
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
            //Material[] mats = spiderAI.rend.materials;
            //mats[1] = GetMaterial(spiderMats, color);
            //spiderAI.rend.materials = mats;

            spiderAI.SetMaterials(new[] { GetMaterial(spiderMats, color) });

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
