using UnityEngine;

[ExecuteInEditMode]
public class EditorOnlyTransformTracker : MonoBehaviour
{
	[Space]
	public GameObject objectToTrack;

	#if UNITY_EDITOR
	void Update()
	{
		if (objectToTrack)
		{
			transform.localPosition = objectToTrack.transform.localPosition;
			transform.localRotation = objectToTrack.transform.localRotation;
		}
	}
	#endif
}