using System;
using UnityEngine;
using UnityEngine.Networking;

public class RoomAugNetworkDiscovery : NetworkDiscovery {
	
    public static RoomAugNetworkDiscovery Instance;

    public Action<string, string> onServerDetected;

    void OnServerDetected(string fromAddress, string data)
    {
        if (onServerDetected != null)
        {
            onServerDetected.Invoke(fromAddress, data);
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
		InitializeNetworkDiscovery();
    }


//    void Start()
//    {
//        InitializeNetworkDiscovery();
//    }

    public bool InitializeNetworkDiscovery()
    {
        return Initialize();
    }

    public void StartBroadcasting()
    {
        StartAsServer();
    }

    public void ReceiveBroadcast()
    {
		Debug.Log ("J# Starting as a Client ");
        StartAsClient();
    }

    public void StopBroadcasting()
    {
        StopBroadcast();
    }

    public void SetBroadcastData(string broadcastPayload)
    {
        broadcastData = broadcastPayload;
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        OnServerDetected(fromAddress.Split(':')[3], data);
    }
}
