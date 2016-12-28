// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Globalization;

namespace ITCC.Geocoding.Yandex
{
    public struct GeoPoint
    {
        public delegate string ToStringFunc(double x, double y);

        public double Longitude;
        public double Latitude;

        public static GeoPoint Parse(string point)
        {
            var splitted = point.Split(new[] { ' ' }, count: 2);
            return new GeoPoint(double.Parse(splitted[0], CultureInfo.InvariantCulture), double.Parse(splitted[1], CultureInfo.InvariantCulture));
        }

        public GeoPoint(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public override string ToString() => $"{Longitude.ToString(CultureInfo.InvariantCulture)} {Latitude.ToString(CultureInfo.InvariantCulture)}";

        public string ToString(string format) => string.Format(format, Longitude.ToString(CultureInfo.InvariantCulture), Latitude.ToString(CultureInfo.InvariantCulture));

        public string ToString(ToStringFunc formatFunc) => formatFunc(Longitude, Latitude);
    }
}
