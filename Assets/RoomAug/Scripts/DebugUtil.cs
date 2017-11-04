using System.Collections;
using System.Collections.Generic;
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
			builder.AppendLine (g.name + " - " + (g.activeInHierarchy == true ? "Active" : "Disabled") );

		}

		Debug.Log("J# " + builder.ToString() );
		AndroidHelper.ShowAndroidToastMessage ( builder.ToString() );
	}
}
