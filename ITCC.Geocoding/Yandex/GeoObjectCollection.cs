using System.Collections.Generic;
using System.Xml;
using ITCC.Logging;

namespace ITCC.Geocoding.Yandex
{
    public class GeoObjectCollection : List<GeoObject>
    {
        public GeoObjectCollection(string xml)
        {
            ParseXml(xml);
        }

        // ToDo: not best specification realise
        //       - null varibles and geo varibles
        //       - null response
        //       + ll,spn bounds
        private void ParseXml(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("ns", "http://maps.yandex.ru/ymaps/1.x");
            ns.AddNamespace("opengis", "http://www.opengis.net/gml");
            ns.AddNamespace("geocoder", "http://maps.yandex.ru/geocoder/1.x");

            // select geo objects
            var nodes = doc.SelectNodes("//ns:ymaps/ns:GeoObjectCollection/opengis:featureMember/ns:GeoObject", ns);
            if (nodes == null)
            {
                Logger.LogEntry("GEO YANDEX", LogLevel.Debug, "Null nodes");
                return;
            }

            foreach (XmlNode node in nodes)
            {
                var pointNode = node.SelectSingleNode("opengis:Point/opengis:pos", ns);
                var boundsNode = node.SelectSingleNode("opengis:boundedBy/opengis:Envelope", ns);
                var metaNode = node.SelectSingleNode("opengis:metaDataProperty/geocoder:GeocoderMetaData", ns);

                var obj = new GeoObject
                {
                    Point = pointNode == null ? new GeoPoint() : GeoPoint.Parse(pointNode.InnerText),
                    BoundedBy = boundsNode == null ? new GeoBound() : new GeoBound(
                        GeoPoint.Parse(boundsNode["lowerCorner"]?.InnerText), GeoPoint.Parse(boundsNode["upperCorner"]?.InnerText)),
                    GeocoderMetaData = new GeoMetaData(metaNode["text"]?.InnerText, metaNode["kind"]?.InnerText)
                };
                Add(obj);
            }
        }
    }
}
