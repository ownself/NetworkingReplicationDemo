using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// represent a logic client, client will create its player and has a LocalPlayer to get input
public class Client
{
	GameObject root;
	int localPlayerID;
	int playerNum;
	Server myServer;
	LocalPlayer localPlayer;
	List<Player> players;
	float zPos;
	float networkLatency;
	List<ServerInfo> receivedPackages;

	public KeyCode rightButton = KeyCode.D;
	public KeyCode leftButton = KeyCode.A;

	public Client()
	{
		receivedPackages = new List<ServerInfo>();
	}

	public void Init(float latency, float startZPos, KeyCode right, KeyCode left)
	{
		zPos = startZPos; // set z pos for this client
		rightButton = right;
		leftButton = left;
		networkLatency = latency;
	}

	public void ConnectTo(Server server)
	{
		if (server != null) {
			myServer = server;
			if (!server.IsNewPlayer(this)) {
				Debug.Log("Client : " + localPlayerID + " has already connected to the server");
			} else {
				localPlayerID = server.AddClient(this);
			}
		} else {
			Debug.LogError("server is not available");
		}
	}

	// here we created load and create player prefabs
	public void OnServerStarted(int num)
	{
		playerNum = num;
		players = new List<Player>();
		float xPos = - Gameplay.xInterval * (num - 1) / 2.0f;
		for (int i = 0; i < num; ++i)
		{
			if (i == localPlayerID) // local player
			{
				GameObject player = GameObject.Instantiate(Resources.Load("LocalPlayer", typeof(GameObject)), new Vector3(xPos, 0.5f, zPos), Quaternion.identity) as GameObject;
				localPlayer = player.GetComponent<LocalPlayer>();
				localPlayer.Init(zPos, rightButton, leftButton);
				players.Add(localPlayer);
			} else { // remote player
				GameObject player = GameObject.Instantiate(Resources.Load("RemotePlayer", typeof(GameObject)), new Vector3(xPos, 0.5f, zPos), Quaternion.identity) as GameObject;
				RemotePlayer remotePlayer = player.GetComponent<RemotePlayer>();
				remotePlayer.Init(zPos);
				players.Add(remotePlayer);
			}
			xPos += Gameplay.xInterval;
		}
	}

	// send the input information to server
	public void Update()
	{
		if (myServer == null) {
			Debug.Log("Client " + localPlayerID + ": Can't find server");
			return;
		}

		if (localPlayerID != -1)
		{
			ClientInfo inputInfo = new ClientInfo();
			inputInfo.playerID = localPlayerID;
			inputInfo.movementVec = localPlayer.GetMovementVec();
			inputInfo.networkLatency = networkLatency;
			myServer.SyncClientInput(inputInfo);
		}

		// update received client packages and calculate
		int index = 0;
		while (index < receivedPackages.Count)
		{
			receivedPackages[index].networkLatency -= Time.deltaTime;
			if (receivedPackages[index].networkLatency <= 0.0f) // package arrived
			{
				ServerInfo si = receivedPackages[index];
				for (int i = 0; i < si.poses.Count; ++i)
				{
					players[i].UpdatePosition(si.poses[i]);
				}
				receivedPackages.RemoveAt(index);
			} else {
				index++;
			}
		}
	}

	// get the update from server sent back
	public void SyncWithServer(ServerInfo poses)
	{
		poses.networkLatency = this.networkLatency;
		receivedPackages.Add(poses);
	}
}
