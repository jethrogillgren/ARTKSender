using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAugFraggedController : FraggedController {

	protected ParticleSystem.MainModule mainFrag;
	protected ParticleSystem.MainModule mainDust;

	protected SlowMotionController slowMotionController;

	public override void Start()
	{
		base.Start ();

		slowMotionController = GetComponentInParent<SlowMotionController> ();

		mainFrag = fragParticles.main;
		mainDust = dustParticles.main;
	}
	public void Update()
	{
		mainFrag.simulationSpeed = slowMotionController.SlowMoTimeScale;
		mainDust.simulationSpeed = slowMotionController.SlowMoTimeScale;
	}
}
