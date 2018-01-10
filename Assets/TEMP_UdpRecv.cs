using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using System.Net;
using System.Threading;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class TEMP_UdpRecv : MonoBehaviour {


	private const int portARToolkitAgentBase = 41700;
	private const int ARToolkit_CamID = 1;

	public UdpClient[] clients = new UdpClient[4]; //Used on server and Client

	public UdpClient GetUDPClient()
	{
		return GetUDPClient(ARToolkit_CamID);
	}
	public UdpClient GetUDPClient(int camId)
	{
		return clients [ camId ];
	}

	void Start(){
		
		for ( int i = 0; i < clients.Length; i++ )
		{
			clients [ i ] = new UdpClient ( portARToolkitAgentBase + i );

			Debug.LogError ( "Created client for CamID" + i );
		}
		//When put inside the loop, i would be passed inconsistently
		new Thread ( () => ARToolkitAgentThread ( 0 ) ).Start ();
		new Thread ( () => ARToolkitAgentThread ( 1 ) ).Start ();
		new Thread ( () => ARToolkitAgentThread ( 2 ) ).Start ();
		new Thread ( () => ARToolkitAgentThread ( 3 ) ).Start ();

	}

	public void ARToolkitAgentThread(int camID)
	{
		Thread.CurrentThread.IsBackground = true;//These threads will not prevent application termination

		/* run your code here */ 
		while (true)
		{
			IPEndPoint ep = new IPEndPoint(IPAddress.Any,0);

			byte [] byteArray = clients [ camID ].Receive ( ref ep );

			string returnData = Encoding.ASCII.GetString(byteArray);
			Debug.LogError (returnData);
		}
	}






//	// Use this for initialization
//	void Start () {
//
//		clients[0] = new UdpClient(portARToolkitAgentBase+ARToolkit_CamID);
//
//		Thread thdUDPServer = new Thread(new
//			ThreadStart(serverThread));
//		thdUDPServer.Start();
//	}
//
//
//	public void serverThread()
//	{
//		while(true)
//		{
//			IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any,0);
//			byte[] receiveBytes = clients[0].Receive(ref RemoteIpEndPoint);
//			string returnData = Encoding.ASCII.GetString(receiveBytes);
//			Debug.LogError (returnData);
//		}
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//	}
}
