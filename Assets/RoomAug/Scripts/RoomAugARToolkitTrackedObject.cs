using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAugARToolkitTrackedObject : ARTrackedObject {

	public string eventReceiverLookupName = "";

	// Use this for initialization
	protected override void Start () {
		LogTag = "RoomAugARToolkitTrackedObject: ";
		Invoke ( "FindEventReciever", 2 );

		base.Start ();
	}


	public void FindEventReciever()
	{
		if (eventReceiverLookupName != "")
			eventReceiver = GameObject.Find ( eventReceiverLookupName );

		if (eventReceiver)
			Debug.Log (name + " found Event Reciever: " + eventReceiver.name );
		else
			Debug.Log ( name + " unable to Find Event Reciever by name: " + eventReceiverLookupName );
		
	}
		
}
