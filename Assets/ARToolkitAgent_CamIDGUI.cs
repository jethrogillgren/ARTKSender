using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARToolkitAgent_CamIDGUI : MonoBehaviour {

	Text timeText;
	RoomAugNetworkController con;

    public ARTrackedObject arto1 ;
    public ARTrackedObject arto2 ;
    public ARTrackedObject arto3 ;
    public ARTrackedObject arto4 ;

    //time last seen
    float gui1 = 0;
    float gui2 = 0;
    float gui3 = 0;
    float gui4 = 0;

    public int secsToShowFOundInGUI = 1;


    void Start()
	{
		 timeText = gameObject.GetComponent<Text>();
		 con = FindObjectOfType<RoomAugNetworkController> ();
	}

	// Use this for initialization
	void Update () {
        if (!timeText)
            return;
        
		if (con)
		{
			timeText.text = "Cam ID: " + con.ARToolkit_CamID;
            if (con.ARToolkit_UdpClient != null)
                timeText.text += "  Connected to: " + RoomAugNetworkController.serverIPAddr;

        } else
        {
            timeText.text = "Loading Scene";
        }

        timeText.text += "\n";

        if (gui1 > (Time.time - secsToShowFOundInGUI))
            timeText.text += "Y";
        else
            timeText.text += "_";

        if (gui2 > (Time.time - secsToShowFOundInGUI))
            timeText.text += "Y";
        else
            timeText.text += "_";

        if (gui3 > (Time.time - secsToShowFOundInGUI))
            timeText.text += "Y";
        else
            timeText.text += "_";

        if (gui4 > (Time.time - secsToShowFOundInGUI))
            timeText.text += "Y";
        else
            timeText.text += "_";
    }
	
    public void MarkSeen(string tag)
    {
        if(tag.Equals(arto1.MarkerTag) )
        {
            gui1 = Time.time;
        }
        else if (tag.Equals(arto2.MarkerTag))
        {
            gui2 = Time.time;
        }
        else if (tag.Equals(arto3.MarkerTag))
        {
            gui3 = Time.time;
        }
        else if (tag.Equals(arto4.MarkerTag))
        {
            gui4 = Time.time;
        }
    }
    

}
