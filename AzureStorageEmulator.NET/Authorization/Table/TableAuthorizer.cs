namespace AzureStorageEmulator.NET.Authorization.Table
{
    public class TableAuthorizer
    {
        public bool Invoke(string authorization, HttpContext context)
        {
            string authType = authorization.Split(' ', StringSplitOptions.TrimEntries)[0];
            if (authType == "SharedKeyLite")
            {
                return new TableSharedKeyLiteAuthorizer().Authorize(context.Request);
            }

            throw new NotImplementedException($"We do not have an authorizer for {authType} for Table yet");
        }
    }
}