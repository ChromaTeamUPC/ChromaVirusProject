using UnityEngine;
using System.Collections;


public class ColoredObjectsManager : MonoBehaviour
{
    [SerializeField]
    private Material whiteMaterial;
    [SerializeField]
    private Material transparentMaterial;

    //Colors
    [SerializeField]
    private Color[] basicColors = new Color[4];

    [SerializeField]
    private Color[] treeColors = new Color[4];

    //Player
    [SerializeField]
    private Material[] player1Mats = new Material[4];

    [SerializeField]
    private Material[] player1ShieldMats = new Material[4];

    [SerializeField]
    private Material[] player1TrailMats = new Material[4];

    //Voxel Materials
    [SerializeField]
    private Material[] voxelMats = new Material[6];
    private Material currentVoxelMat;

    //Spider Materials
    [SerializeField]
    private Material[] spiderMats = new Material[4];
    private Material currentSpiderMat;

    //Mosquito Materials
    [SerializeField]
    private Material[] mosquitoMats = new Material[4];
    private Material currentMosquitoMat;

    //Worm Materials
    [SerializeField]
    private Material wormGreyMat;
    [SerializeField]
    private Material wormWireframeMat;
    [SerializeField]
    private Material[] wormMats = new Material[4];

    //Floor Materials
    [SerializeField]
    private Material floorWhiteMat;
    [SerializeField]
    private Material[] floorMats = new Material[4];

    //Bridge Materials
    [SerializeField]
    private Material[] bridgeMats = new Material[4];

    //Capacitor Materials
    [SerializeField]
    private Material[] capacitorEmptyMats = new Material[4];
    [SerializeField]
    private Material[] capacitor33Mats = new Material[4];
    [SerializeField]
    private Material[] capacitor66Mats = new Material[4];
    [SerializeField]
    private Material[] capacitorFullMats = new Material[4];

    private PlayerShotPool[] player1ShotPools = new PlayerShotPool[4];
    private PlayerShotPool currentPlayer1ShotPool;

    private PlayerMuzzlePool[] player1MuzzlePools = new PlayerMuzzlePool[4];
    private PlayerMuzzlePool currentPlayer1MuzzlePool;

    private SpiderPool spiderPool;
    private MosquitoPool mosquitoPool;
    private MosquitoWeakShotPool[] mosquitoWeakShotPools = new MosquitoWeakShotPool[4];
    private VoxelPool voxelPool;

    private ChromaColor currentColor;

	void Start ()
    {
        //Debug.Log("Colored Objects Manager created");

        player1ShotPools[0] = rsc.poolMng.player1ShotRedPool;
        player1ShotPools[1] = rsc.poolMng.player1ShotGreenPool;
        player1ShotPools[2] = rsc.poolMng.player1ShotBluePool;
        player1ShotPools[3] = rsc.poolMng.player1ShotYellowPool;

        player1MuzzlePools[0] = rsc.poolMng.player1MuzzleRedPool;
        player1MuzzlePools[1] = rsc.poolMng.player1MuzzleGreenPool;
        player1MuzzlePools[2] = rsc.poolMng.player1MuzzleBluePool;
        player1MuzzlePools[3] = rsc.poolMng.player1MuzzleYellowPool;

        spiderPool = rsc.poolMng.spiderPool;
        mosquitoPool = rsc.poolMng.mosquitoPool;

        mosquitoWeakShotPools[0] = rsc.poolMng.mosquitoWeakShotRedPool;
        mosquitoWeakShotPools[1] = rsc.poolMng.mosquitoWeakShotGreenPool;
        mosquitoWeakShotPools[2] = rsc.poolMng.mosquitoWeakShotBluePool;
        mosquitoWeakShotPools[3] = rsc.poolMng.mosquitoWeakShotYellowPool;

        voxelPool = rsc.poolMng.voxelPool;
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        currentColor = rsc.colorMng.CurrentColor;
        SetCurrentMaterials();
    }

    void OnDestroy()
    {
        if(rsc.eventMng != null)
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        //Debug.Log("Colored Objects Manager destroyed");
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

        currentPlayer1ShotPool = player1ShotPools[colorIndex];
        currentPlayer1MuzzlePool = player1MuzzlePools[colorIndex];
        currentVoxelMat = voxelMats[colorIndex];
        currentSpiderMat = spiderMats[colorIndex];
        currentMosquitoMat = mosquitoMats[colorIndex];    
    }

    public Color GetColor()
    {
        return GetColor(currentColor);
    }

    public Color GetColor(ChromaColor color)
    {
        return basicColors[(int)color];
    }

    public Color GetTreeColor(ChromaColor color)
    {
        return treeColors[(int)color];
    }

    private Material GetMaterial(Material[] matArray, ChromaColor color)
    {
        return matArray[(int)color];
    }

    public Material WhiteMaterial { get { return whiteMaterial; } }

    public Material TransparentMaterial { get { return transparentMaterial; } }

    public Material GetPlayer1Material(ChromaColor color)
    {
        return GetMaterial(player1Mats, color);
    }

    public Material GetPlayer1ShieldMaterial(ChromaColor color)
    {
        return GetMaterial(player1ShieldMats, color);
    }

    public Material GetPlayer1TrailMaterial(ChromaColor color)
    {
        return GetMaterial(player1TrailMats, color);
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

    //Spider methods
    public SpiderAIBehaviour GetSpider(bool random = false)
    {
        if(random)
            return GetSpider(ChromaColorInfo.Random);

        //Get a spider from pool
        SpiderAIBehaviour spider = spiderPool.GetObject();       

        if (spider != null)
        {
            spider.color = currentColor;
            spider.SetMaterials(new[] { currentSpiderMat });
            return spider;
        }

        return null;
    }

    public SpiderAIBehaviour GetSpider(int offset)
    {
        int colorIndex = ((int)currentColor + offset) % ChromaColorInfo.Count;

        //Offset can be negative
        if (colorIndex < 0)
            colorIndex = ChromaColorInfo.Count + colorIndex;

        ChromaColor color = (ChromaColor)colorIndex;

        return GetSpider(color);
    }

    public SpiderAIBehaviour GetSpider(ChromaColor color)
    {
        //Get a spider from pool
        SpiderAIBehaviour spider = spiderPool.GetObject();

        if (spider != null)
        {
            spider.color = color;

            spider.SetMaterials(new[] { GetMaterial(spiderMats, color) });

            return spider;
        }

        return null;
    }


    //Mosquito methods
    public MosquitoAIBehaviour GetMosquito(bool random = false)
    {
        if (random)
            return GetMosquito(ChromaColorInfo.Random);

        //Get a mosquito from pool
        MosquitoAIBehaviour mosquito = mosquitoPool.GetObject();

        if (mosquito != null)
        {
            mosquito.color = currentColor;
            mosquito.SetMaterials(new[] { currentMosquitoMat });
            return mosquito;
        }

        return null;
    }

    public MosquitoAIBehaviour GetMosquito(int offset)
    {
        int colorIndex = ((int)currentColor + offset) % ChromaColorInfo.Count;

        //Offset can be negative
        if (colorIndex < 0)
            colorIndex = ChromaColorInfo.Count + colorIndex;

        ChromaColor color = (ChromaColor)colorIndex;

        return GetMosquito(color);
    }

    public MosquitoAIBehaviour GetMosquito(ChromaColor color)
    {
        //Get a mosquito from pool
        MosquitoAIBehaviour mosquito = mosquitoPool.GetObject();

        if (mosquito != null)
        {
            mosquito.color = color;
            mosquito.SetMaterials(new[] { GetMaterial(mosquitoMats, color) });

            return mosquito;
        }

        return null;
    }

    public MosquitoWeakShotController GetMosquitoWeakShot(ChromaColor color)
    {
        return mosquitoWeakShotPools[(int)color].GetObject();
    }

    //Worm methods
    public Material GetWormGreyMaterial()
    {
        return wormGreyMat;
    }

    public Material GetWormWireframeMaterial()
    {
        return wormWireframeMat;
    }

    public Material GetWormBodyMaterial(ChromaColor color)
    {
        return GetMaterial(wormMats, color);
    }

    //Voxel methods
    public VoxelController GetVoxel()
    {
        //Get a voxel from pool
        VoxelController voxel = voxelPool.GetObject();

        if (voxel != null)
        {
            voxel.GetComponent<Renderer>().sharedMaterial = currentVoxelMat;
        }

        return voxel;
    }

    public VoxelController GetVoxel(ChromaColor color)
    {
        //Get a voxel from pool
        VoxelController voxel = voxelPool.GetObject();

        if (voxel != null)
        {
            voxel.GetComponent<Renderer>().sharedMaterial = GetMaterial(voxelMats, color);       
        }

        return voxel;
    }

    public Material GetVoxelMaterial(ChromaColor color)
    {
        return GetMaterial(voxelMats, color);
    }

    public Material GetVoxelWhiteMaterial()
    {
        return voxelMats[4];
    }

    public Material GetVoxelGreyMaterial()
    {
        return voxelMats[5];
    }

    public Material GetVoxelRandomMaterial()
    {
        return GetVoxelMaterial(ChromaColorInfo.Random);
    }

    public Material GetFloorWhiteMaterial()
    {
        return floorWhiteMat;
    }

    public Material GetFloorMaterial(ChromaColor color)
    {
        return GetMaterial(floorMats, color);
    }

    public Material GetBridgeMaterial(ChromaColor color)
    {
        return GetMaterial(bridgeMats, color);
    }

    public Material GetCapacitorMaterial(CapacitorController.CapacitorLevel chargeLevel, ChromaColor color)
    {
        switch (chargeLevel)
        {
            case CapacitorController.CapacitorLevel.EMPTY:
                return GetMaterial(capacitorEmptyMats, color);

            case CapacitorController.CapacitorLevel.ONE_THIRD:
                return GetMaterial(capacitor33Mats, color);

            case CapacitorController.CapacitorLevel.TWO_THIRDS:
                return GetMaterial(capacitor66Mats, color);

            case CapacitorController.CapacitorLevel.FULL:
                return GetMaterial(capacitorFullMats, color);

            default:
                return GetMaterial(capacitorEmptyMats, color);
        }
    }
}
