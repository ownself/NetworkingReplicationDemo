using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// represent local player, it can handle input
public class LocalPlayer : Player
{
	public KeyCode rightButton = KeyCode.D;
	public KeyCode leftButton = KeyCode.A;

	// Use this for initialization
	// void Start() {}

	public void Init(float startZPos, KeyCode right, KeyCode left)
	{
		base.Init(startZPos);
		rightButton = right;
		leftButton = left;
	}
	
	// Update is called once per frame
	override protected void Update()
	{
		HandleInput();
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