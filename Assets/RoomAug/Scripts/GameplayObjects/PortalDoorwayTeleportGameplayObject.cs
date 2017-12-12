using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SKStudios.Portals;

//SKStudios integrated Doorway Portal.
//One for each side
//Requires the two SKPortals objects to be named SKPortalA and SKPortalB
public class PortalDoorwayTeleportGameplayObject : BaseTeleportGameplayObject {

	//Add in the extra B side.   A is in the Base Class
	public GameplayRoom targetGameplayRoomB;
	public bool teleportOpenB = false; //True if players can currently all use this teleport

	//Links to the SKStudios code
	SKStudios.Portals.PortalController portalController;
	SKStudios.Portals.PortalController portalControllerB;

	public Material portalOpenPortalMaterial;
	public Material portalOpenPortalMaterialB;


	public override void Start()
	{
		base.Start ();

		foreach(SKStudios.Portals.PortalController pc in GetComponentsInChildren<SKStudios.Portals.PortalController>())
		{
			if (pc.name == "SKPortalA")
				portalController = pc;
			else if (pc.name == "SKPortalB")
				portalControllerB = pc;
			else
				Debug.LogError ("Invalid Name for PortalController: " + pc);
		}

//		portalController = (SKStudios.Portals.PortalController) GameObject.Find ( "SKPortalA" );
		if (!portalController)
			Debug.LogWarning (name + " did not find SKPortalA in its children");
//		portalControllerB = GameObject.Find ( "SKPortalB" );
		if (!portalControllerB)
			Debug.LogWarning (name + " did not find SKPortalB in its children");

		foreach( Camera cam in portalController.GetComponentsInChildren<Camera>() ) //TODO Get by name as this list doesn't change
			SetCullingMask (cam);
		foreach( Camera cam in portalControllerB.GetComponentsInChildren<Camera>() ) //TODO Get by name as this list doesn't change
			SetCullingMask (cam, true);
	}

	public override void SetTeleportOpen(bool open = true, bool altSide = false) {
		if (altSide)
		{
			SetTeleportOpenB ();
			return;
		}
		
		base.SetTeleportOpen (); //teleportOpen bool and calls AnimateOpening()
		portalController.PortalScript.Enterable = true;
	}
	public void SetTeleportOpenB(bool openB = true) {
		base.SetTeleportOpen (); //teleportOpen bool and calls AnimateOpening()
		portalControllerB.PortalScript.Enterable = true;
	}

	//Graphical animation showing the Portal is now enterable
	public override void AnimateOpening(bool altSide = false) {
		if (altSide){
			AnimateOpeningB ();
			return;
		}
		
		portalController.PortalScript.PortalMaterial = portalOpenPortalMaterial;
	}
	//Graphical animation showing the Portal is now enterable
	public void AnimateOpeningB() {
		portalControllerB.PortalScript.PortalMaterial = portalOpenPortalMaterialB;
	}

	//Do the Teleport.  Clients Only
	public override void Trigger(bool altSide = false) {//Can be called to force a teleport
		if (altSide){
			TriggerB ();
			return;
		}
		
		if( teleportOpen && portalController && isClient )
			roomController.SwapRoomInMainRoom(targetGameplayRoom);
	}
	public void TriggerB() {//Can be called to force a teleport
		if( teleportOpenB && portalControllerB && isClient )
			roomController.SwapRoomInMainRoom(targetGameplayRoomB);
	}
		

	protected void SetCullingMask(Camera cam, bool altSide = false)
	{
		
		cam.cullingMask = (1 << LayerMask.NameToLayer("Default"))
						| (1 << LayerMask.NameToLayer("Occlusion"))
						| (1 << LayerMask.NameToLayer("Portal"))
				| (1 << LayerMask.NameToLayer("Animal"));

		if (!altSide)
			cam.cullingMask = cam.cullingMask | (1 << LayerMask.NameToLayer(targetGameplayRoom.roomName));
		else if (altSide)
			cam.cullingMask = cam.cullingMask | (1 << LayerMask.NameToLayer(targetGameplayRoomB.roomName));
		
	}

}