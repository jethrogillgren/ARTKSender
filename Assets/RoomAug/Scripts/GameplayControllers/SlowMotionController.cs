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
	[SyncVar]
	private bool syn_pleaseSlowTime = false ;//Set this on server to make everyone slow-mo.
	[SyncVar]
	private float syn_slowMoTimeScale;
	[SyncVar]
	private float syn_slowMoSpeed;


	//Callees manual slowmo
	public void Svr_PleaseDisableSlowTime()
	{
		if (isClient)
			return;
		
		syn_pleaseSlowTime = false;
	}
	public void Svr_PleaseEnableSlowTime()
	{
		if (isClient)
			return;
		
		syn_pleaseSlowTime = true;
	}


	//Slow down time then back up again in a set manner
	public bool Svr_SlowDownUpDefault()
	{
		if (isClient)
			return false;
		
		if( Time.timeScale == 1 )
		{
			SetDefaultTimes ();
			syn_pleaseSlowTime = true;

			return true;

		}
		else
		{
			Debug.LogWarning ("Asked to Slow Down and back Up but we are already slowing - ignoring");
			return false;

		}

	}

	public bool Svr_SlowDownUpDeer()
	{
		if (isClient)
			return false;
		
		if( Time.timeScale == 1 )
		{
			syn_slowMoTimeScale = 0.08f;
			syn_slowMoSpeed = 1.25f;
			Svr_PleaseEnableSlowTime ();

			Invoke ( "Svr_PleaseDisableSlowTime", syn_slowMoSpeed );

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
		syn_slowMoTimeScale = defaultSlowMoTimeScale;
		syn_slowMoSpeed = defaultSlowMoSpeed;
	}



	protected void Update()
	{
		//If we're asked to slow and we are currently not
		if (syn_pleaseSlowTime && Time.timeScale == 1)
		{
			Debug.Log ("Slowing Time");
			StartCoroutine(SlowTime());
			Time.fixedDeltaTime = 0.02F * Time.timeScale;

		}
		//If we don't want to slow but we are set to slowest
		else if (!syn_pleaseSlowTime && Time.timeScale == syn_slowMoTimeScale)
		{
			StartCoroutine(RestartTime());
			Time.fixedDeltaTime = 0.02F * Time.timeScale;

		}

	}

	IEnumerator SlowTime()
	{
		while (Time.timeScale > syn_slowMoTimeScale)
		{
			Time.timeScale -= 1/syn_slowMoSpeed * Time.unscaledDeltaTime;
			Time.fixedDeltaTime = 0.02F * Time.timeScale;
			yield return null;
		}
		Time.timeScale = syn_slowMoTimeScale;
	}

	IEnumerator RestartTime()
	{
		while (Time.timeScale <1)
		{
			Time.timeScale += 1 / syn_slowMoSpeed * Time.unscaledDeltaTime;
			yield return null;
		}
		Debug.Log ("Resored Time");

		Time.timeScale = 1;
	}

}
