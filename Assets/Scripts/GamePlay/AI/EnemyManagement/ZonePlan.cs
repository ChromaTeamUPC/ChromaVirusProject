using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZonePlan {

    public int enemiesThreshold;
    public List<WaveAction> initialActions;
    public List<List<WaveAction>> triggerWaves;
    public List<List<WaveAction>> sequentialWaves;

    public ZonePlan()
    {
        enemiesThreshold = 3;
        initialActions = new List<WaveAction>();
        triggerWaves = new List<List<WaveAction>>();
        sequentialWaves = new List<List<WaveAction>>();
    }

    public int GetPlanTotalInfection()
    {
        int result = 0;

        foreach (WaveAction wave in initialActions)
            result += wave.GetWaveTotalInfection();

        foreach (List<WaveAction> waveList in triggerWaves)
            foreach (WaveAction wave in waveList)
                result += wave.GetWaveTotalInfection();

        foreach (List<WaveAction> waveList in sequentialWaves)
            foreach (WaveAction wave in waveList)
                result += wave.GetWaveTotalInfection();

        return result;
    }
}
