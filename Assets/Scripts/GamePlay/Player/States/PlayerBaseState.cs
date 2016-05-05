using UnityEngine;
using System.Collections;

public class PlayerBaseState
{   
    protected PlayerController player;

	public virtual void Init(PlayerController controller)
    {
        player = controller;
    }

    public virtual void OnStateEnter() { }

    public virtual void OnStateExit() { }


    public virtual PlayerBaseState Update()
    {
        /*actions check list:
        is he grounded?
        is he in a border?
        can he do a special?
        can he do a dash?
        can he turn?
        can he shoot?
        can he move?
        */

        return null;
    }
}
