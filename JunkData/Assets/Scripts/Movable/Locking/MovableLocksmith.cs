using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableLocksmith
{

    //// If the lock data is still within a relevant distance. If not, also remove it and unlock it.
    //private bool LockDataIsRelevant()
    //{
    //    bool isRelevant = potentialLockData.ContainsKey(GetCurrentMoveState()) ?
    //        Mathf.Abs(potentialLockData[GetCurrentMoveState()].point.x - GetFallFocus()) <= GetCheckingMagnitude() : false;

    //    // If exists and is irrelevant remove it from the data set.
    //    if (potentialLockData.ContainsKey(GetCurrentMoveState()) && !isRelevant)
    //    {
    //        potentialLockData.Remove(GetCurrentMoveState());
    //        Unlock(GetCurrentMoveState());
    //    }

    //    return isRelevant;
    //}

    //// Determine which directions should be locked.
    //private void SetLocks()
    //{
    //    if (LockDataIsRelevant())
    //    {
    //        // If the fall point's x magnitude outstrips the danger point's magnitude...
    //        RaycastHit2D closestDoom = Mathf.Abs(GetFallFocus() - postfall.point.x) < Mathf.Abs(GetFallFocus() - postdeath.point.x) ? postfall : postdeath;

    //        // DEBUG:
    //        Debug.DrawRay(new Vector2(closestDoom.point.x, unit.GetRB2D().position.y), Vector2.down, Color.red, 1f);
    //        Debug.DrawRay(new Vector2(GetFallFocus(MoveInput.LEFT), unit.GetRB2D().position.y), Vector2.down, Color.yellow, 0.05f);
    //        Debug.DrawRay(new Vector2(GetFallFocus(MoveInput.RIGHT), unit.GetRB2D().position.y), Vector2.down, Color.yellow, 0.05f);

    //        // Determine which direction to lock movement.
    //        if (GetFallFocus(MoveInput.LEFT) < closestDoom.point.x && GetCurrentMoveState() == MoveInput.LEFT)
    //            SetLock(MoveInput.LEFT, closestDoom);
    //        else if (GetFallFocus(MoveInput.RIGHT) > closestDoom.point.x && GetCurrentMoveState() == MoveInput.RIGHT)
    //            SetLock(MoveInput.RIGHT, closestDoom);
    //    }

    //    // Check if our datapoint should be locking.
    //    if (lockStates.Count > 0)
    //        CheckUnlock();
    //}

    // Set a particular direction as locked and record its hit data.
    //private void SetLock(MoveInput state, RaycastHit2D stateData)
    //{
    //    lockStates.Remove(state);
    //    lockStates.Add(state);
    //}

    // Unlock a given state and clear its hit data.
//    private void Unlock(MoveInput state) { lockStates.Remove(state); }

    // Check if any of the locking states don't need to be locked anymore.
    //private void CheckUnlock()
    //{
    //    // If fall focus isn't within the hit error range, unlock it.
    //    List<MoveInput> states = new List<MoveInput>();
    //    foreach (MoveInput s in lockStates)
    //        if (ShouldBeUnlocked(s, unlockReqs[s]))
    //            states.Add(s);
    //    foreach (MoveInput s in states)
    //        Unlock(s);
    //}

    // Check if a hit is within lockrange.
//    private bool ShouldBeUnlocked(MoveInput state, ShouldUnlock req) { return req(potentialLockData[state]); }
}
