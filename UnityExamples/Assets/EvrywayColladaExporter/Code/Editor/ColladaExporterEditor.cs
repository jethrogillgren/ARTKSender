using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Evryway.ColladaExporter
{

    public class ColladaExporterEditor
    { 

        [MenuItem("GameObject/Collada/Export", false, 32)]
        public static void ExportGOToCollada()
        {
            var selected = Selection.activeObject as GameObject;
            var path = EditorUtility.SaveFilePanel("Export to Collada", ".", selected.name, "dae");

            var toexport = new ColladaRoot(path);
            var texturePaths = new List<string>();

            toexport.Construct(selected, texturePaths);
            toexport.Save();
        }

        [MenuItem("GameObject/Collada/Archive", false, 33)]
        public static void ExportGOToZip()
        {

            ExportGOToCollada();

            var selected = Selection.activeObject as GameObject;
            var path = EditorUtility.SaveFilePanel("Export to Archive", ".", selected.name, "zip");
            var toexport = new ColladaRoot(path);

            var texturePaths = new List<string>();

            toexport.Construct(selected, texturePaths);
            toexport.Save();

            var working_dir = System.IO.Path.GetDirectoryName(path);

            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(path))
            {
                zip.AddDirectory(working_dir, "exported");
                zip.Save();
            }


        }

        /*
        [MenuItem("EvrywayCore/Utility/XML/TestSave &v")]
        static void XMLTS()
        {
            var path = Application.dataPath + "/Evryway-Core-Assets/Art/Generated/Collada/Test1/cubetest1.dae";
            var cold = new ColladaRoot(path);


            var prefab = Resources.Load("Prefabs/Debug/RoundTripCube") as GameObject;
            var testGo = GameObject.Instantiate(prefab);

            cold.Construct(testGo, null);

            cold.Save();

            AssetDatabase.Refresh();

            GameObject.DestroyImmediate(testGo);
        }
        */

    }
}
