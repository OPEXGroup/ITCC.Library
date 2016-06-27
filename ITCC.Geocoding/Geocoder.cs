using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geocoding;
using Geocoding.Google;
using ITCC.Geocoding.Enums;
using ITCC.Geocoding.Utils;
using ITCC.Geocoding.Yandex;

namespace ITCC.Geocoding
{
    public static class Geocoder
    {
        #region public

        public static async Task<Point> GeocodeAsync(string location, GeocodingApi apiType)
        {
            Point result = null;
            switch (apiType)
            {
                case GeocodingApi.Yandex:
                    var codingResult = await YandexGeocoder.GeocodeAsync(location);
                    if (!codingResult.Any())
                        return null;
                    var firstPoint = codingResult.First().Point;
                    result = new Point
                    {
                        Latitude = firstPoint.Latitude,
                        Longitude = firstPoint.Longitude
                    };
                    break;
                case GeocodingApi.Google:
                    GoogleGeocoder geocoder;
                    lock (KeyLock)
                    {
                        geocoder = new GoogleGeocoder {ApiKey = _googleApiKey};
                    }
                    var addresses = await geocoder.GeocodeAsync(location);
                    var firstAddress = addresses?.FirstOrDefault();
                    if (firstAddress == null)
                        return null;

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(apiType), apiType, null);
            }
            return result;
        }

        public static void SetApiKey(string key, GeocodingApi apiType)
        {
            switch (apiType)
            {
                case GeocodingApi.Yandex:
                    YandexApiKey = key;
                    break;
                case GeocodingApi.Google:
                    GoogleApiKey = key;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(apiType), apiType, null);
            }
        }

        public static string YandexApiKey
        {
            get
            {
                lock (KeyLock)
                {
                    return _yandexApiKey;
                }
            }
            set
            {
                lock (KeyLock)
                {
                    _yandexApiKey = value;
                    YandexGeocoder.Key = _yandexApiKey;
                }
            }
        }

        public static string GoogleApiKey
        {
            get
            {
                lock (KeyLock)
                {
                    return _googleApiKey;
                }
            }
            set
            {
                lock (KeyLock)
                {
                    _googleApiKey = value;
                }
            }
        }

        #endregion

        #region private

        private static string _yandexApiKey;
        private static string _googleApiKey;
        private static readonly object KeyLock = new object();

        #endregion
    }
}
