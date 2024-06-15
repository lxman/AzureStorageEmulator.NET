HttpClient client = new();
HttpResponseMessage response = await client.GetAsync(new Uri("http://localhost:10001/devstoreaccount1?comp=list"));
response.EnsureSuccessStatusCode();
Console.WriteLine(await response.Content.ReadAsStringAsync());