using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using InControl;

public class rsc : MonoBehaviour
{
    //Objects maintained between scenes
    public static DebugManager debugMng;
    public static EventManager eventMng;
    public static InControlManager inputMng;
    public static TutorialManager tutorialMng;
    public static AudioManager audioMng;
    public static EnemyManager enemyMng;
    public static PoolManager poolMng;
    public static ColorManager colorMng;
    public static ColoredObjectsManager coloredObjectsMng;
    public static RumbleManager rumbleMng;
    public static GameManager gameMng;
    public static VoxelizationManager voxelizationMng;
    public static StatsManager statsMng;
    public static CoroutineHelper coroutineHlp;

    public static GameInfo gameInfo;

    //Objects loaded every scene
    public static CamerasManager camerasMng;

    private static bool objectsInitialized = false;
    public static bool ObjectsInitialized { get { return objectsInitialized; } }

    /*public static bool LoadResources()
    {
        if (!objectsInitialized)
        {
            SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);

            GameObject instance = GameObject.Find("Resources");
            if (instance != null)
                instance.GetComponent<rsc>().InitStaticObjects();

            return true;
        }

        return false;
    }*/

    //Objects created in every scene
    [SerializeField]
    private DebugManager debugManager;
    [SerializeField]
    private EventManager eventManager;
    [SerializeField]
    private InControlManager inputManager;
    [SerializeField]
    private TutorialManager tutorialManager;
    [SerializeField]
    private AudioManager audioManager;
    [SerializeField]
    private EnemyManager enemyManager;
    [SerializeField]
    private PoolManager poolManager;
    [SerializeField]
    private ColorManager colorManager;
    [SerializeField]
    private ColoredObjectsManager coloredObjectsManager;
    [SerializeField]
    private RumbleManager rumbleManager;
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private VoxelizationManager voxelizationManager;
    [SerializeField]
    private StatsManager statsManager;
    [SerializeField]
    private CoroutineHelper coroutineHelper;

    [SerializeField]
    private GameObject player1prefab;
    [SerializeField]
    private GameObject player2prefab;

    [SerializeField]
    private Vector3 gameCameraOffset;
    [SerializeField]
    private Transform gameCameraRotation;

    void Awake()
    {
        InitStaticObjects();
    }

    private void InitStaticObjects()
    {
        if(!objectsInitialized)
        {
            DontDestroyOnLoad(gameObject);

            //Double check for every static variable
            if (debugMng == null)
            {
                //Debug.Log("Storing Debug Manager");
                debugMng = debugManager;
            }

            if (eventMng == null)
            {
                //Debug.Log("Storing Event Manager");
                eventMng = eventManager;
            }

            if (inputMng == null)
            {
                //Debug.Log("Storing Input Manager");
                inputMng = inputManager;
            }

            if (tutorialMng == null)
            {
                //Debug.Log("Storing Tutorial Manager");
                tutorialMng = tutorialManager;
            }

            if (audioMng == null)
            {
                //Debug.Log("Storing Audio Manager");
                audioMng = audioManager;
            }

            if (enemyMng == null)
            {
                //Debug.Log("Storing Enemy Manager");
                enemyMng = enemyManager;
            }

            if (poolMng == null)
            {
                //Debug.Log("Storing Pool Manager");
                poolMng = poolManager;
            }

            if (colorMng == null)
            {
                //Debug.Log("Storing Color Manager");
                colorMng = colorManager;
            }

            if (coloredObjectsMng == null)
            {
                //Debug.Log("Storing Colored Objects Manager");
                coloredObjectsMng = coloredObjectsManager;
            }

            if (rumbleMng == null)
            {
                //Debug.Log("Storing Rumble Manager");
                rumbleMng = rumbleManager;
            }

            if (gameMng == null)
            {
                //Debug.Log("Storing Game Manager");
                gameMng = gameManager;
            }

            if (voxelizationMng == null)
            {
                //Debug.Log("Storing Voxelization Manager");
                voxelizationMng = voxelizationManager;
            }

            if (statsMng == null)
            {
                //Debug.Log("Storing Stats Manager");
                statsMng = statsManager;
            }

            if (coroutineHlp == null)
            {
                //Debug.Log("Storing Coroutine Helper");
                coroutineHlp = coroutineHelper;
            }

            if (gameInfo == null)
            {
                gameInfo = new GameInfo();

                gameInfo.player1 = GameObject.Instantiate<GameObject>(player1prefab);
                gameInfo.player1.name = "Player1";
                DontDestroyOnLoad(gameInfo.player1);
                gameInfo.player1Controller = gameInfo.player1.GetComponent<PlayerController>();

                gameInfo.player2 = GameObject.Instantiate<GameObject>(player2prefab);
                gameInfo.player2.name = "Player2";
                DontDestroyOnLoad(gameInfo.player2);
                gameInfo.player2Controller = gameInfo.player2.GetComponent<PlayerController>();

                gameInfo.gameCameraOffset = gameCameraOffset;
                gameInfo.gameCameraRotation = gameCameraRotation.rotation;
            } 

            objectsInitialized = true;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
}
