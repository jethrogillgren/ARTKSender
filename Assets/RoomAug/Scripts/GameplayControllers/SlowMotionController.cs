using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;


public class SlowMotionController : NetworkBehaviour {


	[Range(0.05f, 1)]
	[SerializeField]  float defaultSlowMoTimeScale = 0.2f;
	[Range (0.1f,10)]
	[SerializeField] float defaultSlowMoSpeed = 2f;


	[Space]
	private bool pleaseSlowTime = false ;//Everyone triggers seperately to maintain sync
	private float slowMoTimeScale;
	private float slowMoSpeed;


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
		
		if( Time.timeScale == 1 )
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

		
		if( Time.timeScale == 1 )
		{
			slowMoTimeScale = 0.08f;
			slowMoSpeed = 1.25f;
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
		slowMoTimeScale = defaultSlowMoTimeScale;
		slowMoSpeed = defaultSlowMoSpeed;
	}



	protected void Update()
	{
		//If we're asked to slow and we are currently not
		if (pleaseSlowTime && Time.timeScale == 1)
		{
			Debug.Log ("Slowing Time");
			StartCoroutine(SlowTime());
			Time.fixedDeltaTime = 0.02F * Time.timeScale;

		}
		//If we don't want to slow but we are set to slowest
		else if (!pleaseSlowTime && Time.timeScale == slowMoTimeScale)
		{
			StartCoroutine(RestartTime());
			Time.fixedDeltaTime = 0.02F * Time.timeScale;

		}

	}

	IEnumerator SlowTime()
	{
		while (Time.timeScale > slowMoTimeScale)
		{
			Time.timeScale -= 1/slowMoSpeed * Time.unscaledDeltaTime;
			Time.fixedDeltaTime = 0.02F * Time.timeScale;
			yield return null;
		}
		Time.timeScale = slowMoTimeScale;
	}

	IEnumerator RestartTime()
	{
		while (Time.timeScale <1)
		{
			Time.timeScale += 1 / slowMoSpeed * Time.unscaledDeltaTime;
			yield return null;
		}
		Debug.Log ("Resored Time");

		Time.timeScale = 1;
	}

}
