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
	List<ServerInfo> receivedPackages;
	int packageNum = 0;
	SortedList<int, Vector3> buffedPackages;

	public KeyCode rightButton = KeyCode.D;
	public KeyCode leftButton = KeyCode.A;

	public Client()
	{
		receivedPackages = new List<ServerInfo>();
		buffedPackages = new SortedList<int, Vector3>();
	}

	public void Init(float startZPos, KeyCode right, KeyCode left)
	{
		zPos = startZPos; // set z pos for this client
		rightButton = right;
		leftButton = left;
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
				localPlayer.Init(i, zPos, Gameplay.serverTickInterval, rightButton, leftButton);
				players.Add(localPlayer);
			} else { // remote player
				GameObject player = GameObject.Instantiate(Resources.Load("RemotePlayer", typeof(GameObject)), new Vector3(xPos, 0.5f, zPos), Quaternion.identity) as GameObject;
				RemotePlayer remotePlayer = player.GetComponent<RemotePlayer>();
				remotePlayer.Init(i, zPos, Gameplay.serverTickInterval);
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
		// simulating networking lag
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
					if (i != localPlayerID || !Gameplay.isClientPredictionEnabled)
					{
						players[i].UpdatePosition(si.poses[i]);
					}
					else // verification
					{
						foreach (KeyValuePair<int, Vector3> verif in si.verifications)
						{
							if (buffedPackages.ContainsKey(verif.Key))
							{
								if (!IsPredictionValid(buffedPackages[verif.Key], verif.Value))
								{
									// Debug.Log("correct player : " + i + "'s position : " + verif.Key);
									Vector3 diffVec = verif.Value - buffedPackages[verif.Key];
									// remove the previous buffers
									for (int j = 0; j < buffedPackages.IndexOfKey(verif.Key); ++j)
									{
										buffedPackages.RemoveAt(j);
									}
									// correct consequent buffers
									for (int j = buffedPackages.IndexOfKey(verif.Key); j < buffedPackages.Count; ++j)
									{
										int buffKey = buffedPackages.Keys[j];
										Vector3 buffValue = buffedPackages.Values[j];
										buffedPackages.RemoveAt(j);
										buffValue += diffVec;
										buffedPackages.Add(buffKey, buffValue);
									}
									localPlayer.CorrectPosition(diffVec); // correction
								}
								buffedPackages.Remove(verif.Key);
							// } else {
							// 	Debug.Log("can't find number : " + verif.Key + "'s buffed package");
							}
						}
					}
				}
				receivedPackages.RemoveAt(index);
			} else {
				index++;
			}
		}
	}

	public void Tick()
	{
		if (myServer == null || localPlayerID == -1) {
			Debug.Log("Client " + localPlayerID + ": Can't find server");
			return;
		}

		ClientInfo inputInfo = new ClientInfo();
		inputInfo.number = packageNum++;
		inputInfo.playerID = localPlayerID;
		if (Gameplay.isClientPredictionEnabled) {
			inputInfo.movementVec = localPlayer.PredictPosition();
			buffedPackages.Add(inputInfo.number, localPlayer.GetPosition());
		} else {
			inputInfo.movementVec = localPlayer.GetMovement();
		}
		localPlayer.ClearMovementVec();

		// constant latency if it's perfect world
		// inputInfo.networkLatency = Gameplay.networkLatency;
		// randomize 50ms latency variation
		inputInfo.networkLatency = Random.Range(Gameplay.networkLatency,
			Gameplay.networkLatency + Gameplay.networkLatencyFloat); 

		myServer.SyncClientInput(inputInfo);
	}

	bool IsPredictionValid(Vector3 predictedPos, Vector3 authorizedPos)
	{
		return predictedPos.x == authorizedPos.x; // we only care about x value in this sample
	}

	// get the update from server sent back
	public void SyncWithServer(ServerInfo poses)
	{
		poses.networkLatency = Gameplay.networkLatency;
		receivedPackages.Add(poses);
	}
}
