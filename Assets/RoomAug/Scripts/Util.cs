using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;



public static class Util
{
	public static readonly int portARToolkitAgentBase = 41600;

	public static readonly float ARToolkitViewportRectX = 0.5f * Screen.width;
	public static readonly float ARToolkitViewportRectY = 0.0f * Screen.height;
	public static readonly float ARToolkitViewportRectW = 0.25f* Screen.width;
	public static readonly float ARToolkitViewportRectH = 0.5f* Screen.height;
    

	public static bool Cnt_IsObjectInMainCamerasFOV( Transform targetPoint){
		return Cnt_IsObjectInCamerasFOV ( Camera.main, targetPoint );
	}

	public static bool Cnt_IsObjectInCamerasFOV(Camera camera, Transform targetPoint)
	{
		if (!camera || !targetPoint)
			Debug.LogError ("IsObjectInCamerasFOV Invalid Params recieved");
		
		Vector3 screenPoint = camera.WorldToViewportPoint(targetPoint.position);
		bool ret = (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1);
		Debug.Log ("Checking if " + targetPoint.name + " is within " + screenPoint + " .... " + ret + "!" );
		return ret;
	}
    


	//Get an average (mean) from more then two quaternions (with two, slerp would be used).
	//Note: this only works if all the quaternions are relatively close together.
	//Usage: 
	//-Cumulative is an external Vector4 which holds all the added x y z and w components.
	//-newRotation is the next rotation to be added to the average pool
	//-firstRotation is the first quaternion of the array to be averaged
	//-addAmount holds the total amount of quaternions which are added (including the one we are now adding)
	//This function returns the current average quaternion
	public static Quaternion AverageQuaternion(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation, int addAmount){

		float w = 0.0f;
		float x = 0.0f;
		float y = 0.0f;
		float z = 0.0f;

		//Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
		//q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
		if(!AreQuaternionsClose(newRotation, firstRotation)){

			newRotation = InverseSignQuaternion(newRotation);	
		}

		//Average the values
		float addDet = 1f/(float)addAmount;
		cumulative.w += newRotation.w;
		w = cumulative.w * addDet;
		cumulative.x += newRotation.x;
		x = cumulative.x * addDet;
		cumulative.y += newRotation.y;
		y = cumulative.y * addDet;
		cumulative.z += newRotation.z;
		z = cumulative.z * addDet;		

		//note: if speed is an issue, you can skip the normalization step
		return NormalizeQuaternion(x, y, z, w);
	}

	public static Quaternion NormalizeQuaternion(float x, float y, float z, float w){

		float lengthD = 1.0f / (w*w + x*x + y*y + z*z);
		w *= lengthD;
		x *= lengthD;
		y *= lengthD;
		z *= lengthD;

		return new Quaternion(x, y, z, w);
	}

	//Changes the sign of the quaternion components. This is not the same as the inverse.
	public static Quaternion InverseSignQuaternion(Quaternion q){

		return new Quaternion(-q.x, -q.y, -q.z, -q.w);
	}

	//Returns true if the two input quaternions are close to each other. This can
	//be used to check whether or not one of two quaternions which are supposed to
	//be very similar but has its component signs reversed (q has the same rotation as
	//-q)
	public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2){

		float dot = Quaternion.Dot(q1, q2);

		if(dot < 0.0f){

			return false;					
		}

		else{

			return true;
		}
	}



	//Used by gameplayRooms, and PandaCubes.
	public enum ElementalType {
		none,
		wood,
		fire,
		earth,
		metal,
		water
	}

	public static Color GetColor( ElementalType elementalType ) {
		switch (elementalType)
		{
			case ElementalType.none:
				return Color.white;
			case ElementalType.wood:
				return Color.green;
			case ElementalType.fire:
				return Color.red;
			case ElementalType.earth:
				return Color.yellow;
			case ElementalType.metal:
				return Color.grey;
			case ElementalType.water:
				return Color.blue;
			default:
				Debug.LogError ( "Invalid CubeType" );
				return Color.black;
		}
	}


    public static List<T> CreateList<T> ( params T [] values )
    {
        return new List<T> ( values );
    }
    

    public static void collectHashSetOfComponents<T> ( ref HashSet<T> setToFill, bool inclDisabled = false )
    {
        if (setToFill == null)
            setToFill = new HashSet<T> ();
        else
            setToFill.Clear ();

        T [] prs = inclDisabled ? Resources.FindObjectsOfTypeAll ( typeof ( T ) ) as T [] : UnityEngine.Object.FindObjectsOfType ( typeof ( T ) ) as T [];

        foreach ( T pr in prs )
        {
            setToFill.Add ( pr );
        }
    }

}