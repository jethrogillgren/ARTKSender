using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Xml;
using System.Xml.Serialization;

namespace Evryway.ColladaExporter
{


    public class LibraryTextures
    {

        [XmlElement("image")]
        public List<ColladaTexture> images = new List<ColladaTexture>();


        Dictionary<Material, ColladaTexture> matTexLookup = new Dictionary<Material, ColladaTexture>();

        public LibraryTextures() { }
        public LibraryTextures(List<Material> materials, List<string> texturePaths)
        {
            if (texturePaths == null) texturePaths = new List<string>();

            foreach (var mat in materials)
            {

                // get the path to the main texture 
                // we'll need to create these files at some point ...

                // we want to path to a (relative) location, which should be "textures/..."
                // the incoming list texturePaths should contain a file called textures/<MATNAME>_maintex.<EXT>
                // with MATNAME being the material name (assuming they are all unique!) and
                // EXT being jpg, png, etc.
                // if we can't find it in the texturePaths list, just fake it - this is the kind
                // of thing people expect to fix up later anyway ...

                // really, we should be doing this PROPERLY.
                // http://docs.unity3d.com/ScriptReference/ShaderUtil.html 
                // http://docs.unity3d.com/ScriptReference/ShaderUtil.ShaderPropertyType.html  -> TexEnv
                // but that's only going to work in the editor.
                // if I want to support arb export of arb texture properties (not just _MainTex) at runtime,
                // some other method will be required ...


                var expected = string.Format("textures/{0}_maintex", mat.name);
                var found = texturePaths.Find(q => q.Contains(expected));
                if (found != null) expected = found;
                else expected = expected + ".jpg";

                string tp = string.Format("file://{0}", expected);

                // but actually, I do NOT want the full path at all, because that's giving me a device path.
                // I just want a path rooted at textures.
                var tps = tp.IndexOf("textures/");
                tp = tp.Substring(tps);

                var img = new ColladaTexture(mat.name, tp);
                images.Add(img);
                matTexLookup[mat] = img;
            }
        }

        public ColladaTexture GetTexForMat(Material mat)
        {
            return matTexLookup.ContainsKey(mat) ? matTexLookup[mat] : null;
        }

    }

    public class ColladaTexture
    {

        [XmlAttribute("id")]
        public string id;

        [XmlAttribute("name")]
        public string name;

        [XmlElement("init_from")]
        public TextElement init_from;

        public ColladaTexture(string name, string path)
        {
            this.id = GetID(name);
            this.name = name;
            this.init_from = new TextElement(path);
        }

        public ColladaTexture() { }

        public string GetID(string name) { return string.Format("{0}-image", name); }
    }

    public class TextElement
    {
        [XmlText]
        public string Text { get { return text; } set { Debug.LogError("todo"); } }

        string text;

        public TextElement() { }
        public TextElement(string text)
        {
            this.text = text;
        }
    }

}
