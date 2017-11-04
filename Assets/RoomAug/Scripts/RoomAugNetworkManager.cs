using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using UnityEngine;

//This is a specialised NetworkManager.  NetworkManager wraps the actual Link connection between Server and Client.
public class RoomAugNetworkManager : NetworkManager {

	//Add in Client Unique Scene assets for clients only
	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		if (Application.platform == RuntimePlatform.Android) {
			Debug.Log ("J# Loading in ClientOnline specialisations");
			SceneManager.LoadScene ("ClientOnline", LoadSceneMode.Additive);
			ClientScene.Ready (conn);
			ClientScene.AddPlayer (conn, 0);

		} else {
			Debug.LogError ("J# Tried to load the ClientScene from a Non-Android Device.");
		}
	}

	//Add in Server Unique Scene assets for servers only
	public override void OnServerSceneChanged(string sceneName)
	{
		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {

			Debug.Log ("J# Loading in ServerOnline specialisations");
			SceneManager.LoadScene ("ServerOnline", LoadSceneMode.Additive);

		} else {
			Debug.LogError ("J# Tried to load the ServerScene from a Non-Server Device.");
		}
	}
}
