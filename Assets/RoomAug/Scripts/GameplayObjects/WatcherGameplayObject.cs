using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WatcherGameplayObject : BaseGameplayObject {

	[SyncVar]
	public GameObject m_LookTarget; //Optional

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame.
	void Update () {
		//SERVER DOES UPDATE.  CLients just read state and ask their Player to act

		if (isClient)
			return;
		
		if (gameplayState == GameplayState.Started) {

			if ( m_LookTarget != null  &&  m_LookTarget.activeInHierarchy ) {
				this.gameObject.transform.LookAt (m_LookTarget.transform.position);
			} else {
				this.gameObject.transform.rotation = Quaternion.LookRotation (
						Camera.main.transform.position - this.gameObject.transform.position);
			}

		}
	}

}