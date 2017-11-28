using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Room Controller.  Knows all about Physical and Gameplay rooms.
//That includes transfer between them (Teleports)
//Provides itineries of all rooms.
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
		foreach (GameplayRoom gr in m_gameplayRooms)
			{

				if (!gr.enabled)
					{
						gr.gameObject.SetActive(true);
						Debug.Log("I Just set Room: " + gr.roomName + " enabled: " + gr.enabled);
					}

				//Servers have all scenes enabled and seperate cameras.  Clients only enable the current Phys/GPRooms
				gr.RegisterAnyParentPhysicalRoom();
				gr.UpdateAllGameplayObjectsVisibility();
				gr.SetAppropiateLayers();

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
	public bool LoadEarthRoomInMainRoom()
	{
		return isClient && unActivate(mainRoom) && activate(earthRoom, mainRoom);
	}

	public bool LoadWoodRoomInMainRoom()
	{
		return isClient && unActivate(mainRoom) && activate(woodRoom, mainRoom);
	}

	public bool LoadMetalRoomInMainRoom()
	{
		return isClient && unActivate(mainRoom) && activate(metalRoom, mainRoom);
	}

	public bool LoadFireRoomInMainRoom()
	{
		return isClient && unActivate(mainRoom) && activate(fireRoom, mainRoom);
	}

	public bool LoadWaterRoomInMainRoom()
	{
		return isClient && unActivate(mainRoom) && activate(waterRoom, mainRoom);
	}

	public bool LoadRoomInMainRoom(GameplayRoom g)
	{
		return isClient && unActivate(mainRoom) && activate(g, mainRoom);
	}



	//Used for the Three Room Door Teleporter
	public void doorSwitchTeleportTriggered (ThreeRoomDoorTeleportGameplayObject t, PhysicalRoom oldRoom, PhysicalRoom newRoom, bool backwards = false) {
		//TODO needs updating to know about which GameplayRooms are involved
		//		if(backwards) {
//			//Do nothing otherwise the jump would be watched by the player...
//
//		} else {
//			//Travelling fowards
//			GameplayRoom oldGameplayRoom = oldRoom.gameplayRoom;
//
//			Util.JLog ("Replacing " + oldGameplayRoom + " with " + m_extraGameplayRoom.roomName + " (In " + oldRoom.roomName + ")" );
//			AndroidHelper.ShowAndroidToastMessage ("TELEPORT:  Replacing " + oldGameplayRoom + " with " + m_extraGameplayRoom.roomName + " (In " + oldRoom.roomName + ")" );
//
//			replace (oldGameplayRoom, m_extraGameplayRoom, oldRoom);
////			unActivate (oldRoom.gameplayRoom);
////			activate (new GameplayRoom(), oldRoom);
//			m_extraGameplayRoom = oldGameplayRoom;
//			m_currentPhysicalRoom = newRoom;
//		}
	}

	public bool replace(GameplayRoom oldGr, GameplayRoom newGr, PhysicalRoom pr)
	{
		if (oldGr.physicalRoom == pr)
			{
				return (unActivate(oldGr) && activate(newGr, pr));

			} else
			{
				Util.JLogErr("Cannot replace " + oldGr.roomName + " with " + newGr.roomName + "  in " + pr.roomName);
				return false;
			}
	}

	//Hide whichever GameplayRoom was active in a physical room
	public bool unActivate(PhysicalRoom pr)
	{
		if (pr && pr.gameplayRoom)
			{ //If room is currently Active
				return unActivate(pr.gameplayRoom);

			} else
			{
				Util.JLogErr("Unable to Unactivate any GameplayRooms from: " + pr);
				return false;
			}
	}

	//Hide a GameplayRoom from whichever Physical Room it was in.  Done on Client
	public bool unActivate(GameplayRoom gr)
	{

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
				Util.JLogErr("Unable to Unactivate a GameplayRoom: " + gr);
				return false;
			}
	}

	//Show a GameplayRoom in a specified PhysicalRoom.
	public bool activate(GameplayRoom gr, PhysicalRoom pr)
	{
        
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
				Util.JLogErr("Unable to Activate a GameplayRoom " + gr + " into PhysicalRoom: " + pr);
				return false;
			}
	}


	//
	// DAO
	//


	public HashSet<BaseGameplayObject> getGameplayObjectsByState(BaseGameplayObject.GameplayState state)
	{
		HashSet<BaseGameplayObject> ret = new HashSet<BaseGameplayObject>();

		if (m_gameplayObjects == null || m_gameplayObjects.Count <= 0)
			{
				Util.JLog("Asked to get Gameplay Objects by state, but there are " + (m_gameplayObjects == null ? "NULL" : m_gameplayObjects.Count.ToString()) + " stored");
				Util.collectGameplayObjects(ref m_gameplayObjects);
			}

		foreach (BaseGameplayObject g in m_gameplayObjects)
			{
				if (g.gameplayState == state)
					{
						ret.Add(g);
					}
			}
		Util.JLog("Returning the " + ret.Count + "/" + m_gameplayObjects.Count + " Gameplay Objects that are " + state);
		return ret;
	}

	public HashSet<OcclusionGameplayObject> getOcclusionGameplayObjects()
	{
		HashSet<OcclusionGameplayObject> ret = new HashSet<OcclusionGameplayObject>();
		foreach (BaseGameplayObject g in m_gameplayObjects)
			{
				if (g.GetType() == typeof(OcclusionGameplayObject))
					ret.Add(g as OcclusionGameplayObject);
			}

		Util.JLog("Returning the " + ret.Count + "/" + m_gameplayObjects.Count + " Gameplay Objects that are Occluders (active or Inactive)");

//		foreach (OcclusionGameplayObject g in m_gameplayObjects)
//			Util.JLog ("TEST:  " + g.name + " " + g.GetType() + " " + g.ToString() );


		return ret;
	}

	//Tries to add the gameplay object, returning true if it was newly added and false if alreadye existed
	public bool addGameplayObject(BaseGameplayObject o)
	{
		if (m_gameplayObjects.Add(o))
			{
				Util.JLog("Added new Gameplay Object: " + o.name + " as a  " + o.GetType());
				return true;
			}
		return false;
	}

	public Component getRoomByName(string roomName)
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
