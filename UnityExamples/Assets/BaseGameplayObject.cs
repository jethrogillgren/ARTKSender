using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameplayObject : MonoBehaviour {

	public enum GameplayState {
		Inactive, //Not interactible or visible to the user.
		Started, //Either interactible with the world, or visible to the user, or both.
		Finished //All interaction is finished.
	}

	public bool m_IsDecorationOnly = true; //True means there is no interactions - it is just for show
	public GameplayState m_GameplayState = GameplayState.Inactive ;


//	public abstract void test(); //Must be implemented
//	public virtual bool test2() { return true; } //Can be implemented


	protected virtual void JLog(string val) {
		Debug.Log ("J# " + val);
	}
	protected virtual void JLogErr(string val) {
		Debug.LogError ("J# " + val);
	}
}