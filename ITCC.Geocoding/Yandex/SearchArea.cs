namespace ITCC.Geocoding.Yandex
{
    public struct SearchArea
    {
        public GeoPoint LongLat, Spread;
        public SearchArea(GeoPoint centerLongLat, GeoPoint spread)
        {
            LongLat = centerLongLat;
            Spread = spread;
        }
    }
}
