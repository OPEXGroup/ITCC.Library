// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ITCC.HTTP.Client.Utils
{
    public class XmlBodySerializer
    {
        #region IBodySerializer

        public Encoding Encoding => Encoding.UTF8;
        public string ContentType => "application/xml";
        public string Serialize(object data)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    var xmlSerializer = new XmlSerializer(data.GetType());
                    xmlSerializer.Serialize(xmlWriter, data);
                }
                return stringWriter.ToString();
            }
        }
        #endregion
    }
}
