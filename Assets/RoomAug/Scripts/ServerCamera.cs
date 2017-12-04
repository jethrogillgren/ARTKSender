using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCamera : MonoBehaviour {

	private Camera cam;

	private Rect originalRect; //Don't edit these or reference them, make copies
	private Rect fullscreenRect;

	private bool pleaseToggleFocus = false;	//We do the actual focusing in LateUpdate(), so all cameras have a chance to register the Update Click.  Otherwise in one click, a unfocus happens and a different camera will pounce on the chance to immedietly focus itself
	public bool focused = false;

	public bool focusToSecondMonitor = false;//As opposed to default Fullscreeen

	// Use this for initialization
	void Start () {
		cam = GetComponent<Camera>();//Not the depth camera, the main one
		originalRect = new Rect ( cam.rect );//

		fullscreenRect = new Rect ( 0, 0, Screen.width, Screen.height );

		if (focused)
			ToggleFocus ();
	}
	
	//We do the actual focusing in LateUpdate(), so all cameras have a chance to register the Update Click
	void Update () {

		//If we clicked
		if( Input.GetMouseButtonDown(0) ) 
		{
			//Get the click Position
			Vector3 mouse = Input.mousePosition;

			//Check if it is within mainCamera.pixelRect;
			if( cam.pixelRect.Contains(mouse) )
			{

				if( focusToSecondMonitor )
				{
					/*/TODO pleaseToggleFocus ();*/

				} else if ( !IsOtherFocused() ) { // 
					pleaseToggleFocus = true; //LateUpdate does the actual change
				} //Else - someone else is fullscreen - don't act as the click was for them.
					
			}

			//If Yes:  Focus();
		}
	}

	void LateUpdate()
	{
		if( pleaseToggleFocus )
		{
			ToggleFocus ();
			pleaseToggleFocus = false;
		}
	}

	//Make this camera the main viewed one, or back again
	//Either make it fullscreen, or if available put it on the second monitor
	public void ToggleFocus()
	{
		if( focused ) //We are shrinking back
		{
			cam.rect = new Rect( originalRect );
			DecreaseDepth ();
		}
		else //We are Focusing in
		{
			IncreaseDepth ();

			if (focusToSecondMonitor)
			{
				//TODO
			}
			else
			{
				cam.rect = new Rect ( fullscreenRect );
			}
		}

		focused = !focused;
	}


	private void IncreaseDepth()
	{
		cam.depth+=5;
		GetComponentInChildren<Camera>().depth += 5;
	}
	private void DecreaseDepth()
	{
		cam.depth-=5;

		GetComponentInChildren<Camera>().depth -= 5;
	}

	private bool IsOtherFocused()
	{
		ServerCamera[] cams = transform.parent.GetComponentsInChildren<ServerCamera> ();
		foreach(ServerCamera c in cams)
		{
			//If focused, or about to focus in LateUpdate
			if ( (c.focused || c.pleaseToggleFocus)  && c.name != this.name)
				return true;
		}

		return false;
	}
}
