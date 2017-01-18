// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ITCC.HTTP.Server.Interfaces;

namespace ITCC.HTTP.Server.Encoders
{
    /// <summary>
    ///     Used to serve application/xml requests
    /// </summary>
    public class XmlBodyEncoder: IBodyEncoder
    {
        public XmlBodyEncoder(bool isDefault = false)
        {
            IsDefault = isDefault;
        }

        #region IBodyEncoder

        public Encoding Encoding => Encoding.UTF8;
        public string Serialize(object body)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    var xmlSerializer = new XmlSerializer(body.GetType());
                    xmlSerializer.Serialize(xmlWriter, body);
                }
                return stringWriter.ToString();
            }
        }
        public string ContentType => "application/xml";
        public bool AutoGzipCompression => true;
        public bool IsDefault { get; }

        #endregion
    }
}
