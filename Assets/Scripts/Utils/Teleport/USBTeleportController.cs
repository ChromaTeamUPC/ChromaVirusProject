using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class USBTeleportController : MonoBehaviour {

    public Transform destiny;
    public Transform[] waypoints;
    public float speed = 8f;

    private PlayerController player1;
    private PlayerController player2;
    //public ParticleSystem electricPS;

    private AudioSource audioSource;

    private bool multiPlayerTransport;
    public float multiPlayerTransportOffset = 1f;

    private List<PlayerController> players = new List<PlayerController>();
    private List<int> playerCurrentDestiny = new List<int>();
    private List<float> playerCurrentLerpTime = new List<float>();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            switch (player.Id)
            {
                case 1:
                    player1 = player;
                    break;

                case 2:
                    player2 = player;
                    break;
            }

            CheckEnterUSB();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            switch (player.Id)
            {
                case 1:
                    player1 = null;
                    break;

                case 2:
                    player2 = null;
                    break;
            }
        }
    }

    private void CheckEnterUSB()
    {
        multiPlayerTransport = false;

        //If only one player enter inmediate
        if (rsc.gameInfo.numberOfPlayers == 1 && player1 != null)
        {
            player1.transform.position = waypoints[0].position;
            player1.EnteredUSB();

            players.Add(player1);
            playerCurrentDestiny.Add(1);
            playerCurrentLerpTime.Add(0f);

            audioSource.Play();
        }
        //two players
        else
        {
            //If both in hexagon enter
            if(player1 != null && player2 != null)
            {
                multiPlayerTransport = true;

                player1.transform.position = waypoints[0].position;
                player1.EnteredUSB();

                players.Add(player1);
                playerCurrentDestiny.Add(1);
                playerCurrentLerpTime.Add(0f);

                player2.transform.position = waypoints[0].position;
                player2.EnteredUSB();

                players.Add(player2);
                playerCurrentDestiny.Add(1);
                playerCurrentLerpTime.Add(0.3f);

                audioSource.Play();
            }
            else
            {
                if(player1 != null && rsc.gameInfo.player2Controller.Lives == 0)
                {
                    player1.transform.position = waypoints[0].position;
                    player1.EnteredUSB();

                    players.Add(player1);
                    playerCurrentDestiny.Add(1);
                    playerCurrentLerpTime.Add(0f);

                    audioSource.Play();
                }

                if (player2 != null && rsc.gameInfo.player1Controller.Lives == 0)
                {
                    player2.transform.position = waypoints[0].position;
                    player2.EnteredUSB();

                    players.Add(player2);
                    playerCurrentDestiny.Add(1);
                    playerCurrentLerpTime.Add(0f);

                    audioSource.Play();
                }
            }
        }       
    }

    void Update()
    {
        if(players.Count > 0)
        {
            int i = 0;
            while (i < players.Count)
            {
                PlayerController player = players[i];
                Vector3 endPoint = waypoints[playerCurrentDestiny[i]].position;
                
                //If not near waypoint, continue moving
                if(Vector3.Distance(player.transform.position, endPoint) > 0.01f)
                {

                    player.transform.position = Vector3.MoveTowards(player.transform.position, endPoint, Time.deltaTime * speed);
                    ++i;
                }
                else
                {
                    playerCurrentDestiny[i]++;       

                    if(playerCurrentDestiny[i] == waypoints.Length)
                    {
                        player.ExitedUSB();

                        if(!multiPlayerTransport)
                            player.transform.position = destiny.position;
                        else
                        {
                            Vector3 exitPos = destiny.TransformPoint(player.Id == 1 ? -multiPlayerTransportOffset : multiPlayerTransportOffset, 0f, 0f);
                            player.transform.position = exitPos;
                        }

                        players.RemoveAt(i);
                        playerCurrentDestiny.RemoveAt(i);
                        //playerCurrentDestiny[i] = 1;
                        //player.transform.position = waypoints[0].position;

                    }
                    else
                    {
                        ++i;
                    }
                }
            }
        }
        else
        {
            audioSource.Stop();
        }
    }
}
