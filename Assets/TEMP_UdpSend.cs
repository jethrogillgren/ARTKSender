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


public class TEMP_UdpSend : MonoBehaviour {

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

	// Use this for initialization
	void Start () {
//		udpClient = new UdpClient();
//		IPEndPoint ep = new IPEndPoint ( IPAddress.Parse ( "192.168.0.2" ), portARToolkitAgentBase+ARToolkit_CamID ); // endpoint where server is listening
//		udpClient.Connect ( ep );

		JoinGameAsARToolkitAgent ("192.168.0.2");
	}

	public void JoinGameAsARToolkitAgent( string serverIp ) {

		//Test
		clients[ARToolkit_CamID] = new UdpClient();
		IPEndPoint ep = new IPEndPoint(IPAddress.Parse(serverIp), portARToolkitAgentBase+ARToolkit_CamID); // endpoint where server is listening
		clients[ARToolkit_CamID].Connect(ep);
	}
	
	// Update is called once per frame
	void Update () {
//		byte[] senddata = Encoding.ASCII.GetBytes("Hello World");
//		GetUDPClient().Send(senddata, senddata.Length);
	}
}
