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
}
