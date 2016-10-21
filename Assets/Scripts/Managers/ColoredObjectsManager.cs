using UnityEngine;
using System.Collections;


public class ColoredObjectsManager : MonoBehaviour
{
    [Header("Basic Materials")]
    [SerializeField]
    private Material whiteMaterial;
    [SerializeField]
    private Material transparentMaterial;

    //Colors
    [SerializeField]
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    private Color[] basicColors = new Color[4];

    [SerializeField]
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    private Color[] floorColors = new Color[4];

    [SerializeField]
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    private Color[] hexagonColors = new Color[4];

    [SerializeField]
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    private Color[] treeColors = new Color[4];
    [SerializeField]
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    private Color[] treeLightColors = new Color[4];

    //Player
    [Header("Player Materials")]
    [SerializeField]
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    private Color[] playerColors = new Color[4];

    [SerializeField]
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    private Color[] playerAimLaserColors = new Color[4];

    [SerializeField]
    private Material[] playerTrailMats = new Material[4];

    //Voxel Materials
    [Header("Voxel Materials")]
    [SerializeField]
    private Material[] voxelMats = new Material[6];
    private Material currentVoxelMat;

    [Header("Enemies Materials", order=0)]
    [Header("Spider Materials", order=1)]
    //Spider Materials
    [SerializeField]
    private Material[] spiderMats = new Material[4];
    private Material currentSpiderMat;

    [Header("Mosquito Materials")]
    //Mosquito Materials
    [SerializeField]
    private Material[] mosquitoMats = new Material[4];
    private Material currentMosquitoMat;

    [SerializeField]
    private Material[] mosquitoLightSpotMats = new Material[4];

    [Header("Worm Materials", order = 0)]
    [Header("Worm Head Materials", order = 1)]
    //Worm Materials
    [SerializeField]
    private Material[] wormHeadMats = new Material[4];
    [Header("Worm Body Materials")]
    [SerializeField]
    private Material wormMainBodyWireframeMat;
    [Header("Worm Body Crystals Materials")]
    [SerializeField]
    private Material wormBodyGreyMat;
    [SerializeField]
    private Material wormBodyWireframeMat;
    [SerializeField]
    private Material[] wormBodyMats = new Material[4];
    [SerializeField]
    private Material[] wormBodyDimMats = new Material[4];
    [Header("Worm Junction Materials")]
    [SerializeField]
    private Material wormJunctionWireframeMat;

    [Header("Props Materials")]
    //Hexagon Materials
    [SerializeField]
    private Material[] hexagonMats = new Material[4];
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

    private PlayerShotPool[] player2ShotPools = new PlayerShotPool[4];
    private PlayerShotPool currentPlayer2ShotPool;

    private MuzzlePool[] playerMuzzlePools = new MuzzlePool[4];
    private MuzzlePool currentPlayerMuzzlePool;

    private PlayerDashPool[] playerDashPools = new PlayerDashPool[4];
    private PlayerDashPool currentPlayerDashPool;

    private SpiderPool spiderPool;
    private MosquitoPool mosquitoPool;
    private MosquitoWeakShotPool[] mosquitoWeakShotPools = new MosquitoWeakShotPool[4];
    private VoxelPool voxelPool;

    private MuzzlePool[] turretMuzzlePools = new MuzzlePool[4];
    private MuzzlePool currentTurretMuzzlePool;

    private ChromaColor currentColor;

	void Start ()
    {
        //Debug.Log("Colored Objects Manager created");

        player1ShotPools[0] = rsc.poolMng.player1ShotRedPool;
        player1ShotPools[1] = rsc.poolMng.player1ShotGreenPool;
        player1ShotPools[2] = rsc.poolMng.player1ShotBluePool;
        player1ShotPools[3] = rsc.poolMng.player1ShotYellowPool;

        player2ShotPools[0] = rsc.poolMng.player2ShotRedPool;
        player2ShotPools[1] = rsc.poolMng.player2ShotGreenPool;
        player2ShotPools[2] = rsc.poolMng.player2ShotBluePool;
        player2ShotPools[3] = rsc.poolMng.player2ShotYellowPool;

        playerMuzzlePools[0] = rsc.poolMng.playerMuzzleRedPool;
        playerMuzzlePools[1] = rsc.poolMng.playerMuzzleGreenPool;
        playerMuzzlePools[2] = rsc.poolMng.playerMuzzleBluePool;
        playerMuzzlePools[3] = rsc.poolMng.playerMuzzleYellowPool;

        playerDashPools[0] = rsc.poolMng.playerDashRedPool;
        playerDashPools[1] = rsc.poolMng.playerDashGreenPool;
        playerDashPools[2] = rsc.poolMng.playerDashBluePool;
        playerDashPools[3] = rsc.poolMng.playerDashYellowPool;

        spiderPool = rsc.poolMng.spiderPool;
        mosquitoPool = rsc.poolMng.mosquitoPool;

        mosquitoWeakShotPools[0] = rsc.poolMng.mosquitoWeakShotRedPool;
        mosquitoWeakShotPools[1] = rsc.poolMng.mosquitoWeakShotGreenPool;
        mosquitoWeakShotPools[2] = rsc.poolMng.mosquitoWeakShotBluePool;
        mosquitoWeakShotPools[3] = rsc.poolMng.mosquitoWeakShotYellowPool;

        turretMuzzlePools[0] = rsc.poolMng.turretMuzzleRedPool;
        turretMuzzlePools[1] = rsc.poolMng.turretMuzzleGreenPool;
        turretMuzzlePools[2] = rsc.poolMng.turretMuzzleBluePool;
        turretMuzzlePools[3] = rsc.poolMng.turretMuzzleYellowPool;

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
        currentPlayer2ShotPool = player2ShotPools[colorIndex];
        currentPlayerMuzzlePool = playerMuzzlePools[colorIndex];
        currentPlayerDashPool = playerDashPools[colorIndex];
        currentVoxelMat = voxelMats[colorIndex];
        currentSpiderMat = spiderMats[colorIndex];
        currentMosquitoMat = mosquitoMats[colorIndex];
        currentTurretMuzzlePool = turretMuzzlePools[colorIndex];    
    }

    public Color GetColor()
    {
        return GetColor(currentColor);
    }

    public Color GetColor(ChromaColor color)
    {
        return basicColors[(int)color];
    }

    public Color GetFloorColor(ChromaColor color)
    {
        return floorColors[(int)color];
    }

    public Color GetHexagonColor(ChromaColor color)
    {
        return hexagonColors[(int)color];
    }

    public Color GetTreeColor(ChromaColor color)
    {
        return treeColors[(int)color];
    }

    public Color GetTreeLightColor(ChromaColor color)
    {
        return treeLightColors[(int)color];
    }

    public Color GetPlayerColor(ChromaColor color)
    {
        return playerColors[(int)color];
    }

    public Color GetPlayerAimLaserColor(ChromaColor color)
    {
        return playerAimLaserColors[(int)color];
    }

    private Material GetMaterial(Material[] matArray, ChromaColor color)
    {
        return matArray[(int)color];
    }

    public Material WhiteMaterial { get { return whiteMaterial; } }

    public Material TransparentMaterial { get { return transparentMaterial; } }

    public Material GetPlayerTrailMaterial(ChromaColor color)
    {
        return GetMaterial(playerTrailMats, color);
    }

    //Player Shot methods
    public PlayerShotController GetPlayer1Shot()
    {
        return currentPlayer1ShotPool.GetObject();
    }

    public PlayerShotController GetPlayer1Shot(ChromaColor color)
    {
        return player1ShotPools[(int)color].GetObject();
    }

    public PlayerShotController GetPlayer2Shot()
    {
        return currentPlayer2ShotPool.GetObject();
    }

    public PlayerShotController GetPlayer2Shot(ChromaColor color)
    {
        return player2ShotPools[(int)color].GetObject();
    }

    public MuzzleController GetPlayerMuzzle()
    {
        return currentPlayerMuzzlePool.GetObject();
    }

    public MuzzleController GetPlayerMuzzle(ChromaColor color)
    {
        return playerMuzzlePools[(int)color].GetObject();
    }

    public PlayerDashController GetPlayerDash()
    {
        return currentPlayerDashPool.GetObject();
    }

    public PlayerDashController GetPlayerDash(ChromaColor color)
    {
        return playerDashPools[(int)color].GetObject();
    }

    //Spider methods
    public SpiderAIBehaviour GetSpider(bool random, Vector3 position)
    {
        if(random)
            return GetSpider(ChromaColorInfo.Random, position);

        //Get a spider from pool
        SpiderAIBehaviour spider = spiderPool.GetObject(position);       

        if (spider != null)
        {
            spider.color = currentColor;
            spider.SetMaterials(new[] { currentSpiderMat });
            return spider;
        }

        return null;
    }

    public SpiderAIBehaviour GetSpider(int offset, Vector3 position)
    {
        int colorIndex = ((int)currentColor + offset) % ChromaColorInfo.Count;

        //Offset can be negative
        if (colorIndex < 0)
            colorIndex = ChromaColorInfo.Count + colorIndex;

        ChromaColor color = (ChromaColor)colorIndex;

        return GetSpider(color, position);
    }

    public SpiderAIBehaviour GetSpider(ChromaColor color, Vector3 position)
    {
        //Get a spider from pool
        SpiderAIBehaviour spider = spiderPool.GetObject(position);

        if (spider != null)
        {
            spider.color = color;

            spider.SetMaterials(new[] { GetMaterial(spiderMats, color) });

            return spider;
        }

        return null;
    }


    //Mosquito methods
    public MosquitoAIBehaviour GetMosquito(bool random, Vector3 position)
    {
        if (random)
            return GetMosquito(ChromaColorInfo.Random, position);

        //Get a mosquito from pool
        MosquitoAIBehaviour mosquito = mosquitoPool.GetObject(position);

        if (mosquito != null)
        {
            mosquito.color = currentColor;
            mosquito.SetMaterials(new[] { currentMosquitoMat });
            return mosquito;
        }

        return null;
    }

    public MosquitoAIBehaviour GetMosquito(int offset, Vector3 position)
    {
        int colorIndex = ((int)currentColor + offset) % ChromaColorInfo.Count;

        //Offset can be negative
        if (colorIndex < 0)
            colorIndex = ChromaColorInfo.Count + colorIndex;

        ChromaColor color = (ChromaColor)colorIndex;

        return GetMosquito(color, position);
    }

    public MosquitoAIBehaviour GetMosquito(ChromaColor color, Vector3 position)
    {
        //Get a mosquito from pool
        MosquitoAIBehaviour mosquito = mosquitoPool.GetObject(position);

        if (mosquito != null)
        {
            mosquito.color = color;
            mosquito.SetMaterials(new[] { GetMaterial(mosquitoMats, color), GetMaterial(mosquitoLightSpotMats, color) });

            return mosquito;
        }

        return null;
    }

    public MosquitoWeakShotController GetMosquitoWeakShot(ChromaColor color)
    {
        return mosquitoWeakShotPools[(int)color].GetObject();
    }

    //Worm methods
    public Material GetWormHeadMaterial(int chargeLevel)
    {
        return wormHeadMats[chargeLevel];
    }

    public Material GetWormBodyGreyMaterial()
    {
        return wormBodyGreyMat;
    }

    public Material[] GetWormBodyWireframeMaterial()
    {
        return new[] { wormMainBodyWireframeMat, wormBodyWireframeMat };
    }

    public Material GetWormBodyMaterial(ChromaColor color)
    {
        return GetMaterial(wormBodyMats, color);
    }

    public Material GetWormBodyDimMaterial(ChromaColor color)
    {
        return GetMaterial(wormBodyDimMats, color);
    }

    public Material GetWormJunctionWireframeMaterial()
    {
        return wormJunctionWireframeMat;
    }


    //Turret methods
    public MuzzleController GetTurretMuzzle()
    {
        return currentTurretMuzzlePool.GetObject();
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

    public Material GetHexagonMaterial()
    {
        return GetMaterial(hexagonMats, ChromaColorInfo.Random);
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
