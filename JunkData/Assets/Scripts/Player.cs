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
    public bool grounded;

    private Vector3 mouseDir;
    private int horizontal;
    private float mouseAngle;
    private bool grasping;
    private bool leftClickUp;
    private Rigidbody2D rb2d;
    private PredictionTool prediction;
    
    private Vector2 NUDGE_LEFT = new Vector2(-0.05f, 0);
    private Vector2 NUDGE_RIGHT = new Vector2(0.05f, 0);
    private const int LEFT_CLICK = 0;
    private const float DETECTION_RANGE = 0.03f;
    private const float STOP_MOVEMENT = 45f;
    private const float RIGHT_ANGLE = 90f;
    private const float WALLDETECT_ANGLE_RESTRICTION = 25f;     // Degrees from 90 at which we're seeing a wall.
    private const float WALLSTICK_ANGLE_RESTRICTION = 8f;
    private const float CLIMB_CORRECTION = 0.5f;

    private void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        prediction = gameObject.GetComponent<PredictionTool>();
    }

    private void Update()
    {
        // Check for walk input.
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        if (horizontal == -1 && prediction.leftLocked || horizontal == 1 && prediction.rightLocked)
            horizontal = 0;

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

        // Move.
        if (leftClickUp && rb2d.velocity.magnitude < float.Epsilon)
        {
            rb2d.AddForce(mouseDir * jumpForce, ForceMode2D.Impulse);
            leftClickUp = false;
        }
        else if (Mathf.Abs(horizontal) > 0 && rb2d.velocity.magnitude < walkVelocity)
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
            float ungravity = moveDir.y * gravityBalance + CLIMB_CORRECTION;
            moveDir.y = ungravity;

            // Normalize right and left movement.
            moveDir.x = horizontal;

            // Move, using the given prediction.
            if (correctionAngle <= lowRestriction || correctionAngle >= highRestriction)
                rb2d.AddForce(moveDir * walkAccel);
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        // Collect contact direction angle.
        ContactPoint2D[] contact = new ContactPoint2D[1];
        other.GetContacts(contact);
        Vector2 collisionDir = (contact[0].point - (Vector2)rb2d.transform.position);
        float collisionAngle = Vector2.Angle(Vector2.right, collisionDir);

        // Check if colliding with a wall.
        if (collisionAngle < WALLSTICK_ANGLE_RESTRICTION)
            rb2d.AddForce(NUDGE_LEFT);
        else if (collisionAngle > 180f - WALLSTICK_ANGLE_RESTRICTION)
            rb2d.AddForce(NUDGE_RIGHT);
        else
            // Otherwise, we're grounded safely.
            grounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        grounded = false;
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
