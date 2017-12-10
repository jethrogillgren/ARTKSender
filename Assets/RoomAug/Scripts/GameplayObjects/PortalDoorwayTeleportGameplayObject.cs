using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SKStudios.Portals;

//SKStudios integrated Doorway Portal.
//One for each side
public class PortalDoorwayTeleportGameplayObject : BaseTeleportGameplayObject {

	SKStudios.Portals.PortalController portalController;

	public Material portalOpenPortalMaterial;

	public override void Start()
	{
		base.Start ();

		portalController = GetComponentInChildren<SKStudios.Portals.PortalController> ();
		if (!portalController)
			Debug.LogWarning (name + " did not find a Portal Object in its children");

		foreach( Camera cam in GetComponentsInChildren<Camera>() ) //TODO Get by name as this list doesn't change
			SetCullingMask (cam);
	}

	public override void SetTeleportOpen(bool open = true) {
		base.SetTeleportOpen (); //teleportOpen bool and calls AnimateOpening()
		portalController.PortalScript.Enterable = true;
	}

	//Graphical animation showing the Portal is now enterable
	public override void AnimateOpening() {
		portalController.PortalScript.PortalMaterial = portalOpenPortalMaterial;
	}

	//Do the Teleport.  Clients Only
	public override void Trigger() {//Can be called to force a teleport
		if( teleportOpen && portalController && isClient )
			roomController.LoadRoomInMainRoom(targetGameplayRoom);
	}
		

	protected void SetCullingMask(Camera cam)
	{
		cam.cullingMask = (1 << LayerMask.NameToLayer("Default"))
						| (1 << LayerMask.NameToLayer("Occlusion"))
						| (1 << LayerMask.NameToLayer("Portal"))
						| (1 << LayerMask.NameToLayer("Animal"))
						| (1 << LayerMask.NameToLayer(targetGameplayRoom.roomName));
	}

}