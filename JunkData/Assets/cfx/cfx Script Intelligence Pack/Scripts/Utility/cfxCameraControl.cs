using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// small script that allows camera to move about and rotate similar 
// to Unity Editor
//
// Copyright (C) 2017 by cf/x AG and Christian Franz
//
//
// Attch this script to the camera, and you can move around the your scenery
// with 
//  - asdw to move left, forward, right, back
//  - qe up down 
//  - yc roll
//  - hold mouse button 1 or 2 to turn and pitch
//

public class cfxCameraControl : MonoBehaviour {

	public float speed = 10;
	public float mouseSpeed = 10f;

	public bool invertY = true;
	public bool globalUp = true; // if false, up/down is in local space

	float heading = 0f;
	float pitch = 0f;
	float roll = 0f;

	void Start()
	{
		// read initial values for camera
		heading = Quaternion.LookRotation(transform.forward).eulerAngles.y;
		pitch = Quaternion.LookRotation(transform.forward).eulerAngles.x;
		roll = Quaternion.LookRotation(transform.forward).eulerAngles.z;
	}

	void Update()
	{
		if (Input.GetMouseButton(1))
		{
			float dX = Input.GetAxis("Mouse X") * mouseSpeed;
			float dY = 0f;
			if (invertY) {
				dY = -(Input.GetAxis ("Mouse Y") * mouseSpeed);
			} else {
				dY = Input.GetAxis ("Mouse Y") * mouseSpeed;
			}

			ChangeHeading(dX);
			ChangePitch(dY);
		} else if (Input.GetMouseButton(0)) // if both are pressed, we do not take this branch
		{
			float dX = Input.GetAxis("Mouse X") * mouseSpeed;
			float dY = 0f;
			if (invertY) {
				dY = -(Input.GetAxis ("Mouse Y") * mouseSpeed);
			} else {
				dY = Input.GetAxis ("Mouse Y") * mouseSpeed;
			}

			ChangeHeading(dX);
			ChangePitch(dY);
		}

		if (Input.GetButton ("Vertical")) {
			float dv = Input.GetAxis ("Vertical");
			MoveForwards (dv * speed * Time.deltaTime);
		}

		if (Input.GetButton ("Horizontal")) {
			float dh = Input.GetAxis ("Horizontal");
			MoveLateral (dh * speed * Time.deltaTime);
		}

		if (Input.GetKey ("e")) {
			ChangeHeight(speed * Time.deltaTime);
		}

		if (Input.GetKey ("q")) {
			ChangeHeight(-speed * Time.deltaTime);
		}

		if (Input.GetKey ("x")) {
			ChangeRoll(speed * Time.deltaTime);
		}

		if (Input.GetKey ("c")) {
			ChangeRoll(-speed * Time.deltaTime);
		}

	}

	void MoveForwards(float delta)
	{
		Vector3 fwd = transform.forward;
		transform.position += delta * fwd;
	}

	void MoveLateral(float delta)
	{
		transform.position += delta * transform.right;
	}

	void ChangeHeight(float delta)
	{
		if (globalUp) {
			transform.position += delta * Vector3.up; // note: we use Vector3.Up, NOT transform.up. 
		} else {
			transform.position += delta * transform.up; // note: we use Vector3.Up, NOT transform.up. 
		}
	}

	void ChangeHeading(float delta)
	{
		heading = heading + delta;
		ClampAngle(ref heading);
		transform.localEulerAngles = new Vector3(pitch, heading, roll);
	}

	void ChangePitch(float delta)
	{
		pitch += delta;
		ClampAngle(ref pitch);
		transform.localEulerAngles = new Vector3(pitch, heading, roll);
	}

	void ChangeRoll(float delta) {
		roll += delta;
		ClampAngle(ref roll);
		transform.localEulerAngles = new Vector3(pitch, heading, roll);
	}
	// 
	// ensure legal values in Euler
	//
	public static void ClampAngle(ref float angle)
	{
		if (angle <= 360.0f)
			angle = angle + 360F;
		if (angle >= 360.0f)
			angle = angle - 360F;
	}
}
