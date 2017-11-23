using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Audio;


public class AudioController : NetworkBehaviour {

	//SFX
	public AudioGameplayObject testSFX;

	//MUSIC
	public AudioGameplayObject testMusic;

	//ROOM SPEECH
	public AudioGameplayObject aiRoomSpeech;

	//TANGO SPEECH
	public AudioGameplayObject aiTangoSpeech;

	//3D Room SOUND
	public AudioGameplayObject woodAmbience;


	//For syncronised time   syncServerTime
	private RoomAugNetworkManager networkManager;



	public void Start ()
	{
		networkManager = GameObject.FindObjectOfType<RoomAugNetworkManager> ();

//		InvokeRepeating ( "DebugSFX", 5, 5 );// Seconds
	}


	//Called to move to the next section of a Rooms music track
	public void ProgressRoomMusic() {
		
	}


//	public void DebugSFX() {
//		Debug.LogWarning ("DebugSFX Called");
//		testSFX.Play();
//	}

	//Alt to just calling it ourselves
	public void TriggerClip(AudioGameplayObject source) {
		source.PlayClientsToo ();
	}
}
