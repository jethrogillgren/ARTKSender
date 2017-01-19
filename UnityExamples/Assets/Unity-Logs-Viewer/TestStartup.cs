using System.Collections;
using UnityEngine;
using Tango;

public class AreaLearningStartup : MonoBehaviour, ITangoLifecycle
{
	private TangoApplication m_tangoApplication;

	public void Start()
	{
		Debug.Log ("JethroExample Starting Up...");
		m_tangoApplication = FindObjectOfType<TangoApplication>();
		if (m_tangoApplication != null)
		{
			m_tangoApplication.Register(this);
			m_tangoApplication.RequestPermissions();
		}
	}

	public void OnTangoPermissions(bool permissionsGranted)
	{
		if (permissionsGranted)
		{
			AreaDescription[] list = AreaDescription.GetList();
			AreaDescription mostRecent = null;
			AreaDescription.Metadata mostRecentMetadata = null;
			Debug.Log ("JethroExample List of AreaDescriptions: ");
			Debug.Log (list);
			if (list != null && list.Length > 0)
			{
				// Find and load the most recent Area Description
				mostRecent = list[0];
				mostRecentMetadata = mostRecent.GetMetadata();
				foreach (AreaDescription areaDescription in list)
				{
					AreaDescription.Metadata metadata = areaDescription.GetMetadata();
					if (metadata.m_dateTime > mostRecentMetadata.m_dateTime)
					{
						mostRecent = areaDescription;
						mostRecentMetadata = metadata;
					}
				}

				m_tangoApplication.Startup(mostRecent);
			}
			else
			{
				// No Area Descriptions available.
				Debug.LogError("No area descriptions available.");
			}
		}
	}

	public void OnTangoServiceConnected()
	{
	}

	public void OnTangoServiceDisconnected()
	{
	}
}