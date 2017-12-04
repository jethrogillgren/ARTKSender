using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGameplayObject : BaseGameplayObject {

	public bool lookAtNearestPlayer = true;
	private MalbersAnimations.Utilities.LookAt lookAt;

	public override void Start()
	{
		lookAt = GetComponentInChildren<MalbersAnimations.Utilities.LookAt> ();
		
		if (lookAtNearestPlayer)
			InvokeRepeating ( "SetLookTargetToNearestPlayer", 0, 1 );
		
		base.Start ();
	}

//	public void OnTriggerEnter ( Collider collision ) {//Handle usual teleport collission with player trigger
//		Debug.Log(name + " Triggered by: " + collision.name );
//	}

	public void SetLookTargetToNearestPlayer()
	{
		RoomAugPlayerController closest = Util.GetNearestPlayer (this.transform);
		if (closest)
			lookAt.Target = closest.transform;
//		else
//			Debug.LogWarning (name + " found no Player to look at...");
	}

	//As base, except we keep our Malbers Animal on Animal Layer
	//Relies on Animals layer being already created
	public override void SetLayer(string roomName = "")
	{
		//If not specified, find our current parent
		if (roomName == null || roomName.Length == 0)
		{
			GameplayRoom gr = GetComponentInParent<GameplayRoom>();
			if (gr)
				roomName = gr.roomName;
		}

		if (roomName != null && roomName.Length > 0)
		{
			//Set Everything to Animal to start
			SetLayerRecursively ( gameObject, LayerMask.NameToLayer ( "Animal" ) );

			//But then make the GameplayObject only the Room Layer.
			gameObject.layer = LayerMask.NameToLayer ( roomName );
		}
		
	}
}