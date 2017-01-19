using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;


namespace Evryway.ColladaExporter
{

    public static class ColladaXML
    {
        // what a load of roundabout to get this working - but here's the deal.
        // I want the default namespace to be the collada one.
        // I want the encoding to be UTF-8.
        // I want whitespace. glorious whitespace.
        // I don't want any other stuff in the file (xsi and xsd namespaces appear if you don't add
        // the XmlSerializerNamespace to the serialize call)

        public static bool Write<T>(string path, T theobj, string defaultns)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, defaultns);
            var serialised = new XmlSerializer(typeof(T), defaultns);
            var enc = System.Text.Encoding.UTF8;


            using (var stream = new MemoryStream())
            {
                XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings()
                {
                    CloseOutput = false,
                    Encoding = enc,
                    OmitXmlDeclaration = false,
                    Indent = true,
                    NewLineChars = "\n",
                };

                Debug.Log("writer created, performing serialise ...");
                using (System.Xml.XmlWriter xw = System.Xml.XmlWriter.Create(stream, xmlWriterSettings))
                {
                    serialised.Serialize(xw, theobj, ns);
                }
                Debug.Log("serialise complete, saving to device.");

                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                var sw = new StreamReader(stream);
                var output = sw.ReadToEnd();

                File.WriteAllText(path, output);

                Debug.Log("write XML to file complete.");
            }
            return true;
        }

    }
}
