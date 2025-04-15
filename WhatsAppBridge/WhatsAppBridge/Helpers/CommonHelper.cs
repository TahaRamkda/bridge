using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace WhatsAppBridge.Helpers
{
    public static class CommonHelper
    {
        /// <summary>
        /// Get full URL
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public static string GetFullUrl(string baseUrl, string requestUri)
        {
            return String.Concat(baseUrl, requestUri);
        }

        /// <summary>
        /// Get sublist for the lists.
        /// </summary>
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            var chunks = source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            return chunks;
        }

        public static Dictionary<string, object> FlattenObject<T>(T source)
         where T : class, new()
        {
            return JObject.FromObject(source)
                .Descendants()
                .OfType<JValue>()
                .ToDictionary(jv => jv.Path, jv => jv.Value<object>());
        }

        public static string ToQueryString(this Dictionary<string, object> dict)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var kvp in dict)
            {
                sb.Append("&")
                    .Append($"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value.ToString())}");
            }

            return sb.ToString()
                .Trim('&');
        }

        public static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static DateTime ConvertFromEpoch(long ticks)
        {
            try
            {
                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(ticks).UtcDateTime;
                return dateTime;
            }
            catch (Exception ex)
            {
                return DateTime.UtcNow;
            }
        }

        public static long ConvertToEpoch(DateTime dateTime)
        {
            try
            {
                long epoch = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                return epoch;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static string ConvertDateTimeFormat(DateTime dateTime)
        {
            string formattedDateTime = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);
            return formattedDateTime;
        }

        public static string EncodeSpecialCharacters(this string source)
        {
            if (String.IsNullOrWhiteSpace(source))
                return "";

            return source.Replace("\n", Environment.NewLine).Replace("&", "%26").Replace("?", "%3F").Trim();
        }
    }
}
