using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Evryway.ColladaExporter
{

    public class LibraryMaterials
    {

        [XmlElement("material")]
        public List<ColladaMaterial> materials = new List<ColladaMaterial>();

        Dictionary<Material, ColladaMaterial> matLookup = new Dictionary<Material, ColladaMaterial>();
        public ColladaMaterial GetMaterial(Material mat) { return matLookup[mat]; }

        Dictionary<Material, ColladaEffect> matEffectLookup = new Dictionary<Material, ColladaEffect>();
        public ColladaEffect GetEffect(Material mat) { return matEffectLookup[mat]; }

        public LibraryMaterials() { }

        public LibraryMaterials(LibraryEffects library_effects)
        {
            foreach (var effect in library_effects.effects)
            {
                var colmat = new ColladaMaterial(effect);
                matEffectLookup[effect.unity_material] = effect;
                matLookup[effect.unity_material] = colmat;
                materials.Add(colmat);
            }
        }
    }


    public class ColladaMaterial
    {

        [XmlAttribute]
        public string id;

        [XmlAttribute]
        public string name;

        [XmlElement("instance_effect")]
        public InstanceEffect instance_effect;


        public ColladaMaterial() { }
        public ColladaMaterial(ColladaEffect effect)
        {
            name = effect.name;
            id = GetID(name);

            // patch in the instance effect.
            instance_effect = new InstanceEffect(effect);
        }


        public string GetID(string name) { return string.Format("{0}-mat", name); }
        public string Symbol { get { return string.Format("{0}-sym", name); } }

        // really don't like this. 
        public static string UVMap(string name) { return string.Format("{0}-{1}", name, MeshSourceNames.UVMap); }
    }


    public class InstanceEffect
    {
        [XmlAttribute("url")]
        public string effect_ref;


        public InstanceEffect() { }
        public InstanceEffect(ColladaEffect effect)
        {
            this.effect_ref = "#" + effect.id;
        }
    }

}
