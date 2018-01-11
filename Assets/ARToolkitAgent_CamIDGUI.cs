using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARToolkitAgent_CamIDGUI : MonoBehaviour {

	Text timeText;
	RoomAugNetworkController con;

	void Start()
	{
		 timeText = gameObject.GetComponent<Text>();
		 con = FindObjectOfType<RoomAugNetworkController> ();
	}

	// Use this for initialization
	void Update () {
		
		if (con && timeText)
		{
			timeText.text = "Cam ID: " + con.ARToolkit_CamID;
			if (con.ARToolkit_UdpClient != null)
				timeText.text += RoomAugNetworkController.serverIPAddr;
		}
		else
		{
			Debug.LogError ( "Unable to render the CamID text box" );
		}
	}
	

}
