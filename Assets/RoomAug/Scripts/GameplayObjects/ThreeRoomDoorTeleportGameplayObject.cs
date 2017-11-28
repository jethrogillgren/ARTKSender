using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A teleporter which is the form of a plane (think a door) and there are 3 rooms which switch behind the player invisibly.
public class ThreeRoomDoorTeleportGameplayObject : BaseTeleportGameplayObject
{

	public GameObject m_playerCollider;
	public bool m_originalSide = true;
	//True means we are behind the BLue Forward line of the teleport to start, in physical space

	public PhysicalRoom originalRoom;
	public PhysicalRoom otherRoom;
	//If null, gameplaycontroller decides what is linked next

	// Use this for initialization
	public override void Start ()
	{
		base.Start ();
	}

	void setRooms ( PhysicalRoom originalRoom, PhysicalRoom otherRoom )
	{
		this.originalRoom = originalRoom;
		this.otherRoom = otherRoom;
	}


	//	TODO - Does not handle well if user enters trigger from one side, but exits it on the same side!
	//	void OnTriggerExit (Collider collision) {
	//		if( gameObject.transform.position.z > collision.transform.position.z )
	//		m_originalSide = !m_originalSide;

	//	}

	public override void OnTriggerEnter ( Collider collision )
	{
		if (isClient)
		{
			RoomAugPlayerController player = FindObjectOfType<RoomAugPlayerController> ();
			if (collision.name == player.GetComponent<Collider> ().name)
			{


				Util.JLog ( gameObject.name + " has been triggered by " + collision.name );

				Util.JLog ( "Collission Forward: " + collision.transform.forward );
				Util.JLog ( "Teleport Forward  : " + gameObject.transform.forward );

				float angle = Vector3.Angle ( collision.transform.forward, gameObject.transform.forward );
				Util.JLog ( "Angle: " + angle + "  We are on the " + ( m_originalSide ? "original" : "other" ) + "side" );

				if (m_originalSide && angle < 90) //Forward to Otherside
				{
					Trigger ();

				}
				else if (!m_originalSide && angle > 90) //Forward to OriginalSide
				{
					Trigger ();

				}
				else if (m_originalSide) //Backwards to Otherside
				{
					Util.JLog ( "Passing Backwards!" );
					roomController.doorSwitchTeleportTriggered ( this, originalRoom, otherRoom, true );

				}
				else if (!m_originalSide) //Backwards to OriginalSide
				{
					Util.JLog ( "Returning Backwards!" );
					roomController.doorSwitchTeleportTriggered ( this, otherRoom, originalRoom, true );

				}

				m_originalSide = !m_originalSide;
			}
		}
	}

	public override void AnimateOpening ()
	{
		//TODO
	}

	//Assumes a forward pass.  
	public override void Trigger ()
	{
		if (m_originalSide)
		{//Assume Forward to Otherside
			Util.JLog ( "Passing Forward " );
			roomController.doorSwitchTeleportTriggered ( this, originalRoom, otherRoom );

		}
		else if (!m_originalSide)
		{//Assume Forward to OriginalSide
			Util.JLog ( "Returning Forward " );
			roomController.doorSwitchTeleportTriggered ( this, otherRoom, originalRoom );

		}
	}
}