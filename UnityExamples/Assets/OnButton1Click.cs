using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnButton1Click : MonoBehaviour, IPointerClickHandler {

	public void OnPointerClick(PointerEventData eventData) // 3
	{
		Debug.Log("JETHRO Button1 Click");
	}
}
