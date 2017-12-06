using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionGameplayObject : BaseGameplayObject {

	public Material m_depthMaskMat;// The reference to the depth mask material to be applied to occlusion meshes.
	public Material m_visibleMat;// The reference to the visible material applied to the mesh.

	public bool occludeOnServer = false;

	public OcclusionGameplayObject(Material depthMaskMat, Material visibleMat) {
		this.m_depthMaskMat = depthMaskMat;
		this.m_visibleMat = visibleMat;
	}

	// Use this for initialization
    public override void Start() {
        base.Start();

		//Our occlusion default is determined by out GameplayState.
        if( m_GameplayState == GameplayState.Started )
		    setOcclusion (true);
        else
            setOcclusion( false );
	}

	// Update is called once per frame
	void Update () {
//		if( m_GameplayState == GameplayState.Started) {
//			//
//		}
	}

	public override void SetLayer( string roomName = "" ) {
		//Do nothing, we don't want to change Occlusion layers
	}

	public override void UpdateVisibility() {

		//GameplayState has first priority on setting enabled state.
		if (gameplayState != GameplayState.Started)
		{
			gameObject.SetActive ( false );
			return;
		}

		//Only clientes normally care about occlusion.  Servers can opt in.
		if (isClient || occludeOnServer)
		{
			setOcclusion ( true );
			base.UpdateVisibility (); //Clients will also follow normal rules for show / hiding
		}
		else
		{
			setOcclusion ( false );
		}



//		else if (Application.platform != RuntimePlatform.Android && Application.isPlaying)
//		{
//			gameObject.SetActive ( false );
//		}
//		else if (Application.platform != RuntimePlatform.Android && !Application.isPlaying)
//		{
//			setOcclusion ( false );
//		}
//		else
//		{
//			Debug.LogWarning ( "The pig flys!" );
//		}
	}



	//Used to disable occulusion while running.  You can also just set GameplayState to Inactive, which os cheaper and more clear.
	public void setOcclusion( bool turnOn ) {

		//Turn occluson on, making the object invisible but blocking further back objects
		if (turnOn  &&  m_depthMaskMat != null) {
			Debug.Log ("Turning " + name + "'s Occlusion On");

			gameObject.layer = LayerMask.NameToLayer ("Occlusion");
			
			foreach (MeshRenderer m in gameObject.GetComponents<MeshRenderer>()) {
				m.material = m_depthMaskMat;
				m.gameObject.layer = LayerMask.NameToLayer("Occlusion");
			}
			foreach (MeshRenderer m in gameObject.GetComponentsInChildren<MeshRenderer>()) {
				m.material = m_depthMaskMat;
				m.gameObject.layer = LayerMask.NameToLayer("Occlusion");
			}

		//Turn occlusion off, making the object a regular visible object
		} else if ( !turnOn  &&  m_visibleMat != null) {
			Debug.Log ("Turning " + name + "'s Occlusion Off");

			gameObject.layer = LayerMask.NameToLayer ("Default");
			foreach (MeshRenderer m in gameObject.GetComponents<MeshRenderer>()) {
				m.material = m_visibleMat;
				m.gameObject.layer = LayerMask.NameToLayer ("Default");
			}
			foreach (MeshRenderer m in gameObject.GetComponentsInChildren<MeshRenderer>()) {
				m.material = m_visibleMat;
				m.gameObject.layer = LayerMask.NameToLayer ("Default");
			}
		} else {
			Debug.LogError ("Unable to act on a SetOcclusion Request: " + turnOn + m_depthMaskMat + m_visibleMat);
		}
	}

}
