using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ReportInput : cfxNotificationSender {

    public string theNotificationName = "Input";
    public List<string> Buttons = new List<string>() { "Jump", "Fire1", "Fire2" };
    public List<string> Keypress = new List<string>() {"a", "s", "d", "w"};
    public bool reportDown;
    public bool reportUp;

    public List<string> Axis = new List<string>() { "Horizontal", "Vertical" };
    public bool reportAxis;

    // Update is called once per frame
    void Update()
    {
        foreach (string whichButton in Buttons)
        {
            if (reportDown)  {
                if (Input.GetButtonDown(whichButton)) {
                    Dictionary<string, object> theInfo = getBasicInfo("ButtonDown");
                    theInfo.Add("Button", whichButton);
                    postNotification(theNotificationName, theInfo);
                }

            }

            if (reportUp)
            {
                if (Input.GetButtonUp(whichButton)) {
                    Dictionary<string, object> theInfo = getBasicInfo("ButtonUp");
                    theInfo.Add("Button", whichButton);
                    postNotification(theNotificationName, theInfo);
                }
            }

        }

        foreach (string whichKey in Keypress)
        {
            if (reportDown) {
                if (Input.GetKeyDown(whichKey))  {
                    Dictionary<string, object> theInfo = getBasicInfo("KeyDown");
                    theInfo.Add("Key", whichKey);
                    postNotification(theNotificationName, theInfo);
                }

            }

            if (reportUp)  {
                if (Input.GetKeyUp(whichKey))  {
                    Dictionary<string, object> theInfo = getBasicInfo("KeyUp");
                    theInfo.Add("Key", whichKey);
                    postNotification(theNotificationName, theInfo);
                }
            }

        }

        if (reportAxis)  {
            foreach (string ax in Axis) {
                Dictionary<string, object> theInfo = getBasicInfo("Axis");
                theInfo.Add("Axis", ax);
                theInfo.Add("Value", Input.GetAxis(ax));
                postNotification(theNotificationName, theInfo);
            }
        }
    }
   
}
