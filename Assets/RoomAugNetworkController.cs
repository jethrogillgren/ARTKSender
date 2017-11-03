using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoomAugNetworkController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		CustomNetworkDiscovery.Instance.onServerDetected += OnReceiveBraodcast;

		#if (UNITY_EDITOR || UNITY_STANDALONE_OSX)
				HostGame();
		#elif ( UNITY_ANDROID )
				ListenForGame();
		#else 
		#error not supported platform
		#endif
	}
	void OnDestroy() {
		CustomNetworkDiscovery.Instance.onServerDetected -= OnReceiveBraodcast;
	}
	
	public void HostGame() {
		Debug.LogError ("J# Hosting Game");

		CustomNetworkDiscovery.Instance.StartBroadcasting();
		NetworkManager.singleton.StartHost();
	}

	public void ListenForGame() {
		Debug.LogError ("J# Looking for Game");

		CustomNetworkDiscovery.Instance.ReceiveBroadcast();
	}

	public void JoinGame( string serverIp ) {

		Debug.LogError ("J# Joining Game at " + serverIp);
		NetworkManager.singleton.networkAddress = serverIp;
		NetworkManager.singleton.StartClient();
		CustomNetworkDiscovery.Instance.StopBroadcasting();
	}

	public void OnReceiveBraodcast(string fromIp, string data) {
		JoinGame ( fromIp );

	}
}
