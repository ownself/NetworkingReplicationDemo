using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// represent local player, it can handle input
public class LocalPlayer : Player
{
	public KeyCode rightButton = KeyCode.D;
	public KeyCode leftButton = KeyCode.A;

	Vector3 correctVec;

	public void Init(float startZPos, KeyCode right, KeyCode left)
	{
		base.Init(startZPos);
		rightButton = right;
		leftButton = left;
	}
	
	// Update is called once per frame
	override protected void Update()
	{
		transform.position += correctVec * Time.deltaTime; // interpolating
		HandleInput();
	}

	public void ConfirmPrediction()
	{
		correctVec = Vector3.zero;
	}

	// set speed for interpolating
	override public void UpdatePosition(Vector3 pos)
	{
		pos.z += zPos;
		correctVec = (pos - transform.position) / Gameplay.serverTickInterval;
		// transform.position = pos;
	}

	public void ClearMovementVec() // TODO : put this into network sync logic
	{
		// clean previous input
		movementVec = Vector3.zero;
	}

	public Vector3 PredictPosition()
	{
		transform.position += movementVec;
		return movementVec;
	}

	virtual protected void HandleInput()
	{
		if (Input.GetKey(rightButton))
		{
			Vector3 dir = new Vector3(speed, 0, 0);
			movementVec += dir * Time.deltaTime;
		}
		if (Input.GetKey(leftButton))
		{
			Vector3 dir = new Vector3(-speed, 0, 0);
			movementVec += dir * Time.deltaTime;
		}
	}
}