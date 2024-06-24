namespace AzureStorageEmulator.NET.Authorization.Blob
{
    public class BlobAuthorizer
    {
        public bool Invoke(string authorization, HttpContext context)
        {
            string authType = authorization.Split(' ', StringSplitOptions.TrimEntries)[0];
            if (authType is "SharedKey" or "SharedKeyLite")
            {
                return new BlobSharedKeyAuthorizer().Authorize(context.Request);
            }

            throw new NotImplementedException($"We do not have an authorizer for {authType} for Queue yet");
        }
    }
}