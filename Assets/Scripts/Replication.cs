using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInfo
{
	public int playerID;
	public Vector3 movementVec;
	public float networkLatency;
}

public class ServerInfo
{
	public List<Vector3> poses = new List<Vector3>();
	public float networkLatency;
}