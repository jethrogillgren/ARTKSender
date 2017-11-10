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
		StringBuilder builder = new StringBuilder ();

		GameObject[] objs = (GameObject[]) GameObject.FindObjectsOfType(typeof(GameObject));
		foreach (GameObject g in objs) {
			builder.AppendLine (g.name + " - " + (g.activeInHierarchy == true ? "Active in Hierachy" : "Disabled in Hierachy") + " - " + (g.activeSelf == true ? "Active" : "Disabled") );

		}


		Debug.Log("J# " + builder.ToString() );
		AndroidHelper.ShowAndroidToastMessage ( builder.ToString() );
	}

	//TEMP assumes 1 physicalroom
	public void OnSwapRoomButtonClick() {
		GameplayController gc = FindObjectOfType<GameplayController> ();


		GameplayRoom newRoom = null;
		foreach (GameplayRoom gr in gc.m_gameplayRooms) {
			if (gr != gc.m_physicalRooms.FirstOrDefault().gameplayRoom)
				newRoom = gr;
		}
		gc.replace (gc.m_physicalRooms.FirstOrDefault().gameplayRoom, newRoom, gc.m_physicalRooms.FirstOrDefault());
		ShoutPhysicalAndGameplayState ();

	}

	public void ShoutPhysicalAndGameplayState() {
		StringBuilder builder = new StringBuilder ();
		GameplayController gc = FindObjectOfType<GameplayController> ();

		foreach (GameplayRoom gr in gc.m_gameplayRooms) {
			if (gr.physicalRoom == null) {
				builder.AppendLine (gr.roomName);

			} else {
				builder.AppendLine (gr.physicalRoom.roomName);
				builder.AppendLine ("-" + gr.roomName);

			}
		}
			
		Util.JLog( builder.ToString() );
		AndroidHelper.ShowAndroidToastMessage ( builder.ToString() );
	}

    //Debug GUI is asking for either 3DR mapping to start, or it to be stopped and saved.
    public void OnButtonCreate3DRToggleClick( bool active ) {

        //the parameter given is stuck to true?!?!?!?!
        bool actualActive = GameObject.Find( "Make3DR" ).GetComponent<UnityEngine.UI.Toggle>().isOn;
        Util.JLog("3DR Toggle Button to: " + active + " but it lied it was actually " + actualActive );

        TangoPlayerApplicationController tac = FindObjectOfType<TangoPlayerApplicationController>();

        if( !actualActive ){
            //We have something to save
            if ( tac.m_dynamicMesh.enabled ) {
                tac.DebugExport3DRMesh();//Save it
            }
        }

        tac.DebugEnable3DR( actualActive );
    }

 //   //Either enable the meshes visibally, or make them occlude only.
 //   public void OnButtonViewMeshToggleClick( bool active ) {
	//	FindObjectOfType<TangoPlayerApplicationController> ().DebugSetOccludersVis (active);
	//}

	public void OnTeleportDropdownChange(string i) {
		Util.JLog ("DROPDOWN "+ i);
	}

	public void OnButton2Click() {
		Util.JLog("J# Button 3 - Locally disabling Monkey");
		GameObject.Find ("TestMonkey").SetActive (false);//local only?
	}

	public void OnEarthTeleport() {
		FindObjectOfType<GameplayController> ().LoadEarthRoomInMainRoom ();
	}
	public void OnWoodTeleport() {
		FindObjectOfType<GameplayController> ().LoadWoodRoomInMainRoom ();
	}
	public void OnMetalTeleport() {
		FindObjectOfType<GameplayController> ().LoadMetalRoomInMainRoom ();
	}
	public void OnFireTeleport() {
		FindObjectOfType<GameplayController> ().LoadFireRoomInMainRoom ();
	}
	public void OnWaterTeleport() {
		FindObjectOfType<GameplayController> ().LoadWaterRoomInMainRoom ();
	}

}
