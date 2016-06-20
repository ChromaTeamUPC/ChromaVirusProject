using UnityEngine;
using System.Collections;

public class DeviceDetectingPlayer : MonoBehaviour {

    public DeviceController device;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetDevice(device);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetDevice(null);
        }
    }
}
