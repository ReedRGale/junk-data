using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Structs;

public class Movable : MonoBehaviour
{
    // DEBUG
    public int debugValue = 0;


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

    // Logic to store the current and previous input values.
    Queue<int> moveInputQueue;

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

    protected MovableCollisionAnalyzer collisionAnalyzer;
    protected MovablePhysics localPhysics;
    protected Rigidbody2D rb2d;
    protected MovableScouter scout;

    // Recorded point of the mouse's position.
    protected Vector3 mousePosition;

    // Recorded point of the mouse's direction.
    protected Vector3 mouseDirection;

        /* State Variables */

    // The time a horizontal button has been held.
    protected float horizontalHeld = 0;

    // The radius of this unit.
    protected float radius = 0;

    // Damage taken this turn.
    protected int damageTaken = 0;

    // Indicate that we're ready to warp.
    protected bool warpReady = false;

    // Indicate that we've jumped.
    protected bool jumped = false;

    // Return whether we're touching the danger zone.
    protected bool isTouchingDZ = false;

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

        /* Constants */

    private const int LEFT_CLICK = 0;                           // Int representing the value of a left click.
    private Vector2 NUDGE_LEFT = new Vector2(-0.05f, 0);
    private Vector2 NUDGE_RIGHT = new Vector2(0.05f, 0);
    private const float DETECTION_RANGE = 0.03f;
    private const float STOP_MOVEMENT = 45f;
    private const float WALLDETECT_ANGLE_RESTRICTION = 25f;     // Degrees from 90 at which we're seeing a wall.
    private const float WALLSTICK_ANGLE_RESTRICTION = 8f;
    private const float CLIMB_CORRECTION = 0.5f;
    private const float LESS_THAN_HALF = 0.49f;
    private const float ACCEPT_LOCK_OVERRIDE = 0.4f;


        /* Monodevelop Callbacks */

        
    protected virtual void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        radius = gameObject.GetComponent<Collider2D>().bounds.extents.x;
        collisionAnalyzer = new MovableCollisionAnalyzer(this);
        localPhysics = new MovablePhysics(this);
        scout = new MovableScouter(this);

        // Prepare category information.
        categoryCallbacks = CategoryManager.instance.BundleCallbacks();
        categoryLogic = CategoryManager.instance.BundleLogic();

        // Set up category queue.
        moveCategoryQueue = new Queue<MoveCategory>();
        moveInputQueue = new Queue<int>();
        ResetMoveCategoryQueue();
        ResetMoveInputQueue();
    }
    
    protected virtual void Update()
    {
        // Update the horizontal value.
        HoldingHorizontal();

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

        // Record data for next loop.
        RecordMoveCategory();
        RecordMoveInput();

        //if (debugValue > 0 && GetMoveCategory() != MoveCategory.JUMPING)
        //    Debug.Log("Move Category:  " + GetMoveCategory());

        //if (GetMoveCategory() == MoveCategory.JUMPING)
        //{
        //    Debug.Log("================================================================================================");
        //    Debug.Log("PrevMoveCategory: " + GetPrevMoveCategory());
        //    debugValue = 3;
        //}


        // DEBUG
        if (GetMoveCategory() == MoveCategory.UNKNOWN)
        {
            Debug.Log("Unknown Occurred:  "
                + "\nPrev Move Category:  " + GetPrevMoveCategory()
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
    }

    protected virtual void FixedUpdate()
    {
        scout.LookAhead();

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

    private void OnCollisionEnter2D(Collision2D collision) { if (collision.gameObject.tag == "DangerZone") isTouchingDZ = true; }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
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
        ResetMoveCategoryQueue();
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
    private void CheckStatic() { isStatic = rb2d.velocity.magnitude < float.Epsilon && IsGrounded(); }


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
        if ((GetMoveCategory() != MoveCategory.FALLING && WalkingUnlocked()) || UnlockForced())
        {
            rb2d.WakeUp();

            // DEBUG
            //Vector2 moveDir = collisionAnalyzer.GetBaseWalkVector() + -localPhysics.ParallelGForce();

            //Debug.DrawRay(rb2d.position, new Vector2(localPhysics.ParallelGForce().x, 0), Color.black, 0.5f);
            //Debug.DrawLine(rb2d.position, new Vector2(rb2d.position.x, rb2d.position.y + (moveDir * walkAccel).y), Color.blue, 0.5f);
            //Debug.DrawLine(rb2d.position, new Vector2(rb2d.position.x + (moveDir * walkAccel).x, rb2d.position.y), Color.blue, 0.5f);

            // Move if we're not at max movespeed.
            if (WithinSpeedLimit())
                rb2d.AddForce((collisionAnalyzer.GetBaseWalkVector() + -localPhysics.ParallelGForce()) * walkAccel);
        }
        else if (IsGrounded())
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

    
    public MovableCollisionAnalyzer GetCollisionAnalyzer() { return collisionAnalyzer; }
    public float GetRadius() { return radius; }
    public float GetWidth() { return radius * 2; }
    public MoveCategory GetPrevMoveCategory() { return moveCategoryQueue.Peek(); }
    public MoveCategory GetMoveCategory() { return category; }
    public Rigidbody2D GetRB2D() { return rb2d; }
    public MovableScouter GetPrediction() { return scout; }
    public int GetMoveXInput() { return (int)Input.GetAxisRaw("Horizontal"); }
    public int GetPrevMoveXInput() { return moveInputQueue.Peek(); }
    public bool GetTouchingDZ() { return isTouchingDZ; }
    public int GetDamage() { return damageTaken; }
    public bool GetNyoooom() { return nyoom; }
    public bool IsGrounded() { return collisionAnalyzer.IsGrounded(true); }
    public int GetEscalation() { return scout.GetEscalation(); }

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

    
    // Returns whether the object is moving within injected limits.
    private bool WithinSpeedLimit() { return Mathf.Abs(rb2d.velocity.x) < walkVelocity; }

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

    // Records the current move input for the next physics check.
    private void RecordMoveInput()
    {
        // Prepares to read the back of the queue.
        Queue<int>.Enumerator moveInputEnum = moveInputQueue.GetEnumerator();
        moveInputEnum.MoveNext();
        moveInputEnum.MoveNext();

        // Update the MoveDir Queue.
        moveInputQueue.Dequeue();
        moveInputQueue.Enqueue(GetMoveXInput());
    }

    // Reset the MoveCategoryQueue.
    private void ResetMoveCategoryQueue()
    {
        moveCategoryQueue.Enqueue(MoveCategory.UNKNOWN);
        moveCategoryQueue.Enqueue(MoveCategory.UNKNOWN);
    }

    // Reset the MoveInputQueue
    private void ResetMoveInputQueue()
    {
        moveInputQueue.Enqueue(0);
        moveInputQueue.Enqueue(0);
    }

    // Check if walking is locked.
    private bool WalkingUnlocked()
    { return    !(GetMoveXInput() <= 0 && scout.lockStates.Contains(MoveState.LEFT)) 
             && !(GetMoveXInput() >= 0 && scout.lockStates.Contains(MoveState.RIGHT)); }

    // Check if the unit is trying to force an unlock.
    private bool UnlockForced() { return !WalkingUnlocked() && horizontalHeld >= ACCEPT_LOCK_OVERRIDE; }

    // Records the amount of time that a value has been holding this value.
    private void HoldingHorizontal()
    { horizontalHeld = GetPrevMoveXInput() == GetMoveXInput() 
            ? horizontalHeld + Time.deltaTime : 0; }

    // DEBUG
    private void AngleShooter(Vector2 start, float degrees)
    {
        Debug.DrawRay(start, Vector2.left.Rotate(degrees), Color.green);
        Debug.Log("Vector: " + "(" + Vector2.left.Rotate(degrees).x + "," + Vector2.left.Rotate(degrees).y + ")");
    }
}
