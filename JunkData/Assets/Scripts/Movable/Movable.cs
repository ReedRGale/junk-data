using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class Movable : MonoBehaviour
{
    public delegate bool ContemplationLogic(Movable m);

        /* Helper Objects */
    protected MovableCollisionAnalyzer collisionAnalyzer;
    protected MovablePhysics localPhysics;
    protected MovableScouter unitScouter;
    protected ContemplationLogic currentContemplation;

        /* Unity Objects */
    protected Rigidbody2D rb2d;
    protected Vector3 mousePosition;
    protected Vector3 mouseDirection;

        /* Primitive Data & Data Structures */
    protected bool nyoom = false;                           // Signal this Movable to fly. Ammy named this.
    protected bool isDeconstructed = false;
    protected bool moveStateLogicChanged = false;
    protected bool inputLocked = false;
    protected float radius = 0;
    protected int damageRecieved = 0;

    protected LinkedList<MovableState> inputState = new LinkedList<MovableState>();
    protected LinkedList<MovableState> actionState = new LinkedList<MovableState>();
    protected LinkedList<int> movementInput = new LinkedList<int>();
    
    public const float JUMP_FORCE = 0.3f;

    // Set the unit's movement schema to contemplate.
    public void InputContemplate()
    {
        //currentContemplation = focusManager.GetFocus(); or something like that
    }

    // Set the unit's movement schema to standard.
    public void InputUnrestricted() { currentContemplation = null; }


        /* Monodevelop Callbacks */


    protected virtual void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        radius = gameObject.GetComponent<Collider2D>().bounds.extents.x;
        collisionAnalyzer = new MovableCollisionAnalyzer(this);
        localPhysics = new MovablePhysics(this);
        unitScouter = new MovableScouter(this);
        inputState.AddFirst(new MovableUnrestrictedState(this));
        actionState.AddFirst(new MovableStaticState(this));
        movementInput.AddFirst(0);
    }

    protected virtual void Update()
    {
        SetMoveInput((int)Input.GetAxisRaw("Horizontal"));
        GetInputState().PerformAction();
    }

    protected virtual void FixedUpdate()
    {
        unitScouter.LookAhead();
        if (!IsInputLocked())
            GetActionState().PerformAction();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "DangerZone")
            isDeconstructed = true;
    }


        /* External State Control */


    // Take damage and stun the unit.
    public void TakeDamage(int damage)
    {
        // Lose health, then see if we're stunned.
        LoseHealth(damage);
        damageRecieved = damage;
    }

    // Take damage without stunning the unit.
    public void LoseHealth(int damage)
    {
        // Probably more logic in the future.
    }

    // Resets the log for all damage done.
    public void ResetDamageLog() { damageRecieved = 0; }

    // Informs this unit that its movement logic is changing.
    public void LogicChanged() { moveStateLogicChanged = true; }

    // Informs this unit that it should stop flying.
    public void CancelFlight() { SetActionState(new MovableStaticState(this)); }


        /* Public Getters and Setters. */


    /* Getters */

    public MovableScouter GetScouter() { return unitScouter; }
    public MovableCollisionAnalyzer GetCollisionAnalyzer() { return collisionAnalyzer; }
    public MovablePhysics GetLocalPhysics() { return localPhysics; }
    public bool IsGrounded() { return collisionAnalyzer.IsGrounded(true); }
    public bool IsInputLocked() { return inputLocked; }

    public float GetRadius() { return radius; }
    public float GetWidth() { return radius * 2; }
    public bool IsDeconstructed() { return isDeconstructed; }
    public int GetDamage() { return damageRecieved; }
    public bool GetNyoooom() { return nyoom; }
    public int GetEscalation() { return unitScouter.GetEscalation(); }
    public Rigidbody2D GetRB2D() { return rb2d; }
    public GameObject GetGameObject() { return gameObject; }
    public Vector3 GetMousePosition() { return mousePosition; }
    public Vector3 GetMouseDirection() { return mouseDirection; }
    public MovableState GetActionState() { return actionState.First.Value; }
    public MovableState GetActionState(int stepBack)
    {
        LinkedListNode<MovableState> prevState = actionState.First;
        for (int i = 0; i < stepBack; i++) { prevState = prevState.Next; }
        return prevState.Value;
    }
    public MovableState GetInputState() { return inputState.First.Value; }
    public MovableState GetInputState(int stepBack)
    {
        LinkedListNode<MovableState> prevState = inputState.First;
        for (int i = 0; i < stepBack; i++) { prevState = prevState.Next; }
        return prevState.Value;
    }
    public int GetMoveInput() { return movementInput.First.Value; }
    public int GetMoveInput(int stepBack)
    {
        LinkedListNode<int> prevState = movementInput.First;
        for (int i = 0; i < stepBack; i++) { prevState = prevState.Next; }
        return prevState.Value;
    }

    /* Setters */

    // Tells the Movable that it thinks it should fly.
    public void SetDeconstructed(bool deconstructed) { isDeconstructed = deconstructed; }
    public void SetNyoooom(bool nyoooom) { nyoom = nyoooom; }
    public void SetInputLocked(bool isLocked) { inputLocked = isLocked; }
    public void SetActionState(MovableActionState state) { actionState.AddBefore(actionState.First, state); }
    public void SetInputState(MovableInputState state) { inputState.AddBefore(inputState.First, state); }
    public void SetMoveInput(int state) { movementInput.AddBefore(movementInput.First, state); }

    public void SetMouseData()
    {
        mousePosition = Input.mousePosition;
        mousePosition.z = 5.0f;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mouseDirection = (mousePosition - gameObject.transform.position).normalized;
    }
}
