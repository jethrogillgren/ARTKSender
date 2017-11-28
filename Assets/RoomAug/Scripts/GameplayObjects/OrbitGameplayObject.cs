using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class OrbitGameplayObject : BaseGameplayObject {

	/// <summary>
	/// The earth around which the moon orbits.
	/// </summary>
	[SyncVar]
	public GameObject m_earth;

	/// <summary>
	/// The radius of orbit.
	/// </summary>
	[SyncVar]
	public float m_radius = 2.0f;

	/// <summary>
	/// The speed at which to orbit.
	/// </summary>
	[SyncVar]
	public float m_orbitSpeed = 10.0f;

	[SyncVar]
	public bool m_rotateY = true;

    public override void Start() {
        base.Start();
    }

	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	private void Update()
	{
		if (isClient || !m_earth)
			return;
		
		// Rotate around the Earth
		Transform center = m_earth.transform;
		transform.RotateAround(center.position, transform.up, m_orbitSpeed * Time.deltaTime);

		if(m_rotateY)
			this.gameObject.transform.Rotate (new Vector3 (0, 1, 0));
		
	}
}
