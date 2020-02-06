using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// info structure sending from client to server
public class ClientInfo
{
	public int number;
	public int playerID;
	public Vector3 movementVec;
	public float networkLatency;
}

// info structure sending from server to client
public class ServerInfo
{
	public int number;
	public List<Vector3> poses = new List<Vector3>();
	public float networkLatency;
	public Dictionary<int, Vector3> verifications = new Dictionary<int, Vector3>();
}