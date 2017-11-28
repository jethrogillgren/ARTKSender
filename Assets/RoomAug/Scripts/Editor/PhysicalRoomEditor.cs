using UnityEditor;

[CustomEditor(typeof(PhysicalRoom))] 
public class PhysicalRoomEditor : Editor {

	public override void OnInspectorGUI()
	{
		PhysicalRoom pr = (PhysicalRoom)target;
		if (pr == null)
			return;

		pr.roomName = EditorGUILayout.TextField("Room Name", pr.roomName);

		pr.registerAnyChildGameplayRoom ();
		if (pr.gameplayRoom)
			EditorGUILayout.LabelField ("Got GameplayRoom", pr.gameplayRoom.roomName);
		else
			pr.m_startGameplayRoom = (GameplayRoom) EditorGUILayout.ObjectField ("Starting GameplayRoom:", pr.m_startGameplayRoom, typeof(GameplayRoom), true);

	}
}
