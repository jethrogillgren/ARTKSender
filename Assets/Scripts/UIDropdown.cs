using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Networking;

public class UIDropdown : NetworkBehaviour {

    Dropdown m_dropdown;

    void Awake() {
		
        if (!isClient)
			return;

		
		while ( isClient && !ClientScene.ready)
			Util.JLogErr ("ClientScene not ready while awaking UIDropdown");
		
		Util.JLogErr ("UI Dropdown Awaking");

        m_dropdown = gameObject.GetComponent<Dropdown>();
        m_dropdown.ClearOptions();

		Util.JLogErr ("UI Dropdown Finding Rooms");
		GameplayRoom[] gameplayRooms = FindObjectsOfType<GameplayRoom> ();

		foreach (GameplayRoom gr in gameplayRooms) {
			Util.JLogErr ("UI Dropdown Room: " + gr);
			AddItemToDropdown(gr.name);
        }

		Util.JLogErr ("UI Dropdown Refresh");
        m_dropdown.RefreshShownValue();

        m_dropdown.onValueChanged.AddListener(delegate {
			Util.JLogErr("Teleporting Into: " + m_dropdown.options[m_dropdown.value].text + " | Item: " + m_dropdown.value);
			RoomController gc = FindObjectOfType<RoomController> ();
			gc.unActivate(gc.m_physicalRooms.FirstOrDefault());
			gc.activate( (GameplayRoom) gc.getRoomByName(m_dropdown.options[m_dropdown.value].text), gc.m_physicalRooms.FirstOrDefault() );
        });
		Util.JLogErr ("UI Dropdown Awoken");
    }

    public void SelectOption(string text) {
        int option_id = 0;
        foreach (Dropdown.OptionData data in m_dropdown.options) {
            if (data.text == text) {
                m_dropdown.value = option_id;
                m_dropdown.RefreshShownValue();
            }
            option_id++;
        }
    }

	public void OnValueChanged() {
		
	}

    public void AddItemToDropdown(string gameplayRoomName, bool select_on_add=false) {
		m_dropdown.options.Add(new Dropdown.OptionData(gameplayRoomName));
		Util.JLogErr ("Added Item " + gameplayRoomName + " to Dropdown");
        if (select_on_add) {
			SelectOption(gameplayRoomName);
        }
    }

    public void AddItemToDropdownAndSelect(string item) {
        AddItemToDropdown(item, true);
    }
}
