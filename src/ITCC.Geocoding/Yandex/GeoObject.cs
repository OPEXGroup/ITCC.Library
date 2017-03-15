// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace ITCC.Geocoding.Yandex
{
    public class GeoObject
    {
        public GeoPoint Point { get; set; }
        public GeoBound BoundedBy { get; set; }
        public GeoMetaData GeocoderMetaData { get; set; }
    }
}
