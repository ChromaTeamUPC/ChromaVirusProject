using UnityEngine;
using System.Collections;

public class GameInfo
{
    public GameObject player1;
    public PlayerController player1Controller;

    public GameObject player2;
    public PlayerController player2Controller;

    public int numberOfPlayers = 1;

    public Vector3 gameCameraOffset;
    public Quaternion gameCameraRotation;
}
