using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveCategory
{
    /* The order of these sets their implicit precident. 
     * If you reorganize them, you change how they're checked. */

    /* When a Movable Object is LOCKED it:
     * Isn't their turn 
     *
     * A LOCKED Object may: 
     * NOT Walk
     * NOT Jump
     * NOT Call Functions
     * NOT Swap Units
     *
     * LOCKED behavior:
     * Logic:       Turn Ends
     * Callback:    N/A
     * Persistent:  No Input Allowed
     * Ends:        Turn Begins
     * 
     * GameManager Ends Turn                    -> 
     * Movable Stops Checking For Logic         ->
     * GameManager Begins Turn  
     */
    LOCKED,

    /* When a Movable Object is CONTEMPLATING it:
     * Called some Function (In-game spell)
     *
     * A CONTEMPLATING Object may: 
     * NOT Walk
     * NOT Jump
     * NOT Call Other Functions
     * NOT Call Warp 
     * 
     * CONTEMPLATING behavior:
     * Logic:       FocusManager Notes it Has an Ability in Focus
     * Callback:    Change the Contemplating Logic of the Unit
     * Persistent:  Run the Contemplating Logic
     * Ends:        Contemplating Logic Returns True
     * 
     * FocusManager Sees Spell                                  -> 
     * Callback uses FocusManager to Change Unit Contemplation  ->
     * Unit runs Contemplating() Until Contemplation is Done                
     */
    CONTEMPLATING,

    /* When a Movable Object is WARPING it:
     * Called warp
     * Got deconstructed and forced a warp
     *
     * A WARPING Object may: 
     * NOT Walk
     * NOT Jump
     * NOT Call Functions
     * NOT Call Warp
     * 
     * WARPING behavior:
     * Logic:       Warp Focused
     * Callback:    N/A
     * Persistent:  N/A
     * Ends:        Warp Complete
     * 
     * FocusManager Sees Warp           ->
     * Contemplate Finds Warp Location  ->
     * Once Location is Chosen Warp to Location 
     * [ This works because warp will be true once contemplating is complete. ]
     */
    WARPING,

    /* When a Movable Object is DECONSTRUCTED it:
     * Took damage on their turn
     * Experienced an effect that was designed to stun 
     *
     * A DECONSTRUCTED Object may: 
     * NOT Walk
     * NOT Jump
     * NOT Call Functions
     * Call Warp
     * 
     * DECONSTRUCTED behavior:
     * Logic:       Danger Zone Touched
     * Callback:    N/A
     * Persistent:  Deconstruction Animation / GameObject Inactive / Set Warp to Focus
     * Ends:        Warp Queued
     * 
     * Danger Zone Touched              ->
     * Play Deconstruction Animation    ->
     * FocusManager Queues Warp         
     */
    DECONSTRUCTED,

    /* When a Movable Object is STUNNED it:
     * Took damage on their turn
     * Experienced an effect that was designed to stun 
     *
     * A STUNNED Object may: 
     * NOT Walk
     * NOT Jump
     * Call Functions
     * Swap Units
     * Call Warp 
     *  
     * STUNNED behavior:
     * Logic:       Unit Took Damage
     * Callback:    N/A
     * Persistent:  N/A [Implicitly Locks Move Input of Lower Precidence]
     * Ends:        Turn Begins
     * 
     * Unit Takes Damage        ->
     * Lock Lesser Impetuses    ->
     * Turn Start
     */
    STUNNED,

    /* For a Movable Object to be FLYING it:
     * Used an ability to enter this state
     * Must be not moving for an short period of time without interruption
     *
     * A FLYING Object may:
     * Fly
     * Can Cancel
     * Call Functions
     * Swap Units
     * Call Warp 
     *  
     * FLYING behavior:
     * Logic:       External Sets Nyoom True [Outside Function Informed Movable it Should Fly]
     * Callback:    Nyoom = False
     * Persistent:  Fly Action Available
     * Ends:        Damage Taken or Ability Cancelled
     * 
     * External Tells the Unit To Fly   ->
     * Set Flight Impetus to False      ->
     * Allow Unit to Fly                ->
     * Take Damage or Cancel
     */
    FLYING,

    /* For a Movable Object to be RISING it:
     * Ended up midair and is climbing in height
     * Is not flying
     * 
     * A RISING Object may: 
     * NOT Walk
     * NOT Jump
     * NOT Call Functions
     * NOT Swap Units
     * NOT Call Warp 
     *  
     * FALLING behavior:
     * Logic:       Rising:  Definition - Y Velocity > 0.5
     * Callback:    N/A
     * Persistent:  N/A [Implicitly Locks Below Actions]
     * Ends:        Unit Comes to Static
     * 
     * Rising                   ->
     * Locks Lesser Impetuses   ->
     * Unit Comes to a Static State
     */
    RISING,

    /* For a Movable Object to be FALLING it:
     * Must have been static or walking
     * Ended up midair without pressing a jump 
     *
     * A FALLING Object may: 
     * NOT Walk
     * NOT Jump
     * NOT Call Functions
     * NOT Swap Units
     * NOT Call Warp 
     *  
     * FALLING behavior:
     * Logic:       Falling:  Definition - Y Velocity < 0.5
     * Callback:    N/A
     * Persistent:  N/A [Implicitly Locks Below Actions]
     * Ends:        Unit Comes to Static
     * 
     * Falling                  ->
     * Locks Lesser Impetuses   ->
     * Unit Comes to a Static State
     */
    FALLING,

    /* For a Movable Object to be JUMPING it:
     * Must have been grounded or walking
     * Must have had an input solicit it
     *
     * A JUMPING Object may: 
     * NOT Walk
     * NOT Jump
     * NOT Call Functions
     * NOT Swap Units
     * NOT Call Warp 
     *  
     * JUMPING behavior:
     * Logic:       Jump Input Pressed
     * Callback:    N/A
     * Persistent:  Jump
     * Ends:        Jump Completes
     * 
     * Mouse Clicked    ->
     * Jump             ->
     * Unit Comes to Static
     */
    JUMPING,

    /* For a Movable Object to be WALKING it:
     * Must have been grounded
     * Must have had an input solicit it
     *
     * A WALKING Object may:
     * Walk
     * Jump
     * NOT Call Functions
     * Swap Units
     * NOT Call Warp 
     *  
     * WALKING behavior:
     * Logic:       Walk Input Pressed
     * Callback:    N/A
     * Persistent:  Walk
     * Ends:        Walk Input Stops
     * 
     * Walk Input Giveth    -> 
     * Walk Input Taketh Away
     */
    WALKING,

    /* For a Movable Object to be SLIDING it:
     * Must have been walking
     * Move must equal X
     *
     * A SLIDING Object may:
     * Walk
     * Jump
     * Call Functions
     * Swap Units
     * Call Warp 
     *  
     * SLIDING behavior:
     * Logic:       Velocity X > float.Epsilon && Grounded
     * Callback:    N/A
     * Persistent:  Sleep Unit
     * Ends:        Once no longer sliding.
     * 
     * Movement and grounded detected   -> 
     * Movement or grounded no longer detected
     */
    SLIDING,

    /* For a Movable Object to be STATIC it:
     * Must have come to rest (velocity == Vector2.zero)
     *
     * A STATIC Object may:
     * Walk
     * Jump
     * Call Functions
     * Swap Units
     * Call Warp 
     *  
     * STATIC behavior:
     * Logic:       Unit is Static: Definition - Velocity Magnitude < float.Epsilon
     * Callback:    N/A
     * Persistent:  N/A
     * Ends:        When Unit Moves
     * 
     * Stand Still  ->
     * Move
     */
    STATIC,

    /* Something went horribly wrong. 
     *  
     * UNKNOWN behavior:
     * Logic:       You shouldn't be here. YOU SHOULDN'T BE-
     * Callback:    N/A
     * Persistent:  N/A
     * Ends:        When other conditions are met.
     * 
     * To end up here, you must be moving in a manner that hasn't been defined.
     */
    UNKNOWN
};
