using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using System.Text;
using UnityEngine.SceneManagement;

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


	public void OnButtonToggleClick() {
		Debug.Log ("J# Turning Monkey towards" + GameObject.Find ("Earth") );

		FindObjectOfType<RoomAugPlayerController>().CmdWatchEarth ();

//		if (GameObject.Find ("TestMonkey").GetComponent<WatcherGameplayObject> ().m_LookTarget == GameObject.Find ("Earth")) {
//			Debug.Log ("J# Turning Monkey towards" + GameObject.Find ("Moon") );
//
//			GameObject.Find ("TestMonkey").GetComponent<WatcherGameplayObject> ().m_LookTarget = GameObject.Find ("Moon");
//		} else if (GameObject.Find ("TestMonkey").GetComponent<WatcherGameplayObject> ().m_LookTarget == GameObject.Find ("Moon")) {
//			Debug.Log ("J# Turning Monkey towards" + GameObject.Find ("Earth") );
//
//			GameObject.Find ("TestMonkey").GetComponent<WatcherGameplayObject> ().m_LookTarget = GameObject.Find ("Earth");
//		} else {
//			Debug.Log ("J# Monkey Business" );
//
//		}

	}

	public void OnTeleportDropdownChange(string i) {
		Util.JLog ("DROPDOWN "+ i);
	}

	public void OnButton2Click() {
		Debug.Log("J# Button 3 - Locally disabling Monkey");
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
