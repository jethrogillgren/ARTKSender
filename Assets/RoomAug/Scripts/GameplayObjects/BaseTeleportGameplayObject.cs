using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Teleports often exist outside of any GameplayRoom (but under a Physicalroom)
//They handle collision with the player and call the RoomController
public abstract class BaseTeleportGameplayObject : BaseGameplayObject {

	public GameplayRoom targetGameplayRoom;

	protected RoomController roomController;
	protected PhysicalRoom physicalRoom; //Not swappable

	public bool teleportOpen = false; //True if players can currently all use this teleport

	// Use this for initialization
	public override void Start() {
		base.Start();

		roomController = FindObjectOfType<RoomController>();
		physicalRoom = GetComponentInParent<PhysicalRoom>();
	}

	public virtual GameplayRoom GetTargetGameplayRoom(bool altSide = false)
	{
		if (altSide)
		{
			Debug.LogError ( "Alt Side of Teleport not configured!" );
			return null;
		}
		else
		{
			return targetGameplayRoom;
		}
	}

	//TODO - if open == false...
	public virtual void SetTeleportOpen(bool open = true, bool altSide = false) {
		teleportOpen = open;
		AnimateOpening ();
	}

	public abstract void AnimateOpening(bool altSide = false);

//	public abstract void OnTriggerEnter ( Collider collision );//Handle usual teleport collission with player trigger
	public abstract void Trigger(bool altSide = false);//Can be called to force a teleport

}