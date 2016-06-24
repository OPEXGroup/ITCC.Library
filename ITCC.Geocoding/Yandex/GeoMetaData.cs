using ITCC.Geocoding.Yandex.Enums;

namespace ITCC.Geocoding.Yandex
{
    public class GeoMetaData
    {
        public KindType Kind = KindType.Locality;
        public string Text = string.Empty;
        //ToDo:
        //AddressDetails
        public GeoMetaData() { }
        public GeoMetaData(string text)
        {
            Text = text;
        }
        public GeoMetaData(string text, string kind)
        {
            Text = text;
            Kind = ParseKind(kind);
        }
        public GeoMetaData(string text, KindType kind)
        {
            Text = text;
            Kind = kind;
        }

        private static KindType ParseKind(string kind)
        {
            switch (kind)
            {
                case "district": return KindType.District;
                case "house": return KindType.House;
                case "locality": return KindType.Locality;
                case "metro": return KindType.Metro;
                case "street": return KindType.Street;
                default: return KindType.Locality;
            }
        }

        public override string ToString() => Text;
    }
}
