using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float walkVelocity;
    public float walkAccel;
    public float heelTurn;
    
    private Rigidbody2D rb2d;

    private void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
    }
    
    void FixedUpdate ()
    {
        if (Grounded() || rb2d.velocity.magnitude > walkVelocity) return;

        // Check for input.
        int horizontal = 0;
        horizontal = (int)Input.GetAxisRaw("Horizontal");

        // Move.
        if (System.Math.Abs(horizontal) > 0)
            if (rb2d.velocity.x * horizontal < 0)
                rb2d.AddForce(-rb2d.velocity * heelTurn);
            else
                rb2d.AddForce(Vector2.right * horizontal * walkAccel);
        else
            rb2d.AddForce(-rb2d.velocity);
    }

    // Check if grounded.
    private bool Grounded() { return System.Math.Abs(rb2d.velocity.y) > float.Epsilon; }
}
