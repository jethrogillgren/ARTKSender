using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SKStudios.Portals;

//SKStudios integrated Doorway Portal.
//One for each side
public class PortalDoorwayTeleportGameplayObject : BaseTeleportGameplayObject {

	SKStudios.Portals.Portal portal;

	public Material portalOpenPortalMaterial;

	public override void Start()
	{
		base.Start ();

		portal = GetComponentInChildren<SKStudios.Portals.Portal> ();
		if (!portal)
			Debug.LogWarning (name + " did not find a Portal Object in its children");
	}

	public override void SetTeleportOpen(bool open = true) {
		base.SetTeleportOpen (); //teleportOpen bool and calls AnimateOpening()
		portal.Enterable = true;
	}

	//Graphical animation showing the Portal is now enterable
	public override void AnimateOpening() {
		portal.PortalMaterial = portalOpenPortalMaterial;
	}

	//Do the Teleport.  Clients Only
	public override void Trigger() {//Can be called to force a teleport
		if( teleportOpen && portal && isClient )
			roomController.LoadRoomInMainRoom(targetGameplayRoom);
	}

}