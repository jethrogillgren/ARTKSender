using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAugMalbersAIAnimalControl : MalbersAnimations.AnimalAIControl {

	protected MalbersAnimalGameplayObject gameplayObject;

	protected override void Start(){
		gameplayObject = GetComponent<MalbersAnimalGameplayObject>();
		base.Start ();
	}

	protected override void Update()
	{
		base.Update ();


		if ( animal.CurrentAnimState.IsTag("Jump") )
		{
			gameplayObject.OnDeerJump ();
		} 
	}


//	//Same as base, except we call out to the MalbersAnimal GameplayObject on Action
//	protected override void DisableAgent()
//	{
//		if ((animal.CurrentAnimState.IsTag("Locomotion") 
//			|| animal.CurrentAnimState.IsTag("Idle")))          //Activate the Agent when the animal is moving
//		{
//			if (!Agent.enabled)
//			{
//				Agent.enabled = true;
//				if(debug) Debug.Log("Enable Agent. Animal " + name + " is Moving");
//				isMoving = false;                               //Important
//
//				//Added jgillgr:  Call out
//				gameplayObject.OnDeerLocomotionOrIdle ( animal );
//			}
//		}
//		else
//		{
//			if (Agent.enabled)                      //Disable the Agent whe is not on Locomotion or Idling
//			{
//				Agent.enabled = false;
//				if (debug) Debug.Log("Disable Agent. Animal "+ name +" is doing an action, jumping or falling");
//
//				//Added jgillgr:  Call out
//				gameplayObject.OnDeerAction ( animal );
//			}
//		}
//
//		if (animal.IsInAir) //Don't rotate if is in the middle of a jump
//		{
//			animal.Move(Vector3.zero);
//		}
//	}

}
