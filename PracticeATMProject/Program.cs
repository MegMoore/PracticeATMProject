using PracticeATMProject;
using System.Text.Json;

internal class Program
{
    private static async Task Main(string[] args)
    {
        const string baseurl = "http://localhost:5555";

        JsonSerializerOptions joptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        HttpClient http = new HttpClient();

        var jsonresponse = await GetUsersAsync(http, joptions); //Make a call
        var customers = jsonresponse.DataReturned as IEnumerable<Customer>;

        foreach (var c in customers)
        {
            Console.WriteLine($"{c.Name}");
        }

        async Task<JsonResponse> GetUsersAsync(HttpClient http, JsonSerializerOptions jsonOptions)
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/customers");//calling what you want
            HttpResponseMessage resp = await http.SendAsync(req);
            Console.WriteLine($"Http ErrorCode is {resp.StatusCode}");
            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            {

            }
            var json = await resp.Content.ReadAsStringAsync();
            var customers = (IEnumerable<Customer>?)JsonSerializer.Deserialize(json, typeof(IEnumerable<Customer>), jsonOptions);
            if (customers is null)
            {
                throw new Exception();
            }

            return new JsonResponse()
            {
                HttpStatusCode = (int)resp.StatusCode,
                DataReturned = customers
            };

        }
    }
}