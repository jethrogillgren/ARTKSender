using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//This class controlls the UNET conecpt of a player - one on the server for each Client Tango connected.
public class PlayerController : NetworkBehaviour
{
	[SyncVar]
	public new string name = "Player #"; //Should be set during construction


	public void setName(string name) {
		this.name = name;
		updatePlayerText ();
	}


	public void updatePlayerText() {
		GetComponentInChildren<TextMesh>().text = name;
	}

	public override void OnStartServer() {

		if (gameObject.GetComponent<Camera> () != null) {
			Debug.LogError ("J# Disabling Camera " + name + " on Server");
			gameObject.GetComponent<Camera> ().enabled = false;
		}
		if (gameObject.GetComponent<Tango.TangoApplication> () != null) {
			Debug.LogError ("J# Disabling Tango " + name + " on Server");
			gameObject.GetComponent<Tango.TangoApplication>().enabled = false;
		}
	}

	[Command]
	public void CmdWatchEarth()
	{
		FindObjectOfType<WatcherGameplayObject>().m_LookTarget = GameObject.Find ("Earth");
	}

}
