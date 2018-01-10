using UnityEngine;
using UnityEngine.Networking;
using System.Threading;

public class TestB : NetworkBehaviour {

	private Object updateLock = new Object();

	float t;

	protected const int sensors = 7;//Number of real world sensors
	float[] svr_transformTimestamps = new float[sensors]; //The last times we heard from that sensor



	public virtual void Svr_SetMarker(Matrix4x4 transformationMatrix, int roomCameraNumber)
	{

		lock ( updateLock )
		{
			svr_transformTimestamps [roomCameraNumber - 1 ] = t;
		}

		Debug.LogError ( "Setter: " + svr_transformTimestamps [ 0 ] );

	}
	
	public virtual void Update() {

		Debug.LogError ( svr_transformTimestamps [ 0 ] );

	}
	public virtual void LateUpdate()  //svr only
	{
		Svr_ApplyTransformations ();
	}

	protected virtual void Svr_ApplyTransformations() {

		lock ( updateLock )
		{
			t = Time.time;

			for ( int i = 0; i < sensors; i++ )
			{
				//If we had a reading within the dropoff delay time
				if (svr_transformTimestamps [ i ] > 0) {
					// ...
				}

			}

		}
	}


}
