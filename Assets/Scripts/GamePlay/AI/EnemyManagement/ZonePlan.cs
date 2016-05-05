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
}
