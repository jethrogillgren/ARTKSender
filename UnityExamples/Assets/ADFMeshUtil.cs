using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Evryway.ColladaExporter;

public class ADFMeshUtil {

	private string m_meshSavePath; //Path where the generated meshes are saved

	public ADFMeshUtil() {
		m_meshSavePath = Application.persistentDataPath + "/meshes";
		Directory.CreateDirectory(m_meshSavePath);
	}

	/// <summary>
	/// Convert a unity mesh to an Area Description mesh.
	/// </summary>
	/// <returns>The Area Description mesh.</returns>
	/// <param name="uuid">The Area Description UUID.</param>
	/// <param name="mesh">The Unity mesh.</param>
	public AreaDescriptionMesh _UnityMeshToAreaDescriptionMesh(string uuid, Mesh mesh)
	{
		AreaDescriptionMesh saveMesh = new AreaDescriptionMesh();
		saveMesh.m_uuid = uuid;
		saveMesh.m_vertices = mesh.vertices;
		saveMesh.m_triangles = mesh.triangles;
		return saveMesh;
	}

	/// <summary>
	/// Convert an Area Description mesh to a unity mesh.
	/// </summary>
	/// <returns>The unity mesh.</returns>
	/// <param name="saveMesh">The Area Description mesh.</param>
	public Mesh _AreaDescriptionMeshToUnityMesh(AreaDescriptionMesh saveMesh)
	{
		Mesh mesh = new Mesh();
		mesh.vertices = saveMesh.m_vertices;
		mesh.triangles = saveMesh.m_triangles;
		mesh.RecalculateNormals();
		return mesh;
	}

	/// <summary>
	/// Serialize an Area Description mesh to file.
	/// </summary>
	/// <param name="saveMesh">The Area Description mesh to serialize.</param>
	public void _SerializeAreaDescriptionMesh(AreaDescriptionMesh saveMesh)
	{
		XmlSerializer serializer = new XmlSerializer(typeof(AreaDescriptionMesh));
		FileStream file = File.Create(m_meshSavePath + "/" + saveMesh.m_uuid);
		serializer.Serialize(file, saveMesh);
		file.Close();
	}

//	/// <summary>
//	/// Serialize an Area Description mesh to file as Collada (.dae)
//	/// </summary>
//	/// <param name="saveMesh">The Area Description mesh to serialize.</param>
//	public void _SerializeAreaDescriptionMeshAsCollada(AreaDescriptionMesh saveMesh)
//	{
//		_SerializeUnityMeshAsCollada ( _AreaDescriptionMeshToUnityMesh(saveMesh), saveMesh.m_uuid );
//	}
//	public void _SerializeUnityMeshAsCollada(Mesh mesh, string name) {
//		ColladaRoot collada_root = new ColladaRoot(m_meshSavePath + "/" + name + ".dae" );
//
//		collada_root.Construct(mesh., null);
//
//		colllada_root.Save();
//	}

	/// <summary>
	/// Deserialize an Area Description mesh from file.
	/// </summary>
	/// <returns>The loaded Area Description mesh.</returns>
	/// <param name="uuid">The UUID of the associated Area Description.</param>
	public AreaDescriptionMesh _DeserializeAreaDescriptionMesh(string uuid)
	{
		if (File.Exists(m_meshSavePath + "/" + uuid))
		{
			XmlSerializer serializer = new XmlSerializer(typeof(AreaDescriptionMesh));
			FileStream file = File.Open(m_meshSavePath + "/" + uuid, FileMode.Open);
			JLog ("Loaded ADF Mesh from " + m_meshSavePath + "/" + uuid);
			AreaDescriptionMesh saveMesh = serializer.Deserialize(file) as AreaDescriptionMesh;
			file.Close();
			return saveMesh;
		} else {
			JLogErr ("No ADF Mesh found for " + uuid + " in " + m_meshSavePath );
		}

		return null;
	}

	/// <summary>
	/// Xml container for vertices and triangles from extracted mesh and linked Area Description.
	/// </summary>
	[XmlRoot("AreaDescriptionMesh")]
	public class AreaDescriptionMesh
	{
		/// <summary>
		/// The UUID of the linked Area Description.
		/// </summary>
		public string m_uuid;

		/// <summary>
		/// The mesh vertices.
		/// </summary>
		public Vector3[] m_vertices;

		/// <summary>
		/// The mesh triangles.
		/// </summary>
		public int[] m_triangles;
	}


	private void JLog(string val) {
		Debug.Log ("J# " + val);
	}
	private void JLogErr(string val) {
		Debug.LogError ("J# " + val);
	}
}
