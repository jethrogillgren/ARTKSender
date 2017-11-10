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

    //Can be implemented.  (abstract must be).  All implementers must call base.start();
    public virtual void Start() {
        updateVisibility();
        SetLayer();
    }

    //If you exist in a GameplayRoom, you have to have that rooms layer otherwise the server don't cull you properly
    //gameobjects are allow to be roomless (global under physicalroom).
    public void SetLayer( string roomName = "" ) {
        
        //If not specified, find our current parent
        if ( roomName == null || roomName.Length == 0 ) {
            GameplayRoom gr = GetComponentInParent<GameplayRoom>();
            if ( gr ) roomName = gr.roomName;
        }

        //We got it
        if ( roomName != null && roomName.Length > 0 )
            SetLayerRecursively( gameObject, LayerMask.NameToLayer( roomName ) );
            //gameObject.layer = LayerMask.NameToLayer( roomName );?
    }

    //Includes all non-GameplayObject children
    public void SetLayerRecursively( GameObject obj, int newLayer  ) {
        obj.layer = newLayer;

        foreach ( Transform child in obj.transform ) {
            SetLayerRecursively( child.gameObject, newLayer );
        }
    }

	//Called to enable or disable this GameplayObject depending on wether it currently exsts in the clients view of the world
    public virtual void updateVisibility() {
        //GameplayState has first priority on setting enabled state.
        if ( gameplayState != GameplayState.Started )
            return;
        
        //Servers see evvveeerythiiiing.  If it knows it shouldn't be turned on, the server trusts that.
        //But if we are not Playing (ie we are editing) skip to the next bit
        if ( !isClient && Application.isPlaying ) {
            gameObject.SetActive( true );
			return;
		}

        //Clients see their current room only.
        //When editing we do this bit
		PhysicalRoom pr = GetComponentInParent<PhysicalRoom>();
        if( pr )
			gameObject.SetActive (true);
		else 
			gameObject.SetActive (false);
	}

//	public abstract void test(); //Must be implemented
//	public virtual bool test2() { return true; } //Can be implemented


}