using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;


public class SlowMotionController : NetworkBehaviour {


	[Range(0.05f, 1)]
	[SerializeField]  float defaultSlowMoTimeScale = 0.2f;
	[Range (0.1f,10)]
	[SerializeField] float defaultSlowMoSpeed = 2f;

	private float m_slowMoTimeScale = 1.0f;
	public float SlowMoTimeScale {
		get{
			return m_slowMoTimeScale;
		}
		set {
			m_slowMoTimeScale = value;
			Debug.LogError ("Set SlowMo Timescale: " + value);
		}
	}

	[Space]
	private bool pleaseSlowTime = false ;//Everyone triggers seperately to maintain sync

	[HideInInspector]
	protected float slowMoTargetTimeScale; //How slow we go
	protected float slowMoSpeed; //How long it takes to get there


	//Callees manual slowmo
	public void PleaseDisableSlowTime()
	{

		pleaseSlowTime = false;
	}
	public void PleaseEnableSlowTime()
	{

		
		pleaseSlowTime = true;
	}


	//Slow down time then back up again in a set manner
	public bool SlowDownUpDefault()
	{
		
		if( SlowMoTimeScale == 1 )
		{
			SetDefaultTimes ();
			pleaseSlowTime = true;

			return true;

		}
		else
		{
			Debug.LogWarning ("Asked to Slow Down and back Up but we are already slowing - ignoring");
			return false;

		}

	}

	public bool SlowDownUpDeer()
	{

		
		if( SlowMoTimeScale == 1 )
		{
//			slowMoTargetTimeScale = 0.08f;
//			slowMoSpeed = 1.25f;
			PleaseEnableSlowTime ();

			Invoke ( "PleaseDisableSlowTime", slowMoSpeed );

			return true;
		}
		else
		{
//			Debug.LogWarning ("Asked to Slow Down and back Up but we are already slowing - ignoring");
			return false;
		}
	}



	protected void Start()
	{
		SetDefaultTimes ();
	}

	//TODO - Svr_ ?
	protected void SetDefaultTimes()
	{
		slowMoTargetTimeScale = defaultSlowMoTimeScale;
		slowMoSpeed = defaultSlowMoSpeed;
	}



	protected void Update()
	{
		//If we're asked to slow and we are currently not
		if (pleaseSlowTime && SlowMoTimeScale == 1)
		{
			Debug.Log ("Slowing Time");
//			StartCoroutine(SlowTime());
			SlowMoTimeScale = slowMoTargetTimeScale;

//			Time.fixedDeltaTime = 0.02F * slowMoTimeScale;

		}
		//If we don't want to slow but we are set to slowest
		else if (!pleaseSlowTime && SlowMoTimeScale == slowMoTargetTimeScale)
		{
//			StartCoroutine(RestartTime());
//			Time.fixedDeltaTime = 0.02F * slowMoTimeScale;
			SlowMoTimeScale = 1;

		}

	}

	IEnumerator SlowTime()
	{
		while (SlowMoTimeScale > slowMoTargetTimeScale)
		{
			SlowMoTimeScale -= 1/slowMoSpeed * Time.deltaTime;
//			Time.fixedDeltaTime = 0.02F * slowMoTimeScale;
			yield return null;
		}
		SlowMoTimeScale = slowMoTargetTimeScale;
	}

	IEnumerator RestartTime()
	{
		while (SlowMoTimeScale <1)
		{
			SlowMoTimeScale += 1 / slowMoSpeed * Time.deltaTime;
			yield return null;
		}
		Debug.Log ("Resored Time");

		SlowMoTimeScale = 1;
	}

}
