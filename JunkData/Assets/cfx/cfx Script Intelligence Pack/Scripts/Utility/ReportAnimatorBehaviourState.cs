using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportAnimatorBehaviourState : cfxNotificationSMBSender
{

    public bool reportEnter = true;
    public bool reportExit = true;
    public bool reportIK = true;
    public bool reportMove = true;
    public bool reportUpdate = true;

	public float delay = 0f;

    private string reportingModule = "ReportAnimatorBehaviourState";

    //
    // note that unlike MonoBehaviours that are attached to GameObjects, Animations and
    // their ilk aren't implemented in Unity as instances, but assets. Therefor we cannot assume
    // that theNotificationManager persists during calls, as a scene change can happen
    // any time. Therefore, theNotificationManager is nulled every time Exit is called,
    // and connection is re-established every time we have an enter.
    //
    // That is also the reason whywe did not connect to the manager at Awake
    //

    private Dictionary<string, object> theInfo = null;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // onEnter we must re-establish connection to NotificationManager
        theNotificationManager = null;
        connectToNotificationManager();
        if (theNotificationManager == null) return;

        if (reportEnter) {
            theInfo = new Dictionary<string, object>();
            addBasicInformation(theInfo) ;
            theInfo["GameObject"] = animator.gameObject;
            theInfo["Animator"] =  animator;
            theInfo["StateInfo"] =  stateInfo;
            theInfo["LayerIndex"] = layerIndex;
			theInfo ["Module"] = reportingModule;
            sendNotification(theNotificationName, "EnterState", theInfo);
        }


    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (theNotificationManager != null) {
            if (reportExit) {
                theInfo = new Dictionary<string, object>();
                addBasicInformation(theInfo);
                theInfo["GameObject"] = animator.gameObject;
                theInfo["Animator"] = animator;
                theInfo["StateInfo"] = stateInfo;
                theInfo["LayerIndex"] = layerIndex;
				theInfo ["Module"] = reportingModule;
                sendNotification(theNotificationName, "ExitState", theInfo);
            }

            theNotificationManager = null;
        }

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (theNotificationManager != null) {
            if (reportUpdate) {
                theInfo = new Dictionary<string, object>();
                addBasicInformation(theInfo);
                theInfo["GameObject"] = animator.gameObject;
                theInfo["Animator"] = animator;
                theInfo["StateInfo"] = stateInfo;
				theInfo ["Module"] = reportingModule;
                theInfo["LayerIndex"] = layerIndex;
                sendNotification(theNotificationName, "UpdateState", theInfo);
            }
        }
    }

    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (theNotificationManager != null) {
            if (reportMove)  {
                theInfo = new Dictionary<string, object>();
                addBasicInformation(theInfo);
                theInfo["GameObject"] = animator.gameObject;
                theInfo["Animator"] = animator;
                theInfo["StateInfo"] = stateInfo;
                theInfo["LayerIndex"] = layerIndex;
				theInfo ["Module"] = reportingModule;
                sendNotification(theNotificationName, "MoveState", theInfo);
            }
        }
    }

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (theNotificationManager != null)  {
            if (reportMove)  {
                theInfo = new Dictionary<string, object>();
                addBasicInformation(theInfo);
                theInfo["GameObject"] = animator.gameObject;
                theInfo["Animator"] = animator;
                theInfo["StateInfo"] = stateInfo;
                theInfo["LayerIndex"] = layerIndex;
				theInfo ["Module"] = reportingModule;
                sendNotification(theNotificationName, "IKState", theInfo);

            }
        }
    }

}
