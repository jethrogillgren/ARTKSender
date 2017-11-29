// Based on http://www.tallior.com/find-missing-references-unity/
// It fixes deprecations and checks for missing references every time a new scene is loaded
// Moreover, it inspects missing references in animators and animation frames

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

[InitializeOnLoad]
public static class LatestScenes
{
	static string currentScene;
	static LatestScenes()
	{
		EditorApplication.hierarchyWindowChanged += hierarchyWindowChanged;
	}
	static void hierarchyWindowChanged()
	{
		if (currentScene != EditorSceneManager.GetActiveScene().name)
		{
			FindMissingReferences.FindMissingReferencesInCurrentScene ();
			currentScene = EditorSceneManager.GetActiveScene().name;
		}
	}
}

public static class FindMissingReferences {

	[MenuItem("Tools/Show Missing Object References in all scenes", false, 51)]
	public static void MissingSpritesInAllScenes()
	{
		foreach (var scene in EditorBuildSettings.scenes.Where(s => s.enabled))
		{
			EditorSceneManager.OpenScene(scene.path);
			var objects = Object.FindObjectsOfType<GameObject> ();
			CheckMissingReferences(scene.path, objects);
		}
	}

	[MenuItem("Tools/Show Missing Object References in scene", false, 50)]
	public static void FindMissingReferencesInCurrentScene()
	{
		var objects = Object.FindObjectsOfType<GameObject> ();
		CheckMissingReferences(EditorSceneManager.GetActiveScene().name, objects);
	}

	[MenuItem("Tools/Show Missing Object References in assets", false, 52)]
	public static void MissingSpritesInAssets()
	{
		var allAssets = AssetDatabase.GetAllAssetPaths();
		var objs = allAssets.Select(a => AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();

		CheckMissingReferences("Project", objs);
	}

	public static void CheckMissingReferences(string sceneName, GameObject[] objects)
	{
		foreach (var go in objects)
		{
			var components = go.GetComponents<Component> ();

			foreach (var c in components)
			{
				var so = new SerializedObject(c);
				var sp = so.GetIterator();

				while (sp.NextVisible(true))
				{
					if (sp.propertyType == SerializedPropertyType.ObjectReference)
					{
						if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
						{
							ShowError(FullObjectPath(go), sp.name, sceneName);
						}
					}
				}
				var animator = c as Animator;
				if (animator != null) {
					CheckAnimatorReferences (animator);
				}
			}
		}
	}

	public static void CheckAnimatorReferences(Animator component)
	{
		if (component.runtimeAnimatorController == null) {
			return;
		}
		foreach (AnimationClip ac in component.runtimeAnimatorController.animationClips) {
			var so = new SerializedObject (ac);
			var sp = so.GetIterator ();

			while (sp.NextVisible (true)) {
				if (sp.propertyType == SerializedPropertyType.ObjectReference) {
					if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0) {
						Debug.LogError ("Missing reference found in: " + FullObjectPath (component.gameObject) + "Animation: " + ac.name + ", Property : " + sp.name + ", Scene: " + EditorSceneManager.GetActiveScene ().name);
					}
				}
			}
		}
	}

	static void ShowError (string objectName, string propertyName, string sceneName)
	{
		Debug.LogError("Missing reference found in: " + objectName + ", Property : " + propertyName + ", Scene: " + sceneName);
	}

	static string FullObjectPath(GameObject go)
	{
		return go.transform.parent == null ? go.name : FullObjectPath(go.transform.parent.gameObject) + "/" + go.name;
	}
}