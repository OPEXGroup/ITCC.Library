using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ITCC.HTTP.Client.Utils
{
    public class XmlBodySerializer
    {
        #region properties
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
