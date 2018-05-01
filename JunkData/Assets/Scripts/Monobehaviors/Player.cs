using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Movable
{
    protected override void Update()
    {
        // DEBUG:
        int debug = (int)Input.GetAxisRaw("Vertical");
        if (debug != 0)
            prediction.DebugData();

        base.Update();
    }
}
