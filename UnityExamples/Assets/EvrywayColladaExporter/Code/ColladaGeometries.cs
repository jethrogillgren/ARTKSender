using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Evryway.ColladaExporter
{

    public class LibraryGeometries
    {
        [XmlElement("geometry")]
        public List<ColladaGeometry> geometries = new List<ColladaGeometry>();

        public LibraryGeometries() { }
        public LibraryGeometries(GameObject go, LibraryMaterials library_materials)
        {

            // we need to pass relevant materials to every mesh geometry.

            var allgos = AllGos(go);

            foreach (var subgo in allgos)
            {
                var mr = subgo.GetComponent<MeshRenderer>();
                var mf = subgo.GetComponent<MeshFilter>();

                // this Go doesn't have both a meshrenderer and a meshfilter.

                // if mf is something, but mr is null - do we have a skinned mesh renderer?

                if (mf != null && mr == null)
                {
                    var smr = subgo.GetComponent<SkinnedMeshRenderer>();
                    Debug.LogFormat("Found a SkinnedMeshRenderer : {0} - not currently handled!", smr.gameObject.name);
                }


                if (mr == null || mf == null) continue;

                var mesh = mf.sharedMesh;
                var umats = mr.sharedMaterials;
                var materials = umats.Select(umat => library_materials.GetMaterial(umat)).ToList();

                var geom = new ColladaGeometry(subgo.name, mesh, materials);

                geometries.Add(geom);
            }
        }


        List<GameObject> AllGos(GameObject root)
        {
            List<GameObject> gos = new List<GameObject>();
            foreach (Transform c in root.transform)
            {
                gos.AddRange(AllGos(c.gameObject));
            }
            gos.Add(root);
            return gos;
        }

    }

    public class ColladaGeometry
    {
        [XmlAttribute("id")]
        public string id;

        [XmlAttribute("name")]
        public string name;

        [XmlElement("mesh")]
        public ColladaMesh cmesh;


        public ColladaGeometry() { }

        public ColladaGeometry(string name, Mesh umesh, List<ColladaMaterial> materials)
        {
            this.name = name;
            id = GetID(name);
            cmesh = new ColladaMesh(name, umesh, materials);
        }

        public static string GetID(string name)
        {
            return string.Format("{0}-geom", name);
        }

    }


    // MESH IS THE GUTS OF THE DATA SET.
    // we have multiple source elements, each with float arrays, for POSITION, NORMAL, UV?

    public static class MeshSourceNames
    {
        public const string Position = "POSITION";
        public const string Normal = "NORMAL";
        public const string UV0 = "UV0";
        public const string Texcoord = "TEXCOORD";
        public const string Vertex = "VERTEX";
        public const string UVMap = "UVMap";
    }


    public class ColladaMesh
    {

        [XmlElement("source")]
        public List<MeshSource> sources = new List<MeshSource>();

        [XmlElement("vertices")]
        public MeshVertices vertices;

        [XmlElement("triangles")]
        public List<MeshTriangles> triangles = new List<MeshTriangles>();

        public ColladaMesh() { }
        public ColladaMesh(string name, Mesh umesh, List<ColladaMaterial> materials)
        {

            // number of materials and number of submeshes should correlate. check that in the materials pass?
            // or check it here?
            int submeshcount = umesh.subMeshCount;
            if (materials.Count != submeshcount)
            {
                Debug.LogError("mismatch between materials count and submesh count");
            }

            // INVERT THE X in the POSITION and NORMAL source arrays, because Collada is RHS

            var pdata = umesh.vertices.SelectMany(v => new List<float> { -v.x, v.y, v.z }).ToList();
            var ndata = umesh.normals.SelectMany(n => new List<float> { -n.x, n.y, n.z }).ToList();
            var uvdata = umesh.uv.SelectMany(_uv => new List<float> { _uv.x, _uv.y }).ToList();

            var vecParamNames = new List<string> { "X", "Y", "Z" };
            var uvParamNames = new List<string> { "S", "T" };

            var position = new MeshSource(pdata, name, MeshSourceNames.Position, 3, vecParamNames);
            var normal = new MeshSource(ndata, name, MeshSourceNames.Normal, 3, vecParamNames);
            var uv = new MeshSource(uvdata, name, MeshSourceNames.UV0, 2, uvParamNames);

            sources.Add(position);
            sources.Add(normal);
            sources.Add(uv);

            vertices = new MeshVertices(name, position.id);

            var inputs = new List<MeshInput>
        {
            new MeshInput(MeshSourceNames.Vertex, position.id, 0),
            new MeshInput(MeshSourceNames.Normal, normal.id, 1),
            new MeshInput(MeshSourceNames.Texcoord, uv.id, 2, 0),
        };

            triangles = new List<MeshTriangles>();
            for (int i = 0; i < submeshcount; i++)
            {
                var ts = umesh.GetTriangles(i);
                // materials should match 1-to-1.
                var mat = materials[i];

                var tris = new MeshTriangles(ts.ToList(), mat, inputs);
                triangles.Add(tris);
            }
        }


    }

    /*

          <mesh>
            <source id="pCube1-POSITION">
              <float_array id = "pCube1-POSITION-array" count="78">
    -1.662922 0.001578 1.66308
    ...
    -1.662922 1.652640 0.012020
              </float_array>
              <technique_common>
                <accessor source = "#pCube1-POSITION-array" count="26" stride="3">
                  <param name = "X" type="float"/>
                  <param name = "Y" type="float"/>
                  <param name = "Z" type="float"/>
                </accessor>
              </technique_common>
            </source>

    */

    // originally, had a selection of derived classes here, but then the namespaces go all fucked up,
    // and I couldn't find any references that let me fix that - so back to one mesh source class.


    public class MeshSource
    {
        [XmlAttribute("id")]
        public string id;

        [XmlElement("float_array")]
        public CFloatArray float_array;

        [XmlElement("technique_common")]
        public FloatArrayTechnique technique;

        string extension;
        public MeshSource() { }
        public MeshSource(List<float> vals, string name, string extension, int stride, List<string> tparamNames)
        {
            this.extension = extension;

            id = SourceID(name);


            string sourceArrayName = SourceArrayID(name);
            float_array = new CFloatArray(vals, sourceArrayName, stride);

            // set up the technique;
            technique = new FloatArrayTechnique(tparamNames, sourceArrayName, stride, vals);



        }
        string SourceID(string name) { return string.Format("{0}-{1}", name, extension); }
        string SourceArrayID(string name) { return string.Format("{0}-array", SourceID(name)); }

    }




    public class CFloatArray
    {
        [XmlAttribute("id")]
        public string id;

        [XmlAttribute("count")]
        public string count;

        [XmlText]
        public string Text { get { return _text; } set { Debug.LogError("todo"); } }

        string _text;

        List<float> vals = new List<float>();
        int stride = 1;

        public CFloatArray() { }
        public CFloatArray(List<float> vals, string id, int stride)
        {
            this.id = id;
            this.vals = vals;
            this.stride = stride;
            this.count = vals.Count.ToString();
            this._text = ValsFormatted();
        }

        string ValsFormatted()
        {
            //var pfn = string.Format("Collada:ValsFormatted_{0}",vals.Count);

            //Debug.LogFormat("ColladaGeometries:CFloatArray:Start {0}",vals.Count);

            // floats are going to be max 9 chars (sign, dot, 4 prec, 3 int) so say 10 plus 3 whitespace.
            int capacity = 14 * vals.Count;
            var sb = ColladaExportUtilities.sb;
            sb.Length = 0;
            sb.EnsureCapacity(capacity);
            sb.Append("\n");
            for (int i = 0; i < vals.Count; i += stride)
            {
                switch (stride)
                {
                    default:
                    case 0:
                        break;
                    case 1: { sb.AppendFormat("{0}\n", vals[i]); break; }
                    case 2: { sb.AppendFormat("{0} {1}\n", vals[i], vals[i + 1]); break; }
                    case 3: { sb.AppendFormat("{0} {1} {2}\n", vals[i], vals[i + 1], vals[i + 2]); break; }
                    case 4: { sb.AppendFormat("{0} {1} {2} {3}\n", vals[i], vals[i + 1], vals[i + 2], vals[i + 3]); break; }
                }
            }
            var s = sb.ToString();

            //Debug.LogFormat("done.");

            return s;
        }
    }

    /*
                <accessor source="#pCube1-POSITION-array" count="26" stride="3">
                  <param name="X" type="float"/>
                  <param name="Y" type="float"/>
                  <param name="Z" type="float"/>
                </accessor>
    */

    public class FloatArrayTechnique
    {
        [XmlElement("accessor")]
        public FloatArrayAccessor accessor;

        public FloatArrayTechnique() { }
        public FloatArrayTechnique(List<string> tparams, string sourceName, int stride, List<float> data)
        {

            // check that the data matches the stride
            if (data.Count % stride != 0)
            {
                Debug.LogError("got the wrong amount of data in ECXColladaFloatArrayTechnique! " + sourceName);
            }

            accessor = new FloatArrayAccessor(sourceName, data.Count / stride, stride, tparams);
        }

    }

    public class FloatArrayAccessor
    {
        [XmlAttribute("source")]
        public string sourceName;

        [XmlAttribute("count")]
        public string count;

        [XmlAttribute("stride")]
        public string stride;

        [XmlElement("param")]
        public List<CFloatParam> cparams = new List<CFloatParam>();

        public FloatArrayAccessor() { }
        public FloatArrayAccessor(string sourceName, int count, int stride, List<string> tparams)
        {
            this.sourceName = "#" + sourceName;
            this.count = count.ToString();
            this.stride = stride.ToString();

            foreach (var tpn in tparams)
            {
                cparams.Add(new CFloatParam(tpn));
            }
        }
    }

    public class CFloatParam
    {
        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("type")]
        public string type;

        public CFloatParam() { }
        public CFloatParam(string name)
        {
            this.name = name;
            this.type = "float";
        }
    }


    //----------------------------------------------
    // Verts and Triangles
    //----------------------------------------------


    /*
            one set of verts, but can have N sets of triangles, each one is a submesh.
            we should be separating up the mesh into N submeshes if we have N materials.

            <vertices id="pCube1-VERTEX">
              <input semantic="POSITION" source="#pCube1-POSITION"/>
            </vertices>

    */

    public class MeshVertices
    {
        [XmlAttribute("id")]
        public string id;

        [XmlElement("input")]
        public MeshInput input;

        const string extension = "VERTICES";



        public MeshVertices() { }
        public MeshVertices(string name, string source)
        {
            id = GetID(name);
            input = new MeshInput(MeshSourceNames.Position, source);

        }

        public string GetID(string name)
        {
            return string.Format("{0}-{1}", name, extension);
        }
    }

    public class MeshInput
    {
        [XmlAttribute("semantic")]
        public string semantic;

        [XmlAttribute("source")]
        public string source;

        [XmlAttribute("offset")]
        public string offset;

        [XmlAttribute("set")]
        public string set;

        public MeshInput() { }

        public MeshInput(string semantic, string source, int offset, int set)
        {

            this.semantic = semantic;
            this.source = "#" + source;
            this.offset = offset >= 0 ? offset.ToString() : null;
            this.set = set >= 0 ? set.ToString() : null;
        }

        public MeshInput(string semantic, string source) : this(semantic, source, -1) { }
        public MeshInput(string semantic, string source, int offset) : this(semantic, source, offset, -1) { }

    }


    /*
            <triangles count="8" material="red">
                <input semantic="VERTEX" offset="0" source="#pCube1-VERTEX"/>
                <input semantic="NORMAL" offset="1" source="#pCube1-Normal0"/>
                <input semantic="TEXCOORD" offset="2" set="0" source="#pCube1-UV0"/>
                <p> 0 0 0 1 1 3 3 2 67 3 3 67 1 4 3 4 5 15 1 6 1 2 7 2 4 8 73 4 9 73 2 10 2 5 11 72 3 12 59 4 13 60 6 14 62 6 15 62 4 16 60 7 17 61 4 18 4 5 19 5 7 20 17 7 21 17 5 22 5 8 23 16</p>
            </triangles>

            // we're not stripping or fanning (that would be <tristrips> or <trifans>)
            // the numbers in the p array are, each VERT in the triangle, the corresponding VERTEX, NORMAL and TEXCOORD index.
            // so with the above array, you've got a triangle with 9 indices : V0 is VERTEX_0, NORMAL_0, TEXCOORD_0. V1 is VERTEX_1, NORMAL 1, TEXCOORD 3. V2 is VERTEX 3, NORMAL 2, TEXCOORD_67.
            // this repeats for every triangle vertex.
            // quite why they didn't have multiple index lists, I dunno - I just love the offset and stride bullshit, don't you?

            // now, if we're round-tripping a Unity asset, what you'll find is that the VERTEX, NORMAL and TEXCOORD indices will all be the same
            // (is that correct? Unity throws a wobbly if they're not the same length, from whenever I've tried) which means the numbers should, in theory,
            // all be the same index value.

            // materials! great! I expected "material" to be a ref to a material ID, but it's not.
            // it's some kind of by-name binding - maybe to the symbols listed in the scene?
            // let's see what happens ...

     */


    public class MeshTriangles
    {
        [XmlAttribute("count")]
        public string count;

        [XmlAttribute("material")]
        public string material_symbol;

        [XmlElement("input")]
        public List<MeshInput> inputs;

        [XmlElement("p")]
        public MeshTriangleIndices indices;

        ColladaMaterial material;
        public ColladaMaterial Material { get { return material; } }

        public MeshTriangles() { }

        public MeshTriangles(List<int> triangles, ColladaMaterial material, List<MeshInput> inputs)
        {
            this.inputs = inputs;
            this.material = material;

            // number of TRIANGLES - so triangle vertex index list / 3.
            count = (triangles.Count / 3).ToString();
            material_symbol = material.Symbol;

            indices = new MeshTriangleIndices(triangles, inputs);
        }
    }

    public class MeshTriangleIndices
    {
        [XmlText]
        public string Text { get { return _text; } set { Debug.LogError("TODO"); } }

        string _text;

        List<int> indices;
        List<MeshInput> inputs;

        public MeshTriangleIndices() { }
        public MeshTriangleIndices(List<int> indices, List<MeshInput> inputs)
        {
            this.inputs = inputs;

            // Unity retains parity in the length of the vertex array, the length of the normal array and the length of the UV arrays.
            // the index into each of these for any specific vertex should be the same idx.
            // we need to provide an index in order for each of the inputs.

            // sanity check here that the offset in each of the inputs is correct? (they should be at increasing values starting at 0)
            // sanity check here that the original mesh does have arrays of the same length for V, N and UV in all cases?

            // I need to INVERT THE WINDING ORDER because COLLADA is RHS (Unity is LHS coordinate system)
            // and I'm inverting the X axis in the POSITION and NORMAL stream.

            var reverse_indices = new List<int>();
            var ci = 0;
            var count = indices.Count;

            while (ci < count)
            {
                reverse_indices.Add(indices[ci]);
                reverse_indices.Add(indices[ci + 2]);
                reverse_indices.Add(indices[ci + 1]);
                ci += 3;
            }
            this.indices = reverse_indices;

            // previously, I did the process of tripling up the vertices (due to the inputs count) here.
            // however, I'm going to do it in the SB instead - working with less data, still the same
            // amount of string formatting.

            _text = ToText();

        }

        string ToText()
        {
            // this shiz is going to be SUPER SLOW.
            // this is likely where all the slowdown is right now.


            /*

            var iz = indices.Select(i => i.ToString()).ToList();

            // chunk up the index list into each triangle (3 verts * number of inputs per line)
            var chunked = iz.ChunkBy(3 * inputs.Count);
            var triangles = chunked.Select(chunk => string.Join(" ", chunk.ToArray()));
            var output = "\n" + string.Join("\n", triangles.ToArray()) + "\n";
            return output;

            */


            // reverse the winding order HERE? (currently doing it above)
            if (this.inputs.Count != 3)
            {
                Debug.LogError(string.Format("collada export - we're expecting 3 inputs here (Pos, Nrm, UV), got {4}", this.inputs.Count));
            }

            var ci = 0;
            var count = indices.Count;

            // three entries per number (assume 4 digits per number) plus 6 whitespace = capacity! (give or take)
            var capacity = (count * 3 * 20); 

            var output = ColladaExportUtilities.sb;
            output.Length = 0;
            output.EnsureCapacity(capacity);
            while (ci < count)
            {
                output.AppendFormat("{0} {0} {0}    {1} {1} {1}    {2} {2} {2}\n", indices[ci], indices[ci + 1], indices[ci + 2]);
                ci += 3;
            }

            var s = output.ToString();

            return s;



        }

    }

}
