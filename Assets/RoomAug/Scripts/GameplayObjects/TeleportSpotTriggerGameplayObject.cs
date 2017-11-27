using UnityEngine;
using System;

//Not tied to any room - it sits directly under the physical room.
public class TeleportSpotTriggerGameplayObject : BaseTeleportGameplayObject {

	// Use this for initialization
    public override void Start() {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //Client
    public override void OnTriggerEnter( Collider collision ) {
        
		Util.JLog ( name + " TriggerEnter: " + collision.name );

		//Client handles player teleports as it affects their own object mainly.
		//Server can still track it OK.
		if (isClient && teleportOpen)
		{
			RoomAugPlayerController player = FindObjectOfType<RoomAugPlayerController> ();

			//Player Teleporting
			if (collision.name == player.GetComponent<Collider> ().name)
			{
				Util.JLog ( name + " Triggering teleport for " + collision.name );
				Trigger ();
			} else {
				Util.JLog ( name + " Skipping Teleport as " + collision.name + " != " + player.GetComponent<Collider> ().name );
			}


		//Servers handle the Cube teleporting as that is gobally scoped
		}
		else if ( !isClient && teleportOpen )
		{
			//Check if it was actually a cube
			PandaCubeGameplayObject cube = collision.GetComponent<PandaCubeGameplayObject>();
			if( cube ) {
				//TODO Animate Portal
				Util.JLog ( "Teleporting Cube " + cube.name + " to " + targetGameplayRoom.roomName );
				cube.TeleportTo (targetGameplayRoom);
			}
		}
    }

	public override void AnimateOpening() {
		//TODO
	}

	public override void Trigger() {
		//TODO animate portal
		roomController.LoadRoomInMainRoom(targetGameplayRoom);
	}

}