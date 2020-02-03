using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is server simulation class to represent the behaviour of authoritive server
public class Server
{
	List<Client> clients;
	List<Vector3> clientsPos;
	float zPos;
	List<ServerPlayer> players;
	List<ClientInfo> receivedPackages; // packages in networking
	List<SortedList<int, Vector3>> packageQueue; // packages are waiting for processing
	List<int> playerProcessedIndexes; // store the packages index for each players
	List<Dictionary<int, Vector3>> authorizedPackages; // packages are ready to send to clients
	int buffLength = (int)(0.05f / Gameplay.clientTickInterval);

	public Server(float startZPos)
	{
		clients = new List<Client>();
		clientsPos = new List<Vector3>();
		players = new List<ServerPlayer>();
		receivedPackages = new List<ClientInfo>();
		packageQueue = new List<SortedList<int, Vector3>>();
		playerProcessedIndexes = new List<int>();
		authorizedPackages = new List<Dictionary<int, Vector3>>();
		zPos = startZPos;
	}

	// in case adding same player
	public bool IsNewPlayer(Client client) { return !clients.Contains(client); }

	public int AddClient(Client client)
	{
		if (!clients.Contains(client))
		{
			clients.Add(client);
			authorizedPackages.Add(new Dictionary<int, Vector3>());
			playerProcessedIndexes.Add(0); // index starts from 0
			packageQueue.Add(new SortedList<int, Vector3>());
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
					ClientInfo ci = receivedPackages[index];
					packageQueue[ci.playerID].Add(ci.number, ci.movementVec);
				}
				receivedPackages.RemoveAt(index);
			} else {
				index++;
			}
		}

		for (int i = 0; i < playerProcessedIndexes.Count; ++i)
		{
			// if (packageQueue[i].Count > 0 && playerProcessedIndexes[i] != packageQueue[i].Keys[0])
			// {
			// 	Debug.Log("player " + i + " disorder happens : " + playerProcessedIndexes[i] + ", "
			// 		+ packageQueue[i].Keys[0]);
			// }
			while (packageQueue[i].Count > 0 && playerProcessedIndexes[i] == packageQueue[i].Keys[0])
			{
				clientsPos[i] += packageQueue[i].Values[0]; // do update
				authorizedPackages[i][playerProcessedIndexes[i]] = clientsPos[i]; // prepare the data to send back
				playerProcessedIndexes[i]++;
				packageQueue[i].RemoveAt(0);
			}
		}

		// update authority server player
		for (int i = 0; i < players.Count; ++i)
		{
			players[i].UpdatePosition(clientsPos[i]);
		}
	}

	public void Tick()
	{
		// sync all the clients
		for (int i = 0; i < clients.Count; ++i)
		{
			ServerInfo si = new ServerInfo();
			for (int j = 0; j < clientsPos.Count; ++j)
			{
				si.poses.Add(clientsPos[j]);
			}

			foreach (KeyValuePair<int, Vector3> verif in authorizedPackages[i])
			{
				si.verifications[verif.Key] = verif.Value;
			}
			authorizedPackages[i].Clear();

			clients[i].SyncWithServer(si);
		}
	}
}
