using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Evryway.ColladaExporter
{
    public class OBJRoot
    {
        public string path { get; private set; }
        public string path_materials { get; private set; }
        public System.Text.StringBuilder sb { get; private set; }
        public List<Material> materials { get; private set; }
        public List<MeshFilter> meshfilters { get; private set; }
        public List<Mesh> meshes { get; private set; }
        public GameObject root { get; private set; }
        public List<string> texturePaths { get; private set; }

        public Dictionary<Material, string> matTexLookup { get; private set; }
        public Dictionary<Mesh, Material> meshMatLookup { get; private set; }
        public Dictionary<MeshFilter, Material> mfMatLookup { get; private set; }
        public Dictionary<Mesh, MeshFilter> meshMfLookup { get; private set; }
        public Dictionary<Mesh, int> meshStartIndices { get; private set; }

        public OBJRoot(string path)
        {
            this.path = path;
            this.path_materials = path.Substring(0,path.Length - 3) + "mtl";
            this.sb = new System.Text.StringBuilder();
        }


        public void Construct(GameObject go, List<string> texturePaths)
        {
            // I've got a game object, which should have a selection of child objects.
            // each of those should be considered one group for material purposes.
            root = go;
            this.texturePaths = texturePaths;

            Prepare();

            ConstructMaterialsFile();
            ConstructGeometryFile();
        }

        public void Prepare()
        {
            // work with a flat list of materials while setting up the material / effects / images libraries.
            materials = root.GetComponentsInChildren<MeshRenderer>().Select(mr => mr.sharedMaterial).ToList();
            // this SHOULD be selectMANY if we want to support multi-material meshes.

            meshfilters = root.GetComponentsInChildren<MeshFilter>().ToList();

            meshes = meshfilters.Select( mf => mf.mesh).ToList();

            // match up the materials to the relevant textures.
            matTexLookup = new Dictionary<Material, string>();

            foreach (var mat in materials)
            {

                var expected = string.Format("textures/{0}_maintex", mat.name);
                var found = texturePaths.Find(q => q.Contains(expected));
                if (found != null) expected = found;
                else expected = expected + ".jpg";

                string tp = string.Format("file://{0}", expected);

                // but actually, I do NOT want the full path at all, because that's giving me a device path.
                // I just want a path rooted at textures.
                var tps = tp.IndexOf("textures/");
                tp = tp.Substring(tps);

                matTexLookup[mat] = tp;
            }

            // EACH MESH SHOULD HAVE ONE MATERIAL. UNTIL I SUPPORT MULTIMATERIALS. WHICH I'M NOT RIGHT NOW.
            meshMatLookup = new Dictionary<Mesh, Material>();
            mfMatLookup = new Dictionary<MeshFilter, Material>();
            meshMfLookup = new Dictionary<Mesh, MeshFilter>();
            foreach (var mf in meshfilters)
            {
                var mesh = mf.mesh;
                var mat = mf.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
                meshMatLookup[mesh] = mat;
                mfMatLookup[mf] = mat;
                meshMfLookup[mesh] = mf;
            }

            meshStartIndices = new Dictionary<Mesh, int>();


        }

        public void ConstructMaterialsFile()
        {

            // need a text file.
            sb.Length = 0;
            sb.Append("# Evryway Scanner material file\n");
            sb.AppendFormat("# contains {0} materials\n",materials.Count);
            foreach (var mat in materials)
            {
                AppendMaterial(mat);
            }

            // we've got it.
            var output = sb.ToString();

            //Debug.LogFormat(output);
            System.IO.File.WriteAllText(path_materials, output);
        }

        // https://en.wikipedia.org/wiki/Wavefront_.obj_file
        public void AppendMaterial(Material mat)
        {
            sb.AppendFormat("# - - - {0}\n",mat.name);
            sb.AppendFormat("newmtl {0}\n",mat.name);
            sb.Append("Ka 1.0 1.0 1.0\n");                // AMBIENT - white
            sb.Append("Kd 1.0 1.0 1.0\n");                // DIFFUSE - white
            sb.Append("illum 1\n");                       // ILLUMINATION MODE (ambient on, diffuse on)

            var mat_path = matTexLookup[mat];

            sb.AppendFormat("map_Kd {0}\n", mat_path);
        }

        public void ConstructGeometryFile()
        {
            int total_vertices = meshes.Sum( mesh => mesh.vertexCount );
            int total_triangles = meshes.Sum( mesh => mesh.triangles.Length );

            var objname = System.IO.Path.GetFileNameWithoutExtension(path);

            sb.Length = 0;
            sb.Append("# Evryway Scanner object file\n");
            sb.AppendFormat("# contains {0} vertices, {1} triangles\n",total_vertices, total_triangles);
            sb.AppendFormat("o EvrywayScanner_{0}\n", objname );

            // put the material library in
            sb.AppendFormat("mtllib {0}\n",System.IO.Path.GetFileName(path_materials));

            // write all vertices
            sb.Append("#--- Vertices\n");
            int tv_idx = 0;
            foreach (var m in meshes)
            {
                meshStartIndices[m] = tv_idx;
                var verts = m.vertices;
                foreach (var v in verts)
                {
                    sb.AppendFormat("v {0} {1} {2}\n",-v.x,v.y,v.z);
                }
                tv_idx += verts.Length;
            }


            // write all normals
            sb.Append("#--- normals\n");
            foreach (var m in meshes)
            {
                var norms = m.normals;
                foreach (var n in norms)
                {
                    sb.AppendFormat("vn {0} {1} {2}\n",-n.x,n.y,n.z);
                }

            }

            // write all UVs
            sb.Append("#--- UVS\n");
            foreach (var m in meshes)
            {
                var uvs = m.uv;
                foreach (var uv in uvs)
                {
                    sb.AppendFormat("vt {0} {1}\n",uv.x, uv.y);
                }
            }

            // write all triangles
            foreach (var m in meshes)
            {

                // write each group for each mesh.
                // each mesh should be in a separate group with a separate material.

                var mat = meshMatLookup[m];
                var mf = meshMfLookup[m];

                sb.AppendFormat("# MESH GROUP {0}\n",mf.name);
                sb.AppendFormat("g {0}\n",mf.name);
                sb.AppendFormat("usemtl {0}\n",mat.name);

                var ts = m.triangles;
                var offset = meshStartIndices[m] + 1;           // OBJ format indices start at 1, not 0.
                for (int i = 0 ; i < ts.Length; i+= 3)
                {
                    var a = ts[i] + offset;
                    var b = ts[i+2] + offset;
                    var c = ts[i+1] + offset;
                    sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",a,b,c);
                }
            }


        }



        public void Save()
        {
            System.IO.File.WriteAllText(path, sb.ToString());

            // clear out the string builder! it's a LOT of memory hanging around.
            sb.Length = 0;
        }

    }
}
