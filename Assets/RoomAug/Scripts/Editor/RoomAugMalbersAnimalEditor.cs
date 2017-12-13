using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CanEditMultipleObjects]
[CustomEditor(typeof(RoomAugMalbersAnimal), true)]
public class RoomAugMalbersAnimalEditor : MalbersAnimations.AnimalEditor {

	protected RoomAugMalbersAnimal roomAugAnimal;

	protected override void DrawAnimalInspector()
	{
		base.DrawAnimalInspector ();

		EditorGUILayout.Space ();

		roomAugAnimal = (RoomAugMalbersAnimal)target;
		roomAugAnimal.slowMoController = (SlowMotionController) EditorGUILayout.ObjectField("SlowMotionController", roomAugAnimal.slowMoController, typeof(SlowMotionController));
		roomAugAnimal.ai = (RoomAugMalbersAIAnimalControl) EditorGUILayout.ObjectField("SlowMotionController", roomAugAnimal.ai, typeof(RoomAugMalbersAIAnimalControl));

	}

}
