using UnityEngine.Networking;

using UnityEngine;

//TODO does not find disabled instances in the scene
public class LoadServerOrClientScopedPrefab : NetworkBehaviour
{
	public GameObject serverOnlyPrefab;
	public GameObject clientOnlyPrefab;


	public override void OnStartServer ()
	{

		if (serverOnlyPrefab && GameObject.Find ( "ServerOnlyPrefab" ) == null)
			Instantiate ( serverOnlyPrefab );

		if ( GameObject.Find ( "ClientOnlyPrefab" ) )
			Destroy ( GameObject.Find ( "ClientOnlyPrefab"  ) );
	}

	public override void OnStartClient ()
	{
		if (clientOnlyPrefab && GameObject.Find ( "ClientOnlyPrefab" ) == null)
			Instantiate ( clientOnlyPrefab );

		if ( GameObject.Find ( "ServerOnlyPrefab" ) )
			Destroy ( GameObject.Find ( "ServerOnlyPrefab"  ) );
	}
}