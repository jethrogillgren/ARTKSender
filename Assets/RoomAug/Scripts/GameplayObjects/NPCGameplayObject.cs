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



	public void SetLookTargetToNearestPlayer()
	{
		RoomAugPlayerController closest = Util.GetNearestPlayer (this.transform);
		if (closest)
			lookAt.Target = closest.transform;
//		else
//			Debug.LogWarning (name + " found no Player to look at...");
	}
}