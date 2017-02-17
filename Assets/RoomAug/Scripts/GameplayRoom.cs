using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayRoom : MonoBehaviour {

	public string roomName;

	private PhysicalRoom m_physicalRoom; //1 to 1 relationship, or empty.
	public PhysicalRoom physicalRoom
	{
		get
		{
			return m_physicalRoom;
		}

		set
		{
			this.m_physicalRoom = value;
//			value.gameplayRoom = this;
		}
	}

	// Use this for initialization
	void Start () {
		
	}



//	public void registerAnyParentPhysicalRoom() {
//		PhysicalRoom pr = GetComponentInParent<PhysicalRoom> ();
//		if (pr)
//			physicalRoom = pr;
//	}

}
