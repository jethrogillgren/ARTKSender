using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System;


public class TestA : NetworkBehaviour {

	public TestB testB; //Set in inspector

	public UdpClient[] clients = new UdpClient[4];

	void Start () {

		Svr_StartARToolkitAgentRecieve ();

	}

	public void Svr_StartARToolkitAgentRecieve() {

		for ( int i = 0; i < clients.Length; i++ )
		{
			clients [ i ] = new UdpClient ( Util.portARToolkitAgentBase + i );
		}

		new Thread( () => Svr_ARToolkitAgentThread(0) ).Start();
		new Thread( () => Svr_ARToolkitAgentThread(1) ).Start();
		new Thread( () => Svr_ARToolkitAgentThread(2) ).Start();
		new Thread( () => Svr_ARToolkitAgentThread(3) ).Start();

	}
	public void Svr_ARToolkitAgentThread(int camID)
	{
		Thread.CurrentThread.IsBackground = true;//These threads will not prevent application termination

		UdpClient udpClient = clients [ camID ];

		/* run your code here */ 
		while (true)
		{
			try {

				IPEndPoint ep = new IPEndPoint(IPAddress.Any,0);

				byte [] byteArray = udpClient.Receive ( ref ep );

				string returnData = Encoding.ASCII.GetString(byteArray);
				//Length 71

				//Recreate the TransformationMatrix
				float [] floatArray = new float[16]; //Manual number matching hte sender
				System.Buffer.BlockCopy ( byteArray, 0, floatArray, 0, 16*4 );
				Matrix4x4 m = new Matrix4x4 ();
				m.m00 = floatArray [ 0 ];
				m.m01 = floatArray [ 1 ];
				m.m02 = floatArray [ 2 ];
				m.m03 = floatArray [ 3 ];
				m.m10 = floatArray [ 4 ];
				m.m11 = floatArray [ 5 ];
				m.m12 = floatArray [ 6 ];
				m.m13 = floatArray [ 7 ];
				m.m20 = floatArray [ 8 ];
				m.m21 = floatArray [ 9 ];
				m.m22 = floatArray [ 10 ];
				m.m23 = floatArray [ 11 ];
				m.m30 = floatArray [ 12 ];
				m.m31 = floatArray [ 13 ];
				m.m32 = floatArray [ 14 ];
				m.m33 = floatArray [ 15 ];


				//De-encode the Tag
				string tag = Encoding.ASCII.GetString ( byteArray.Skip ( 16*4 ).ToArray () );

				OnMarkerTracked ( camID, tag, m );

			} catch(Exception e) {
				Debug.LogError ( "ARToolkitAgentThread hit exception: " + e.Message ) ;
			}
		}
	}


	public void OnMarkerTracked ( int camOffset, string tag, Matrix4x4 transformationMatrix )
	{
		if (testB)
			testB.Svr_SetMarker (transformationMatrix, camOffset);
	}
}