using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor.Audio;


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

	public override void OnStartServer ()
	{

	}



	//Alt to just calling it ourselves
	public void TriggerClip(AudioGameplayObject source) {
		source.PlayEverywhere ();
	} 
}
