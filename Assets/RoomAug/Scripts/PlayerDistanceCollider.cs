using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Is only enabled on Servers, by the PlayerController
[RequireComponent(typeof(Collider))]
public class PlayerDistanceCollider : MonoBehaviour {

	public enum Type {
		Close, //Small Collider
		Near   //Bigger Collider
	}
	public Type type = Type.Close;

	RoomAugPlayerController playerController;

	void Start()
	{
		playerController = GetComponentInParent<RoomAugPlayerController> ();

	}


	public void OnTriggerEnter ( Collider collision )
	{

		if (type == Type.Near && collision.name == "RoomAugDeerNPC")
			playerController.ScareDeer ();
	}
}