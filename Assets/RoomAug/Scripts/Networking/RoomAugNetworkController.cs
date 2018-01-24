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
	public UdpClient ARToolkit_UdpClient;
//	public IPEndPoint[] remoteEPs = new IPEndPoint[4]; //Used on server and Client


	public static string serverIPAddr;

	// Use this for initialization
	void Start () {
        Application.runInBackground = true;

		RoomAugNetworkDiscovery.Instance.onServerDetected += OnReceiveBraodcast;
        RoomAugNetworkDiscovery.Instance.ReceiveBroadcast();

    }

    void OnDestroy() {
		RoomAugNetworkDiscovery.Instance.onServerDetected -= OnReceiveBraodcast;
	}
	

	public void OnReceiveBraodcast(string fromIp, string data) {
		serverIPAddr = fromIp;

        Debug.Log("J# Joining Game at " + serverIPAddr + " as a ARToolkit Camera Sender");
        RoomAugNetworkDiscovery.Instance.StopBroadcasting();

        //Test
        ARToolkit_UdpClient = new UdpClient();
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(serverIPAddr), Util.portARToolkitAgentBase + ARToolkit_CamID); // endpoint where server is listening
        Debug.LogError("Connecting out to " + IPAddress.Parse(serverIPAddr).ToString() + " : " + (Util.portARToolkitAgentBase + ARToolkit_CamID));
        ARToolkit_UdpClient.Connect(ep);
    }
}
