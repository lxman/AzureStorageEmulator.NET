using System.Security.Cryptography;
using System.Text;

namespace AzureStorageEmulator.NET.Authentication
{
    public class QueueSharedKeyAuthenticator : IAuthenticator
    {
        public bool Authenticate(HttpRequest request)
        {
            string authType = request.Headers.Authorization.ToString().Split(':')[0].Split(' ')[0];
            string requestToSign = GetHeadersToSign(authType, request);
            string canonicalizedHeaders = CanonicalizedHeaders(request);
            return true;
        }

        private string Sign(byte[] key, HttpRequest request)
        {
            using HMACSHA256 hmac = new(key);
            byte[] hashValue = hmac.ComputeHash(Encoding.Unicode.GetBytes(CanonicalizedHeaders(request)));
            return Encoding.Unicode.GetString(hashValue);
        }

        private static string GetHeadersToSign(string type, HttpRequest request)
        {
            string[] requestToSign = type switch
            {
                "SharedKey" =>
                [
                    "Content-Encoding", "Content-Language", "Content-Length", "Content-MD5", "Content-Type", "Date",
                    "If-Modified-Since", "If-Match", "If-None-Match", "If-Unmodified-Since", "Range"
                ],
                "SharedKeyLite" => ["Content-MD5", "Content-Type", "Date",],
                _ => []
            };

            return string.Join("\n",
                request.Headers.Where(h => requestToSign.Contains(h.Key))
                    .OrderBy(h => h.Key.ToLowerInvariant())
                    .Select(h => h.Key.ToLowerInvariant() + ":" + h.Value.First()));
        }

        private static string CanonicalizedHeaders(HttpRequest request)
        {
            IEnumerable<string> canonicalized = request.Headers.OrderBy(h => h.Key.ToLowerInvariant())
                .Select(h => h.Key.ToLowerInvariant() + ":" + h.Value.First());
            return string.Join("\n", canonicalized);
        }
    }
}
