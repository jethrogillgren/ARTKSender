using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Tracks what side the player is on it, and reports to GameplayController.
public class TeleportTriggerGameplayObject : BaseGameplayObject {

	public GameObject m_playerCollider;
	public bool m_originalSide = true; //True means we are behind the BLue Forward line of the teleport to start, in physical space

	public GameplayController gameplayController;
	public PhysicalRoom originalRoom;
	public PhysicalRoom otherRoom; //If null, gameplaycontroller decides what is linked next

	// Use this for initialization
	void Start () {
	}

	void setRooms(PhysicalRoom originalRoom, PhysicalRoom otherRoom) {
		this.originalRoom = originalRoom;
		this.otherRoom = otherRoom;
	}


//	TODO - Does not handle well if user enters trigger form one side, but exits it on the same side!
//	void OnTriggerExit (Collider collision) {
//		if( gameObject.transform.position.z > collision.transform.position.z )
//		m_originalSide = !m_originalSide;

//	}

	void OnTriggerEnter (Collider collision) {
		
		if (m_playerCollider && collision.name == m_playerCollider.name) {
			Util.JLog ( gameObject.name + " has been triggered by " + collision.name);

			Util.JLog ("Collission Forward: " + collision.transform.forward);
			Util.JLog ("Teleport Forward  : " + gameObject.transform.forward);

			float angle = Vector3.Angle(collision.transform.forward, gameObject.transform.forward);
			Util.JLog ( "Angle: " + angle + "  We are on the " + (m_originalSide?"original":"other") + "side" );

			if( m_originalSide && angle < 90) {//Forward to Otherside
				Util.JLog ("Passing Forward ");
				gameplayController.teleportTriggered (this, originalRoom, otherRoom );

			} else if ( !m_originalSide && angle > 90 ) {//Forward to OriginalSide
				Util.JLog ("Returning Forward ");
				gameplayController.teleportTriggered (this,  otherRoom, originalRoom);

			} else if (m_originalSide) {//Backwards to Otherside
				Util.JLog ("Passing Backwards!");
				gameplayController.teleportTriggered (this, originalRoom, otherRoom, true );

			} else if (!m_originalSide) {//Backwards to OriginalSide
				Util.JLog ("Returning Backwards!");
				gameplayController.teleportTriggered (this,  otherRoom, originalRoom, true);

			}

			m_originalSide = !m_originalSide;
		}
	}
}