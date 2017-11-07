using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//[RequireComponent(typeof(NetworkIdentity))]
public abstract class BaseGameplayObject : NetworkBehaviour {

	protected string LogTag = "J#";

	[SyncVar]
	public bool m_IsDecorationOnly = true; //True means there is no interactions - it is just for show or graphics.  False means the user will interact with it as part of a clue.

	public enum GameplayState {
		Inactive, //Not interactible or visible to any user.
		Started, //Either interactible with the world, or potentially visible to the user, or both.
		Finished //All interaction is finished.
	}

	public GameplayState gameplayState 
	{
		get
		{
			return m_GameplayState;
		}

		set
		{
			if ( value != BaseGameplayObject.GameplayState.Finished ) {
				
				m_GameplayState = value;
				Util.JLog ("Setting " + gameObject.name + " to " + m_GameplayState);

				if (m_GameplayState == GameplayState.Inactive) {
					gameObject.SetActive (false);
				} else if (m_GameplayState == GameplayState.Started) {
					gameObject.SetActive (true);
				} else if (m_GameplayState == GameplayState.Finished) {
					gameObject.SetActive (false);
				} else {
					Util.JLogErr ("Trying to set " + name + "'s GameplayState to " + value.ToString ());
				}

			}
		}
	}

	[SyncVar]
	public GameplayState m_GameplayState = GameplayState.Started;

	//Called to enable or disable this GameplayObject depending on wether it currently exsts in the clients view of the world
	public virtual void updateClientVisibility() {
		//TODO
		if (isServer) {
			Util.JLogErr ("Server tried to set Client Visibility - this can only be done by the client as it is local");
			return;
		}

		PhysicalRoom pr = GetComponentInParent<PhysicalRoom>();
		if( pr )
			gameObject.SetActive (true);
		else 
			gameObject.SetActive (false);
	}

//	public abstract void test(); //Must be implemented
//	public virtual bool test2() { return true; } //Can be implemented


}