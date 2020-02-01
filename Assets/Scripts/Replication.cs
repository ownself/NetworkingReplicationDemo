using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInfo
{
	public int number;
	public int playerID;
	public Vector3 movementVec;
	public float networkLatency;
}

public class ServerInfo
{
	public int number;
	public List<Vector3> poses = new List<Vector3>();
	public float networkLatency;
	public Dictionary<int, Vector3> verifications = new Dictionary<int, Vector3>();
}