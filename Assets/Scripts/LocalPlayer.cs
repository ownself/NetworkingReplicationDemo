using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// represent local player, it can handle input
public class LocalPlayer : Player
{
	public KeyCode rightButton = KeyCode.D;
	public KeyCode leftButton = KeyCode.A;

	public void Init(float startZPos, float tickInterval, KeyCode right, KeyCode left)
	{
		base.Init(startZPos, tickInterval);
		rightButton = right;
		leftButton = left;
	}
	
	// Update is called once per frame
	override protected void Update()
	{
		base.Update();
		HandleInput();
	}

	public void CorrectPosition(Vector3 pos)
	{
		pos.z = 0.0f; // get rid of z
		transform.position += pos; // snap to position
	}

	public void ClearMovementVec() // TODO : put this into network sync logic
	{
		// clean previous input
		movementVec = Vector3.zero;
	}

	public Vector3 GetMovement()
	{
		return movementVec;
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