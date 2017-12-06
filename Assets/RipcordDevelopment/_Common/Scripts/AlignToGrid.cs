using UnityEngine;
using System.Collections;
using UnityEditor;

// /-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\
//
// 						  Ripcord Tools, Copyright © 2017, Ripcord Development
//											  AlignToGrid.cs
//												 v1.0.0
//										   info@ripcorddev.com
//
// \-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/

//ABOUT - This script acts on all currently selected objects
//		- This script aligns the position of each object to the nearest increment of 0.5 on each axis
//		  and aligns the rotation of each object to the nearest increment of 90 on each axis

namespace RipcordDevelopment
{

	public class AlignToGrid : MonoBehaviour
	{

		[MenuItem ( "Tools/Align to Grid 2" )]
		static void GridAlign ()
		{

			foreach ( Transform selection in Selection.transforms )
			{

				Vector3 roundedPosition = selection.position;
				roundedPosition.x = Mathf.Round ( roundedPosition.x * 2.0f ) / 2.0f;
				roundedPosition.y = Mathf.Round ( roundedPosition.y * 2.0f ) / 2.0f;
				roundedPosition.z = Mathf.Round ( roundedPosition.z * 2.0f ) / 2.0f;
				selection.position = roundedPosition;

				Vector3 roundedAngles = selection.eulerAngles;
				roundedAngles.x = Mathf.Round ( roundedAngles.x / 90 ) * 90;
				roundedAngles.y = Mathf.Round ( roundedAngles.y / 90 ) * 90;
				roundedAngles.z = Mathf.Round ( roundedAngles.z / 90 ) * 90;
				selection.eulerAngles = roundedAngles;
			}
		}

		[MenuItem ( "Tools/Align to Grid 4" )]
		static void GridAlign2 ()
		{
	
			foreach ( Transform selection in Selection.transforms )
			{

				Vector3 roundedPosition = selection.position;
				roundedPosition.x = Mathf.Round ( roundedPosition.x * 4.0f ) / 4.0f;
				roundedPosition.y = Mathf.Round ( roundedPosition.y * 4.0f ) / 4.0f;
				roundedPosition.z = Mathf.Round ( roundedPosition.z * 4.0f ) / 4.0f;
				selection.position = roundedPosition;
	
				Vector3 roundedAngles = selection.eulerAngles;
				roundedAngles.x = Mathf.Round ( roundedAngles.x / 90 ) * 90;
				roundedAngles.y = Mathf.Round ( roundedAngles.y / 90 ) * 90;
				roundedAngles.z = Mathf.Round ( roundedAngles.z / 90 ) * 90;
				selection.eulerAngles = roundedAngles;
			}
		}

		[MenuItem ( "Tools/Align to Local Grid 2" )]
		static void GridAlign3 ()
		{

			foreach ( Transform selection in Selection.transforms )
			{

				Vector3 roundedPosition = selection.localPosition;
				roundedPosition.x = Mathf.Round ( roundedPosition.x * 2.0f ) / 2.0f;
				roundedPosition.y = Mathf.Round ( roundedPosition.y * 2.0f ) / 2.0f;
				roundedPosition.z = Mathf.Round ( roundedPosition.z * 2.0f ) / 2.0f;
				selection.localPosition = roundedPosition;

				Vector3 roundedAngles = selection.localEulerAngles;
				roundedAngles.x = Mathf.Round ( roundedAngles.x / 90 ) * 90;
				roundedAngles.y = Mathf.Round ( roundedAngles.y / 90 ) * 90;
				roundedAngles.z = Mathf.Round ( roundedAngles.z / 90 ) * 90;
				selection.localEulerAngles = roundedAngles;
			}
		}

		[MenuItem ( "Tools/Align to Local Grid 4" )]
		static void GridAlign4 ()
		{

			foreach ( Transform selection in Selection.transforms )
			{

				Vector3 roundedPosition = selection.localPosition;
				roundedPosition.x = Mathf.Round ( roundedPosition.x * 4.0f ) / 4.0f;
				roundedPosition.y = Mathf.Round ( roundedPosition.y * 4.0f ) / 4.0f;
				roundedPosition.z = Mathf.Round ( roundedPosition.z * 4.0f ) / 4.0f;
				selection.localPosition = roundedPosition;

				Vector3 roundedAngles = selection.localEulerAngles;
				roundedAngles.x = Mathf.Round ( roundedAngles.x / 90 ) * 90;
				roundedAngles.y = Mathf.Round ( roundedAngles.y / 90 ) * 90;
				roundedAngles.z = Mathf.Round ( roundedAngles.z / 90 ) * 90;
				selection.localEulerAngles = roundedAngles;
			}
		}
	}
}