using UnityEngine;

[ExecuteInEditMode]
public class EditorOnlyGameplayGhostTracker : MonoBehaviour {

	[Space]
	public GameplayRoom roomToTrack;

	#if UNITY_EDITOR
	void Update()
	{
		if (roomToTrack)
		{
			transform.localPosition = roomToTrack.offsetLocalPosition;
		}
	}
	#endif
}
