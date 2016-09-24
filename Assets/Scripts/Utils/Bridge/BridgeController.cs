﻿using UnityEngine;
using System.Collections;

public class BridgeController : MonoBehaviour
{
    public GameObject model;
    private Renderer rend;

    public GameObject[] fragments;
    private BridgeFragmentController[] fragmentControllers;

    private ChromaColor currentColor;
    private bool active;

    void Awake()
    {
        rend = model.GetComponentInChildren<Renderer>();
        active = false;

        fragmentControllers = new BridgeFragmentController[fragments.Length];
        for(int i = 0; i < fragments.Length; ++i)
        {
            fragmentControllers[i] = fragments[i].GetComponent<BridgeFragmentController>();
        }
    }

	// Use this for initialization
	void Start ()
    {
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
	}

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        }
    }

    void ColorChanged(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;
        currentColor = info.newColor;

        SetMaterial();
    }

    private void SetMaterial()
    {
        rend.sharedMaterial = rsc.coloredObjectsMng.GetBridgeMaterial(currentColor);       
    }

    public void Activate()
    {
        if (!active)
        {
            active = true;
            StartCoroutine(EnterFragments());
        } 
    }

    private IEnumerator EnterFragments()
    {
        for(int i = 0; i < fragments.Length; ++i)
        {
            fragments[i].SetActive(true);
            fragmentControllers[i].Enter();
            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(0.43f);

        model.SetActive(true);
        for (int i = 0; i < fragments.Length; ++i)
            fragments[i].SetActive(false);
    }

    public void Deactivate()
    {
        if (active)
        {
            active = false;
            StartCoroutine(ExitFragments());          
        }
           
    }

    private IEnumerator ExitFragments()
    {
        for (int i = 0; i < fragments.Length; ++i)
        {
            fragments[i].SetActive(true);
            fragmentControllers[i].Wait();
        }

        model.SetActive(false);

        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < fragments.Length; ++i)
        {
            fragmentControllers[i].Exit();
            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(0.43f);

        for (int i = 0; i < fragments.Length; ++i)
            fragments[i].SetActive(false);
    }
}
