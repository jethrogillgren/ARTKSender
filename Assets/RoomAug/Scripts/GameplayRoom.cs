﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayRoom : MonoBehaviour {

	public string roomName;

	private PhysicalRoom m_physicalRoom; //1 to 1 relationship, or empty.  relationship controlled by parent
	public PhysicalRoom physicalRoom
	{
		get
		{
			return m_physicalRoom;
		}

		set
		{
			this.m_physicalRoom = value;
//			value.gameplayRoom = this;  Not done as a loop would form.  Parent controls the ownership.
		}
	}

	public bool roomActive{
		get  {
			return (m_physicalRoom != null);
		}
	}

	// Use this for initialization
	void Start () {
		registerAnyParentPhysicalRoom ();
	}

	public void registerAnyParentPhysicalRoom() {
		physicalRoom  = GetComponentInParent<PhysicalRoom> (); //TODO check depth
	}

	public void activateAllGameplayObjects() {
		setAllGameplayObjects (BaseGameplayObject.GameplayState.Started, true);
	}

	public void deactivateAllGameplayObjects() {
		setAllGameplayObjects (BaseGameplayObject.GameplayState.Inactive, true);
	}

	public void setAllGameplayObjects(BaseGameplayObject.GameplayState state, bool includeInactive) {
		BaseGameplayObject[] objs = GetComponentsInChildren<BaseGameplayObject> (includeInactive);
		Util.JLog ("Setting all " + objs.Length + " Gamepbjects in room: " + roomName + " to " + state );

		foreach (BaseGameplayObject o in objs) {
			o.gameplayState = state;
		}
	}

}
