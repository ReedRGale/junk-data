using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Movable
{
    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown("down"))
        {
            Debug.Log("DEBUG COMMAND RUN");
            Debug.Log("Estimated Gravity:  " + Physics2D.gravity);
            Debug.Log("Estimated Gravity Force:  " + GetRB2D().mass * Physics2D.gravity);
        }
    }
}
