using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;


using UnityEngine;

//This is a specialised NetworkManager.  NetworkManager wraps the actual Link connection between Server and Client.
public class RoomAugNetworkManager : NetworkManager {


	//Start loading the scene and flag ClientScene.ready once done
	IEnumerator AsyncAddClientSpecificScene(NetworkConnection conn)
	{
		Util.JLog ("Loading in ClientOnline specialisations");

		// The Application loads the Scene in the background at the same time as the current Scene.
		//This is particularly good for creating loading screens. You could also load the scene by build //number.
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("ClientOnline", LoadSceneMode.Additive);

		//Wait until the last operation fully loads to return anything
		while (!asyncLoad.isDone)
		{
			yield return null;
		}

		ClientScene.Ready (conn);
		ClientScene.AddPlayer (conn, 0);
	}

	//Add in Client Unique Scene assets for clients only.
	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		if (Application.platform == RuntimePlatform.Android) {
			
			StartCoroutine( AsyncAddClientSpecificScene (conn) );
//
//			SceneManager.LoadScene("ClientOnline", LoadSceneMode.Additive);
//			ClientScene.Ready (conn);
//			ClientScene.AddPlayer (conn, 0);

		} else {
			Util.JLogErr("Tried to load the ClientScene from a Non-Android Device.");
		}
	}

	//Add in Server Unique Scene assets for servers only.
	public override void OnServerSceneChanged(string sceneName)
	{
		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {

			Debug.Log ("J# Loading in ServerOnline specialisations");
			SceneManager.LoadScene ("ServerOnline", LoadSceneMode.Additive);

		} else {
			Util.JLogErr("Tried to load the ServerScene from a Non-Server Device.");
		}
	}

	//Control adding new Players on client connection
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
		player.GetComponent<RoomAugPlayerController> ().setName( "Player " + numPlayers );

		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

		Debug.Log ( "J# Added a new player: " + player.GetComponent<RoomAugPlayerController> ().name );
	}

}
