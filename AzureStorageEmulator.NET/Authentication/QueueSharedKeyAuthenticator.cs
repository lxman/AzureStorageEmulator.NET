using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace AzureStorageEmulator.NET.Authentication
{
    public class QueueSharedKeyAuthenticator : IAuthenticator
    {
        public bool Authenticate(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authorization"))
            {
                return false;
            }
            if (request.Headers.Authorization == StringValues.Empty)
            {
                return false;
            }
            if (string.IsNullOrEmpty(request.Method) || string.IsNullOrEmpty(request.Path.Value))
            {
                return false;
            }

            byte[] key =
                Convert.FromBase64String(
                    "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==");
            string authType = request.Headers.Authorization.ToString().Split(':')[0].Split(' ')[0];
            string headersToSign = GetHeadersToSign(authType, request);
            string resource = GetCanonicalizedResource(request);
            string toSign = $"{headersToSign}{resource}";
            string calculatedSignature = Sign(key, toSign);
            string providedSignature = request.Headers.Authorization.ToString().Split(' ')[1].Split(':')[1];
            return calculatedSignature == providedSignature;
        }

        private static string Sign(byte[] key, string toEncode)
        {
            using HMACSHA256 hmac = new(key);
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(toEncode));
            return Convert.ToBase64String(hashValue);
        }

        private static string GetHeadersToSign(string type, HttpRequest request)
        {
            List<string> headerNames = type switch
            {
                "SharedKey" =>
                [
                    "Content-Encoding",
                    "Content-Language",
                    "Content-Length",
                    "Content-MD5",
                    "Content-Type",
                    "Date",
                    "If-Modified-Since",
                    "If-Match",
                    "If-None-Match",
                    "If-Unmodified-Since",
                    "Range"
                ],
                "SharedKeyLite" =>
                [
                    "Content-MD5",
                    "Content-Type",
                    "Date"
                ],
                _ => []
            };

            string result = $"{request.Method}\n";
            string headerValues = string.Empty;
            headerNames.ForEach(h =>
            {
                bool found = request.Headers.TryGetValue(h, out StringValues values);
                headerValues +=
                    $"{(
                        found
                            ? $"{(values != StringValues.Empty
                                ? values.First() != "0"
                                    ? string.Join(',', values!)
                                    : string.Empty
                                : string.Empty)}"
                            : string.Empty)}\n";
            });
            result += headerValues;
            result += CanonicalizedHeaders(request);
            return $"{result}\n";
        }

        private static string CanonicalizedHeaders(HttpRequest request)
        {
            IEnumerable<string> canonicalized = request.Headers.OrderBy(h => h.Key.ToLowerInvariant())
                .DistinctBy(h => h.Key)
                .Where(h => h.Key.ToLowerInvariant().StartsWith("x-ms-", StringComparison.OrdinalIgnoreCase))
                .Select(h => new KeyValuePair<string, string>(h.Key, h.Value.First()?.Replace("  ", " ")!))
                .Select(h => h.Key.ToLowerInvariant() + ":" + h.Value);
            return string.Join("\n", canonicalized);
        }

        private static string GetCanonicalizedResource(HttpRequest request)
        {
            string resource = $"{request.Path.Value!}";
            string? query = request.QueryString.Value;
            if (query is null || query == string.Empty)
            {
                return $"/devstoreaccount1{resource}";
            }
            query = query?[1..];
            List<string> queryList = [.. query?.Split('&').Order()];
            List<string> queryResults = [];
            queryList.ForEach(q =>
            {
                string[] parts = q.Split('=');
                queryResults.Add($"{parts[0]}:{parts[1]}");
            });
            return $"/devstoreaccount1{resource}\n{string.Join('\n', queryResults)}";
        }
    }
}