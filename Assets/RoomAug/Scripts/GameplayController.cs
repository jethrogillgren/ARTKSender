using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : MonoBehaviour {

	public HashSet<BaseGameplayObject> m_gameplayObjects;
	public HashSet<PhysicalRoom> m_physicalRooms;
	public HashSet<TeleportTriggerGameplayObject> m_teleportTriggers;

	public GameplayRoom m_extraGameplayRoom;//TODO - hardcoding in a 2/3 Room Toggle Teleport... add in Mesh n room!
	public PhysicalRoom m_currentPhysicalRoom; //TODO initialization?

	// Use this for initialization
	void Start () {
		collectGameplayObjects ();
		collectPhysicalRooms ();

//		m_offscreenPhysicalRoom = new PhysicalRoom ();
//		m_offscreenPhysicalRoom.transform.position.x
	}




	public void teleportTriggered (TeleportTriggerGameplayObject t, PhysicalRoom oldRoom, PhysicalRoom newRoom, bool backwards = false) {
		if(backwards) {
			//Do nothing otherwise the jump would be watched by the player...

		} else {
			//Travelling fowards
			Util.JLog ("Replacing " + oldRoom.roomName + " with " + m_extraGameplayRoom.roomName );
			GameplayRoom oldGameplayRoom = oldRoom.gameplayRoom;
			replace (oldGameplayRoom, m_extraGameplayRoom, oldRoom);
//			unActivate (oldRoom.gameplayRoom);
//			activate (new GameplayRoom(), oldRoom);
			m_extraGameplayRoom = oldGameplayRoom;
			m_currentPhysicalRoom = newRoom;
		}
	}

	private bool replace(GameplayRoom oldGr, GameplayRoom newGr, PhysicalRoom pr) {
		if (oldGr.physicalRoom == pr) {
			return (unActivate (oldGr) && activate (newGr, pr));

		} else {
			Util.JLogErr ("Cannot replace " + oldGr.roomName + " with " + newGr.roomName + "  in " + pr.roomName );
			return false;
		}
	}

	private bool unActivate(GameplayRoom gr) {
		
		if( gr && gr.roomActive ) { //If room is currently Active
			if (gr.physicalRoom)
				gr.physicalRoom.gameplayRoom = null;
			gr.transform.SetParent(this.transform, false );
			gr.physicalRoom = null;
			gr.deactivateAllGameplayObjects();
			return true;

		} else {
			Util.JLogErr ( "Unable to Unactivate a GameplayRoom: " + gr );
			return false;
		}
	}
	private bool activate(GameplayRoom gr, PhysicalRoom pr) {
		
		if( gr && !gr.roomActive && pr && pr.roomEmpty ) {
			gr.transform.SetParent(pr.transform);
			pr.gameplayRoom = gr;
			gr.activateAllGameplayObjects ();
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


	//Add only new Gameplay Objects
	public void collectGameplayObjects() {
		if(	m_gameplayObjects == null)
			m_gameplayObjects = new HashSet<BaseGameplayObject>();
		else
			m_gameplayObjects.Clear ();

		BaseGameplayObject[] objs = FindObjectsOfType(typeof(BaseGameplayObject)) as BaseGameplayObject[];
		foreach (BaseGameplayObject o in objs) {
			if (m_gameplayObjects.Add (o)) //Returns false if already exists
				Util.JLog ("Tracking " + o.name + " as a  " + o.GetType ());
		}

	}

	public void collectPhysicalRooms() {
		if (m_physicalRooms == null)
			m_physicalRooms = new HashSet<PhysicalRoom>();
		else
			m_physicalRooms.Clear ();

		PhysicalRoom[] prs = FindObjectsOfType( typeof(PhysicalRoom) ) as PhysicalRoom[];
		foreach (PhysicalRoom pr in prs) {
			m_physicalRooms.Add (pr);
		}
	}

	public void collectTeleportTriggers() {
		if (m_teleportTriggers == null)
			m_teleportTriggers = new HashSet<TeleportTriggerGameplayObject>();
		else
			m_teleportTriggers.Clear ();

		TeleportTriggerGameplayObject[] tt = FindObjectsOfType( typeof(TeleportTriggerGameplayObject) ) as TeleportTriggerGameplayObject[];
		foreach (TeleportTriggerGameplayObject t in tt) {
			m_teleportTriggers.Add (t);
		}
	}
}
