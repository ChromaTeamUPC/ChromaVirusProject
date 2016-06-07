using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class USBTeleportController : MonoBehaviour {

    public Transform destiny;
    public Transform[] waypoints;
    public float speed = 8f;

    private PlayerController player;
    //public ParticleSystem electricPS;

    private List<PlayerController> players = new List<PlayerController>();
    private List<int> playerCurrentDestiny = new List<int>();
    private List<float> playerCurrentLerpTime = new List<float>();

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            //other.transform.position = destiny.position;
            PlayerController player = other.GetComponent<PlayerController>();
            player.transform.position = waypoints[0].position;
            player.EnteredUSB();

            players.Add(player);
            playerCurrentDestiny.Add(1);
            playerCurrentLerpTime.Add(0f);
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
                        player.transform.position = destiny.position;

                        players.RemoveAt(i);
                        playerCurrentDestiny.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
        }
    }
}
