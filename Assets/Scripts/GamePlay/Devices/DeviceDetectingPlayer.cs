using UnityEngine;
using System.Collections;

public class DeviceDetectingPlayer : MonoBehaviour {

    public DeviceController device;

    private PlayerController p1;
    private PlayerController p2;

    void Awake()
    {
        p1 = null;
        p2 = null;
    }

    void OnDisable()
    {
        if (p1 != null)
            p1.SetDevice(null);

        if (p2 != null)
            p2.SetDevice(null);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetDevice(device);

            if (player.Id == 1)
                p1 = player;
            else
                p2 = player;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetDevice(null);

            if (player.Id == 1)
                p1 = null;
            else
                p2 = null;
        }
    }
}
