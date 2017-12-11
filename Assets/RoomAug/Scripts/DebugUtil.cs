using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using System.Text;
using UnityEngine.SceneManagement;

//This is debug stuff that will be removed before release.  Client GUIs, 3DR mapping etc...
public class DebugUtil : MonoBehaviour {


	//Debug something to the screen
	public void OnButtonShoutClick()
	{
        //		string ret = " Current Pose Position.  World: " + Camera.main.transform.position + "   local:" + Camera.main.transform.localPosition;
#pragma warning disable XS0001 // Find APIs marked as TODO in Mono
        StringBuilder builder = new StringBuilder ();
#pragma warning restore XS0001 // Find APIs marked as TODO in Mono

        GameObject[] objs = (GameObject[]) Object.FindObjectsOfType(typeof(GameObject));
		foreach (GameObject g in objs) {
            builder.AppendLine (g.name + " " + LayerMask.LayerToName( g.layer) );

		}


		Debug.Log("J# " + builder.ToString() );
		AndroidHelper.ShowAndroidToastMessage ( builder.ToString() );
	}

	//TEMP assumes 1 physicalroom
	public void OnSwapRoomButtonClick() {
		RoomController gc = FindObjectOfType<RoomController> ();


		GameplayRoom newRoom = null;
		foreach (GameplayRoom gr in gc.m_gameplayRooms) {
			if (gr != gc.m_physicalRooms.FirstOrDefault().gameplayRoom)
				newRoom = gr;
		}
		gc.Cnt_Replace (gc.m_physicalRooms.FirstOrDefault().gameplayRoom, newRoom, gc.m_physicalRooms.FirstOrDefault());
		ShoutPhysicalAndGameplayState ();

	}

	public void ShoutPhysicalAndGameplayState() {
#pragma warning disable XS0001 // Find APIs marked as TODO in Mono
        StringBuilder builder = new StringBuilder ();
#pragma warning restore XS0001 // Find APIs marked as TODO in Mono
        RoomController gc = FindObjectOfType<RoomController> ();

		foreach (GameplayRoom gr in gc.m_gameplayRooms) {
			if (gr.physicalRoom == null) {
				builder.AppendLine (gr.roomName);

			} else {
                builder.AppendLine (gr.physicalRoom.roomName + " Pos: " + gr.physicalRoom.transform.position );
                builder.AppendLine ("-" + gr.roomName + " is " + (gr.enabled ? "Enabled" : "Not Enabled") + " Pos: " + gr.transform.position);

			}
		}
			
		Debug.Log( builder.ToString() );
		AndroidHelper.ShowAndroidToastMessage ( builder.ToString() );
	}

    //Debug GUI is asking for either 3DR mapping to start, or it to be stopped and saved.
    public void OnButtonCreate3DRToggleClick( bool active ) {

        //the parameter given is stuck to true?!?!?!?!
        bool actualActive = getActualToggleBoxParam( "Make3DR" );

        TangoApplicationController tac = FindObjectOfType<TangoApplicationController>();

        if( !actualActive ){
            //We have something to save
            if ( tac.m_dynamicMesh.enabled ) {
                tac.DebugExport3DRMesh();//Save it
            }
        }

        tac.DebugEnable3DR( actualActive );
    }

    private bool getActualToggleBoxParam(string gameobjectName) {
        return GameObject.Find( gameobjectName ).GetComponent<UnityEngine.UI.Toggle>().isOn;
    }

    //Either enable the meshes visibally, or make them occlude only.
    public void OnButtonViewMeshToggleClick( bool active ) {
        bool actualActive = getActualToggleBoxParam( "ToggleMeshView" );

        Debug.Log("OnButtonViewMeshToggleClick: " + actualActive );
        //FindObjectOfType<RoomAugPlayerController> ().CmdSetOccludersVis (actualActive);//Request on server
        FindObjectOfType<TangoApplicationController>().DebugSetOccludersVis( !actualActive );//Request on server

	}

	public void OnTeleportDropdownChange(string i) {
		Debug.Log ("DROPDOWN "+ i);
	}

	public void OnButton2Click() {
		Debug.Log("J# Button 3 - Locally disabling Monkey");
		GameObject.Find ("TestMonkey").SetActive (false);//local only?
	}

	public void OnEarthTeleport() {
		FindObjectOfType<RoomController> ().Cnt_LoadEarthRoomInMainRoom ();
	}
	public void OnWoodTeleport() {
		FindObjectOfType<RoomController> ().Cnt_LoadWoodRoomInMainRoom ();
	}
	public void OnMetalTeleport() {
		FindObjectOfType<RoomController> ().Cnt_LoadMetalRoomInMainRoom ();
	}
	public void OnFireTeleport() {
		FindObjectOfType<RoomController> ().Cnt_LoadFireRoomInMainRoom ();
	}
	public void OnWaterTeleport() {
		FindObjectOfType<RoomController> ().Cnt_LoadWaterRoomInMainRoom ();
	}

}