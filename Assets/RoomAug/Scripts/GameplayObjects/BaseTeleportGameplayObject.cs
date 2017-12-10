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

	public virtual void SetTeleportOpen(bool open = true) {
		teleportOpen = open;
		AnimateOpening ();
	}

	public abstract void AnimateOpening();

//	public abstract void OnTriggerEnter ( Collider collision );//Handle usual teleport collission with player trigger
	public abstract void Trigger();//Can be called to force a teleport
}