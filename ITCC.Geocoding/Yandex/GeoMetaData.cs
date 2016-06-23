namespace ITCC.Geocoding.Yandex
{
    public class GeoMetaData
    {
        public KindType Kind = KindType.locality;
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

        public static KindType ParseKind(string kind)
        {
            switch (kind)
            {
                case "district": return KindType.district;
                case "house": return KindType.house;
                case "locality": return KindType.locality;
                case "metro": return KindType.metro;
                case "street": return KindType.street;
                default: return KindType.locality;
            }
        }

        public override string ToString() => Text;
    }
}
