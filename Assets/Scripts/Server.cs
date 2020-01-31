using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is server simulation class to represent the behaviour of authoritive server
public class Server
{
	// int playerNum = 0;
	List<Client> clients;
	List<Vector3> clientsPos;
	float zPos;
	List<ServerPlayer> players;
	List<ClientInfo> receivedPackages;

	public Server(float startZPos)
	{
		clients = new List<Client>();
		clientsPos = new List<Vector3>();
		players = new List<ServerPlayer>();
		receivedPackages = new List<ClientInfo>();
		zPos = startZPos;
	}

	// in case adding same player
	public bool IsNewPlayer(Client client) { return !clients.Contains(client); }

	public int AddClient(Client client)
	{
		if (!clients.Contains(client))
		{
			clients.Add(client);
			return clients.Count - 1;
		}
		return -1;
	}

	public void Start() // notify clients how many players are actually in the game
	{
		float xPos = - Gameplay.xInterval * (clients.Count - 1) / 2.0f;
		for (int i = 0; i < clients.Count; ++i)
		{
			// add server player in server for authority
			GameObject player = GameObject.Instantiate(Resources.Load("ServerPlayer", typeof(GameObject)), new Vector3(xPos, 0.5f, zPos), Quaternion.identity) as GameObject;
			ServerPlayer serverPlayer = player.GetComponent<ServerPlayer>();
			serverPlayer.Init(zPos);
			players.Add(serverPlayer);

			clientsPos.Add(new Vector3(xPos, 0.5f, 0.0f));
			xPos += Gameplay.xInterval;

			// notify client to start
			clients[i].OnServerStarted(clients.Count);
		}
	}

	// gather the input information from clients
	public void SyncClientInput(ClientInfo inputInfo)
	{
		receivedPackages.Add(inputInfo); // start simulating networking travel
	}

	// do authorized upate and send back to client
	public void Update()
	{
		// update received client packages and calculate
		int index = 0;
		while (index < receivedPackages.Count)
		{
			receivedPackages[index].networkLatency -= Time.deltaTime;
			if (receivedPackages[index].networkLatency <= 0.0f) // package arrived
			{
				if (clientsPos.Count > receivedPackages[index].playerID) // valid player ID
				{
					clientsPos[receivedPackages[index].playerID] += receivedPackages[index].movementVec;
				}
				receivedPackages.RemoveAt(index);
			} else {
				index++;
			}
		}

		// update authority server player
		for (int i = 0; i < players.Count; ++i)
		{
			players[i].UpdatePosition(clientsPos[i]);
		}

		// sync all the clients
		for (int i = 0; i < clients.Count; ++i)
		{
			ServerInfo si = new ServerInfo();
			for (int j = 0; j < clientsPos.Count; ++j)
			{
				si.poses.Add(clientsPos[j]);
			}
			// si.poses = clientsPos;
			clients[i].SyncWithServer(si);
		}
	}
}
