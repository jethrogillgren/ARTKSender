using UnityEngine;
using System;

//Not tied to any room - it sits directly under the physical room.
public class TeleportSpotTriggerGameplayObject : BaseGameplayObject {

    public GameplayRoom targetGameplayRoom;

    //private GameObject m_playerCollider;
    private RoomController roomController;
    private PhysicalRoom physicalRoom;

	// Use this for initialization
    public override void Start() {
        base.Start();

        roomController = FindObjectOfType<RoomController>();
        physicalRoom = GetComponentInParent<PhysicalRoom>();

        if( !targetGameplayRoom) {
            throw new Exception(name + " MUST SPECIFY TARGET TELEPORT");
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //Client
    void OnTriggerEnter( Collider collision ) {
        
        if( isClient ) {
            RoomAugPlayerController player = FindObjectOfType<RoomAugPlayerController>();
            if( collision.name == player.GetComponent<Collider>().name ) {
                Util.JLog( name + " Triggering teleport for " + collision.name );
                roomController.LoadRoomInMainRoom(targetGameplayRoom);
            }

        } else {
            Util.JLogErr("Server ignoring collission for " + name + " and " + collision.name );
        }
    }
}
