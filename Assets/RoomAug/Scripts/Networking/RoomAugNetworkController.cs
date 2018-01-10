using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using System.Net;
using System.Threading;
using System.Text;
using System;
using System.Linq;
using System.Net.Sockets;

public class RoomAugNetworkController : MonoBehaviour {

	public bool ARToolkitSender = false;
	public int ARToolkit_CamID = 0; //Only applicable for ARToolkitAgents
	public UdpClient[] clients = new UdpClient[4]; //Used on server and Client
//	public IPEndPoint[] remoteEPs = new IPEndPoint[4]; //Used on server and Client

	public readonly int portARToolkitAgentBase = 41600;

	private PandaCubeController cubeController = null;
	public PandaCubeController GetCubeController()
	{
		if (cubeController == null)
			cubeController = GameObject.FindObjectOfType<PandaCubeController>();
		return cubeController;
	}


	public UdpClient GetUDPClient()
	{
		return GetUDPClient(ARToolkit_CamID);
	}
	public UdpClient GetUDPClient(int camId)
	{
		return clients [ camId ];
	}

	// Use this for initialization
	void Start () {
		RoomAugNetworkDiscovery.Instance.onServerDetected += OnReceiveBraodcast;

		GetCubeController ();

		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {

			if (ARToolkitSender)
				ListenForGame();
			else
				HostGame ();
			
		} else if (Application.platform == RuntimePlatform.Android) {
			ListenForGame ();
		} else {
			Debug.LogError ("J# RoomAugNetworkController Unknown Platform - not Initialising/");
		}

	}
	
	void OnDestroy() {
		RoomAugNetworkDiscovery.Instance.onServerDetected -= OnReceiveBraodcast;
	}
	
	public void HostGame() {

		Debug.Log ("J# Hosting Game");

		RoomAugNetworkDiscovery.Instance.StartBroadcasting();
		NetworkManager.singleton.StartServer();

		//Listen for the Direct Connections.  Manually add for more room cameras
		for ( int i = 0; i < clients.Length; i++ )
		{
			clients [ i ] = new UdpClient ( portARToolkitAgentBase + i );
//			clients [ i ].Connect (remoteEPs [ i ]);//The Agent connects to us

			Debug.LogError ( "Created clients and Endpoints for CamID" + i );
		}

		GetCubeController().Svr_StartARToolkitAgentRecieve ();

//		while (true)
//		{
//			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 11000);
//			byte[] data = udpServer.Receive(ref remoteEP); // listen on port 11000
//			Debug.Log("receive data from " + remoteEP.ToString());
////			udpServer.Send(new byte[] { 1 }, 1, remoteEP); // reply back
//		}
	}

//	public void ARToolkitAgentThread(PandaCubeController cubeCon, int camID)
//	{
//		Thread.CurrentThread.IsBackground = true;//These threads will not prevent application termination
//
//		/* run your code here */ 
//		while (true)
//		{
//			try {
//				
//				Debug.LogError ( "Thread running for " + camID );
//
//				IPEndPoint ep = new IPEndPoint(IPAddress.Any,0);
//				Debug.LogError ("Listening to " + IPAddress.Any.ToString() + " : " +  (portARToolkitAgentBase + camID));
//
//				byte [] byteArray = clients [ camID ].Receive ( ref ep );
//
//				string returnData = Encoding.ASCII.GetString(byteArray);
//				Debug.LogError ("Whole data (byteArray.Length: " + byteArray.Length + ") and as ASCII: " + returnData);
//				//Length 71
//
//				//Recreate the TransformationMatrix
//				float [] floatArray = new float[16]; //Manual number matching hte sender
//				Debug.LogError("Made float[]  .Length " + floatArray.Length ); //Length 17
//				System.Buffer.BlockCopy ( byteArray, 0, floatArray, 0, 16*4 );
//				Debug.LogError("Made BlockCopy");
//				Matrix4x4 m = new Matrix4x4 ();
//				m.m00 = floatArray [ 0 ];
//				m.m01 = floatArray [ 1 ];
//				m.m02 = floatArray [ 2 ];
//				m.m03 = floatArray [ 3 ];
//				m.m10 = floatArray [ 4 ];
//				m.m11 = floatArray [ 5 ];
//				m.m12 = floatArray [ 6 ];
//				m.m13 = floatArray [ 7 ];
//				m.m20 = floatArray [ 8 ];
//				m.m21 = floatArray [ 9 ];
//				m.m22 = floatArray [ 10 ];
//				m.m23 = floatArray [ 11 ];
//				m.m30 = floatArray [ 12 ];
//				m.m31 = floatArray [ 13 ];
//				m.m32 = floatArray [ 14 ];
//				m.m33 = floatArray [ 15 ];
//
//				Debug.LogError("Reconstructed the Matrix4x4");
//	
//				//De-encode the Tag
//				string tag = Encoding.ASCII.GetString ( byteArray.Skip ( 16*4 ).ToArray () );
//	
//				Debug.LogError ( "Passing data out to : " + cubeCon );
//				cubeCon.OnMarkerTracked ( camID, tag, m );
//
//			} catch(Exception e) {
//				Debug.LogError ( "ARToolkitAgentThread hit exception: " + e.Message ) ;
//			}
//		}
//	}

	public void ListenForGame() {
		Debug.Log ("J# Looking for Game");

		RoomAugNetworkDiscovery.Instance.ReceiveBroadcast();
	}


	public void JoinGameAsARToolkitAgent( string serverIp ) {

		Debug.Log ( "J# Joining Game at " + serverIp + " as a ARToolkit Camera Sender" );
		RoomAugNetworkDiscovery.Instance.StopBroadcasting ();

		//Test
		clients[ARToolkit_CamID] = new UdpClient();
		IPEndPoint ep = new IPEndPoint(IPAddress.Parse(serverIp), portARToolkitAgentBase+ARToolkit_CamID); // endpoint where server is listening
		Debug.LogError ("Connecting out to " + IPAddress.Parse(serverIp).ToString() + " : " +  (portARToolkitAgentBase+ARToolkit_CamID));
		clients[ARToolkit_CamID].Connect(ep);

//		// send data
//		while (true)
//		{
//			client.Send ( new byte[] { 1, 2, 3, 4, 5 }, 5 );
//			Debug.Log ( "Send UDP Stream" );
//		}
	}

	public void JoinGameAsPlayer( string serverIp ) {
		Debug.Log ( "J# Joining Game at " + serverIp + " as a Player" );

		NetworkManager.singleton.networkAddress = serverIp;
		NetworkManager.singleton.StartClient ();
		RoomAugNetworkDiscovery.Instance.StopBroadcasting ();
	}

	public void OnReceiveBraodcast(string fromIp, string data) {
		if (ARToolkitSender)
			JoinGameAsARToolkitAgent ( fromIp );
		else
			JoinGameAsPlayer ( fromIp );
	}
}
