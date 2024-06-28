namespace TestClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            HttpClient client = new();
            await client.GetAsync(new Uri("http://127.0.0.1:49502/itsme/"));
        }
    }
}