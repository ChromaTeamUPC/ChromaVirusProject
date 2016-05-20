using UnityEngine;
using System.Collections;

public class BlinkController : MonoBehaviour {

    private struct RendMaterials
    {
        public Renderer renderer;
        public Material[] materials;
    }

    [SerializeField]
    private float blinkSeconds = 0.01f;

    private RendMaterials[] rendererMaterials;

    private Material white;
    private bool materialsNeedUpdate;

    void Start()
    {
        white = rsc.coloredObjectsMng.WhiteMaterial;

        materialsNeedUpdate = false;

        GameObject model = transform.Find("Model").gameObject;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            rendererMaterials = new RendMaterials[renderers.Length];
            for(int i = 0; i < renderers.Length; ++i)
            {
                rendererMaterials[i] = new RendMaterials();
                rendererMaterials[i].renderer = renderers[i];
                rendererMaterials[i].materials = rendererMaterials[i].renderer.materials;
            }
        }       
    }

    public void InvalidateMaterials()
    {
        materialsNeedUpdate = true;
    }

    private void UpdateMaterials()
    {
        for (int i = 0; i < rendererMaterials.Length; ++i)
        {
            rendererMaterials[i].materials = rendererMaterials[i].renderer.materials;
        }

        materialsNeedUpdate = false;
    }

    public void Blink()
    {
        if (materialsNeedUpdate) UpdateMaterials();
        StartCoroutine(DoBlink());
    }

    private IEnumerator DoBlink()
    {
        for (int i = 0; i < rendererMaterials.Length; ++i)
        {
            if (rendererMaterials[i].materials.Length == 1)
                rendererMaterials[i].renderer.material = white;
            else
            {
                Material[] mats = rendererMaterials[i].renderer.materials;
                for (int j = 0; j < mats.Length; ++j)
                    mats[j] = white;
                rendererMaterials[i].renderer.materials = mats;
            }
        }

        yield return new WaitForSeconds(blinkSeconds);

        for (int i = 0; i < rendererMaterials.Length; ++i)
        {
            if (rendererMaterials[i].materials.Length == 1)
                rendererMaterials[i].renderer.material = rendererMaterials[i].materials[0];
            else
            {
                Material[] mats = rendererMaterials[i].renderer.materials;
                for (int j = 0; j < mats.Length; ++j)
                    mats[j] = rendererMaterials[i].materials[j];
                rendererMaterials[i].renderer.materials = mats;
            }
        }
    }
}
