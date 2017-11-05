using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


// 1..n Physical Rooms which can parent 2..n GameplayRooms.  Provides access for triggers to make any switch in the configuration.
//The switches are local to each client.  All objects exist on server authoratively still, and are hidden using UNET visibility on clients.
public class GameplayController : NetworkBehaviour {

	public HashSet<BaseGameplayObject> m_gameplayObjects;
	public HashSet<PhysicalRoom> m_physicalRooms;
	public HashSet<GameplayRoom> m_gameplayRooms;
	public HashSet<TeleportDoorwayToggleTriggerGameplayObject> m_teleportTriggers;

	public PhysicalRoom mainRoom;

	public GameplayRoom earthRoom;
	public GameplayRoom woodRoom;
	public GameplayRoom metalRoom;
	public GameplayRoom fireRoom;
	public GameplayRoom waterRoom;


//	public GameplayRoom m_extraGameplayRoom;//TODO - hardcoding in a 2/3 Room Toggle Teleport... add in Mesh n room!
//	public PhysicalRoom m_currentPhysicalRoom; //TODO initialization?

	// Use this for initialization
	void Start () {
		collectGameplayObjects ();
		collectPhysicalRooms ();
		collectGameplayRooms ();
		collectTeleportTriggers ();

//		m_offscreenPhysicalRoom = new PhysicalRoom ();
//		m_offscreenPhysicalRoom.transform.position.x
	}


	//Util functions.  Assume a room is loaded already.
	public bool LoadEarthRoomInMainRoom () {
		return unActivate (mainRoom) && activate (earthRoom, mainRoom);
	}
	public bool LoadWoodRoomInMainRoom () {
		return unActivate (mainRoom) && activate (woodRoom, mainRoom);
	}
	public bool LoadMetalRoomInMainRoom () {
		return unActivate (mainRoom) && activate (metalRoom, mainRoom);
	}
	public bool LoadFireRoomInMainRoom () {
		return unActivate (mainRoom) && activate (fireRoom, mainRoom);
	}
	public bool LoadWaterRoomInMainRoom () {
		return unActivate (mainRoom) && activate (waterRoom, mainRoom);
	}



//	public void teleportTriggered (TeleportDoorwayToggleTriggerGameplayObject t, PhysicalRoom oldRoom, PhysicalRoom newRoom, bool backwards = false) {
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
//	}

	public bool replace(GameplayRoom oldGr, GameplayRoom newGr, PhysicalRoom pr) {
		if (oldGr.physicalRoom == pr) {
			return (unActivate (oldGr) && activate (newGr, pr));

		} else {
			Util.JLogErr ("Cannot replace " + oldGr.roomName + " with " + newGr.roomName + "  in " + pr.roomName );
			return false;
		}
	}

	//Hide whichever GameplayRoom was active in a physical room
	public bool unActivate(PhysicalRoom pr) {
		if( pr && pr.gameplayRoom ) { //If room is currently Active
			return unActivate (pr.gameplayRoom);

		} else {
			Util.JLogErr ( "Unable to Unactivate any GameplayRooms from: " + pr );
			return false;
		}
	}

	//Hide a GameplayRoom from whichever Physical Room it was in.  Done on Client
	public bool unActivate(GameplayRoom gr) {

		if( gr && gr.roomActive ) { //If room is currently Active
			if (gr.physicalRoom)
				gr.physicalRoom.gameplayRoom = null;
			gr.transform.SetParent(this.transform, false );
			gr.physicalRoom = null;
			gr.updateAllGameplayObjectsVisibility ();
			return true;

		} else {
			Util.JLogErr ( "Unable to Unactivate a GameplayRoom: " + gr );
			return false;
		}
	}

	//Show a GameplayRoom in a specified PhysicalRoom
	public bool activate(GameplayRoom gr, PhysicalRoom pr) {
		
		if( gr && !gr.roomActive && pr && pr.roomEmpty ) {
			gr.transform.SetParent(pr.transform);
			pr.gameplayRoom = gr;
			gr.updateAllGameplayObjectsVisibility ();
			gr.transform.localPosition = new Vector3 (0, 0, 0);

			Util.JLog ("GR lPos: " + gr.transform.localPosition + "     pr lpos: " + pr.transform.localPosition);
			return true;

		} else {
			Util.JLogErr ( "Unable to Activate a GameplayRoom " + gr + " into PhysicalRoom: " + pr );
			return false;
		}
	}


	//
	// DAO
	//


	public HashSet<BaseGameplayObject> getGameplayObjectsByState(BaseGameplayObject.GameplayState state) {
		HashSet<BaseGameplayObject> ret = new HashSet<BaseGameplayObject>();

		if (m_gameplayObjects == null || m_gameplayObjects.Count <= 0) {
			Util.JLog ("Asked to get Gameplay Objects by state, but there are " +  (m_gameplayObjects == null ? "NULL" : m_gameplayObjects.Count.ToString()) + " stored");
			collectGameplayObjects ();
		}

		foreach(BaseGameplayObject g in m_gameplayObjects) {
			if (g.gameplayState == state) {
				ret.Add (g);
			}
		}
		Util.JLog ( "Returning the " + ret.Count + "/" + m_gameplayObjects.Count + " Gameplay Objects that are " + state );
		return ret;
	}

	public HashSet<OcclusionGameplayObject> getOcclusionGameplayObjects() {
		HashSet<OcclusionGameplayObject> ret = new HashSet<OcclusionGameplayObject>();
		foreach (BaseGameplayObject g in m_gameplayObjects) {
			if ( g.GetType() == typeof(OcclusionGameplayObject) )
				ret.Add (g as OcclusionGameplayObject);
		}

		Util.JLog ( "Returning the " + ret.Count + "/" + m_gameplayObjects.Count + " Gameplay Objects that are Occluders (active or Inactive)" );

//		foreach (OcclusionGameplayObject g in m_gameplayObjects)
//			Util.JLog ("TEST:  " + g.name + " " + g.GetType() + " " + g.ToString() );


		return ret;
	}

	//Tries to add the gameplay object, returning true if it was newly added and false if alreadye existed
	public bool addGameplayObject(BaseGameplayObject o) {
		if ( m_gameplayObjects.Add (o) ) {
			Util.JLog("Added new Gameplay Object: " + o.name + " as a  " + o.GetType () );
			return true;
		}
		return false;
	}

	public Component getRoomByName(string roomName) {
		foreach (GameplayRoom gr in m_gameplayRooms)
			if (gr.roomName == roomName)
				return gr;
		foreach (PhysicalRoom pr in m_physicalRooms)
			if (pr.roomName == roomName)
				return pr;
		return null;
	}


	//Collecting Objects
	public void collectGameplayObjects() {
		collectHashSetOfComponents<BaseGameplayObject> (ref m_gameplayObjects);
	}
	public void collectPhysicalRooms() {
		collectHashSetOfComponents<PhysicalRoom> (ref m_physicalRooms);

	}
	public void collectGameplayRooms() {
		collectHashSetOfComponents<GameplayRoom> (ref m_gameplayRooms);
	}
	public void collectTeleportTriggers() {
		collectHashSetOfComponents<TeleportDoorwayToggleTriggerGameplayObject> (ref m_teleportTriggers);
	}

	public void collectHashSetOfComponents<T>( ref HashSet<T> setToFill ) {
		if (setToFill == null)
			setToFill = new HashSet<T>();
		else
			setToFill.Clear ();

		T[] prs = FindObjectsOfType( typeof(T) ) as T[];
		foreach (T pr in prs) {
			setToFill.Add (pr);
		}
	}
}
