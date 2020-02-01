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
	int packageNum = 0;
	Dictionary<int, Vector3> buffedPackages;

	public KeyCode rightButton = KeyCode.D;
	public KeyCode leftButton = KeyCode.A;

	public Client()
	{
		receivedPackages = new List<ServerInfo>();
		buffedPackages = new Dictionary<int, Vector3>();
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
					if (i != localPlayerID)
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
									players[i].UpdatePosition(verif.Value);
									Debug.Log("correct player : " + i + "'s position");
								}
								buffedPackages.Remove(verif.Key);
							} else {
								Debug.Log("can't find number : " + verif.Key + "'s buffed package");
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
		inputInfo.movementVec = localPlayer.PredictPosition();
		localPlayer.ClearMovementVec();
		inputInfo.networkLatency = networkLatency;
		myServer.SyncClientInput(inputInfo);
		buffedPackages[inputInfo.number] = localPlayer.GetPosition();
	}

	bool IsPredictionValid(Vector3 predictedPos, Vector3 authorizedPos)
	{
		return predictedPos.x == authorizedPos.x; // we only care about x value in this sample
	}

	// get the update from server sent back
	public void SyncWithServer(ServerInfo poses)
	{
		poses.networkLatency = this.networkLatency;
		receivedPackages.Add(poses);
	}
}
