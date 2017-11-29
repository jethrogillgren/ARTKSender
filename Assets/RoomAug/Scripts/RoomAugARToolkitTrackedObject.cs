using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAugARToolkitTrackedObject : ARTrackedObject {

	public string eventReceiverLookupName = "";

	// Use this for initialization
	protected override void Start () {
		LogTag = "RoomAugARToolkitTrackedObject: ";

		if (!eventReceiver && eventReceiverLookupName != "")
			eventReceiver = GameObject.Find (eventReceiverLookupName);

		base.Start ();
	}
		
}
