using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Room Controller.  Knows all about Physical and Gameplay rooms.
//That includes transfer between them (Teleports)
//Provides itineries of all rooms.
//Knows positions of eg ADF/Scan zero, Cameras, etc..
public class RoomController : NetworkBehaviour
{

	public HashSet<BaseGameplayObject> m_gameplayObjects;
	public HashSet<PhysicalRoom> m_physicalRooms;
	public HashSet<GameplayRoom> m_gameplayRooms;
	public HashSet<BaseTeleportGameplayObject> m_teleportTriggers;

	public PhysicalRoom mainRoom;

	public GameplayRoom earthRoom;
	public GameplayRoom woodRoom;
	public GameplayRoom metalRoom;
	public GameplayRoom fireRoom;
	public GameplayRoom waterRoom;

	public Transform trackingZeroPosition; //Where the ADF and 3DR scans call home
	public Transform camera1ZeroPosition; //Where a Room Webcam is
	public Transform camera2ZeroPosition; //Where a Room Webcam is

	private bool firstUpdate = true;


	//	public GameplayRoom m_extraGameplayRoom;//TODO - hardcoding in a 2/3 Room Toggle Teleport... add in Mesh n room!
	//	public PhysicalRoom m_currentPhysicalRoom; //TODO initialization?

	// Use this for initialization
	void Start()
	{
		
		Util.collectGameplayObjects(ref m_gameplayObjects);
		collectPhysicalRooms();
		collectGameplayRooms();
		collectTeleportTriggers();

		//If we disabled while editing, undo that.  Then make them all appropiately visible for the start
		foreach ( GameplayRoom gr in m_gameplayRooms )
		{

			if (!gr.enabled)
			{
				gr.gameObject.SetActive ( true );
				Debug.Log ( "I Just set Room: " + gr.roomName + " enabled: " + gr.enabled );
			}

			//Servers have all scenes enabled and seperate cameras.  Clients only enable the current Phys/GPRooms
			gr.RegisterAnyParentPhysicalRoom ();
			gr.UpdateAllGameplayObjectsVisibility ();
			gr.SetAppropiateLayers ();

		}
	}

	public void Update()
	{

		if (firstUpdate)
		{
			foreach (GameplayRoom gr in m_gameplayRooms)
			{
				gr.RegisterAnyParentPhysicalRoom();
				gr.UpdateAllGameplayObjectsVisibility();
				gr.SetAppropiateLayers();
			}
			firstUpdate = false;
		}
	}



	//Util Teleport functions.  Assume a room is loaded already.
	public bool Cnt_LoadEarthRoomInMainRoom()
	{
		return isClient && Cnt_UnActivate(mainRoom) && Cnt_Activate(earthRoom, mainRoom);
	}

	public bool Cnt_LoadWoodRoomInMainRoom()
	{
		return isClient && Cnt_UnActivate(mainRoom) && Cnt_Activate(woodRoom, mainRoom);
	}

	public bool Cnt_LoadMetalRoomInMainRoom()
	{
		return isClient && Cnt_UnActivate(mainRoom) && Cnt_Activate(metalRoom, mainRoom);
	}

	public bool Cnt_LoadFireRoomInMainRoom()
	{
		return isClient && Cnt_UnActivate(mainRoom) && Cnt_Activate(fireRoom, mainRoom);
	}

	public bool Cnt_LoadWaterRoomInMainRoom()
	{
		return isClient && Cnt_UnActivate(mainRoom) && Cnt_Activate(waterRoom, mainRoom);
	}

	public bool Cnt_LoadRoomInMainRoom(GameplayRoom g)
	{
		return isClient && Cnt_UnActivate(mainRoom) && Cnt_Activate(g, mainRoom);
	}


	//Used for all SKPortal Teleports.  Called when SKPortal tells us a valid teleport is happening.
	//SK's portal teleporting is disabled, as we move the rooms instead.
	public void OnSKPortalTeleport(SKStudios.Portals.Portal portal, SKStudios.Portals.Teleportable movingObject)
	{
		BaseTeleportGameplayObject teleportGPO = portal.GetComponentInParent<BaseTeleportGameplayObject> ();
		if( teleportGPO  &&  movingObject == SKStudios.Portals.GlobalPortalSettings.PlayerTeleportable)
		{
			if (portal.gameObject.name == "SKPortalA")
				teleportGPO.Trigger ();
			else if (portal.gameObject.name == "SKPortalB")
				teleportGPO.Trigger ( true );
			else
				Debug.LogError ("Not teleporting as " + portal.gameObject.name + " is not SKPortalA/B");
		}
		else
		{
			Debug.LogError ("Not teleporting " + movingObject.name + " as it does not have a Parent BaseTeleportGameplayObject");
		}
	}

	//Used for the Three Room Door Teleporter
	public void Cnt_DoorSwitchTeleportTriggered (ThreeRoomDoorTeleportGameplayObject t, PhysicalRoom oldRoom, PhysicalRoom newRoom, bool backwards = false) {
		//TODO needs updating to know about which GameplayRooms are involved
		if (!isClient)
			return;
		
		//		if(backwards) {
//			//Do nothing otherwise the jump would be watched by the player...
//
//		} else {
//			//Travelling fowards
//			GameplayRoom oldGameplayRoom = oldRoom.gameplayRoom;
//
//			Debug.Log ("Replacing " + oldGameplayRoom + " with " + m_extraGameplayRoom.roomName + " (In " + oldRoom.roomName + ")" );
//			AndroidHelper.ShowAndroidToastMessage ("TELEPORT:  Replacing " + oldGameplayRoom + " with " + m_extraGameplayRoom.roomName + " (In " + oldRoom.roomName + ")" );
//
//			replace (oldGameplayRoom, m_extraGameplayRoom, oldRoom);
////			unActivate (oldRoom.gameplayRoom);
////			activate (new GameplayRoom(), oldRoom);
//			m_extraGameplayRoom = oldGameplayRoom;
//			m_currentPhysicalRoom = newRoom;
//		}
	}

	public bool Cnt_Replace(GameplayRoom oldGr, GameplayRoom newGr, PhysicalRoom pr)
	{
		if (!isClient)
			return false;
		
		if (oldGr.physicalRoom == pr)
		{
			return (Cnt_UnActivate(oldGr) && Cnt_Activate(newGr, pr));

		} else
		{
			Debug.LogError("Cannot replace " + oldGr.roomName + " with " + newGr.roomName + "  in " + pr.roomName);
			return false;
		}
	}

	//Hide whichever GameplayRoom was active in a physical room
	public bool Cnt_UnActivate(PhysicalRoom pr)
	{
		if (!isClient)
			return false;
		
		if (pr && pr.gameplayRoom)
		{ //If room is currently Active
			return Cnt_UnActivate(pr.gameplayRoom);

		} else
		{
			Debug.LogError("Unable to Unactivate any GameplayRooms from: " + pr);
			return false;
		}
	}

	//Hide a GameplayRoom from whichever Physical Room it was in.  Done on Client
	public bool Cnt_UnActivate(GameplayRoom gr)
	{
		if (!isClient)
			return false;
		
		if (gr && gr.roomActive)
			{ //If room is currently Active
				if (gr.physicalRoom)
					gr.physicalRoom.gameplayRoom = null;
				gr.transform.SetParent(this.transform, false);

				gr.physicalRoom = null;
				gr.UpdateAll();

//			gr.UpdateAll ();
				return true;

			} else
			{
				Debug.LogError("Unable to Unactivate a GameplayRoom: " + gr);
				return false;
			}
	}

	//Show a GameplayRoom in a specified PhysicalRoom.
	public bool Cnt_Activate(GameplayRoom gr, PhysicalRoom pr)
	{
		if (!isClient)
			return false;
        
		if (gr && !gr.roomActive && pr && pr.roomEmpty)
			{
				gr.transform.SetParent(pr.transform);
				gr.transform.localPosition = new Vector3(0, 0, 0);//TODO shouldn't be needed?
				pr.gameplayRoom = gr;

				gr.UpdateAll();

//			gr.UpdateAll();

				Handheld.Vibrate();
				return true;

			} else
			{
				Debug.LogError("Unable to Activate a GameplayRoom " + gr + " into PhysicalRoom: " + pr);
				return false;
			}
	}



	public HashSet<BaseGameplayObject> GetGameplayObjectsByState(BaseGameplayObject.GameplayState state)
	{
		HashSet<BaseGameplayObject> ret = new HashSet<BaseGameplayObject>();

		if (m_gameplayObjects == null || m_gameplayObjects.Count <= 0)
			{
				Debug.Log("Asked to get Gameplay Objects by state, but there are " + (m_gameplayObjects == null ? "NULL" : m_gameplayObjects.Count.ToString()) + " stored");
				Util.collectGameplayObjects(ref m_gameplayObjects);
			}

		foreach (BaseGameplayObject g in m_gameplayObjects)
			{
				if (g.gameplayState == state)
					{
						ret.Add(g);
					}
			}
		Debug.Log("Returning the " + ret.Count + "/" + m_gameplayObjects.Count + " Gameplay Objects that are " + state);
		return ret;
	}

	public HashSet<OcclusionGameplayObject> GetOcclusionGameplayObjects()
	{
		HashSet<OcclusionGameplayObject> ret = new HashSet<OcclusionGameplayObject>();
		foreach (BaseGameplayObject g in m_gameplayObjects)
			{
				if (g.GetType() == typeof(OcclusionGameplayObject))
					ret.Add(g as OcclusionGameplayObject);
			}

		Debug.Log("Returning the " + ret.Count + "/" + m_gameplayObjects.Count + " Gameplay Objects that are Occluders (active or Inactive)");

//		foreach (OcclusionGameplayObject g in m_gameplayObjects)
//			Debug.Log ("TEST:  " + g.name + " " + g.GetType() + " " + g.ToString() );


		return ret;
	}

	//Tries to add the gameplay object, returning true if it was newly added and false if alreadye existed
	public bool AddGameplayObject(BaseGameplayObject o)
	{
		if (m_gameplayObjects.Add(o))
			{
				Debug.Log("Added new Gameplay Object: " + o.name + " as a  " + o.GetType());
				return true;
			}
		return false;
	}

	public GameplayRoom GetGameplayRoomByName(string roomName)
	{
		foreach (GameplayRoom gr in m_gameplayRooms)
			if (gr.roomName == roomName)
				return gr;

		return null;
	}
	public PhysicalRoom GetPhysicalRoomByName(string roomName)
	{
		foreach (PhysicalRoom pr in m_physicalRooms)
			if (pr.roomName == roomName)
				return pr;
		return null;
	}
	public Component GetRoomByName(string roomName)
	{
		foreach (GameplayRoom gr in m_gameplayRooms)
			if (gr.roomName == roomName)
				return gr;
		foreach (PhysicalRoom pr in m_physicalRooms)
			if (pr.roomName == roomName)
				return pr;
		return null;
	}


	//Collecting Objects
	public void collectPhysicalRooms()
	{
		Util.collectHashSetOfComponents<PhysicalRoom>(ref m_physicalRooms, true);
	}

	public void collectGameplayRooms()
	{
		Util.collectHashSetOfComponents<GameplayRoom>(ref m_gameplayRooms, true);
	}

	public void collectTeleportTriggers()
	{
		Util.collectHashSetOfComponents<BaseTeleportGameplayObject>(ref m_teleportTriggers);
	}


}
