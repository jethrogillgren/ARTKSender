using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace Evryway.ColladaExporter
{

    // some example data :

    /*
        <effect id="red-fx" name="red">
          <profile_COMMON>
            <technique sid="standard">
              <lambert>
                <emission>
                  <color sid="emission">0.000000  0.000000 0.000000 1.000000</color>
                </emission>
                <ambient>
                  <color sid="ambient">0.000000  0.000000 0.000000 1.000000</color>
                </ambient>
                <diffuse>
                  <texture texture="file1-sampler" texcoord="CHANNEL0">
                    <extra>
                      <technique profile="MAYA">
                        <wrapU sid="wrapU0">TRUE</wrapU>
                        <wrapV sid="wrapV0">TRUE</wrapV>
                        <blend_mode>NONE</blend_mode>
                      </technique>
                    </extra>
                  </texture>
                </diffuse>
                <transparent opaque="RGB_ZERO">
                  <color sid="transparent">0.000000  0.000000 0.000000 1.000000</color>
                </transparent>
                <transparency>
                  <float sid="transparency">1.000000</float>
                </transparency>
              </lambert>
            </technique>
          </profile_COMMON>
        </effect>

    */





    public class LibraryEffects
    {
        [XmlElement("effect")]
        public List<ColladaEffect> effects = new List<ColladaEffect>();




        public LibraryEffects() { }

        public LibraryEffects(List<Material> materials)
        {

            foreach (var mat in materials)
            {

                // set up the surface and the sampler2D for the texture
                // (referenced in the effect diffuse)
                var surface = new EffectSurface(mat.name);
                var surfaceparam = new EffectNewParam(mat, surface, null);

                var sampler = new EffectSampler2D(mat.name);
                var samplerparam = new EffectNewParam(mat, null, sampler);

                var texparams = new List<EffectNewParam> { surfaceparam, samplerparam };



                var emcol = new EffectColour("emission");
                var amcol = new EffectColour("ambient");
                var trancol = new EffectColour("transparent");

                var em = new EffectEmission(emcol);
                var amb = new EffectAmbient(amcol);



                var tr = new EffectTexRef(samplerparam, mat);
                var dif = new EffectDiffuse(tr);
                var tranc = new EffectTransparent(trancol);
                var tflot = new CFloat("transparency", 1.0f);
                var tranv = new EffectTransparency(tflot);

                var lamb = new EffectLambert(em, amb, dif, tranc, tranv);
                var tech = new EffectTechnique(lamb);



                var prof = new EffectProfile(texparams, tech);


                effects.Add(new ColladaEffect(prof, mat));
            }

        }


    }

    public class ColladaEffect
    {
        [XmlAttribute("id")]
        public string id;

        [XmlAttribute("name")]
        public string name;

        [XmlElement("profile_COMMON")]
        public EffectProfile profile;

        [XmlIgnore]
        public Material unity_material { get; private set; }

        public ColladaEffect() { }

        public ColladaEffect(EffectProfile inProfile, Material mat)
        {
            unity_material = mat;
            name = mat.name;
            id = GetID();
            profile = inProfile;
        }

        public string GetID()
        {
            return name + "-fx";
        }

    }


    public class EffectProfile
    {
        [XmlElement("newparam")]
        public List<EffectNewParam> newparams = new List<EffectNewParam>();


        [XmlElement("technique")]
        public EffectTechnique technique;


        public EffectProfile() { }
        public EffectProfile(List<EffectNewParam> inParams, EffectTechnique inTechnique)
        {
            newparams = inParams;
            technique = inTechnique;
        }
    }


    public class EffectNewParam
    {
        [XmlAttribute("sid")]
        public string sid;

        [XmlElement("surface")]
        public EffectSurface surface;

        [XmlElement("sampler2D")]
        public EffectSampler2D sampler2D;


        public EffectNewParam() { }
        public EffectNewParam(Material mat, EffectSurface surface, EffectSampler2D sampler2D)
        {
            this.sid = surface != null ? GetSIDSurface(mat.name) : GetSIDSampler2D(mat.name);
            this.surface = surface;
            this.sampler2D = sampler2D;
        }

        string GetSIDSurface(string name) { return string.Format("{0}-surface", name); }
        string GetSIDSampler2D(string name) { return string.Format("{0}-sampler", name); }

    }

    public class EffectSurface
    {
        [XmlAttribute("type")]
        public string stype = "2D";

        [XmlElement("init_from")]
        public TextElement init_from;

        public EffectSurface() { }
        public EffectSurface(string name)
        {
            init_from = new TextElement(name + "-image");
        }
    }

    public class EffectSampler2D
    {
        [XmlElement("source")]
        public TextElement source;

        public EffectSampler2D() { }
        public EffectSampler2D(string name)
        {
            source = new TextElement(name + "-surface");
        }
    }






    public class EffectTechnique
    {
        [XmlAttribute("sid")]
        public string sid = "standard";

        [XmlElement("lambert")]
        public EffectLambert lambert;

        public EffectTechnique() { }
        public EffectTechnique(EffectLambert inLambert)
        {
            lambert = inLambert;
        }
    }

    public class EffectLambert
    {
        [XmlElement("emission")]
        public EffectEmission emission;

        [XmlElement("ambient")]
        public EffectAmbient ambient;

        [XmlElement("diffuse")]
        public EffectDiffuse diffuse;

        [XmlElement("transparent")]
        public EffectTransparent transparent;

        [XmlElement("transparency")]
        public EffectTransparency transparency;

        public EffectLambert() { }
        public EffectLambert(EffectEmission inEmission, EffectAmbient inAmbient, EffectDiffuse inDiffuse,
            EffectTransparent inTransparent, EffectTransparency inTransV)
        {
            emission = inEmission;
            ambient = inAmbient;
            diffuse = inDiffuse;
            transparent = inTransparent;
            transparency = inTransV;
        }
    }


    public class EffectColour
    {
        Color col = Color.black;

        [XmlAttribute("sid")]
        public string sid = "unknown";

        [XmlText]
        public string Text { get { return string.Format("{0} {1} {2} {3}", col.r, col.g, col.b, col.a); } set { ParseCol(value); } }

        public EffectColour() { }
        public EffectColour(string inSid)
        {
            sid = inSid;
        }

        public void Set(Color col) { this.col = col; }

        public void ParseCol(string text) { Debug.LogError("TODO"); }

    }


    public class EffectEmission
    {
        [XmlElement("color")]
        public EffectColour colour;

        public EffectEmission() { }
        public EffectEmission(EffectColour inColour) { colour = inColour; }
    }

    public class EffectAmbient
    {
        [XmlElement("color")]
        public EffectColour colour;

        public EffectAmbient() { }
        public EffectAmbient(EffectColour inColour) { colour = inColour; }
    }





    public class EffectTransparent
    {
        [XmlAttribute("opaque")]
        public string opaque = "RGB_ZERO";


        [XmlElement("color")]
        public EffectColour colour;

        public EffectTransparent() { }
        public EffectTransparent(EffectColour inColour) { colour = inColour; }



    }


    /*
        <diffuse>
          <texture texture="file1-image" texcoord="CHANNEL0">
            <extra>
              <technique profile="MAYA">
                <wrapU sid="wrapU0">TRUE</wrapU>
                <wrapV sid="wrapV0">TRUE</wrapV>
                <blend_mode>NONE</blend_mode>
              </technique>
            </extra>
          </texture>
        </diffuse>
    */

    public class EffectDiffuse
    {
        [XmlElement("texture")]
        public EffectTexRef texture;

        public EffectDiffuse() { }
        public EffectDiffuse(EffectTexRef inTexRef) { texture = inTexRef; }

    }


    public class EffectTexRef
    {
        [XmlAttribute("texture")]
        public string texture_sampler;

        [XmlAttribute("texcoord")]
        public string texcoord;

        // could add in the extra stuff for UV wraps here, but I'm hoping I don't need to.

        public EffectTexRef() { }
        public EffectTexRef(EffectNewParam samplerParam, Material mat)
        {
            // appears to just be the ID of the texture.
            // in fact, it's the SAMPLER for the texture.

            texture_sampler = (samplerParam != null) ?
                 samplerParam.sid :
                 "Unknown-texture-id-sampler";

            texcoord = ColladaMaterial.UVMap(mat.name);

        }

    }

    // PRETTY SURE I DON'T REALLY NEED THIS
    /*
                <transparency>
                  <float sid="transparency">1.000000</float>
                </transparency>
    */

    public class EffectTransparency
    {
        [XmlElement("float")]
        public CFloat flot;

        public EffectTransparency() { }
        public EffectTransparency(CFloat inFloat) { flot = inFloat; }

    }

    public class CFloat
    {
        [XmlAttribute]
        public string sid;

        [XmlText]
        public string Text { get { return string.Format("{0}", val); } set { Debug.LogError("todo"); } }
        float val;

        public CFloat() { }
        public CFloat(string sid, float val) { this.sid = sid; this.val = val; }
    }


}