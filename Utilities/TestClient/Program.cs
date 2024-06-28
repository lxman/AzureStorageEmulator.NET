using System.Net.Http.Headers;

namespace TestClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");
            HttpResponseMessage result = await client.GetAsync(new Uri("http://127.0.0.1:10010/api/status"));
            if (result.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
                string content = await result.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            else
            {
                Console.WriteLine($"Failure {result.StatusCode}");
                Console.WriteLine(await result.Content.ReadAsStringAsync());
                Console.WriteLine(result.ReasonPhrase);
            }

            Console.ReadKey();
        }
    }
}