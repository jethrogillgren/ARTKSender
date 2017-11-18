using UnityEngine.Networking;

using UnityEngine;

public class LoadServerOrClientScopedPrefab : NetworkBehaviour
{
  public GameObject serverOnlyPrefab;
  public GameObject clientOnlyPrefab;


    public void Start() {
        Debug.LogError("############");
    }

  public override void OnStartServer()
  {
      Debug.LogError("SERVER PREFAB LOADING");

      if(serverOnlyPrefab)
        Instantiate(serverOnlyPrefab);
  }
      
  public override void OnStartClient()
    {
    Debug.LogError("CLIENT PREFAB LOADING");
      if(clientOnlyPrefab)
        Instantiate(clientOnlyPrefab);
    }
}