//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.IO;
//
//// Exporter - does a load of pre-processing to conform to the type of mesh constructed by EvrywayScanner.
//
//namespace Evryway.ColladaExporter
//{
//
//    public class CombinerGroup
//    {
//        public int total_vertices { get { return combiners.Select(c => c.mesh.vertexCount).Sum(); } }
//        public int total_triangles { get { return combiners.Select(c => (c.mesh.triangles.Length / 3)).Sum(); } }
//        public List<CombineInstance> combiners { get; private set; }
//        public Material material { get; private set; }
//
//        public string Name { get { return string.Format("CG_{0}", id); } }
//
//        public const int MAX_TRIANGLES = 48000;
//        public const int MAX_VERTICES = 60000;
//
//        public int id { get; private set; }
//
//        public CombinerGroup(int id, Material mat)
//        {
//            //Debug.Log(string.Format("new combiner group {0}", id));
//
//            this.id = id;
//            combiners = new List<CombineInstance>();
//            material = mat;
//        }
//        public void Add(CombineInstance ci)
//        {
//            combiners.Add(ci);
//            if (total_triangles > MAX_TRIANGLES)
//            {
//                Debug.LogWarning("trying to put too many triangles into a combiner group. Unity will end up auto-splitting, and that might cause issues.");
//            }
//        }
//
//        public bool Fits(CombineInstance ci)
//        {
//            // always allow a mesh into a group if there's nothing else in the group, even if it's
//            // GINORMOUS.
//
//            if (combiners.Count == 0)
//            {
//                //Debug.Log("first mesh, guaranteed to fit.");
//                return true;
//            }
//
//
//            if ((total_triangles + (ci.mesh.triangles.Length / 3) < MAX_TRIANGLES) &&
//                (total_vertices + (ci.mesh.vertexCount) < MAX_VERTICES))
//            {
//                // mesh fits just fine.
//                //Debug.Log(string.Format("mesh fits."));
//                return true;
//            }
//
//            Debug.LogFormat("mesh won't fit into combiner group : T {0} + {1} -> {2} : V {3} + {4} -> {5}", total_triangles, ci.mesh.triangles.Length ,MAX_TRIANGLES, total_vertices, ci.mesh.vertexCount, MAX_VERTICES );
//            return false;
//        }
//    }
//
//
//
//    public class EvrywayScannerExporter
//    {
//        DynamicScene scene;
//        List<TexturedMesh> textured_meshes;
//        GameObject exportGo;
//        List<GameObject> combinedGos = new List<GameObject>();
//        Dictionary<int, Material> materials = new Dictionary<int, Material>();
//        List<string> texturePaths = new List<string>();
//
//        string export_dir;
//
//        static string internal_root_dir = Application.persistentDataPath + "/AssetExport";
//        static string working_dir = internal_root_dir + "/working";
//        const string default_export_dir = "/sdcard/EvrywayScanner";
//
//        string save_as;
//
//        public EvrywayScannerExporter(DynamicScene scene, List<TexturedMesh> theMeshes, string export_dir = default_export_dir, string save_as = "Collada")
//        {
//            this.scene = scene;
//            this.export_dir = export_dir;
//            this.save_as = save_as;
//            textured_meshes = theMeshes;
//        }
//
//
//        public void Process()
//        {
//
//            // what the HELL do we do here?
//            // I know!
//            //
//            // let's get a list of steps!
//
//
//            // GET ALL THE TEXTUREDMESHES IN THE WORLD RIGHT NOW (DONE)
//            //
//            // ITERATE OVER ALL THE MATERIALS, SAVING OUT THE TEXTURES
//            //
//            // COMBINE ALL THE MESHES INTO A SINGLE MESH
//            //
//            // GLUE THAT ONTO A GAME OBJECT
//            //
//            // SAVE OUT THE GAME OBJECT
//
//            /*
//            var textured_meshes = ECDynamicMesh
//
//
//            */
//            Log(string.Format("Public Service Broadcast: Go! {0}", save_as));
//
//            CreateExportGo();
//            if (!Prepare())
//            {
//                Debug.LogError("failed to prepare gracefully!");
//                return;
//            }
//            ProcessTextures();
//            CombineMeshes();
//
//
//            if (save_as == "Collada")
//            {
//                DoExportCollada();
//            }
//            else if (save_as == "OBJ")
//            {
//                DoExportOBJ();
//            }
//            else
//            {
//                Debug.LogError("unknown save type. TODO : Write this properly!");
//            }
//
//            GenerateArchive();
//            Cleanup();
//
//            Log("all done, thank you for your attention during this briefing.");
//
//            Log("OK, We're off to a good start, play it cool.");
//
//        }
//
//        public void CreateExportGo()
//        {
//            Log("creating GO for export. WE ARE GO!");
//            exportGo = new GameObject("exportGo");
//
//            exportGo.transform.parent = scene.transform;
//            exportGo.transform.localPosition = Vector3.zero;
//            exportGo.transform.localRotation = Quaternion.identity;
//
//            //exportGo.AddComponent<MeshRenderer>();
//            //Log("finished making go.");
//        }
//
//        public bool Prepare()
//        {
//            //Log("preparing");
//
//            // clean out the working directory.
//            Log("clearing out working directory ...");
//            if (Directory.Exists(working_dir)) Directory.Delete(working_dir, recursive: true);
//            Directory.CreateDirectory(working_dir);
//
//            Log("working directory clear, verifying materials ...");
//
//
//            // prepare the materials list
//            Log(string.Format("looking at {0} meshes", textured_meshes.Count));
//
//            if (textured_meshes == null)
//            {
//                Debug.LogError("textured meshes is null in prepare!");
//                return false;
//            }
//
//            if (textured_meshes.Any( tm => tm == null) )
//            {
//                Debug.LogError("found null textured mesh during export.");
//                return false;
//            }
//
//            if (textured_meshes.Any( tm => tm.atlas == null) )
//            {
//                Debug.LogError("didn't find an atlas on a textured mesh during export.");
//                return false;
//            }
//
//            // get the atlases from all textured meshes.
//            var atlases = new List<Atlas>(textured_meshes.Select(tm => tm.atlas).Distinct());
//
//            //Log(string.Format("using {0} atlases", atlases.Count));
//
//            foreach (var atlas in atlases)
//            {
//                if (atlas.tex == null)
//                {
//                    Debug.LogWarning(string.Format("trying to save RenderTexture during mesh export, but the texture on atlas {0} isn't valid.", atlas.id));
//                    continue;
//                }
//
//                var mat = new Material(atlas.material);
//                mat.SetTexture("_MainTex", atlas.tex);
//
//                //Log(string.Format("storing material {0}", mat.name));
//                materials[atlas.id] = mat;
//            }
//            return true;
//        }
//
//        public void ProcessTextures()
//        {
//
//            // iterate over all the materials, getting the main texture object, which should be a rendertexture
//            // assuming the materials are NOT named correctly, let's name them here.
//            Log("processing textures ...");
//            int mat_idx = 0;
//            foreach (var kvp in materials)
//            {
//                //var grid = kvp.Key;
//                var mat = kvp.Value;
//                var tt = mat.GetTexture("_MainTex");
//                var rt = tt as RenderTexture;
//                var path = string.Format("{0}/textures/{1}_maintex.png", working_dir, mat.name);
//                var ok = TextureUtils.WriteToDevice(rt, path);
//                if (ok)
//                {
//                    texturePaths.Add(path);
//                }
//                mat_idx++;
//            }
//            Log("processing textures complete.");
//        }
//
//        public void CombineMeshes()
//        {
//            Log(string.Format("Combining {0} meshes.", textured_meshes.Count));
//
//
//            List<CombinerGroup> combinerGroups = CreateCombinerGroups();
//            //Log("created all combiner groups, now to combine them ...");
//
//            foreach (var cg in combinerGroups)
//            {
//                var cg_go = new GameObject(cg.Name);
//                cg_go.transform.parent = exportGo.transform;
//                cg_go.transform.localPosition = Vector3.zero;
//                cg_go.transform.localRotation = Quaternion.identity;
//
//                var mf = cg_go.AddComponent<MeshFilter>();
//                var mr = cg_go.AddComponent<MeshRenderer>();
//
//                //Debug.LogFormat("cg {0} : contains {1} combiners", cg.id, cg.combiners.Count);
//
//                Mesh bigMesh = new Mesh();
//                bigMesh.CombineMeshes(cg.combiners.ToArray(), mergeSubMeshes: true, useMatrices: true);
//
//                CheckForDegenerateTriangles(bigMesh.triangles.ToList(), cg.Name);
//
//                //Log("meshes combined, assigning ...");
//                mf.sharedMesh = bigMesh;
//                //Log("assigning complete. material ...");
//                mr.sharedMaterial = cg.material;
//                //Log("material complete, combination complete.");
//            }
//
//            Log(string.Format("{0} combiner groups processed.", combinerGroups.Count));
//
//        }
//
//
//
//        public List<CombinerGroup> CreateCombinerGroups()
//        {
//
//            //Log(string.Format("combining by atlas ..."));
//
//            List<CombinerGroup> combinerGroups = new List<CombinerGroup>();
//
//            var atlases = textured_meshes.Select(tm => tm.atlas).Distinct();
//
//            foreach (var atlas in atlases)
//            {
//                var mat = materials[atlas.id];
//                var combinerGroup = new CombinerGroup(combinerGroups.Count, mat);
//
//                var meshes = textured_meshes.FindAll(tm => tm.atlas == atlas);
//                foreach (var m in meshes)
//                {
//                    var ci = new CombineInstance();
//                    ci.mesh = m.mesh;
//                    var mt = m.gameObject.transform;
//                    ci.transform = Matrix4x4.TRS(mt.localPosition, mt.localRotation, Vector3.one);
//                    ci.subMeshIndex = 0;
//
//                    if (!combinerGroup.Fits(ci))
//                    {
//                        // we need to make a new group with the same material / atlas.
//                        combinerGroups.Add(combinerGroup);
//                        combinerGroup = new CombinerGroup(combinerGroups.Count, mat);
//                    }
//                    combinerGroup.Add(ci);
//                }
//                combinerGroups.Add(combinerGroup);
//            }
//
//            return combinerGroups;
//
//        }
//
//
//        public void CheckForDegenerateTriangles(List<int> idxs, string name)
//        {
//            if (idxs.Count % 3 != 0)
//            {
//                Debug.LogWarning(string.Format("triangle list isn't mod 3! {0}", name));
//            }
//
//            for (int i = 0; i < idxs.Count; i += 3)
//            {
//                var a = idxs[i];
//                var b = idxs[i + 1];
//                var c = idxs[i + 2];
//                if ((a == b) || (a == c) || (b == c))
//                {
//                    Debug.LogWarning(string.Format("degenerate triangle! {0} {1} {2} {3}", name, a, b, c));
//                }
//            }
//
//        }
//
//
//        string TSNice()
//        {
//            var ts_nice = System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
//            return ts_nice;
//        }
//
//        public string DoExportCollada()
//        {
//            Log("beginning export (COLLADA FORMAT) ...");
//            var path = string.Format("{0}/scan_{1}.dae", working_dir, TSNice());
//            var dn = Path.GetDirectoryName(path);
//            if (!Directory.Exists(dn)) Directory.CreateDirectory(dn);
//
//            if (!Directory.Exists(export_dir)) Directory.CreateDirectory(export_dir);
//
//            Log(string.Format("exporting to {0}", path));
//            var toexport = new ColladaRoot(path);
//            Log("constructing COLLADA ...");
//
//            var start = Time.realtimeSinceStartup;
//            toexport.Construct(exportGo, texturePaths);
//            Log(string.Format("finished construct in {0}",Time.realtimeSinceStartup - start));
//            Log("saving out asset ...");
//
//            start = Time.realtimeSinceStartup;
//            toexport.Save();
//            Log(string.Format("finished save in {0}",Time.realtimeSinceStartup - start));
//            Log("export complete.");
//            return path;
//        }
//
//        public string DoExportOBJ()
//        {
//            Log("beginning export (OBJ FORMAT) ...");
//            var path = string.Format("{0}/scan_{1}.obj", working_dir, TSNice());
//            var dn = Path.GetDirectoryName(path);
//            if (!Directory.Exists(dn)) Directory.CreateDirectory(dn);
//
//            if (!Directory.Exists(export_dir)) Directory.CreateDirectory(export_dir);
//
//            Log(string.Format("exporting to {0}", path));
//            var toexport = new OBJRoot(path);
//
//            var start = Time.realtimeSinceStartup;
//            toexport.Construct( exportGo, texturePaths);
//            Log(string.Format("finished construct in {0}",Time.realtimeSinceStartup - start));
//
//            start = Time.realtimeSinceStartup;
//            toexport.Save();
//            Log(string.Format("finished save in {0}",Time.realtimeSinceStartup - start));
//
//            Log("export complete.");
//            return path;
//        }
//
//
//        public string GenerateArchive()
//        {
//            var start = Time.realtimeSinceStartup;
//
//            var arch_ts = TSNice();
//
//            var path = string.Format("{0}/scan_{1}.zip", export_dir, arch_ts);
//
//            var dirname = Path.GetDirectoryName(path);
//            var fname = Path.GetFileName(path);
//
//            Log(string.Format("generating archive {0}", fname));
//            Log(string.Format("at path {0}", dirname));
//
//            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(path))
//            {
//                zip.AddDirectory(working_dir, arch_ts);
//                zip.Save();
//            }
//            Log(string.Format("finished GenerateArchive in {0}",Time.realtimeSinceStartup - start));
//            Log(string.Format("archive created."));
//
//            return path;
//        }
//
//        public void Cleanup()
//        {
//            Log("cleaning up ...");
//
//            foreach (var go in combinedGos)
//            {
//                GameObject.Destroy(go);
//            }
//            combinedGos.Clear();
//            combinedGos = null;
//
//
//            GameObject.Destroy(exportGo);
//            exportGo = null;
//            textured_meshes = null;
//            materials = null;
//            Log("cleanup complete.");
//        }
//
//        const bool dolog = true;
//        void Log(string stuff)
//        {
//#pragma warning disable 0162
//            if (!dolog) return;
//            Debug.Log(stuff);
//#pragma warning restore 0612
//        }
//    }
//}
