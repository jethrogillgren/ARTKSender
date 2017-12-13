using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MalbersAnimations;
using MalbersAnimations.Utilities;

public class MalbersAnimalGameplayObject : BaseGameplayObject {

	public MWayPoint scareWaypoint;
	public bool lookAtNearestPlayer = true;

	protected LookAt lookAt;

	protected Animal animal;
	protected AnimalAIControl aiControl;

	protected bool scared = false;

	protected SlowMotionController slowMo;


	public override void Start()
	{
		lookAt = GetComponentInChildren<LookAt> ();
		aiControl = GetComponentInChildren<AnimalAIControl> ();
		animal = GetComponentInChildren<Animal> ();

		slowMo = GameObject.Find ( "GameplayController" ).GetComponent<SlowMotionController>();

		if (lookAtNearestPlayer)
			InvokeRepeating ( "SetLookTargetToNearestPlayer", 0, 1 );
		
		base.Start ();
	}


	//Called on server when a player gets close to the deer.
	//It smashes through the fragment wall
	public void ScareOverWall()
	{
		if (scared)
			return;
		
		scared = true;

		animal.Speed3 = true; //Make it run so it can jump.  We assume it has enough time to build up speed.
		aiControl.SetTarget (scareWaypoint.transform);
	}

	public void OnDeerLocomotionOrIdle(Animal animal)
	{
		
	}


	public void OnDeerJump()
	{

		slowMo.Svr_SlowDownUpDeer ();


//
//
//		if (animal.CurrentAnimState.IsTag ( "Jump" ))
//			slowMo.SlowDownUpDeer ();
//		else
//			Debug.LogWarning ("Ignoring Deer Action: " + animal.CurrentAnimState );
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

//	public override void UpdateVisibility()
//	{
//		return;
//	}


	//As base, except we keep our Malbers Animal on Animal Layer
	//Relies on Animals layer being already created
	public override void SetLayer(string roomName = "")
	{
		SetLayerRecursively ( gameObject, LayerMask.NameToLayer ( "Animal" ) );



//		//If not specified, find our current parent
//		if (roomName == null || roomName.Length == 0)
//		{
//			GameplayRoom gr = GetComponentInParent<GameplayRoom>();
//			if (gr)
//				roomName = gr.roomName;
//		}
//
//		if (roomName != null && roomName.Length > 0)
//		{
//			//Set Everything to Animal to start
//			SetLayerRecursively ( gameObject, LayerMask.NameToLayer ( "Animal" ) );

//			//But then make the GameplayObject only the Room Layer.
//			gameObject.layer = LayerMask.NameToLayer ( roomName );
//		}
		
	}
}