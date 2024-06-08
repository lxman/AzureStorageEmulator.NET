using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Authentication
{
    public class QueueSharedKey : IAuthenticator
    {
        public bool Authenticate(HttpWebRequest request)
        {
            throw new NotImplementedException();
        }

        private string Sign(byte[] key, HttpRequestHeaders headers)
        {
            using HMACSHA256 hmac = new(key);
            byte[] hashValue = hmac.ComputeHash(Encoding.Unicode.GetBytes(CanonicalizedHeaders(headers)));
            return Encoding.Unicode.GetString(hashValue);
        }

        private string GetHeadersToSign(string type, HttpRequestHeaders headers)
        {
            string[] headersToSign = type switch
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
                headers.Where(h => headersToSign.Contains(h.Key))
                    .OrderBy(h => h.Key.ToLowerInvariant())
                    .Select(h => h.Key.ToLowerInvariant() + ":" + h.Value.First()));
        }

        private string CanonicalizedHeaders(HttpRequestHeaders headers)
        {
            IEnumerable<string> canonicalized = headers.OrderBy(h => h.Key.ToLowerInvariant())
                .Select(h => h.Key.ToLowerInvariant() + ":" + h.Value.First());
            return string.Join("\n", headers);
        }
    }
}
