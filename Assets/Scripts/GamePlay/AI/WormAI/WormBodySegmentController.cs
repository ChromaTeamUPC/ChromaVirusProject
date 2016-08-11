﻿using UnityEngine;
using System.Collections;

public class WormBodySegmentController : MonoBehaviour 
{
    private enum State
    {
        SETTING,
        NORMAL,
        NORMAL_DISABLED,
        DEACTIVATED,
        DESTROYED
    }

    private State state;
    private float currentHealth;
    private float currentHealthWrongColor;

    [SerializeField]
    private ChromaColor color;

    private BlinkController blinkController;
    private Renderer rend;
    private BoxCollider col;
    private VoxelizationClient voxelization;

    private WormBlackboard bb;
    private WormAIBehaviour worm; //Shortcut

    void Awake()
    {
        blinkController = GetComponent<BlinkController>();
        rend = GetComponentInChildren<Renderer>();
        col = GetComponent<BoxCollider>();
        voxelization = GetComponentInChildren<VoxelizationClient>();
        state = State.NORMAL;
    }

    void Start()
    {
        SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyMaterial(color) });
    }

    public void SetBlackboard(WormBlackboard bb)
    {
        this.bb = bb;
        worm = bb.worm;
        currentHealth = bb.bodyMaxHealth;
        currentHealthWrongColor = bb.bodyMaxHealth;
    }

    public void SetInitialState()
    {
        currentHealth = bb.bodyMaxHealth;
        currentHealthWrongColor = bb.bodyMaxHealth;
        SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyMaterial(color) });

        ColorEventInfo.eventInfo.newColor = color;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_ACTIVATED, ColorEventInfo.eventInfo);

        state = State.NORMAL;
    }

    public void ResetColor(ChromaColor color)
    {
        if (state == State.DESTROYED) return;

        ChromaColor oldColor = this.color;
        if(state == State.DEACTIVATED)
        {
            ColorEventInfo.eventInfo.newColor = color;
            rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_ACTIVATED, ColorEventInfo.eventInfo);
        }
        else
        {
            ColorEventInfo.eventInfo.oldColor = oldColor;
            ColorEventInfo.eventInfo.newColor = color;
            rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_COLOR_CHANGED, ColorEventInfo.eventInfo);
        }

        this.color = color;
        currentHealth = bb.bodyMaxHealth;
        currentHealthWrongColor = bb.bodyMaxHealth;
        state = State.SETTING;

        StartCoroutine(SetRandomColors());
    }

    public void Consolidate(ChromaColor color)
    {
        if (state == State.DESTROYED) return;

        if (state == State.DEACTIVATED)
        {
            SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyWireframeMaterial() });
            state = State.DESTROYED;
            col.enabled = false;
        }
        else
        {
            ChromaColor oldColor = this.color;
            this.color = color;
            currentHealth = bb.bodyMaxHealth;
            currentHealthWrongColor = bb.bodyMaxHealth;

            ColorEventInfo.eventInfo.oldColor = oldColor;
            ColorEventInfo.eventInfo.newColor = color;
            rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_COLOR_CHANGED, ColorEventInfo.eventInfo);

            state = State.SETTING;
            StartCoroutine(SetRandomColors());
        }
    }

    private IEnumerator SetRandomColors()
    {
        float duration = Random.Range(bb.bodySettingMinTime, bb.bodySettingMaxTime);

        float elapsedTime = 0;

        while(elapsedTime < duration)
        {
            SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyMaterial(ChromaColorInfo.Random) });

            yield return new WaitForSeconds(bb.bodySettingChangeTime);
            elapsedTime += bb.bodySettingChangeTime;
        }

        SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyMaterial(color) });
        state = State.NORMAL;
    }

    public void Disable()
    {
        if (state != State.NORMAL) return;

        SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyDimMaterial(color) });
        state = State.NORMAL_DISABLED;
    }


    public void Explode()
    {
        StartCoroutine(RandomizeAndExplode());
    }

    private IEnumerator RandomizeAndExplode()
    {
        float elapsedTime = 0;

        while (elapsedTime < bb.bodySettingMinTime)
        {
            SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyMaterial(ChromaColorInfo.Random) });

            yield return new WaitForSeconds(bb.bodySettingChangeTime);
            elapsedTime += bb.bodySettingChangeTime;
        }

        voxelization.SpawnFakeVoxels();
        Destroy(gameObject);
    }

    public void ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        if (state != State.NORMAL) return;

        blinkController.BlinkWhiteOnce();

        if (shotColor != color)
        {
            currentHealthWrongColor -= damage * bb.bodyWrongColorDamageModifier;

            if(currentHealthWrongColor <= 0)
            {
                state = State.DEACTIVATED; //not really deactivated but flagged to allow notify properly when colors reset

                EnemyDiedEventInfo.eventInfo.color = color;
                EnemyDiedEventInfo.eventInfo.infectionValue = 0;
                EnemyDiedEventInfo.eventInfo.killerPlayer = player;
                EnemyDiedEventInfo.eventInfo.killedSameColor = (color == shotColor);
                rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_DESTROYED, EnemyDiedEventInfo.eventInfo);

                worm.DischargeHead();
            }
        }
        else
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                //Set material grey
                SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyGreyMaterial() });
                state = State.DEACTIVATED;

                //Explosion FX?

                EnemyDiedEventInfo.eventInfo.color = color;
                EnemyDiedEventInfo.eventInfo.infectionValue = 0;
                EnemyDiedEventInfo.eventInfo.killerPlayer = player;
                EnemyDiedEventInfo.eventInfo.killedSameColor = (color == shotColor);
                rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SECTION_DESTROYED, EnemyDiedEventInfo.eventInfo);

                worm.ChargeHead();
            }
        }
    }

    private void SetMaterial(Material[] materials)
    {
        Material[] mats = rend.sharedMaterials;

        if (mats[1] != materials[0])
        {
            mats[1] = materials[0];
            rend.sharedMaterials = mats;

            blinkController.InvalidateMaterials();
        }
    }

    public void SetVisible(bool visible)
    {
        rend.enabled = visible;
    }
}