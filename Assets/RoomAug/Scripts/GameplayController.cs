using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : MonoBehaviour {

	public HashSet<BaseGameplayObject> m_gameplayObjects;

	// Use this for initialization
	void Start () {
		m_gameplayObjects = new HashSet<BaseGameplayObject>();
		collectGameplayObjects ();
	}



	public HashSet<BaseGameplayObject> getGameplayObjectsByState(BaseGameplayObject.GameplayState state) {
		HashSet<BaseGameplayObject> ret = new HashSet<BaseGameplayObject>();

		if (m_gameplayObjects == null || m_gameplayObjects.Count <= 0) {
			JLog ("Asked to get Gameplay Objects by state, but there are " +  (m_gameplayObjects == null ? "NULL" : m_gameplayObjects.Count.ToString()) + " stored");
			collectGameplayObjects ();
		}

		foreach(BaseGameplayObject g in m_gameplayObjects) {
			if (g.m_GameplayState == state) {
				ret.Add (g);
			}
		}
		JLog ( "Returning the " + ret.Count + "/" + m_gameplayObjects.Count + " Gameplay Objects that are " + state );
		return ret;
	}

	public HashSet<OcclusionGameplayObject> getOcclusionGameplayObjects() {
		HashSet<OcclusionGameplayObject> ret = new HashSet<OcclusionGameplayObject>();
		foreach (BaseGameplayObject g in m_gameplayObjects) {
			if ( g.GetType() == typeof(OcclusionGameplayObject) )
				ret.Add (g as OcclusionGameplayObject);
		}

		JLog ( "Returning the " + ret.Count + "/" + m_gameplayObjects.Count + " Gameplay Objects that are Occluders (active or Inactive)" );

//		foreach (OcclusionGameplayObject g in m_gameplayObjects)
//			JLog ("TEST:  " + g.name + " " + g.GetType() + " " + g.ToString() );


		return ret;
	}

	private void collectGameplayObjects() {
		m_gameplayObjects.Clear ();

//		foreach (Transform child in this.transform) {
//			foreach( BaseGameplayObject c in  child.GetComponents<BaseGameplayObject>() ) {
//				JLog ( "Tracking " + c.name + " as a  " + c.GetType() );
//				m_gameplayObjects.Add (c);
//			}
//		}

		BaseGameplayObject[] objs = FindObjectsOfType(typeof(BaseGameplayObject)) as BaseGameplayObject[];
		foreach (BaseGameplayObject o in objs) {
			if (m_gameplayObjects.Add (o)) //Returns false if already exists
				JLog ("Tracking " + o.name + " as a  " + o.GetType ());
		}

		JLog ("Tracking " + m_gameplayObjects.Count + " Gameplay Objects." );
	}

	//Tries to add the gameplay object, returning true if it was newly added and false if alreadye existed
	public bool addGameplayObject(BaseGameplayObject o) {
		if ( m_gameplayObjects.Add (o) ) {
			JLog("Added new Gameplay Object: " + o.name + " as a  " + o.GetType () );
			return true;
		}
		return false;
	}



	private void JLog(string val) {
		Debug.Log ("J# GameplayController: " + val);
	}
	private void JLogErr(string val) {
		Debug.LogError ("J# GameplayController: " + val);
	}
}
