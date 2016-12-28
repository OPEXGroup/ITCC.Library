// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.Geocoding.Yandex
{
    public struct SearchArea
    {
        public GeoPoint LongLat;
        public GeoPoint Spread;

        public SearchArea(GeoPoint centerLongLat, GeoPoint spread)
        {
            LongLat = centerLongLat;
            Spread = spread;
        }
    }
}
