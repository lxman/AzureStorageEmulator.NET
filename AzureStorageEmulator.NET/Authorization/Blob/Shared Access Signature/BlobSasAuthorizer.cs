using System.Security.Cryptography;
using System.Text;
using AzureStorageEmulator.NET.Common;

namespace AzureStorageEmulator.NET.Authorization.Blob.Shared_Access_Signature
{
    public class BlobSasAuthorizer : IAuthorizer
    {
        public bool Authorize(HttpRequest request)
        {
            // Assume a user delegation SAS for now
            IQueryCollection queries = request.Query;
            Dictionary<string, string> queryDict = queries.ToDictionary(x => x.Key, x => Uri.UnescapeDataString(x.Value.ToString()));
            BlobSasSignatureComponents components = new();
            queryDict.Keys.ToList().ForEach(k =>
            {
                SasSignatureComponent? component = components.Components.FirstOrDefault(c => c.ShortName == k);
                if (component is not null)
                {
                    component.Value = queryDict[k];
                }
            });
            string? signature = components.Components.FirstOrDefault(c => c.ShortName == "sig")?.Value;
            string stringToSign = string.Empty;
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "sp")?.Value);
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "st")?.Value);
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "se")?.Value);
            stringToSign += $"/blob/devstoreaccount1/{request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries)[1]}\n";
            stringToSign += "\n";
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "sip")?.Value);
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "spr")?.Value);
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "sv")?.Value);
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "rscc")?.Value);
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "rscd")?.Value);
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "rsce")?.Value);
            stringToSign += AddPart(components.Components.FirstOrDefault(c => c.ShortName == "rscl")?.Value);
            stringToSign += components.Components.FirstOrDefault(c => c.ShortName == "rsct")?.Value;
            byte[] key = DevstoreAccount1.Key;
            string calculatedSignature = Sign(key, stringToSign);
            return signature == calculatedSignature;
        }

        private static string AddPart(string? content)
        {
            return $"{(string.IsNullOrEmpty(content) ? string.Empty : content)}\n";
        }

        private static string Sign(byte[] key, string toEncode)
        {
            using HMACSHA256 hmac = new(key);
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(toEncode));
            return Convert.ToBase64String(hashValue);
        }
    }
}
