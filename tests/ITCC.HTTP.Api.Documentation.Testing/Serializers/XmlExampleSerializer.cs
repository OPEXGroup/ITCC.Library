// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ITCC.HTTP.API.Interfaces;

namespace ITCC.HTTP.Api.Documentation.Testing.Serializers
{
    internal class XmlExampleSerializer : IExampleSerializer
    {
        public string ExampleHeader => "Xml example";
        public string Serialize(object example)
        {
            using (var stringWriter = new StringWriter())
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true
                };

                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    var xmlSerializer = new XmlSerializer(example.GetType());
                    xmlSerializer.Serialize(xmlWriter, example);
                }
                return stringWriter.ToString();
            }
        }
    }
}
