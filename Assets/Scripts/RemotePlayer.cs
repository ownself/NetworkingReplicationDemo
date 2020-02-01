using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// remote players are only existed on client, it only represent other players on client and has not authority, neither handling input
public class RemotePlayer : Player
{
	Vector3 speedVec;

	void Update()
	{
		transform.position += speedVec * Time.deltaTime; // interpolating
	}

	// set speed for interpolating
	override public void UpdatePosition(Vector3 pos)
	{
		pos.z += zPos;
		speedVec = (pos - transform.position) / Gameplay.serverTickInterval;
		// transform.position = pos;
	}
}