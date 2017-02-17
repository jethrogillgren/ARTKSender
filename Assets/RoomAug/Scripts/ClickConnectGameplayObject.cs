using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickConnectGameplayObject : BaseGameplayObject {

	public enum ConnectState {
		inactive,
		dragging,
		connected
	}

	public Material m_draggingMaterial;
	public LineRenderer m_lineRenderer;

	private Material m_startMaterial;
	private MeshRenderer m_renderer;
	private ConnectState m_state = ConnectState.inactive;
	private ClickConnectGameplayObject m_connectedObject;


	// Use this for initialization
	void Start () {
		m_renderer = gameObject.GetComponent<MeshRenderer> ();
		m_startMaterial = m_renderer.material;
	}
	
	// Update is called once per frame
	void Update () {
		if ( m_GameplayState == GameplayState.Started ) {
				
			if (Input.touchCount == 1) { //One finger is touching screen
				Touch t = Input.GetTouch (0);
				GameObject worldHit = screenToWorldHit (t);
				//JLog ( name + " saw a Touch at " + t.position + " hitting " + (worldHit==null ? "Nothing" : worldHit.name) );

				//If we have started a touch on this object
				if (t.phase == TouchPhase.Began  && worldHit == gameObject ) {
					
					//Animate and setup for dragging
					m_renderer.material = m_draggingMaterial;
					m_state = ConnectState.dragging;

					//If we are dragging from this object
				} else if (m_state == ConnectState.dragging && t.phase == TouchPhase.Moved) {
					//TODO Update Linerender while dragging

//					if (worldHit) {
//						JLog ("Drawing Line to world: " + worldHit.transform.position );
//						drawLineTo (worldHit.transform.position);
//
//					} else {
//						float distanceToMe = Vector3.Distance ( Camera.main.transform.position, gameObject.transform.position );
//						Vector3 dir = Camera.main.ScreenPointToRay (t.position).direction;
//						Vector3 pos = Camera.main.transform.position + (Camera.main.transform.forward * distanceToMe);
//						JLog ("Drwaing Line to space:" + pos);
//						drawLineTo ( pos);
//					}

				//If user is dragging over us, but the drag started from some other object
				} else if ( m_state != ConnectState.dragging  && t.phase == TouchPhase.Moved  &&  worldHit == gameObject ) {
					//Animate a valid DropLanding   TODO if drag was from a ClickConnect, not just any drag
					m_renderer.material = m_draggingMaterial;
				//If user is dragging over someone else, started from some other object, but was previously dragging over us
				} else if ( m_state != ConnectState.dragging  && t.phase == TouchPhase.Moved  &&  worldHit != gameObject ) {
					//Animate a valid DropLanding   TODO if drag was from a ClickConnect, not just any drag
					m_renderer.material = m_startMaterial;

				//If we have Ended the touch
				} else if ( m_state == ConnectState.dragging  &&  t.phase == TouchPhase.Ended) {


					if ( worldHit != null )
					{
						//Check what the raycast hit actually was
						ClickConnectGameplayObject otherConnection = worldHit.GetComponent<ClickConnectGameplayObject> ();
						if ( otherConnection != null )
						{
							//We released on a ClickConnector
							if( otherConnection != this ) {
								otherConnection.connectionFrom (this);
								connectTo (otherConnection);
								drawLineTo (otherConnection);
								return;
							}
						}
					}

					resetAll ();

				//If the touch was cancelled
				} else if (t.phase == TouchPhase.Canceled) {
					resetAll ();
				}



			}

		} //Else we are not active - do nothing
	}

	private GameObject screenToWorldHit(Touch t) {
		RaycastHit hitInfo;

		if (Physics.Raycast (Camera.main.ScreenPointToRay (t.position), out hitInfo )) {
			return hitInfo.collider.gameObject;
		} else {
			return null;
		}
	}

	private void connectionFrom( ClickConnectGameplayObject incomingConnection ) {
		m_connectedObject = incomingConnection;
		m_state = ConnectState.connected;
	}
	private void connectTo( ClickConnectGameplayObject otherConnection ) {
		m_connectedObject = otherConnection;
		m_state = ConnectState.connected;
	}

	private void drawLineTo( Vector3 end ) {
		drawLineTo (gameObject.transform.position, end);
	}
	private void drawLineTo( ClickConnectGameplayObject end ) {
		drawLineTo (gameObject.transform.position, end.gameObject.transform.position);
	}
	private void drawLineTo( Vector3 start, Vector3 end ) {
		m_lineRenderer.SetPosition(0, start);
		m_lineRenderer.SetPosition(1, end);
		m_lineRenderer.enabled = true;
	}

	private void resetAll() {
		m_state = ConnectState.inactive;
		m_renderer.material = m_startMaterial;
		m_lineRenderer.enabled = false;
	}


}
