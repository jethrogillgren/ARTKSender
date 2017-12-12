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
    public void OnTriggerEnter( Collider collision ) {
        
		//Client handles player teleports as it affects their own object mainly.
		//Server can still track it OK.
		if (isClient && teleportOpen)
		{
			RoomAugPlayerController player = FindObjectOfType<RoomAugPlayerController> ();

			//Player Teleporting
			if (collision.name == player.GetComponent<Collider> ().name)
			{
				Debug.Log ( name + " Triggering teleport for " + collision.name );
				Trigger ();
			} else {
				Debug.Log ( name + " Skipping Teleport as " + collision.name + " != " + player.GetComponent<Collider> ().name );
			}


		//Servers handle the Cube teleporting as that is gobally scoped
		}
		else if ( !isClient && teleportOpen )
		{
			//Check if it was actually a cube
			PandaCubeGameplayObject cube = collision.GetComponent<PandaCubeGameplayObject>();
			if( cube ) {
				//TODO Animate Portal
				Debug.Log ( "Teleporting Cube " + cube.name + " to " + targetGameplayRoom.roomName );
				cube.Svr_TeleportTo (targetGameplayRoom);
			}
		}
    }

	public override void AnimateOpening(bool altSide = false) {
		//TODO
	}

	public override void Trigger(bool altSide = false) {
		//TODO animate portal
		roomController.Cnt_LoadRoomInMainRoom(targetGameplayRoom);
	}

}