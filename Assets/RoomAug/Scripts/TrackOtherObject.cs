using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class TrackOtherObject : NetworkBehaviour {

	public GameObject other;

	public bool trackPosition = true;
	public bool trackRotation = true;

	public string autoLocateCamera = ""; //Fill with name of camera to locate and track

	// Update is called once per frame
	void LateUpdate () {
		if (!isLocalPlayer)
		{
			// exit from update if this is not the local player
			return;
		}

		if (other && other.gameObject.activeInHierarchy) {
			if (trackPosition)
				this.gameObject.transform.position = other.gameObject.transform.position;

			if( trackRotation )
				this.gameObject.transform.rotation = other.gameObject.transform.rotation;

		
		} else if (autoLocateCamera != "") {

			foreach (Camera c in Camera.allCameras) {
				Debug.Log ("J# Cam: " + c.name );
			}

			if ( Camera.main != null  &&  Camera.main.name == autoLocateCamera ) {
				other = Camera.main.gameObject;
				Debug.Log ("J# " + name + " located " + other.name + " to Track");
			} else {
				Debug.LogError ("J# TrackOtherObject autoLocateTangoCamera No Camera called " + autoLocateCamera);
			}
		}
	}
}