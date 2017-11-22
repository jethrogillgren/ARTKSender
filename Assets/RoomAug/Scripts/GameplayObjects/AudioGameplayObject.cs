using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;


//Just call play on this, it knows where it should play.
[RequireComponent(typeof(AudioSource))]
public class AudioGameplayObject : BaseGameplayObject {

	public bool playOnServer = true;
	public bool playOnClient = true;

	public bool playOnSpawn = false;

	private AudioSource audioSource;

	// Use this for initialization for both 
	public override void Start()
	{
		base.Start ();
		audioSource = GetComponent<AudioSource> ();
		if (audioSource.playOnAwake)
			playOnSpawn = true;
		audioSource.playOnAwake = false;

		if( playOnSpawn ) {
			Play ();
		}
	}

	
	// Update is called once per frame
	void Update () {
		
	}

	//Called on the server to play this track whereever it's needed
	public void Play() {
		//If i'm me and I want to play
		if(   	 (isClient && playOnClient && gameplayState == GameplayState.Started) ||
				(!isClient && playOnServer && gameplayState == GameplayState.Started) )
			audioSource.PlayOneShot ( audioSource.clip );//Play
	}

	//Network Control
	public void PlayEverywhere() {
		Play ();
		if (isClient)
			CmdPlay ();
		else
			RpcPlay();
	}
	[ClientRpc]
	public void RpcPlay()
	{
		Play ();
	}
	[Command]
	public void CmdPlay()
	{
		Play ();
	}
}