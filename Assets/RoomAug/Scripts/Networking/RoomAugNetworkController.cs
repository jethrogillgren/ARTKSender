using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoomAugNetworkController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		RoomAugNetworkDiscovery.Instance.onServerDetected += OnReceiveBraodcast;

		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
			HostGame ();
		} else if (Application.platform == RuntimePlatform.Android) {
			ListenForGame ();
		} else {
			Debug.LogError ("J# RoomAugNetworkController Unknown Platform - not Initialising/");
		}

	}
	
	void OnDestroy() {
		RoomAugNetworkDiscovery.Instance.onServerDetected -= OnReceiveBraodcast;
	}
	
	public void HostGame() {
		Debug.Log ("J# Hosting Game");

		RoomAugNetworkDiscovery.Instance.StartBroadcasting();
		NetworkManager.singleton.StartServer();
	}

	public void ListenForGame() {
		Debug.Log ("J# Looking for Game");

		RoomAugNetworkDiscovery.Instance.ReceiveBroadcast();
	}


	public void JoinGame( string serverIp ) {

		Debug.Log ("J# Joining Game at " + serverIp);
		NetworkManager.singleton.networkAddress = serverIp;
		NetworkManager.singleton.StartClient();
		RoomAugNetworkDiscovery.Instance.StopBroadcasting();
	}

	public void OnReceiveBraodcast(string fromIp, string data) {
		JoinGame ( fromIp );

	}
}
