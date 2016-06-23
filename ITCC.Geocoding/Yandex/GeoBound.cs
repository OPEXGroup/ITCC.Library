namespace ITCC.Geocoding.Yandex
{
    public struct GeoBound
    {
        public GeoPoint LowerCorner, UpperCorner;
        public GeoBound(GeoPoint lowerCorner, GeoPoint upperCorner)
        {
            LowerCorner = lowerCorner;
            UpperCorner = upperCorner;
        }

        public override string ToString()
        {
            return $"[{LowerCorner}] [{UpperCorner}]";
        }
    }
}
