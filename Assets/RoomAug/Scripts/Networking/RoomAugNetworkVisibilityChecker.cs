using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//All GameplayObjects should have one of these.  true shows objects, false hides them.  Used to manage gameplay rooms changing
[RequireComponent(typeof(BaseGameplayObject))]
public class RoomAugNetworkVisibilityChecker : NetworkBehaviour
{

	public bool forceHidden = false;

//	// The OnCheckObservers function is called on the server on each networked object when a new player enters the game.
//	//If it returns true, then that player is added to the object’s observers.
//	public override bool OnCheckObserver(NetworkConnection newObserver)
//	{
//		if (forceHidden)
//			return false;
//
//		// this cant use newObserver.playerControllers[0]. must iterate to find a valid player.
//		GameObject player = null;
//		foreach (var p in newObserver.playerControllers)
//		{
//			if (p != null && p.gameObject != null)
//			{
//				player = p.gameObject;
//				break;
//			}
//		}
//		if (player == null)
//			return false;
//
//		var pos = player.transform.position;
//		return (pos - transform.position).magnitude < visRange;
//	}

	//The OnRebuildObservers function is called on the server when RebuildObservers is invoked. This function expects the set of
	//observers to be populated with the players that can see the object. The NetworkServer then handles sending ObjectHide and
	//ObjectSpawn messages based on the differences between the old and new visibility sets. 
	public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
	{
		if (forceHidden)
		{
			// ensure player can still see themself
			var uv = GetComponent<NetworkIdentity>();
			if (uv.connectionToClient != null)
			{
				observers.Add(uv.connectionToClient);
			}
			return true;
		}


		//Get the Clients who have this object under a PhysicalRoom, and include them.

		//For each Client
			//Find if they have the object under a physical room
				//If true, add that client

//		PhysicalRoom pr = GetComponentInParent<PhysicalRoom>;
//		foreach ( NetworkConnection nc in NetworkServer.connections ) {
//			//Assuming each nc has a single player
//			foreach( PlayerController pc in nc.playerControllers ) {
//				Debug.Log("NC: " + nc.address + " Pc: " + pc.gameObject.name );
//			}
//
//		}

//
//
//		// find players within range
//		switch (checkMethod)
//		{
//		case CheckMethod.Physics3D:
//			{
//				var hits = Physics.OverlapSphere(transform.position, visRange);
//				foreach (var hit in hits)
//				{
//					// (if an object has a connectionToClient, it is a player)
//					var uv = hit.GetComponent<NetworkIdentity>();
//					if (uv != null && uv.connectionToClient != null)
//					{
//						observers.Add(uv.connectionToClient);
//					}
//				}
//				return true;
//			}
//
//		case CheckMethod.Physics2D:
//			{
//				var hits = Physics2D.OverlapCircleAll(transform.position, visRange);
//				foreach (var hit in hits)
//				{
//					// (if an object has a connectionToClient, it is a player)
//					var uv = hit.GetComponent<NetworkIdentity>();
//					if (uv != null && uv.connectionToClient != null)
//					{
//						observers.Add(uv.connectionToClient);
//					}
//				}
//				return true;
//			}
//		}
		return false;
	}

	//This function is invoked on all networked scripts on objects that change visibility state on the host. This allow each
	//script to customize how it should respond, such as by disabling HUD elements or renderers.
	public override void OnSetLocalVisibility(bool vis)
	{
		SetVis(gameObject, vis);
	}

	static void SetVis(GameObject go, bool vis)
	{
		foreach (var r in go.GetComponents<Renderer>())
		{
			r.enabled = vis;
		}
		for (int i = 0; i < go.transform.childCount; i++)
		{
			var t = go.transform.GetChild(i);
			SetVis(t.gameObject, vis);
		}
	}
}