using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionGameplayObject : BaseGameplayObject {

	public Material m_depthMaskMat;// The reference to the depth mask material to be applied to occlusion meshes.
	public Material m_visibleMat;// The reference to the visible material applied to the mesh.

	public OcclusionGameplayObject(Material depthMaskMat, Material visibleMat) {
		this.m_depthMaskMat = depthMaskMat;
		this.m_visibleMat = visibleMat;
	}

	// Use this for initialization
	void Start () {
		setOcclusion (false);
	}

	// Update is called once per frame
	void Update () {
//		if( m_GameplayState == GameplayState.Started) {
//			//
//		}
	}

	//Used to disable occulusion while running.  You can also just set GameplayState to Inactive, which os cheaper and more clear.
	public void setOcclusion( bool turnOn ) {

		//Turn occluson on, making the pject invisible but blocking further back objects
		if (turnOn  &&  m_depthMaskMat != null) {
			JLog ("Turning " + name + "'s Occlusion On");

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
			JLog ("Turning " + name + "'s Occlusion Off");

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
			JLogErr ("Unable to act on a SetOcclusion Request: " + turnOn + m_depthMaskMat + m_visibleMat);
		}
	}

}
