using UnityEditor;


[CustomEditor(typeof(WGO), true)] 
public class WGOEditor : Editor {

    public override void OnInspectorGUI() {
        WGO wgo = ( WGO ) target;
        if ( wgo == null )
            return;


        wgo.multi = EditorGUILayout.Toggle( "Multi", wgo.multi );

        if(wgo.multi) {
            SerializedProperty inputs = serializedObject.FindProperty( "inputs" );
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField( inputs, true );
            if ( EditorGUI.EndChangeCheck() )
                serializedObject.ApplyModifiedProperties();


            SerializedProperty outputs = serializedObject.FindProperty( "outputs" );
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField( outputs, true );
            if ( EditorGUI.EndChangeCheck() )
                serializedObject.ApplyModifiedProperties();
            
        } else {
            wgo.input = ( WGO ) EditorGUILayout.ObjectField(  "Input:", wgo.input, typeof( WGO ), true );
            wgo.inputs = new WGO[1];
            wgo.inputs[0] = wgo.input;

            wgo.output = ( WGO ) EditorGUILayout.ObjectField( "Output:", wgo.output, typeof( WGO ), true );
            wgo.outputs = new WGO[1];
            wgo.outputs[0] = wgo.output;
        }

        wgo.acting = EditorGUILayout.Toggle( "Acting", wgo.acting );

        EditorGUILayout.LabelField("WaterIn:", wgo.WaterIn.ToString());

    }

}
