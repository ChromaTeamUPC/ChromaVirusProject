using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class InitialSetup : MonoBehaviour {

    public DebugManager debugManager;
    public EventManager eventManager;
    public EnemyManager enemyManager;
    public PoolManager poolManager;
    public ColorManager colorManager;
    public ColoredObjectsManager coloredObjectsManager;
    public GameManager gameManager;

    public GameObject player1;
    public GameObject player2;

    public Vector3 gameCameraOffset;
    public Transform gameCameraRotation;

    void Awake()
    {
        /*DontDestroyOnLoad(gameObject);

        if (rsc.debugManager == null)
        {
            Debug.Log("Storing Debug Manager");
            rsc.debugManager = debugManager;
        }

        if (rsc.eventManager == null)
        {
            Debug.Log("Storing Event Manager");
            rsc.eventManager = eventManager;
        }

        if (rsc.enemyManager == null)
        {
            Debug.Log("Storing Enemy Manager");
            rsc.enemyManager = enemyManager;
        }

        if (rsc.poolManager == null)
        {
            Debug.Log("Storing Pool Manager");
            rsc.poolManager = poolManager;
        }

        if (rsc.colorManager == null)
        {
            Debug.Log("Storing Color Manager");
            rsc.colorManager = colorManager;
        }

        if (rsc.coloredObjectsManager == null)
        {
            Debug.Log("Storing Colored Objects Manager");
            rsc.coloredObjectsManager = coloredObjectsManager;
        }

        if (rsc.gameManager == null)
        {
            Debug.Log("Storing Game Manager");
            rsc.gameManager = gameManager;
        }

        if (GameInfo.player1 == null)
        {
            GameInfo.player1 = GameObject.Instantiate<GameObject>(player1);
            GameInfo.player1.name = "Player1";
            DontDestroyOnLoad(GameInfo.player1);
            GameInfo.player1Controller = GameInfo.player1.GetComponent<PlayerController>();
        }

        if (GameInfo.player2 == null)
        {
            GameInfo.player2 = GameObject.Instantiate<GameObject>(player2);
            GameInfo.player2.name = "Player2";
            DontDestroyOnLoad(GameInfo.player2);
            GameInfo.player2Controller = GameInfo.player2.GetComponent<PlayerController>();
        }

        GameInfo.gameCameraOffset = gameCameraOffset;
        GameInfo.gameCameraRotation = gameCameraRotation.rotation;

        SceneMng.LoadScene("MainMenu");*/
    }
}
