using UnityEngine;
using System.Collections;

public class BridgeController : MonoBehaviour
{
    public GameObject model;
    public Material modelMat;

    public GameObject[] fragments;
    private BridgeFragmentController[] fragmentControllers;

    private bool active;

    void Awake()
    {
        active = false;

        fragmentControllers = new BridgeFragmentController[fragments.Length];
        for(int i = 0; i < fragments.Length; ++i)
        {
            fragmentControllers[i] = fragments[i].GetComponent<BridgeFragmentController>();
        }
    }

    public void Activate()
    {
        if (!active)
        {
            modelMat.mainTextureOffset = new Vector2(0.0f, 0.0f);
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
