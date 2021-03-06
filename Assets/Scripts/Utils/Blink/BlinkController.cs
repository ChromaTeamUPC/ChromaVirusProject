﻿using UnityEngine;
using System.Collections;

public class BlinkController : MonoBehaviour {

    private struct RendMaterials
    {
        public Renderer renderer;
        public Material[] sharedMaterialsCopy;
    }

    [Header("Blink Once")]
    [SerializeField]
    private const float blinkOnceDefaultDuration = 0.045f;

    [Header("Blink Multiple Times")]
    [SerializeField]
    private const float totalMultipleDefaultDuration = 1f;
    [SerializeField]
    private const float blinkMultipleIntervalDefaultDuration = 0.1f;
    [SerializeField]
    private const float normalMultipleIntervalDefaultDuration = 0.1f;

    [Header("Blink Incremental")]
    [SerializeField]
    private const float totalIncrementalDefaultDuration = 1f;
    [SerializeField]
    private const float blinkIncrementalIntervalDefaultDuration = 0.01f;
    [SerializeField]
    private const float normalIncrementalIntervalDefaultDuration = 0.1f;
    [SerializeField]
    private const float normalIntervalDefaultReductionRatio = 0.5f;

    private RendMaterials[] rendererMaterials;

    //private Material white;
    //private Material transparent;
    private bool materialsNeedUpdate;

    private bool blinking;
    private Material currentBlinkMaterial;

    void Awake()
    {
        FindRenderers();
    }

    void Start()
    {
        //white = rsc.coloredObjectsMng.WhiteMaterial;
        //transparent = rsc.coloredObjectsMng.TransparentMaterial;

        materialsNeedUpdate = true;

        //FindRenderers();        

        blinking = false;     
    }

    private void FindRenderers()
    {
        GameObject model = transform.FindDeepChild("Model").gameObject;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            rendererMaterials = new RendMaterials[renderers.Length];
            for (int i = 0; i < renderers.Length; ++i)
            {
                rendererMaterials[i] = new RendMaterials();
                rendererMaterials[i].renderer = renderers[i];
                rendererMaterials[i].sharedMaterialsCopy = rendererMaterials[i].renderer.sharedMaterials;
            }
        }
    }

    void OnDisable()
    {
        StopPreviousBlinkings();
    }

    public void InvalidateMaterials()
    {
        if(blinking)
        {
            //If blink is in process update inmediately only changed material and restore blinking one
            for (int i = 0; i < rendererMaterials.Length; ++i)
            {
                RendMaterials rendMaterials = rendererMaterials[i];

                Material[] sharedMaterials = rendMaterials.renderer.sharedMaterials; //This creates a copy of the array, not the materials

                bool modified = false;

                for (int j = 0; j < sharedMaterials.Length; ++j)
                {
                    if( (sharedMaterials[j] != currentBlinkMaterial) &&
                        (sharedMaterials[j] != rendMaterials.sharedMaterialsCopy[j]))
                    {
                        rendMaterials.sharedMaterialsCopy[j] = sharedMaterials[j];
                        sharedMaterials[j] = currentBlinkMaterial;
                        modified = true;
                    }
                }

                if (modified)
                    rendMaterials.renderer.sharedMaterials = sharedMaterials;
            }
        }
        else
            materialsNeedUpdate = true;
    }

    private void SaveMaterials()
    {
        for (int i = 0; i < rendererMaterials.Length; ++i)
        {
            rendererMaterials[i].sharedMaterialsCopy = rendererMaterials[i].renderer.sharedMaterials;
        }

        materialsNeedUpdate = false;
    }

    private void SubstituteMaterials()
    {
        for (int i = 0; i < rendererMaterials.Length; ++i)
        {
            if (rendererMaterials[i].sharedMaterialsCopy.Length == 1)
            {
                rendererMaterials[i].renderer.sharedMaterial = currentBlinkMaterial;
            }
            else
            {
                Material[] mats = rendererMaterials[i].renderer.sharedMaterials;
                for (int j = 0; j < mats.Length; ++j)
                    mats[j] = currentBlinkMaterial;
                rendererMaterials[i].renderer.sharedMaterials = mats;
            }
        }
    }

    private void RestoreMaterials()
    {
        for (int i = 0; i < rendererMaterials.Length; ++i)
        {
            if (rendererMaterials[i].sharedMaterialsCopy.Length == 1)
            {
                rendererMaterials[i].renderer.sharedMaterial = rendererMaterials[i].sharedMaterialsCopy[0];
            }
            else
            {
                rendererMaterials[i].renderer.sharedMaterials = rendererMaterials[i].sharedMaterialsCopy;
            }
        }
    }

    public void StopPreviousBlinkings()
    {
        StopAllCoroutines();

        if (blinking)
        {
            RestoreMaterials();
            blinking = false;
        }
    }

    public void BlinkWhiteOnce(float duration = blinkOnceDefaultDuration)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkOnce(rsc.coloredObjectsMng.WhiteMaterial, duration));
    }

    public void BlinkTransparentOnce(float duration = blinkOnceDefaultDuration)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkOnce(rsc.coloredObjectsMng.TransparentMaterial, duration));
    }

    public void BlinkCustomOnce(Material mat, float duration = blinkOnceDefaultDuration)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkOnce(mat, duration));
    }

    private IEnumerator DoBlinkOnce(Material mat, float duration)
    {
        currentBlinkMaterial = mat;

        if (materialsNeedUpdate) SaveMaterials();

        blinking = true;

        SubstituteMaterials();

        yield return new WaitForSeconds(duration);

        RestoreMaterials();
        blinking = false;
    }

    public void BlinkWhiteMultipleTimes(float totalDuration = totalMultipleDefaultDuration,
                                        float blinkInterval = blinkMultipleIntervalDefaultDuration,
                                        float normalInterval = normalMultipleIntervalDefaultDuration)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkMultiple(rsc.coloredObjectsMng.WhiteMaterial, totalDuration, blinkInterval, normalInterval));
    }

    public void BlinkTransparentMultipleTimes(float totalDuration = totalMultipleDefaultDuration,
                                              float blinkInterval = blinkMultipleIntervalDefaultDuration,
                                              float normalInterval = normalMultipleIntervalDefaultDuration)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkMultiple(rsc.coloredObjectsMng.TransparentMaterial, totalDuration, blinkInterval, normalInterval));
    }

    public void BlinkCustomMultipleTimes(Material mat,
                                         float totalDuration = totalMultipleDefaultDuration,
                                         float blinkInterval = blinkMultipleIntervalDefaultDuration,
                                         float normalInterval = normalMultipleIntervalDefaultDuration)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkMultiple(mat, totalDuration, blinkInterval, normalInterval));
    }

    private IEnumerator DoBlinkMultiple(Material mat, float totalDuration, float blinkInterval, float normalInterval)
    {
        currentBlinkMaterial = mat;

        if (materialsNeedUpdate) SaveMaterials();

        blinking = true;

        float elapsedTime = 0f;
        bool blink = true;

        while (elapsedTime < totalDuration)
        {
            if(blink)
            {
                SubstituteMaterials();
                yield return new WaitForSeconds(blinkInterval);
                elapsedTime += blinkInterval;
            }
            else
            {
                RestoreMaterials();
                yield return new WaitForSeconds(normalInterval);
                elapsedTime += normalInterval;
            }

            blink = !blink;
        }

        RestoreMaterials();

        blinking = false;
    }


    public void BlinkWhiteNoStop(float blinkInterval = blinkMultipleIntervalDefaultDuration,
                                 float normalInterval = normalMultipleIntervalDefaultDuration)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkNoStop(rsc.coloredObjectsMng.WhiteMaterial, blinkInterval, normalInterval));
    }

    public void BlinkTransparentNoStop(float blinkInterval = blinkMultipleIntervalDefaultDuration,
                                       float normalInterval = normalMultipleIntervalDefaultDuration)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkNoStop(rsc.coloredObjectsMng.TransparentMaterial, blinkInterval, normalInterval));
    }

    public void BlinkCustomNoStop(Material mat,
                                  float blinkInterval = blinkMultipleIntervalDefaultDuration,
                                  float normalInterval = normalMultipleIntervalDefaultDuration)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkNoStop(mat, blinkInterval, normalInterval));
    }

    private IEnumerator DoBlinkNoStop(Material mat, float blinkInterval, float normalInterval)
    {
        currentBlinkMaterial = mat;

        if (materialsNeedUpdate) SaveMaterials();

        blinking = true;

        bool blink = true;

        while (true)
        {
            if (blink)
            {
                SubstituteMaterials();
                yield return new WaitForSeconds(blinkInterval);
            }
            else
            {
                RestoreMaterials();
                yield return new WaitForSeconds(normalInterval);
            }

            blink = !blink;
        }
    }


    public void BlinkWhiteIncremental(float totalDuration = totalIncrementalDefaultDuration,
                                      float blinkInterval = blinkIncrementalIntervalDefaultDuration,
                                      float initialNormalInterval = normalIncrementalIntervalDefaultDuration,
                                      float normalIntervalReductionRatio = normalIntervalDefaultReductionRatio)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkIncremental(rsc.coloredObjectsMng.WhiteMaterial, totalDuration, blinkInterval, initialNormalInterval, normalIntervalReductionRatio));
    }

    public void BlinkTransparentIncremental(float totalDuration = totalIncrementalDefaultDuration,
                                            float blinkInterval = blinkIncrementalIntervalDefaultDuration,
                                            float initialNormalInterval = normalIncrementalIntervalDefaultDuration,
                                            float normalIntervalReductionRatio = normalIntervalDefaultReductionRatio)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkIncremental(rsc.coloredObjectsMng.TransparentMaterial, totalDuration, blinkInterval, initialNormalInterval, normalIntervalReductionRatio));
    }

    public void BlinkCustomIncremental(Material mat, 
                                       float totalDuration = totalIncrementalDefaultDuration,
                                       float blinkInterval = blinkIncrementalIntervalDefaultDuration,
                                       float initialNormalInterval = normalIncrementalIntervalDefaultDuration,
                                       float normalIntervalReductionRatio = normalIntervalDefaultReductionRatio)
    {
        StopPreviousBlinkings();
        StartCoroutine(DoBlinkIncremental(mat, totalDuration, blinkInterval, initialNormalInterval, normalIntervalReductionRatio));
    }

    private IEnumerator DoBlinkIncremental(Material mat, float totalDuration, float blinkInterval, float initialNormalInterval, float normalIntervalReductionRatio)
    {
        currentBlinkMaterial = mat;

        if (materialsNeedUpdate) SaveMaterials();

        blinking = true;

        float elapsedTime = 0f;
        bool blink = true;
        float currentNormalInterval = initialNormalInterval;
        float halfTime = totalDuration / 2;

        while (elapsedTime < totalDuration)
        {
            if (blink)
            {
                SubstituteMaterials();
                yield return new WaitForSeconds(blinkInterval);
                elapsedTime += blinkInterval;
            }
            else
            {
                RestoreMaterials();
                yield return new WaitForSeconds(currentNormalInterval);
                elapsedTime += currentNormalInterval;
            }

            if(elapsedTime > halfTime)
            {
                currentNormalInterval *= normalIntervalReductionRatio;
                halfTime += ((totalDuration - halfTime) / 2);
            }

            blink = !blink;
        }

        RestoreMaterials();

        blinking = false;
    }
}
