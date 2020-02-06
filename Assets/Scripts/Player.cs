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
	protected float interval;
	protected Vector3 speedVec;
	public TextMesh label;

	protected void Start() {}
	virtual protected void Update()
	{
		if (Gameplay.isEntityInterpolationEnabled)
		{
			transform.position += speedVec * Time.deltaTime; // interpolating
		}
	}

	virtual protected string GetTypeName() { return ""; }

	public void Init(int id, float startZPos, float tickInterval)
	{
		playerID = id;
		zPos = startZPos;
		interval = tickInterval;
		label.text = GetTypeName() + (playerID + 1).ToString();
	}
	virtual public Vector3 GetPosition() { return transform.position; }
	virtual public void UpdatePosition(Vector3 pos)
	{
		pos.z += zPos;
		if (Gameplay.isEntityInterpolationEnabled) {
			speedVec = (pos - transform.position) / interval;
		} else {
			transform.position = pos;
		}

	}
}
