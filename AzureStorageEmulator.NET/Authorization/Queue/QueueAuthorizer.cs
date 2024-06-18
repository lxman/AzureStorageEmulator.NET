namespace AzureStorageEmulator.NET.Authorization.Queue
{
    public class QueueAuthorizer
    {
        public bool Invoke(string authentication, HttpContext context, RequestDelegate next)
        {
            string authType = authentication.Split(' ', StringSplitOptions.TrimEntries)[0];
            if (authType is "SharedKey" or "SharedKeyLite")
            {
                return new QueueSharedKeyAuthorizer().Authorize(context.Request);
            }

            throw new NotImplementedException($"We do not have an authorizer for {authType} yet");
        }
    }
}
