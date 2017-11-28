using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;

public class ShoutStuff : MonoBehaviour {


	// Use this for initialization
	void Start () {

//		// pick a random color
//		Color newColor = new Color( UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f );
//		JLog ("Coloured the cube " + newColor.ToString() );
//		// apply it on current object's material
//		GetComponent<MeshRenderer>().material.color = newColor;
	}

	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseDown() {

//		StringBuilder builder = new StringBuilder();

		//Empty
	}

	private void debugAndToast(string str) {
		JLog ( str );
		AndroidHelper.ShowAndroidToastMessage ( str);
	}

	protected virtual void JLog(string val) {
		Debug.Log ("J# " + val);
	}
	protected virtual void JLogErr(string val) {
		Debug.LogError ("J# " + val);
	}
}
