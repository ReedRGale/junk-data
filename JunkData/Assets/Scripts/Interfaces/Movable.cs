using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Structs;

public class Movable : MonoBehaviour
{
    public bool debugValue = true;

    /* Delegates */


    // A delegate that sets a move category and tells us whether it was set or not.
    public delegate bool CategoryCallback(Movable m, out MoveCategory category);

    // A delegate that explains how to set off a move category.
    public delegate bool CategoryLogic(Movable m);

    // A delegate that defines concentration logic.
    public delegate bool ContemplationLogic(Movable m);


    /* Move Category Logic */


    // List of information containing what to do upon changing from one state to another.
    private List<CategoryCallback> categoryCallbacks;

    // Dictionary of information about when a movestate's conditions have been met.
    private Dictionary<MoveCategory, CategoryLogic> categoryLogic;

    // The current movement category.
    private MoveCategory category;

    // Logic to store the current and previous move category.
    private Queue<MoveCategory> moveCategoryQueue;

    // The current logic which we should follow when contemplating.
    private ContemplationLogic currentContemplation;

    // Denotes that a change in move logic is occurring mid-game.
    private bool logicChanged = false;

    private bool isLocked = false;
    private bool isContemplating = false;
    private bool isWarping = false;
    private bool isStunned = false;
    private bool isDeconstructed = false;
    private bool isFlying = false;
    private bool isRising = false;
    private bool isFalling = false;
    private bool isJumping = false;
    private bool isWalking = false;
    private bool isSliding = false;
    private bool isStatic = false;


    /* Unit Control */

    protected Rigidbody2D rb2d;
    protected Lookahead lookahead;

    // Recorded point of the mouse's position.
    protected Vector3 mousePosition;

    // Recorded point of the mouse's direction.
    protected Vector3 mouseDirection;


    /* State Variables */


    // The radius of this unit.
    protected float radius = 0;

    // The width of this unit.
    protected float width = 0;

    // Damage taken this turn.
    protected int damageTaken = 0;

    // Indicate that we're ready to warp.
    protected bool warpReady = false;

    // Indicate that we've jumped.
    protected bool jumped = false;

    // Return whether we're touching the danger zone.
    protected bool isTouchingDZ = false;

    // Is grounded.
    protected bool isGrounded = false;

    // The greatest angle between collisions experiencced and the down vector.
    protected float downToCollisionAngle = 0;

    // The position at the beginning of the FixedUpdate.
    protected Vector2 prevPosition = Vector2.zero;

    // External signaled this Movable to fly. Ammy named this.
    protected bool nyoom = false;

    // The force with which the unit propels itself when jumping.
    protected float jumpForce = 0.4f;

    // The force with which the unit propels itself when flying.
    protected float flightForce = 0.2f;

    // Max velocity the unit can speed up to.
    protected float walkVelocity = 1.25f;

    // Magnitude of force the unit experiences while moving.
    protected float walkAccel = 0.7f;

    // A value to, when walking over bumps, counteract gravity.
    protected float gravityBalance = 1.3f;


    // Move into constants object.
    private const int LEFT_CLICK = 0;                           // Int representing the value of a left click.
    private Vector2 NUDGE_LEFT = new Vector2(-0.05f, 0);
    private Vector2 NUDGE_RIGHT = new Vector2(0.05f, 0);
    private Vector2 MIN_CONTACT_RANGE = new Vector2(-0.9993908f, -0.0348995f);
    private Vector2 MAX_CONTACT_RANGE = new Vector2(0.9993908f, -0.0348995f);
    private const float DELTA_CONTACT_ANGLE = 0.5f;
    private const float RADIUS_CORRECTION = 0.01f;
    private const float DETECTION_RANGE = 0.03f;
    private const float STOP_MOVEMENT = 45f;
    private const float RIGHT_ANGLE = 90f;
    private const float WALLDETECT_ANGLE_RESTRICTION = 25f;     // Degrees from 90 at which we're seeing a wall.
    private const float WALLSTICK_ANGLE_RESTRICTION = 8f;
    private const float CLIMB_CORRECTION = 0.5f;
    private const float LESS_THAN_HALF = 0.49f;


        /* Monodevelop Callbacks */

        
    protected virtual void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        lookahead = gameObject.GetComponent<Lookahead>();
        radius = gameObject.GetComponent<Collider2D>().bounds.extents.x;
        width = radius * 2;

        // Prepare category information.
        categoryCallbacks = CategoryManager.instance.BundleCallbacks();
        categoryLogic = CategoryManager.instance.BundleLogic();

        // Set up category queue.
        moveCategoryQueue = new Queue<MoveCategory>();
        ResetMoveQueue();
    }
    
    protected virtual void Update()
    {
        // If true, move logic is changing in the middle of the game.
        if (logicChanged)
        {
            categoryLogic = CategoryManager.instance.BundleLogic();
            logicChanged = false;
        }

        if (!IsLocked())
        {
            // Check unit input.
            WarpInput();
            FlyingInput();
            JumpInput();
            WalkInput();

            // Check unit state.
            CheckSliding();
            CheckStatic();
            CheckDeconstructed();
            CheckStunned();
            CheckRising();
            CheckFalling();
            CheckSliding();
        }

        // Determine and initialize the current move category.
        foreach (CategoryCallback C in categoryCallbacks)
            if (C(this, out category)) break;

        if (GetMoveCategory() == MoveCategory.JUMPING)
        {
            Debug.Log("Prev Move Category:  " + GetPrevMoveCategory() 
                + "\nisLocked:  " + isLocked 
                + "\nisContemplating:  " + isContemplating 
                + "\nisWarping:  " + isWarping
                + "\nisStunned:  " + isStunned
                + "\nisDeconstructed:  " + isDeconstructed
                + "\nisFlying:   " + isFlying 
                + "\nisRising:  " + isRising
                + "\nisFalling:  " + isFalling
                + "\nisJumping:  " + isJumping
                + "\nisWalking:  " + isWalking
                + "\nisSliding:  " + isSliding
                + "\nisStatic:  " + isStatic);
        }

            // Record move category data for next loop.
            RecordMoveCategory();
    }

    protected virtual void FixedUpdate()
    {
        // Collect previous position for walking calculations.
        prevPosition = rb2d.position;

        switch (category)
        {
            case MoveCategory.CONTEMPLATING:
                Contemplate();
                break;
            case MoveCategory.WARPING:
                Warp();
                break;
            case MoveCategory.DECONSTRUCTED:
                Deconstruct();
                break;
            case MoveCategory.FLYING:
                Fly();
                break;
            case MoveCategory.FALLING:          // We check this case, because falling off edges, we still need to lock.
                if (GetPrevMoveCategory() == MoveCategory.WALKING) Walk();
                break;
            case MoveCategory.JUMPING:
                Jump();
                break;
            case MoveCategory.WALKING:
                Walk();
                break;
            case MoveCategory.SLIDING:
                Slide();
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // DEBUG
        {
            Vector3 contactPoint = collision.contacts[0].point;
            Collider2D[] c2ds = new Collider2D[1];
            rb2d.GetAttachedColliders(c2ds);
            Vector3 center = c2ds[0].bounds.center;

            Debug.DrawLine(center, contactPoint, Color.magenta, 1f);
        }

        // Record collision data.
        RecordGrounded();
        RecordCollisionAngle();
        lookahead.DetermineEscalation(collision, this);

        // If touching danger zone, change this to true.
        if (collision.gameObject.tag == "DangerZone") isTouchingDZ = true;
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Record collision data.
        RecordGrounded();
        RecordCollisionAngle();
        lookahead.DetermineEscalation(collision, this);

        // Collect contact direction angle.
        // CHANGE TO DETECT IF WE HAVE A 'HIT' IN TEH VALID ZONE
        ContactPoint2D[] contact = new ContactPoint2D[1];
        collision.GetContacts(contact);

        Vector2 collisionDir = (contact[0].point - (Vector2)rb2d.transform.position);
        float rightToCollisionAngle = Vector2.Angle(Vector2.right, collisionDir);

        // Check if colliding with a wall.
        if (rightToCollisionAngle < WALLSTICK_ANGLE_RESTRICTION)
            rb2d.AddForce(NUDGE_LEFT);
        else if (rightToCollisionAngle > 180f - WALLSTICK_ANGLE_RESTRICTION)
            rb2d.AddForce(NUDGE_RIGHT);
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        RecordGrounded();
        RecordCollisionAngle();
    }


        /* External State Control */


    // Set this unit up for beginning their turn.
    public void TurnStart()
    {
        // Reset all cateogories.
        isLocked = false;
        isContemplating = false;
        isWarping = false;
        isStunned = false;
        isDeconstructed = false;
        isFlying = false;
        isFalling = false;
        isJumping = false;
        isWalking = false;
        isStatic = false;
    }

    // Clean up data for the end of this unit's turn.
    public void TurnEnd()
    {
        isLocked = true;
        ResetMoveQueue();
    }

    // Take damage and stun the unit.
    public void TakeDamage(int damage)
    {
        // Lose health, then see if we're stunned.
        LoseHealth(damage);
        damageTaken = damage;
    }

    // Take damage without stunning the unit.
    public void LoseHealth(int damage)
    {
        // Probably more logic in the future.
    }

    // Informs this unit that its movement logic is changing.
    public void LogicChanged() { logicChanged = true; }

    // Informs this unit that it should stop flying.
    public void CancelFlight() { isFlying = false; }


        /* Movement Category Control */


    // Check for CONTEMPLATING conditions.
    public void CheckContemplating() { isContemplating = categoryLogic[MoveCategory.CONTEMPLATING](this); }

    // Check for WARPING conditions.
    public void WarpInput() { isWarping = categoryLogic[MoveCategory.WARPING](this); }

    // Check for DECONSTRUCTED conditions.
    public void CheckDeconstructed() { isDeconstructed = categoryLogic[MoveCategory.DECONSTRUCTED](this); }

    // Check for STUNNED conditions.
    private void CheckStunned() { isStunned = categoryLogic[MoveCategory.STUNNED](this); }

    // Check for FLYING conditions.
    private void FlyingInput() { isFlying = categoryLogic[MoveCategory.FLYING](this); }

    // Check for FALLING conditions
    private void CheckRising() { isRising = categoryLogic[MoveCategory.RISING](this); }

    // Check for FALLING conditions
    private void CheckFalling() { isFalling = categoryLogic[MoveCategory.FALLING](this); }

    // Check for JUMPING conditions.
    private void JumpInput() { isJumping = categoryLogic[MoveCategory.JUMPING](this); }

    // Check for WALKING conditions.
    private void WalkInput() { isWalking = categoryLogic[MoveCategory.WALKING](this); }

    // Check for SLIDING conditions.
    private void CheckSliding() { isSliding = categoryLogic[MoveCategory.SLIDING](this); }

    // Check for STATIC conditions.
    private void CheckStatic() { isStatic = rb2d.velocity.magnitude < float.Epsilon && isGrounded; }


        /* Movement Logic */


    // Perform the WARP action.
    private void Warp()
    {
        if (warpReady)
        {
            // Set true, just in case we're deconstructed.
            gameObject.SetActive(true);

            // Warp to the given position.
            rb2d.position = mousePosition;

            // We've completed the warp, so we're no longer warp-ready.
            warpReady = false;
        }

        // One way or the other, the warp is done.
        isWarping = false;
    }

    // Perform the CONTEMPLATE action.
    private void Contemplate() { if (currentContemplation(this)) isContemplating = false; }

    // then PERISH
    private void Deconstruct()
    {
        // Set the logic for the deconstruction animation.

        // When that is done, deactivate the object.
        gameObject.SetActive(false);

        // Set the focus to warp automatically.
        // FocusManager.SetFocus(Functions.Warp, Mode.CantCancel);

        // Now that we've PERISHED we don't need to deconstruct anymore.
        isDeconstructed = false;
    }

    // Perform the FLYING action.
    private void Fly()
    {
        if (rb2d.velocity.magnitude < float.Epsilon)
            rb2d.AddForce(mouseDirection * jumpForce, ForceMode2D.Impulse);
        else
            rb2d.AddForce(-rb2d.velocity);
    }

    // Perform the JUMPING action.
    private void Jump()
    {
        rb2d.WakeUp();
        rb2d.AddForce(mouseDirection * jumpForce, ForceMode2D.Impulse);
    }

    // Perform the WALKING action.
    private void Walk()
    {
        // Cast a prediction of the model in front of the movement.
        int horizontal = GetMoveDirInput();

        if (GetMoveCategory() != MoveCategory.FALLING && WalkingUnlocked(horizontal))
        {
            rb2d.WakeUp();

            // Prepare to move.
            // X:   Magnitude of 1 in the input direction.
            // Y:   Magnitude relative to the angle of collision.
            Vector2 moveDir = new Vector2(  horizontal * MoveX(), 
                                            lookahead.GetEscalation() * MoveY());

            // DEBUG
            Debug.DrawLine(rb2d.position, new Vector2(rb2d.position.x, rb2d.position.y + (moveDir * walkAccel).y), Color.blue, 0.5f);
            Debug.DrawLine(rb2d.position, new Vector2(rb2d.position.x + (moveDir * walkAccel).x, rb2d.position.y), Color.blue, 0.5f);

            // Move if we're not at max movespeed.
            if (Mathf.Abs(rb2d.velocity.x) < walkVelocity) rb2d.AddForce(moveDir * walkAccel);
        }
        else if (isGrounded)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.Sleep();
        }
        else if (GetMoveCategory() == MoveCategory.FALLING) { rb2d.velocity = new Vector2(0f, rb2d.velocity.y); }
    }

    // Perform the SLIDING action. (For now, this is just stopping motion.)
    private void Slide() { rb2d.Sleep(); }


        /* Public Getters and Setters. */


    /* Getters */

    public MoveCategory GetPrevMoveCategory() { return moveCategoryQueue.Peek(); }
    public MoveCategory GetMoveCategory() { return category; }
    public Rigidbody2D GetRB2D() { return rb2d; }
    public Lookahead GetPrediction() { return lookahead; }
    public int GetMoveDirInput() { return (int)Input.GetAxisRaw("Horizontal"); }
    public bool GetTouchingDZ() { return isTouchingDZ; }
    public int GetDamage() { return damageTaken; }
    public bool GetNyoooom() { return nyoom; }
    public bool IsGrounded() { return isGrounded; }
    public List<CircumferenceHit> GetGroundedHits()
    {
        // CHECK IF WE'VE ALREADY DONE THIS THIS ITERATION AND JUST RETURN THE LIST IF WE HAVE.

        // OTHERWISE...
        // Collect list of contacts.
        List<CircumferenceHit> hits = new List<CircumferenceHit>();
        Vector2 direction = MIN_CONTACT_RANGE;
        float angle = 0;
        while (direction.x <= MAX_CONTACT_RANGE.x)
        {
            // Record raycast if it is within contact radius.
            CircumferenceHit cHit = new CircumferenceHit(Physics2D.Raycast(rb2d.position, direction), angle, MIN_CONTACT_RANGE);
            if (cHit.hit.distance <= radius + RADIUS_CORRECTION) { hits.Add(cHit); }

            // Increment Angle
            angle += DELTA_CONTACT_ANGLE;
            direction = direction.Rotate(DELTA_CONTACT_ANGLE);
        }

        // Cull extraneous hits that fit the bill and limit each cluster to only the closest hits.
        if (hits.Count > 0) { hits = ExactifyHitClusters(hits); }

        return hits;
    }

    // Return the proper bool per category.
    public bool IsCategory(MoveCategory c)
    {
        switch (c)
        {
            case MoveCategory.LOCKED:
                return isLocked;
            case MoveCategory.WARPING:
                return isWarping;
            case MoveCategory.DECONSTRUCTED:
                return isDeconstructed;
            case MoveCategory.STUNNED:
                return isStunned;
            case MoveCategory.FLYING:
                return isFlying;
            case MoveCategory.JUMPING:
                return isJumping;
            case MoveCategory.FALLING:
                return isFalling;
            case MoveCategory.WALKING:
                return isWalking;
            case MoveCategory.STATIC:
                return isStatic;
            default:
                return false;
        }
    }

    public bool IsLocked() { return isLocked; }
    public bool IsContemplating() { return isContemplating; }
    public bool IsWarping() { return isWarping; }
    public bool IsDeconstructed() { return isDeconstructed; }
    public bool IsStunned() { return isStunned; }
    public bool IsFlying() { return isFlying; }
    public bool IsJumping() { return isJumping; }
    public bool IsRising() { return isRising; }
    public bool IsFalling() { return isFalling; }
    public bool IsWalking() { return isWalking; }
    public bool IsSliding() { return isSliding; }
    public bool IsStatic() { return isStatic; }

    /* Setters */

    public void SetGravityScale(float scale) { rb2d.gravityScale = scale; }

    // Sets mouse data to current position and direction (relative to this unit)
    public void SetMouseData()
    {
        mousePosition = Input.mousePosition;
        mousePosition.z = 5.0f;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mouseDirection = (mousePosition - gameObject.transform.position).normalized;
    }  //...To current mouse location.

    // Tells the Movable that it thinks it should fly.
    public void SetNyoooom(bool nyoooom) { nyoom = nyoooom; }

    public void SetWarpReady() { warpReady = true; }


        /* Helper Functions */


    // Takes a list of clusters of hits and culls it down to only the most relevant hits.
    private List<CircumferenceHit> ExactifyHitClusters(List<CircumferenceHit> list)
    {
        // Determine clusters.
        List<List<CircumferenceHit>> clusters = DetermineHitClusters(list, 3 * DELTA_CONTACT_ANGLE);

        // Retrieve the collision closest to the circumference.
        list.Clear();
        foreach (List<CircumferenceHit> c in clusters) { list.Add(ExactifyHitCluster(c)); }

        return list;
    }

    // Cuts out clusters from a list of hits.
    private List<List<CircumferenceHit>> DetermineHitClusters(List<CircumferenceHit> hitList, float clusterDeviance)
    {
        // Check list for clusters and store them.
        List<List<CircumferenceHit>> clusters = new List<List<CircumferenceHit>>();

        // If there's only one contact point, there's nothing to compare.
        if (hitList.Count == 1) { clusters.Add(hitList); }
        else
        {
            List<CircumferenceHit> cluster = new List<CircumferenceHit>();
            for (int i = 1; i < hitList.Count; i++)
                if (WithinClusterDeviance(hitList, i, clusterDeviance))
                    cluster.Add(hitList[i - 1]);
                else
                {
                    cluster.Add(hitList[i - 1]);
                    clusters.Add(cluster);
                    cluster = new List<CircumferenceHit>();
                }
            ManageFinalClusterHit(ref cluster, ref clusters, hitList);
        }

        return clusters;
    }

    // Checks if, within a list, at a given index whether it's preceding element is within cluster deviation.
    private bool WithinClusterDeviance(List<CircumferenceHit> list, int index, float clusterDeviance)
    {
        return Mathf.Abs(list[index].angle - list[index - 1].angle) <= clusterDeviance;
    }

    // Ties up loose ends involving the final value of the cluster.
    private void ManageFinalClusterHit(ref List<CircumferenceHit> cluster, ref List<List<CircumferenceHit>> clusters, List<CircumferenceHit> hitList)
    {
        if (cluster.Count == 0) return;

        cluster.Add(hitList[hitList.Count - 1]);
        clusters.Add(cluster);
    }

    // Takes a cluster and finds the hit closest to the center of the collider.
    private CircumferenceHit ExactifyHitCluster(List<CircumferenceHit> list)
    {
        CircumferenceHit exactHit = list[0];
        Color debugColor = new Color(Random.value, Random.value, Random.value);
        for (int i = 1; i < list.Count; i++)
            if (list[i].hit.distance < list[i - 1].hit.distance)
                exactHit = list[i];

        Debug.DrawRay(exactHit.hit.point, Vector2.up, debugColor, 0.05f);
        return exactHit;
    }

    // Records whether the unit is grounded or not.
    private void RecordGrounded()
    {
        // Presume not grounded upon checking.
        isGrounded = false;

        // Iterate through all contacts and check for grounded state.
        List<CircumferenceHit> contactList = GetGroundedHits();
        foreach (CircumferenceHit ch in contactList)
            if (ch.hit.point.y < prevPosition.y &&
                ch.hit.point.x < prevPosition.x + (LESS_THAN_HALF * width) &&
                ch.hit.point.x > prevPosition.x - (LESS_THAN_HALF * width))
                isGrounded = true;
    }

    // Record the collision angle, measured from Vector2.down.
    private void RecordCollisionAngle()
    {
        // Assume colliding with ground initially.
        downToCollisionAngle = 0;

        // Iterate through all contacts and set the collision angle to the greatest value.
        List<CircumferenceHit> contactList = GetGroundedHits();
        foreach (CircumferenceHit ch in contactList)
        {
            Vector2 length = ch.hit.point - prevPosition;
            float angle = Vector2.Angle(length, Vector2.down);
            downToCollisionAngle = angle > downToCollisionAngle ? angle : downToCollisionAngle;
        }
    }

    // Records the current move state for the next physics check.
    private void RecordMoveCategory()
    {
        // Prepares to read the back of the queue.
        Queue<MoveCategory>.Enumerator moveCategoryEnum = moveCategoryQueue.GetEnumerator();
        moveCategoryEnum.MoveNext();
        moveCategoryEnum.MoveNext();

        // Update the MoveDir Queue.
        if (category != moveCategoryEnum.Current)
        {
            moveCategoryQueue.Dequeue();
            moveCategoryQueue.Enqueue(category);
        }
    }

    // Convert angle to Y Force.
    private float MoveY() { return downToCollisionAngle < RIGHT_ANGLE ? 1.5f * (downToCollisionAngle / RIGHT_ANGLE) : 0; }

    // Convert angle to X Force.
    private float MoveX() { return downToCollisionAngle < RIGHT_ANGLE ? (RIGHT_ANGLE - downToCollisionAngle) / RIGHT_ANGLE : 0; }

    // Reset the MoveQueue.
    private void ResetMoveQueue()
    {
        moveCategoryQueue.Enqueue(MoveCategory.UNKNOWN);
        moveCategoryQueue.Enqueue(MoveCategory.UNKNOWN);
    }

    // Check if walking is locked.
    private bool WalkingUnlocked(int horizontal) { return !(horizontal == -1 && lookahead.lockStates.Contains(MoveState.LEFT)) && 
                                                          !(horizontal == 1 && lookahead.lockStates.Contains(MoveState.RIGHT)); }
}
