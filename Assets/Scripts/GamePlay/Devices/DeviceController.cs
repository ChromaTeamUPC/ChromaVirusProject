using UnityEngine;
using System.Collections;

public class DeviceController : MonoBehaviour
{
    public enum Type
    {
        VIDEO,
        AUDIO,
        INPUT
    }

    public enum InfectionLevel
    {
        LEVEL0,
        LEVEL1,
        LEVEL2,
        LEVEL3
    }

    private bool active;
    private float currentInfection = 0f;
    private InfectionLevel infectionLevel;

    public Type type;
    public float level0To1Threshold = 33;
    public float level1To2Threshold = 66;
    public float level2To3Threshold = 33;

    public float CurrentInfectionAmount { get { return currentInfection; } }
    public InfectionLevel CurrentInfectionLevel { get { return infectionLevel; } }

    void Awake()
    {
        active = false;
        currentInfection = 0f;
        infectionLevel = InfectionLevel.LEVEL0;
    }

    public void Activate()
    {
        active = true;
        DeviceEventInfo.eventInfo.device = this;
        rsc.eventMng.TriggerEvent(EventManager.EventType.DEVICE_ACTIVATED, DeviceEventInfo.eventInfo);
    }

    public void Deactivate()
    {
        DeviceEventInfo.eventInfo.device = this;
        rsc.eventMng.TriggerEvent(EventManager.EventType.DEVICE_DEACTIVATED, DeviceEventInfo.eventInfo);
        active = false;
    }

    private void NotifyInfectionChange()
    {
        DeviceEventInfo.eventInfo.device = this;
        rsc.eventMng.TriggerEvent(EventManager.EventType.DEVICE_INFECTION_LEVEL_CHANGED, DeviceEventInfo.eventInfo);
    }

    public void Infect(float amount)
    {
        if (!active) return;

        currentInfection += amount;
        if (currentInfection > 100f)
            currentInfection = 100f;
    }

    public void Disinfect(float amount)
    {
        if (!active) return;

        currentInfection -= amount;
        if (currentInfection < 0f)
            currentInfection = 0f;
    }

    void Update()
    {
        if (!active) return;

        switch (infectionLevel)
        {
            case InfectionLevel.LEVEL0:
                if(currentInfection >= level2To3Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL3;
                    NotifyInfectionChange();
                }
                else if (currentInfection >= level1To2Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL2;
                    NotifyInfectionChange();
                }
                else if (currentInfection >= level0To1Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL1;
                    NotifyInfectionChange();
                }
                break;

            case InfectionLevel.LEVEL1:
                if (currentInfection >= level2To3Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL3;
                    NotifyInfectionChange();
                }
                else if (currentInfection >= level1To2Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL2;
                    NotifyInfectionChange();
                }
                else if (currentInfection < level0To1Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL0;
                    NotifyInfectionChange();
                }
                break;

            case InfectionLevel.LEVEL2:
                if (currentInfection >= level2To3Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL3;
                    NotifyInfectionChange();
                }
                else if (currentInfection < level0To1Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL0;
                    NotifyInfectionChange();
                }
                else if (currentInfection < level1To2Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL1;
                    NotifyInfectionChange();
                }
                break;

            case InfectionLevel.LEVEL3:
                if (currentInfection < level0To1Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL0;
                    NotifyInfectionChange();
                }
                else if (currentInfection < level1To2Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL1;
                    NotifyInfectionChange();
                }
                else if (currentInfection < level2To3Threshold)
                {
                    infectionLevel = InfectionLevel.LEVEL2;
                    NotifyInfectionChange();
                }
                break;
            default:
                break;
        }
    }
}
