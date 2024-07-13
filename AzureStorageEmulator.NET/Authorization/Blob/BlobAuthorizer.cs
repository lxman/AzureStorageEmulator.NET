using AzureStorageEmulator.NET.Authorization.Blob.Shared_Access_Signature;

namespace AzureStorageEmulator.NET.Authorization.Blob
{
    public class BlobAuthorizer
    {
        public bool Invoke(string? authorization, HttpContext context)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                // If there is no authorization header, try SAS
                return new BlobSasAuthorizer().Authorize(context.Request);
            }
            string authType = authorization.Split(' ', StringSplitOptions.TrimEntries)[0];
            if (authType is "SharedKey" or "SharedKeyLite")
            {
                return new BlobSharedKeyAuthorizer().Authorize(context.Request);
            }

            throw new NotImplementedException($"We do not have an authorizer for {authType} for Blob yet");
        }
    }
}