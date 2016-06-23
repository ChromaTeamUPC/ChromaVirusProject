using UnityEngine;
using System.Collections;

public class WaveAction
{
    protected float initialDelay = 0f;
    protected bool executing = false;

    public float InitialDelay { get { return initialDelay; } }

    public bool Executing { get { return executing; } }

    public virtual void Execute()
    {
        //Do nothing by default
    }

    public virtual int GetWaveTotalInfection() { return 0; }
}
