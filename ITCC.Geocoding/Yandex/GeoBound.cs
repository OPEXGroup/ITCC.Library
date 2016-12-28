// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.Geocoding.Yandex
{
    public struct GeoBound
    {
        public GeoPoint LowerCorner;
        public GeoPoint UpperCorner;
        public GeoBound(GeoPoint lowerCorner, GeoPoint upperCorner)
        {
            LowerCorner = lowerCorner;
            UpperCorner = upperCorner;
        }

        public override string ToString() => $"[{LowerCorner}] [{UpperCorner}]";
    }
}
