using UnityEngine;
using System.Collections;

public class ElevatorController : MonoBehaviour 
{
    public GameObject colliders;

    void Start()
    {
        colliders.SetActive(true);
    }

    public void Deactivate()
    {
        colliders.SetActive(false);
    }
}
