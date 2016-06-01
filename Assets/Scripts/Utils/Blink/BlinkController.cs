using UnityEngine;
using System.Collections;

public class BlinkController : MonoBehaviour {

    private struct RendMaterials
    {
        public Renderer renderer;
        public Material[] materials;
    }

    [SerializeField]
    private const float blinkDefaultSeconds = 0.01f;

    private RendMaterials[] rendererMaterials;

    private Material white;
    private Material transparent;
    private bool materialsNeedUpdate;

    private bool blinking;

    void Start()
    {
        white = rsc.coloredObjectsMng.WhiteMaterial;
        transparent = rsc.coloredObjectsMng.TransparentMaterial;

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

        blinking = false;     
    }

    public void InvalidateMaterials()
    {
        materialsNeedUpdate = true;
    }

    private void UpdateMaterials()
    {
        if (!blinking) //we can not update materials while blinking because we would store the wrong ones
        {
            for (int i = 0; i < rendererMaterials.Length; ++i)
            {
                rendererMaterials[i].materials = rendererMaterials[i].renderer.materials;
            }

            materialsNeedUpdate = false;
        }
    }

    public void BlinkWhiteOnce(float duration = blinkDefaultSeconds)
    {
        if (materialsNeedUpdate) UpdateMaterials();
        StartCoroutine(DoBlinkOnce(white, duration));
    }

    public void BlinkTransparentOnce(float duration = blinkDefaultSeconds)
    {
        if (materialsNeedUpdate) UpdateMaterials();
        StartCoroutine(DoBlinkOnce(transparent, duration));
    }

    private IEnumerator DoBlinkOnce(Material mat, float duration)
    {
        blinking = true;
        for (int i = 0; i < rendererMaterials.Length; ++i)
        {
            if (rendererMaterials[i].materials.Length == 1)
                rendererMaterials[i].renderer.material = mat;
            else
            {
                Material[] mats = rendererMaterials[i].renderer.materials;
                for (int j = 0; j < mats.Length; ++j)
                    mats[j] = mat;
                rendererMaterials[i].renderer.materials = mats;
            }
        }

        yield return new WaitForSeconds(duration);

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
        blinking = false;
    }
}
