using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

using UnityEngine;

//This is a specialised NetworkManager.  NetworkManager wraps the actual Link connection between Server and Client.
public class RoomAugNetworkManager : NetworkManager {



	//Control adding new Players on client connection
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		Debug.Log ( "J# Adding a new player: " + conn + " " + playerControllerId );

		GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
		player.GetComponent<RoomAugPlayerController> ().setName( "Player " + numPlayers );

		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

	}











//	bool _isSyncTimeWithServer = false;
//	public static double syncServerTime;
//	public static double latencyMs = 0;
//
//	void Update()
//	{
//		if (_isSyncTimeWithServer)
//		{
//			syncServerTime += Time.deltaTime;
//		}
//	}

//	public override void OnStartServer ()
//	{
//		base.OnStartServer();
//		// we're the Server, dont need to sync with anyone :)
//		_isSyncTimeWithServer = true;
//		syncServerTime = Network.time;
//	}
//
//	/// <summary>
//	/// On server, be called when a client connected to Server
//	/// </summary>
//	public override void OnServerConnect (NetworkConnection conn)
//	{
//		base.OnServerConnect(conn);
//		Debug.Log("---- Server send syncTime to client : " + conn.connectionId);
//
//		var syncTimeMessage = new SyncTimeMessage();
//		syncTimeMessage.timeStamp = Network.time;
//		NetworkServer.SendToClient(conn.connectionId, CustomMsgType.SyncTime, syncTimeMessage);
//	}
//
//
////	//Add in Client Unique Scene assets for clients only.
////	public override void OnClientSceneChanged(NetworkConnection conn)
////	{
////		if (Application.platform == RuntimePlatform.Android) {
////
////			ClientScene.Ready (conn);
////			ClientScene.AddPlayer (conn, 0);
////
////		} else {
////			Util.JLogErr("Tried to load the ClientScene from a Non-Android Device.");
////		}
////	}
//
//	public override void OnStartClient (NetworkClient client)
//	{
//		base.OnStartClient(client);
//		client.RegisterHandler(CustomMsgType.SyncTime, OnReceiveSyncTime);
//		latencyMs = client.GetRTT ();
//	}
//
//
//	void OnReceiveSyncTime(NetworkMessage msg)
//	{
//		var castMsg = msg.ReadMessage<SyncTimeMessage>();
//		_isSyncTimeWithServer = true;
//		syncServerTime = castMsg.timeStamp + latencyMs;
//		Debug.Log("--------Client receive : " + syncServerTime);
//	}
//
//
//
//
//
//	public class CustomMsgType
//	{
//		public const short SyncTime = MsgType.Highest + 1;
//	}
//
//
//	public class SyncTimeMessage : MessageBase
//	{
//		public double timeStamp;
//	}
}
