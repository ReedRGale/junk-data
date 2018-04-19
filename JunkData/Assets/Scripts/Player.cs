using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float walkVelocity;
    public float walkAccel;
    public float heelTurn;
    public float jumpForce;
    public float gravityBalance;
    public GameObject marker;

    private Vector3 mouseDir;
    private int horizontal;
    private float mouseAngle;
    private bool grounded;
    private bool grasping;
    private bool leftClickUp;
    private bool stopRight;
    private bool stopLeft;
    private Rigidbody2D rb2d;
    
    private Vector2 NUDGE_LEFT = new Vector2(-0.05f, 0);
    private Vector2 NUDGE_RIGHT = new Vector2(0.05f, 0);
    private const int LEFT_CLICK = 0;
    private const float DETECTION_RANGE = 0.05f;
    private const float STOP_MOVEMENT = 45f;
    private const float RIGHT_ANGLE = 90f;
    private const float WALLDETECT_ANGLE_RESTRICTION = 25f;
    private const float WALLSTICK_ANGLE_RESTRICTION = 8f;
    private const float FLOORSTICK_ANGLE_RESTRICTION = 60f;

    private void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Check for walk input.
        horizontal = (int)Input.GetAxisRaw("Horizontal");

        // Check for jump input.
        leftClickUp = !leftClickUp && grounded ? Input.GetMouseButtonUp(LEFT_CLICK) : leftClickUp;

        // If mouseup, record the mouse's position.
        if (leftClickUp)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 5.0f;
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            mouseDir = (mousePosition - gameObject.transform.position).normalized;
        }
    }

    void FixedUpdate ()
    {
        // Don't allow movement input if not grounded.
        if (!grounded) return;

        // We're moving, so gravity must apply.
        rb2d.gravityScale = 1.0f;

        // Move.
        if (leftClickUp && rb2d.velocity.magnitude < float.Epsilon)
        {
            rb2d.AddForce(mouseDir * jumpForce, ForceMode2D.Impulse);
            leftClickUp = false;
        }
        else if (System.Math.Abs(horizontal) > 0 && rb2d.velocity.magnitude < walkVelocity)
        {
            if (rb2d.velocity.x * horizontal < 0) rb2d.AddForce(-rb2d.velocity * heelTurn);
            else
            {
                // Cast a prediction of the model in front of the movement.
                RaycastHit2D[] lowBarrier = new RaycastHit2D[1];
                rb2d.Cast(Vector2.right * horizontal, lowBarrier, DETECTION_RANGE);

                // Use the collider points from the cast model to determine force angle.
                Vector2 collisionDir = (lowBarrier[0].point - lowBarrier[0].centroid);
                collisionDir = Vector2.zero.Equals(collisionDir) ? Vector2.down : collisionDir; 
                Vector2 moveDir = Rotate(collisionDir, RIGHT_ANGLE * horizontal).normalized;

                // Restricted angles so we don't start moving up at walls.
                float highRestriction = WALLDETECT_ANGLE_RESTRICTION + RIGHT_ANGLE;
                float lowRestriction = RIGHT_ANGLE - WALLDETECT_ANGLE_RESTRICTION;
                float correctionAngle = Vector2.Angle(Vector2.right, moveDir);

                // Correct for gravity.
                float ungravity = moveDir.y * gravityBalance;
                moveDir.y = ungravity;

                // Move, using the given prediction.
                if (stopLeft)
                    rb2d.AddForce(NUDGE_RIGHT);
                if (stopRight)
                    rb2d.AddForce(NUDGE_LEFT);
                if (correctionAngle <= lowRestriction || correctionAngle >= highRestriction)
                    rb2d.AddForce(moveDir * walkAccel);
            }
        }
        else rb2d.AddForce(-rb2d.velocity);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        // Collect contact direction angle.
        ContactPoint2D[] contact = new ContactPoint2D[1];
        other.GetContacts(contact);
        Vector2 collisionDir = (contact[0].point - (Vector2)rb2d.transform.position);
        float collisionAngle = Vector2.Angle(Vector2.right, collisionDir);

        if (collisionAngle < WALLSTICK_ANGLE_RESTRICTION)
            rb2d.AddForce(NUDGE_LEFT);
        else if (collisionAngle > 180f - WALLSTICK_ANGLE_RESTRICTION)
            rb2d.AddForce(NUDGE_RIGHT);
        else if (collisionAngle < STOP_MOVEMENT)
        {
            Debug.Log("stopLeft");
            stopLeft = true;
        }
        else if (collisionAngle > 180f - STOP_MOVEMENT)
        {
            Debug.Log("stopRight");
            stopRight = true;
        }
        else if (collisionAngle > FLOORSTICK_ANGLE_RESTRICTION || collisionAngle < 180f - FLOORSTICK_ANGLE_RESTRICTION)
        {
            stopLeft = false;
            stopRight = true;
            grounded = true;
        }
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}
