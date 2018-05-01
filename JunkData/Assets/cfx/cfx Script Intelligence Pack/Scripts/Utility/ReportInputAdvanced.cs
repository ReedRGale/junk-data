using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportInputAdvanced : cfxNotificationSender {

	[Header("Enter they buttons to check and the notification it generates below.")]
	public Dictionary<string, string> Buttons = new Dictionary<string, string>() { {"Jump", "JumpButton"}, {"Fire1", "Fire1Button"}, { "Fire2", "Fire2Button" }};
	[Header("Enter they keys to check and the notification it generates below.")]
	public Dictionary<string, string> Keypress = new Dictionary<string, string>() { {"a", "Up"} , { "s" , "Down" } , {"d", "Left"}, { "w", "Right"} };
	[Header("If there is nothing to edit above, Unity should *really* get a better edito for generic lists")]
	public bool reportDown;
	public bool reportUp;

	public List<string> Axis = new List<string>() { "Horizontal", "Vertical" };
	public bool reportAxis;
	public string axisNotificationName;

	// Update is called once per frame
	void Update()
	{
		foreach (string whichButton in Buttons.Keys)
		{
			if (reportDown)  {
				if (Input.GetButtonDown(whichButton)) {
					string theNotificationName = Buttons[whichButton];
					Dictionary<string, object> theInfo = getBasicInfo("ButtonDown");
					theInfo.Add("Button", whichButton);
					postNotification(theNotificationName, theInfo);
				}

			}

			if (reportUp)
			{
				if (Input.GetButtonUp(whichButton)) {
					string theNotificationName = Buttons[whichButton];
					Dictionary<string, object> theInfo = getBasicInfo("ButtonUp");
					theInfo.Add("Button", whichButton);
					postNotification(theNotificationName, theInfo);
				}
			}

		}

		foreach (string whichKey in Keypress.Keys)
		{
			if (reportDown) {
				if (Input.GetKeyDown(whichKey))  {
					string theNotificationName = Buttons[whichKey];
					Dictionary<string, object> theInfo = getBasicInfo("KeyDown");
					theInfo.Add("Key", whichKey);
					postNotification(theNotificationName, theInfo);
				}

			}

			if (reportUp)  {
				if (Input.GetKeyUp(whichKey))  {
					string theNotificationName = Buttons[whichKey];
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
				postNotification(axisNotificationName, theInfo);
			}
		}
	}
}
