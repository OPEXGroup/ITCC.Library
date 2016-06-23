using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ITCC.Logging;

namespace ITCC.Geocoding.Yandex
{
    public static class YandexGeocoder
    {
        #region const, fields, constructor, properties
        private const string RequestUrlFormat = "http://geocode-maps.yandex.ru/1.x/?geocode={0}&format=xml&results={1}&lang={2}";
        private const short DefaultResultCount = 10;

        public static string Key { get; set; } = string.Empty;

        private static string KeyQueryParam => string.IsNullOrEmpty(Key) ? string.Empty : "&apikey=" + Key;

        #endregion

        #region GeocodeAsync
        public static async Task<GeoObjectCollection> GeocodeAsync(string location, short results = DefaultResultCount, LangType lang = LangType.en_US)
        {
            var requestUrl = BuildCommonUrlPart(location, results, lang) + KeyQueryParam;
            Logger.LogEntry("GEO RAW", LogLevel.Trace, $"Requesting:\n{requestUrl}");
            var response = await DownloadStringAsync(requestUrl);
            Logger.LogEntry("GEO RAW", LogLevel.Trace, $"Raw response:\n{response}");
            return new GeoObjectCollection(response);
        }

        public static async Task<GeoObjectCollection> GeocodeAsync(string location, short results, LangType lang, SearchArea searchArea, bool rspn = false)
        {
            var requestUrl = BuildCommonUrlPart(location, results, lang) +
                $"&ll={searchArea.LongLat.ToString("{0},{1}")}&spn={searchArea.Spread.ToString("{0},{1}")}&rspn={(rspn ? 1 : 0)}" +
                KeyQueryParam;
            Logger.LogEntry("GEO RAW", LogLevel.Trace, $"Requesting:\n{requestUrl}");
            var response = await DownloadStringAsync(requestUrl);
            Logger.LogEntry("GEO RAW", LogLevel.Trace, $"Raw response:\n{response}");
            return new GeoObjectCollection(response);
        }
        #endregion


        // ToDo: back geocoding methods
        #region ReGeokode
        private static string ReGeocode(double _long, double lat)
        {
            return "";
        }
        #endregion

        #region helpers
        // ToDo: encode
        private static string StringMakeValid(string location)
        {
            // return Uri.EscapeUriString(location);
            location = location.Replace(" ", "+").Replace("&", "").Replace("?", "");
            return location;
        }

        private static string BuildCommonUrlPart(string location, short results, LangType lang)
        {
            return string.Format(RequestUrlFormat, StringMakeValid(location), results, LangTypeToStr(lang));
        }

        private static string LangTypeToStr(LangType lang)
        {
            switch (lang)
            {
                case LangType.ru_RU: return "ru-RU";
                case LangType.uk_UA: return "uk-UA";
                case LangType.be_BY: return "be-BY";
                case LangType.en_US: return "en-US";
                case LangType.en_BR: return "en-BR";
                case LangType.tr_TR: return "tr-TR";
                default: return "ru-RU";
            }
        }

        private static async Task<string> DownloadStringAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode)
                        return await response.Content.ReadAsStringAsync();

                    Logger.LogEntry("GEO BAD", LogLevel.Trace, $"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");
                    foreach (var httpResponseHeader in response.Headers)
                    {
                        Logger.LogEntry("GEO BAD", LogLevel.Trace, $"{httpResponseHeader.Key}: {string.Join(";", httpResponseHeader.Value)}");
                    }
                    return null;
                }
            }
        }
        #endregion
    }

}
