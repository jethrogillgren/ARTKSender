using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickSwitchGameplayObject : BaseGameplayObject {

	private int m_count = 0;

	void OnMouseDown() {
		m_count++;
		GetComponent<Animation>().Play("SpinAnimation");
		JLog (name + " (" + this.GetType() + ") counted to " + m_count );
	}
}