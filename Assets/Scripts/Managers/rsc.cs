using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class rsc : MonoBehaviour
{
    //Objects maintained between scenes
    public static DebugManager debugMng;
    public static EventManager eventMng;
    public static EnemyManager enemyMng;
    public static PoolManager poolMng;
    public static ColorManager colorMng;
    public static ColoredObjectsManager coloredObjectsMng;
    public static GameManager gameMng;

    public static GameInfo gameInfo;

    //Objects loaded every scene
    public static CamerasManager camerasMng;

    private static bool objectsInitialized = false;
    public static bool ObjectsInitialized { get { return objectsInitialized; } }

    public static bool LoadResources()
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
    }

    //Objects created in every scene
    /*public static CameraManager cameraManager;
    */

    [SerializeField]
    private DebugManager debugManager;
    [SerializeField]
    private EventManager eventManager;
    [SerializeField]
    private EnemyManager enemyManager;
    [SerializeField]
    private PoolManager poolManager;
    [SerializeField]
    private ColorManager colorManager;
    [SerializeField]
    private ColoredObjectsManager coloredObjectsManager;
    [SerializeField]
    private GameManager gameManager;

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
                Debug.Log("Storing Debug Manager");
                debugMng = debugManager;
            }

            if (eventMng == null)
            {
                Debug.Log("Storing Event Manager");
                eventMng = eventManager;
            }

            if (enemyMng == null)
            {
                Debug.Log("Storing Enemy Manager");
                enemyMng = enemyManager;
            }

            if (poolMng == null)
            {
                Debug.Log("Storing Pool Manager");
                poolMng = poolManager;
            }

            if (colorMng == null)
            {
                Debug.Log("Storing Color Manager");
                colorMng = colorManager;
            }

            if (coloredObjectsMng == null)
            {
                Debug.Log("Storing Colored Objects Manager");
                coloredObjectsMng = coloredObjectsManager;
            }

            if (gameMng == null)
            {
                Debug.Log("Storing Game Manager");
                gameMng = gameManager;
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
    }
}
