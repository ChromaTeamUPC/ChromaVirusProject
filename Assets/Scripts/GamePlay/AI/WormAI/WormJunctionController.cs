using UnityEngine;
using System.Collections;

public class WormJunctionController : MonoBehaviour 
{
    private Renderer rend;
    private Material wireframeMat;

	// Use this for initialization
	void Awake () 
	{
        rend = GetComponentInChildren<Renderer>();
    }

    void Start()
    {
        wireframeMat = rsc.coloredObjectsMng.GetWormJunctionWireframeMaterial();
    }
	
	public void SetWireframe()
    {
        if(rend.sharedMaterial != wireframeMat)
            rend.sharedMaterial = wireframeMat;
    }
}
