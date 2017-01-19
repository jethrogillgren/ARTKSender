using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;


/*
    https://www.khronos.org/files/collada_reference_card_1_4.pdf

    // can have N library_visual scenes (sticking with one for now)
    // can have N visual scenes in the library? no idea, let's stick with one.
    // require 1 node, can have N nodes.
    // going to skip the <extra> elements for now, not sure if they're required.


// SAMPLE DATA

<library_visual_scenes>
    <visual_scene id="Cube_01" name="Cube_01">
        <node name="pCube1" id="pCube1" sid="pCube1">
            <matrix sid="matrix">
1.000000 0.000000 0.000000 0.000000
0.000000 1.000000 0.000000 0.000000
0.000000 0.000000 1.000000 0.000000
0.000000 0.000000 0.000000 1.000000
            </matrix>
            <instance_geometry url="#pCube1-lib">
                <bind_material>
                    <technique_common>
                        <instance_material symbol="red" target="#red"/>
                        <instance_material symbol="white" target="#white"/>
                        <instance_material symbol="purple" target="#purple"/>
                        <instance_material symbol="green" target="#green"/>
                        <instance_material symbol="yellow" target="#yellow"/>
                        <instance_material symbol="blue" target="#blue"/>
                    </technique_common>
                </bind_material>
            </instance_geometry>
            <extra>
                <technique profile="FCOLLADA">
                    <visibility>1.000000</visibility>
                </technique>
            </extra>
        </node>
        <extra>
            <technique profile="MAX3D">
                <frame_rate>24.000000</frame_rate>
            </technique>
            <technique profile="FCOLLADA">
                <start_time>0.041667</start_time>
                <end_time>8.333333</end_time>
            </technique>
        </extra>
    </visual_scene>
</library_visual_scenes>
scene>
    <instance_visual_scene url="#Cube_01"></instance_visual_scene>
</scene>
*/

namespace Evryway.ColladaExporter
{

    public class LibraryVisualScenes
    {
        // for now - ONE SCENE.

        [XmlElement("visual_scene")]
        public VisualScene visual_scene;

        public LibraryVisualScenes() { }
        public LibraryVisualScenes(LibraryGeometries library_geometries, LibraryMaterials library_materials)
        {
            visual_scene = new VisualScene(library_geometries, library_materials);
        }
    }

    public class VisualScene
    {
        [XmlAttribute("id")]
        public string id;

        [XmlAttribute("name")]
        public string name;

        [XmlElement("node")]
        public List<VSNode> nodes = new List<VSNode>();

        public VisualScene() { }
        public VisualScene(LibraryGeometries library_geometries, LibraryMaterials library_materials)
        {
            name = "EVScene";
            id = GetID(name);

            // for each geom, create a scene node.
            foreach (var geom in library_geometries.geometries)
            {
                var node = new VSNode(geom);
                nodes.Add(node);
            }

        }

        public string GetID(string name) { return string.Format("{0}-scene", name); }
    }

    public class VSNode
    {
        [XmlAttribute("id")]
        public string id;

        //[XmlAttribute("sid")]
        //public string sid;

        [XmlAttribute("name")]
        public string name;

        [XmlElement("matrix")]
        public VSNMatrix matrix;

        [XmlElement("instance_geometry")]
        public InstanceGeometry instance_geometry;



        public VSNode() { }
        public VSNode(ColladaGeometry geometry)
        {
            id = GetID(geometry.name);
            //sid = GetID(geometry.name);             // not sure if this needs to be different to ID...
            name = geometry.name;

            matrix = new VSNMatrix(geometry);
            instance_geometry = new InstanceGeometry(geometry);
        }

        public string GetID(string name)
        {
            return string.Format("{0}-node", name);
        }
    }

    public class VSNMatrix
    {
        [XmlAttribute("sid")]
        public string sid = "matrix";

        [XmlText]
        public string Text { get { return ToText(); } set { Debug.LogError("todo"); } }

        public VSNMatrix() { }
        public VSNMatrix(ColladaGeometry geometry)
        {
            // just using defaults - take the TRS matrix from the geometry / parent GO?
        }

        public string ToText()
        {
            // we want a 4x4 matrix, and for now let's use the identity.
            // could take the position of the game object in real world space or something like that?
            // would need to pass it all the way down.
            return ("\n1 0 0 0\n0 1 0 0\n0 0 1 0\n0 0 0 1\n");
        }
    }

    public class InstanceGeometry
    {
        [XmlAttribute("url")]
        public string geometry_ref;

        [XmlElement("bind_material")]
        public BindMaterial bind_material;

        public InstanceGeometry() { }
        public InstanceGeometry(ColladaGeometry geometry)
        {
            geometry_ref = "#" + geometry.id;
            bind_material = new BindMaterial(geometry);
        }
    }


    /*
                    <bind_material>
                        <technique_common>
                            <instance_material symbol = "red" target="#red"/>
                            <instance_material symbol = "white" target="#white"/>
                            <instance_material symbol = "purple" target="#purple"/>
                            <instance_material symbol = "green" target="#green"/>
                            <instance_material symbol = "yellow" target="#yellow"/>
                            <instance_material symbol = "blue" target="#blue"/>
                        </technique_common>
                    </bind_material>
                    */


    public class BindMaterial
    {
        [XmlElement("technique_common")]
        public BMTechnique technique_common;

        public BindMaterial() { }
        public BindMaterial(ColladaGeometry geometry)
        {
            technique_common = new BMTechnique(geometry);
        }
    }

    public class BMTechnique
    {
        [XmlElement("instance_material")]
        public List<InstanceMaterial> instance_materials = new List<InstanceMaterial>();

        public BMTechnique() { }
        public BMTechnique(ColladaGeometry geometry)
        {
            var tris = geometry.cmesh.triangles;
            foreach (var tri in tris)
            {
                var instance_mat = new InstanceMaterial(tri.Material);
                instance_materials.Add(instance_mat);
            }
        }
    }


    public class InstanceMaterial
    {
        [XmlAttribute("symbol")]
        public string symbol;

        [XmlAttribute("target")]
        public string material_ref;

        [XmlElement("bind_vertex_input")]
        public BindVertexInput bind_vertex_input;


        public InstanceMaterial() { }
        public InstanceMaterial(ColladaMaterial material)
        {
            symbol = material.Symbol;
            material_ref = "#" + material.id;

            // this needs to match up with the effects diffuse texture texcoord attribute.
            // not sure if they need to be material-unique? or effect-unique? possibly!
            bind_vertex_input = new BindVertexInput(ColladaMaterial.UVMap(material.name));
        }
    }

    public class BindVertexInput
    {
        [XmlAttribute("semantic")]
        public string semantic;

        [XmlAttribute("input_semantic")]
        public string input_semantic = MeshSourceNames.Texcoord;

        [XmlAttribute("input_set")]
        public string input_set = "0";      // TODO if we want to support multiple UV sets, this would be the place to fix it.


        public BindVertexInput() { }
        public BindVertexInput(string semantic)
        {
            this.semantic = semantic;
        }

    }


    // The scene itself. references a visual scene from the library.

    public class ColladaScene
    {
        [XmlElement("instance_visual_scene")]
        public InstanceVisualScene instance_visual_scene;

        public ColladaScene() { }

        public ColladaScene(LibraryVisualScenes library_visual_scenes)
        {
            // grab the first scene.
            instance_visual_scene = new InstanceVisualScene(library_visual_scenes);
        }

    }

    public class InstanceVisualScene
    {
        [XmlAttribute("url")]
        public string instance_visual_scene_ref;

        public InstanceVisualScene() { }
        public InstanceVisualScene(LibraryVisualScenes library_visual_scenes)
        {
            instance_visual_scene_ref = "#" + library_visual_scenes.visual_scene.id;
        }
    }
}
