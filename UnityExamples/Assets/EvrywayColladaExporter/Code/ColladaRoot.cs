using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Evryway.ColladaExporter
{


    // example data:
    /*
      <asset>
        <contributor>
            <author></author>
            <authoring_tool>FBX COLLADA exporter</authoring_tool>
            <comments></comments>
        </contributor>
        <created>2016-03-15T09:26:35Z</created>
        <keywords></keywords>
        <modified>2016-03-15T09:26:35Z</modified>
        <revision></revision>
        <subject></subject>
        <title></title>
        <unit meter="0.010000" name="centimeter"></unit>
        <up_axis>Y_UP</up_axis>
        </asset>
    */

    [XmlRoot("COLLADA")]
    public class ColladaRoot
    {
        [XmlAttribute]
        public string version = "1.4.1";

        [XmlElement("asset")]
        public ECXColladaAsset asset = new ECXColladaAsset();

        [XmlText]
        public string Text { get; set; }

        [XmlElement("library_images")]
        public LibraryTextures library_images;

        [XmlElement("library_materials")]
        public LibraryMaterials library_materials;

        [XmlElement("library_effects")]
        public LibraryEffects library_effects;

        [XmlElement("library_geometries")]
        public LibraryGeometries library_geometries;

        [XmlElement("library_visual_scenes")]
        public LibraryVisualScenes library_visual_scenes;

        [XmlElement("scene")]
        public ColladaScene scene;


        string path;

        // texture paths contains known full paths to (previously created) textures on device.
        // names of these textures should partially match the names of textures in the materials.
        public void Construct(GameObject go, List<string> texturePaths)
        {


            // work with a flat list of materials while setting up the material / effects / images libraries.
            List<Material> materials = go.GetComponentsInChildren<MeshRenderer>().SelectMany(mr => mr.sharedMaterials).ToList();


            LogTimer("ConstructImages");
            library_images = new LibraryTextures(materials, texturePaths);
            LogTimerDone();

            LogTimer("ConstructEffects");
            library_effects = new LibraryEffects(materials);
            LogTimerDone();

            LogTimer("ConstructMaterials");
            library_materials = new LibraryMaterials(library_effects);
            LogTimerDone();

            // pass in the root game object, generate all the geometries.
            LogTimer("ConstructGeometries");
            library_geometries = new LibraryGeometries(go, library_materials);
            LogTimerDone();

            LogTimer("ConstructLibraryVisualScenes");
            library_visual_scenes = new LibraryVisualScenes(library_geometries, library_materials);
            LogTimerDone();

            LogTimer("ConstructScene");
            scene = new ColladaScene(library_visual_scenes);
            LogTimerDone();

            Debug.Log("construction complete.");

        }


        public void Save()
        {
            path = Path.GetFullPath(path);
            Debug.Log(path);
            var dn = Path.GetDirectoryName(path);
            if (!Directory.Exists(dn)) Directory.CreateDirectory(dn);

            var dns = "http://www.collada.org/2005/11/COLLADASchema";

            ColladaXML.Write<ColladaRoot>(path, this, dns);
        }

        public ColladaRoot() { }
        public ColladaRoot(string path)
        {
            this.path = path;
        }


        float timer_store;
        string timer_id;
        void LogTimer(string id)
        {
            UnityEngine.Profiling.Profiler.BeginSample(id);
            timer_store = Time.realtimeSinceStartup;
            timer_id = id;

        }

        void LogTimerDone()
        {
            Debug.LogFormat("done {0} in {1}",timer_id, Time.realtimeSinceStartup - timer_store);
            UnityEngine.Profiling.Profiler.EndSample();
        }


    }



    public class ECXColladaAsset
    {


        [XmlElement("contributor")]
        public Contributor contributor = new Contributor();

        public string created = System.DateTime.UtcNow.ToString("o");
        public string modified = System.DateTime.UtcNow.ToString("o");
        public string revision = "1";
        public string subject = string.Empty;
        public string title = string.Empty;

        [XmlElement("unit")]
        public ColladaUnit unit = new ColladaUnit();

        public string up_axis = "Y_UP";
    }

    public class Contributor
    {
        public string author = "Evryway";
        public string authoring_tool = "Everyway EC";
    }

    public class ColladaUnit
    {
        [XmlAttribute]
        public float meter = 1.0f;

        [XmlAttribute]
        public string name = "meter";

    }

}
