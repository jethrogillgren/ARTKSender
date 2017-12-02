using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCamera : MonoBehaviour {

	private Camera mainCamera;

	// Use this for initialization
	void Start () {
		mainCamera = GetComponent<Camera>;//Not the depth camera, the main one

	}
	
	// Update is called once per frame
	void Update () {

		//If we clicked
		if( Input.GetMouseButtonDown() ) 
		{
			//Get the click Position

			//Check if it is within mainCamera.pixelRect;

			//If Yes:  Focus();
		}
	}

	//Make this camera the main viewed one.
	//Either make it fullscreen, or if available put it on the second monitor
	public void Focus()
	{
		//TODO
	}

}
