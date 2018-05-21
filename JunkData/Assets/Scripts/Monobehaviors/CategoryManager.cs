using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CategoryManager : MonoBehaviour
{
    // Singleton instance
    public static CategoryManager instance = null;

    // Dictionary to link MoveCategories to logic replacements.
    private Dictionary<MoveCategory, Movable.CategoryLogic> logicMap;

    // Pair of concurrent lists that track modification to default nature of callbacks.
    private List<MoveCategory> callbackCategoryList;
    private List<Movable.CategoryCallback> callbackDelegateList;


    // MOVE TO CONSTANTS OBJECT.
    private string REDEFINITION_ERROR = "This redefinition collides with another.";
    private string UNDEFINABLE_ERROR = "You can't redefine this logic.";
    private const int LEFT_CLICK = 0;                       // Int representing the value of a left click.

    // Singleton Pattern Logic
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        // Initialize the data structures.
        logicMap = new Dictionary<MoveCategory, Movable.CategoryLogic>();
        callbackCategoryList = new List<MoveCategory>();
        callbackDelegateList = new List<Movable.CategoryCallback>();
    }

        /* CategoryLogic */
        /* Category logic defines for the unit whether it should shift from one category to another.
     * Any changes that an outside Function (In-game spells) wishes to do a unit should pass
     * a CategoryLogic function into the CategoryManager through Redefine.
     * 
     * We further promise that ONLY ONE category logic redefinition can be given, and that
     * the logic for being STATIC and being LOCKED cannot change.*/


    // Redefine a particular category's checking logic with a particular function.
    public void Redefine(MoveCategory category, Movable.CategoryLogic C)
    {
        // Add new function to concurrent lists.
        if (category != MoveCategory.LOCKED && category != MoveCategory.STATIC && !logicMap.ContainsKey(category))
            logicMap[category] = C;
        else if (category == MoveCategory.LOCKED || category == MoveCategory.STATIC)
            throw new ArgumentException(UNDEFINABLE_ERROR);
        else
            throw new ArgumentException(REDEFINITION_ERROR);
    }

    // Bundle all logic information.
    public Dictionary<MoveCategory, Movable.CategoryLogic> BundleLogic()
    {
        Dictionary<MoveCategory, Movable.CategoryLogic> logic = DefaultLogic();

        // If a redefinition is given, overwrite the default logic.
        foreach (MoveCategory c in logicMap.Keys)
            logic[c] = logicMap[c];

        return logic;
    }

    // Prepare the default logic dictionary.
    private Dictionary<MoveCategory, Movable.CategoryLogic> DefaultLogic()
    {
        Dictionary<MoveCategory, Movable.CategoryLogic> logic = new Dictionary<MoveCategory, Movable.CategoryLogic>();
        logic.Add(MoveCategory.CONTEMPLATING, ShouldContemplate);
        logic.Add(MoveCategory.WARPING, ShouldWarp);
        logic.Add(MoveCategory.DECONSTRUCTED, ShouldDeconstruct);
        logic.Add(MoveCategory.STUNNED, ShouldStun);
        logic.Add(MoveCategory.FLYING, ShouldFly);
        logic.Add(MoveCategory.RISING, ShouldRise);
        logic.Add(MoveCategory.FALLING, ShouldFall);
        logic.Add(MoveCategory.JUMPING, ShouldJump);
        logic.Add(MoveCategory.WALKING, ShouldWalk);
        logic.Add(MoveCategory.SLIDING, ShouldSlide);
        return logic;
    }

    /* Default Logic */

    private bool ShouldContemplate(Movable m)
    {
        // return Object.NotNull(FocusManager.FunctionFocus())
        return false;
    }
    private bool ShouldWarp(Movable m)
    {
        // FocusManager.FunctionFocus().name.Equals("warp");
        return false;
    }
    private bool ShouldDeconstruct(Movable m) { return m.GetTouchingDZ(); }
    private bool ShouldStun(Movable m) { return m.GetDamage() > 0; }
    private bool ShouldFly(Movable m) { return m.GetNyoooom() || (m.IsFlying() && m.GetDamage() <= 0); }
    private bool ShouldRise(Movable m) { return m.GetRB2D().velocity.y > 0.5f && !m.IsGrounded(); }
    private bool ShouldFall(Movable m) { return m.GetRB2D().velocity.y < -0.5f && !m.IsGrounded(); }
    private bool ShouldJump(Movable m) { return m.GetMoveCategory() > MoveCategory.JUMPING && m.IsGrounded() ? Input.GetMouseButtonUp(LEFT_CLICK) : false; }
    private bool ShouldWalk(Movable m) { return Mathf.Abs(m.GetMoveDirInput()) > 0; }
    private bool ShouldSlide(Movable m) { return m.IsGrounded() && Mathf.Abs(m.GetRB2D().velocity.x) > float.Epsilon; }


        /* CategoryCallbacks */
        /* Category callbacks defines for the unit whether it should shift from one state to another.
         * Any changes that an outside Function (In-game spells) wishes to do a unit should pass
         * a CategoryLogic function into the CategoryManager through Redefine. */


    // Redefine a particular category's callback functionality with a particular function.
    public void Redefine(MoveCategory category, Movable.CategoryCallback C)
    {
        // Add new function to concurrent lists.
        AddToCallbackLists(category, C);
    }

    // Gather all callback information.
    public List<Movable.CategoryCallback> BundleCallbacks()
    {
        // Prepare the list of delegates.
        List<Movable.CategoryCallback> delegates = new List<Movable.CategoryCallback>();
        foreach (MoveCategory c in Enum.GetValues(typeof(MoveCategory)))
            if (callbackCategoryList.Count > 0 && callbackCategoryList.Contains(c))
            {
                // Define the anonymous delegate.
                List<Movable.CategoryCallback> temp = new List<Movable.CategoryCallback>();
                int i = 0;
                while (callbackCategoryList.Contains(c))
                {
                    if (callbackCategoryList[i] == c)
                    {
                        temp.Add(callbackDelegateList[i]);
                        RemoveFromCallbackLists(c);
                    }
                    i++;
                }
                delegates.Add(delegate (Movable m, out MoveCategory category)
                              {
                                  category = MoveCategory.UNKNOWN;
                                  if (m.IsCategory(c))
                                      foreach (Movable.CategoryCallback C in temp) { C(m, out category); }
                                  return m.IsCategory(c);
                              });
            }
            else delegates.Add(DefaultCallbacks(c));

        return delegates;
    }

    // Return the proper bool per category.
    public Movable.CategoryCallback DefaultCallbacks(MoveCategory c)
    {
        switch (c)
        {
            case MoveCategory.LOCKED:
                return Locked;
            case MoveCategory.CONTEMPLATING:
                return Contemplating;
            case MoveCategory.WARPING:
                return Warping;
            case MoveCategory.DECONSTRUCTED:
                return Deconstructed;
            case MoveCategory.STUNNED:
                return Stunned;
            case MoveCategory.FLYING:
                return Flying;
            case MoveCategory.JUMPING:
                return Jumping;
            case MoveCategory.RISING:
                return Rising;
            case MoveCategory.FALLING:
                return Falling;
            case MoveCategory.WALKING:
                return Walking;
            case MoveCategory.SLIDING:
                return Sliding;
            case MoveCategory.STATIC:
                return Static;
            default:
                return delegate (Movable m, out MoveCategory category)
                {
                    category = MoveCategory.UNKNOWN;
                    return false;
                };
        }
    }

    /* Default Callbacks */

    private bool Locked(Movable m, out MoveCategory category)
    {
        category = MoveCategory.UNKNOWN;
        if (m.IsLocked())
            category = MoveCategory.LOCKED;
        return m.IsLocked();
    }
    private bool Contemplating(Movable m, out MoveCategory category)
    {
        // FocusManager.FunctionFocus().AssignLogic()
        category = MoveCategory.UNKNOWN;
        if (m.IsContemplating())
            category = MoveCategory.CONTEMPLATING;
        return m.IsContemplating();
    }
    private bool Warping(Movable m, out MoveCategory category)
    {
        category = MoveCategory.UNKNOWN;
        if (m.IsWarping())
            category = MoveCategory.WARPING;
        return m.IsWarping();
    }
    private bool Deconstructed(Movable m, out MoveCategory category)
    {
        category = MoveCategory.UNKNOWN;
        if (m.IsDeconstructed())
            category = MoveCategory.DECONSTRUCTED;
        return m.IsDeconstructed();
    }
    private bool Stunned(Movable m, out MoveCategory category)
    {
        category = MoveCategory.UNKNOWN;
        if (m.IsStunned())
            category = MoveCategory.STUNNED;
        return m.IsStunned();
    }
    private bool Flying(Movable m, out MoveCategory category)
    {
        m.SetNyoooom(false);
        category = MoveCategory.UNKNOWN;
        if (m.IsFlying())
            category = MoveCategory.FLYING;
        return m.IsFlying();
    }
    private bool Rising(Movable m, out MoveCategory category)
    {
        category = MoveCategory.UNKNOWN;
        if (m.IsRising())
            category = MoveCategory.RISING;
        return m.IsRising();
    }
    private bool Falling(Movable m, out MoveCategory category)
    {
        category = MoveCategory.UNKNOWN;
        if (m.IsFalling())
            category = MoveCategory.FALLING;
        return m.IsFalling();
    }
    private bool Jumping(Movable m, out MoveCategory category)
    {
        m.SetMouseData();
        category = MoveCategory.UNKNOWN;
        if (m.IsJumping())
            category = MoveCategory.JUMPING;
        return m.IsJumping();
    }
    private bool Walking(Movable m, out MoveCategory category)
    {
        category = MoveCategory.UNKNOWN;
        if (m.IsWalking())
            category = MoveCategory.WALKING;
        return m.IsWalking();
    }
    private bool Sliding(Movable m, out MoveCategory category)
    {
        category = MoveCategory.UNKNOWN;
        if (m.IsSliding())
            category = MoveCategory.SLIDING;
        return m.IsSliding();
    }
    private bool Static(Movable m, out MoveCategory category)
    {
        category = MoveCategory.UNKNOWN;
        if (m.IsStatic())
            category = MoveCategory.STATIC;
        return m.IsStatic();
    }


        /* Helper Functions */


    // Method to make sure that all the values stay concurrent.
    private void AddToCallbackLists(MoveCategory c, Movable.CategoryCallback C)
    {
        callbackCategoryList.Add(c);
        callbackDelegateList.Add(C);
    }

    // Method to remove values from lists, so they remain concurrent.
    private void RemoveFromCallbackLists(MoveCategory c)
    {
        for (int i = 0; i < callbackCategoryList.Count; i++)
        {
            if (callbackCategoryList[i].Equals(c))
            {
                callbackCategoryList.RemoveAt(i);
                callbackDelegateList.RemoveAt(i);
            }
        }
    }
}
