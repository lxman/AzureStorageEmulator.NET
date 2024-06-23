using System.Security.Cryptography;
using System.Text;
using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.Extensions;
using Microsoft.Extensions.Primitives;

namespace AzureStorageEmulator.NET.Authorization.Table
{
    public class TableSharedKeyLiteAuthorizer
    {
        public bool Authorize(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authorization"))
            {
                return false;
            }
            if (request.Headers.Authorization == StringValues.Empty)
            {
                return false;
            }

            byte[] key = DevstoreAccount1.Key;
            string headersToSign = GetHeadersToSign(request);
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

        private static string GetHeadersToSign(HttpRequest request)
        {
            if (request.Headers.TryGetValue("x-ms-date", out StringValues xMsDateValue))
            {
                return $"{xMsDateValue.First()?.RemoveLinearWhitespaceNotQuoted()}\n";
            }
            return request.Headers.TryGetValue("Date", out StringValues dateValue) ? $"Date:{dateValue.First()?.RemoveLinearWhitespaceNotQuoted()}\n"
                : "\n";
        }

        private static string GetCanonicalizedResource(HttpRequest request)
        {
            string resource = $"{request.Path.Value!}";
            string? query = request.QueryString.Value;
            if (string.IsNullOrEmpty(query))
            {
                return $"/devstoreaccount1{resource}";
            }

            query = query[1..];
            List<string> compQuery = [.. query.Split('&').Where(q => q.StartsWith("comp"))];
            return $"/devstoreaccount1{resource}{(compQuery.Count != 0 ? $"?{compQuery.First()}" : string.Empty)}";
        }
    }
}