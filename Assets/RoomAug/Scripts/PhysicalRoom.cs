﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalRoom : MonoBehaviour {

	public string roomName;

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

	// Use this for initialization
	void Start () {
		registerAnyChildGameplayRoom ();
	}


	public void registerAnyChildGameplayRoom() {
		gameplayRoom  = GetComponentInChildren<GameplayRoom> ();
//		if (gr)
//			gameplayRoom = gr;
//		else
//			gameplayRoom = null;
	}
}
