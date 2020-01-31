using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class of server, local and remote players
abstract public class Player : MonoBehaviour
{
	protected float speed = 3.0f; // player moving speed
	protected Vector3 movementVec = Vector3.zero;
	protected int playerID = -1;
	protected float zPos;

	protected void Start() {}
	virtual protected void Update() {}

	public void Init(float startZPos) { zPos = startZPos; }
	virtual public Vector3 GetMovementVec() { return movementVec; }
	virtual public void UpdatePosition(Vector3 pos)
	{
		pos.z += zPos;
		transform.position = pos;
	}
}
