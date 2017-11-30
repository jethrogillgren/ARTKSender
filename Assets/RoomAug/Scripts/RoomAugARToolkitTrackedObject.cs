using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAugARToolkitTrackedObject : ARTrackedObject {

//	public string eventReceiverLookupName = "";
//	private PandaCubeController pandaCubeController;

	// Use this for initialization
	protected override void Start () {
		LogTag = "RoomAugARToolkitTrackedObject: ";

//		pandaCubeController =FindObjectOfType<PandaCubeController> ();


		Invoke ( "FindEventReciever", 2 );

//		base.Start (); Without this, we do not draw ourselves
	}


	public void FindEventReciever()
	{
		eventReceiver = (GameObject) FindObjectOfType<PandaCubeController> ().gameObject;
//		if (eventReceiverLookupName != "")
//			eventReceiver = GameObject.Find ( eventReceiverLookupName );
	}

	//Overridden version strips all unnessecary (for us) draws
	protected override void LateUpdate()
	{
		// Update tracking if we are running in the Player.
		if (Application.isPlaying) {

			// Sanity check, make sure we have an ARMarker assigned.
			ARMarker marker = GetMarker();
			if (marker == null) {
				//visible = visibleOrRemain = false;
			} else {

				// Note the current time
				float timeNow = Time.realtimeSinceStartup;

				if (marker.Visible) {

					if (!visible) {
						// Marker was hidden but now is visible.
						visible = true;
						if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerFound", marker, SendMessageOptions.DontRequireReceiver);

					}

					if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerTracked", marker, SendMessageOptions.DontRequireReceiver);

				} else {
					
					if ( timeNow - timeTrackingLost >= secondsToRemainVisible) {
						if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerLost", marker, SendMessageOptions.DontRequireReceiver);
					}
				}
			} // marker

		} // Application.isPlaying

	}
}
