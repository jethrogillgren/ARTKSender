using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Not Networked!
public class PhysicalRoom : MonoBehaviour {

	public string roomName = "main";

	public GameplayRoom startGameplayRoom;//Only used if no child is present

    //Client scoped
	private GameplayRoom m_gameplayRoom; //Different on Servers and Each Client!
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
				value.cnt_PhysicalRoom = this;
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
			gc.Activate (startGameplayRoom, this);
		}
	}

	public void registerAnyChildGameplayRoom() {
		gameplayRoom  = GetComponentInChildren<GameplayRoom> ();//TODO check depth
	}
}
