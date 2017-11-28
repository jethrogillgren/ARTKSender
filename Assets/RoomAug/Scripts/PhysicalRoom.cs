using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PhysicalRoom : MonoBehaviour {

	public string roomName = "main";

	public GameplayRoom m_startGameplayRoom;//Only used if no child is present

    //Client scoped
	private GameplayRoom m_gameplayRoom; //1 to 1 relationship, or empty.  Use accessor below
	public GameplayRoom gameplayRoom
	{
		get
		{
			return m_gameplayRoom;
		}

		set
		{
			this.m_gameplayRoom = value;

			if (value) {
				value.physicalRoom = this;
			}
		}
	}

	public bool roomEmpty{
		get  {
			return (m_gameplayRoom == null);
		}
	}

	// Use this for initialization
	void Start () {
		registerAnyChildGameplayRoom ();
		if( !gameplayRoom ) {
			RoomController gc = FindObjectOfType<RoomController> ();
			gc.activate (m_startGameplayRoom, this);
		}
	}


	public void registerAnyChildGameplayRoom() {
		gameplayRoom  = GetComponentInChildren<GameplayRoom> ();//TODO check depth
	}
}
