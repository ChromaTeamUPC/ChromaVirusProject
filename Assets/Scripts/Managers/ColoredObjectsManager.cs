using UnityEngine;
using System.Collections;


public class ColoredObjectsManager : MonoBehaviour
{
    //Platform 1 Materials

    //Player
    public Material[] player1Mats = new Material[4];
    private Material currentPlayer1;

    //Player Shot Materials
    public Material[] playerShotMats = new Material[4];
    private Material currentPlayerShot;

    //Player Shot Light Color
    public Color[] playerShotLights = new Color[4];
    private Color currentPlayerShotLight;

    //Voxel Materials
    public Material[] voxelMats = new Material[4];
    private Material currentVoxel;

    //Spider Materials
    public Material[] spiderMats = new Material[4];
    private Material currentSpider;

    private ObjectPool playerShotPool;
    private ObjectPool spiderPool;
    private ScriptObjectPool<VoxelController> voxelPool;

    private ChromaColor currentColor;

	void Start ()
    {
        Debug.Log("Colored Objects Manager created");

        playerShotPool = rsc.poolMng.playerShotPool;
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
        currentPlayerShot = playerShotMats[colorIndex];
        currentPlayerShotLight = playerShotLights[colorIndex];
        currentVoxel = voxelMats[colorIndex];
        currentSpider = spiderMats[colorIndex];

        /*switch (currentColor)
        {
            case ChromaColor.RED:
                //currentPlatform1 = platform1Red;
                currentPlayer1 = player1Red;
                currentPlayerShot = playerShotRed;
                currentPlayerShotLight = playerShotRedLight;
                currentVoxel = voxelRed;
                currentSpider = spiderRed;
                break;
            case ChromaColor.GREEN:
                //currentPlatform1 = platform1Green;
                currentPlayer1 = player1Green;
                currentPlayerShot = playerShotGreen;
                currentPlayerShotLight = playerShotGreenLight;
                currentVoxel = voxelGreen;
                currentSpider = spiderGreen;
                break;
            case ChromaColor.BLUE:
                //currentPlatform1 = platform1Blue;
                currentPlayer1 = player1Blue;
                currentPlayerShot = playerShotBlue;
                currentPlayerShotLight = playerShotBlueLight;
                currentVoxel = voxelBlue;
                currentSpider = spiderBlue;
                break;
            case ChromaColor.YELLOW:
                //currentPlatform1 = platform1Yellow;
                currentPlayer1 = player1Yellow;
                currentPlayerShot = playerShotYellow;
                currentPlayerShotLight = playerShotYellowLight;
                currentVoxel = voxelYellow;
                currentSpider = spiderYellow;
                break;
        }*/
    }

    private Material GetMaterial(Material[] matArray, ChromaColor color)
    {
        return matArray[(int)color];
    }

    public Material GetPlayer1Material(ChromaColor color)
    {
        return GetMaterial(player1Mats, color);
    }

    /*public Material GetPlatform1Material(ChromaColor color)
    {
        //switch (color)
        //{
        //    case ChromaColor.RED:
        //        return platform1Red;
        //    case ChromaColor.GREEN:
        //        return platform1Green;
        //    case ChromaColor.BLUE:
        //        return platform1Blue;
        //    case ChromaColor.YELLOW:
        //        return platform1Yellow;
        //}

        return null; //Should not reach here
    }*/

    //Player Shot methods
    public GameObject GetPlayerShot()
    {
        //Get a shot from pool
        GameObject shot = playerShotPool.GetObject();

        if (shot != null)
        {
            shot.GetComponent<PlayerShotController>().color = currentColor;
            shot.GetComponent<Renderer>().material = currentPlayerShot;
        }

        return shot;
    }

    public GameObject GetPlayerShot(ChromaColor color)
    {
        //Get a shot from pool
        GameObject shot = playerShotPool.GetObject();

        if (shot != null)
        {
            shot.GetComponent<Renderer>().material = GetMaterial(playerShotMats, color);
            shot.GetComponent<PlayerShotController>().color = color;
        }

        return shot;
    }

    public Material GetPlayerShotMaterial(ChromaColor color)
    {
        return GetMaterial(playerShotMats, color);
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
            Material[] mats = spiderAI.spiderRenderer.materials;
            mats[1] = currentSpider;
            spiderAI.spiderRenderer.materials = mats;

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
            Material[] mats = spiderAI.spiderRenderer.materials;
            mats[1] = GetMaterial(spiderMats, color);
            spiderAI.spiderRenderer.materials = mats;

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
