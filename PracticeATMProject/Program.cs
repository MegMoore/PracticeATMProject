using PracticeATMProject;
using System.Text.Json;
using static System.Net.WebRequestMethods;

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
        bool isActive = true;

        while (isActive==true) {
            bool Logged = false;
            int CID = Convert.ToInt32(Login(joptions));
            if (CID != 0) {
                Logged = true;
            }
            int Pcode = 1;
            while (Logged) {
                Pcode = Convert.ToInt32(Display(Pcode, CID, joptions));
            }
        }

        async Task<JsonResponse> GetCustomersAsync(HttpClient http, JsonSerializerOptions jsonOptions)
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
        async Task<JsonResponse> GetAccountsAsync(HttpClient http, JsonSerializerOptions jsonOptions, int CID) {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/accounts/{CID}");//calling what you want
            HttpResponseMessage resp = await http.SendAsync(req);
            Console.WriteLine($"Http ErrorCode is {resp.StatusCode}");
            if (resp.StatusCode != System.Net.HttpStatusCode.OK) {}
            var json = await resp.Content.ReadAsStringAsync();
            var Accounts = (IEnumerable<Account>?)JsonSerializer.Deserialize(json, typeof(IEnumerable<Account>), jsonOptions);
            if (Accounts is null) {throw new Exception();}
            return new JsonResponse() {
                HttpStatusCode = (int)resp.StatusCode,
                DataReturned = Accounts
            };

        }

        async Task<int> Login(JsonSerializerOptions jsonOptions) {
            Console.WriteLine("CC: ");
            int CC = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("PC: ");
            int PC = Convert.ToInt32(Console.ReadLine());
            int CID = 0;
            var jsonResponse = await GetCustomersAsync(http, jsonOptions);
            var Customers = (IEnumerable<Customer>)jsonResponse.DataReturned;
            foreach (var c in Customers) {
                if (c.CardCode == CC && c.PinCode == PC) {
                    CID = c.ID; break;
                }
            }
            return CID;
        }
        async Task<int> Display(int pcode, int CID, JsonSerializerOptions jsonOptions) {
            switch (pcode) {
                case 1:
                    Console.WriteLine($"(1)Balance\n(2)Deposit\n(3)Withdraw\n(4)Transfer\n(5)Show Transactions)");
                    Console.WriteLine("Enter Menu Option 1-5: ");
                    var Input1 = Convert.ToInt32(Console.ReadLine());
                    var Output = 0;
                        switch (Input1) {
                    case 1:
                    Output = 2;
                    break;
                    case 2:
                    Output = 3;
                    break;
                    case 3:
                    Output = 4;
                    break;
                    case 4:
                    Output = 5;
                    break;
                    case 5:
                    Output = 6;
                    break;
                    default:
                    Output = 1;
                    break;
                }
                return Output;
                case 2:
                    var balance =  await GetBalanceByCID(CID, jsonOptions);
                    Console.WriteLine($"Balance: {balance}");
                return 1;
                default: return 1;

            }
        }


        async Task<decimal> GetBalanceByCID(int cID, JsonSerializerOptions jsonOptions) {
            int AID = Convert.ToInt32(DisplayAccounts(cID, jsonOptions));
        }
        async Task<int> DisplayAccounts(int cID, JsonSerializerOptions jsonOptions) {
            var jsonResponse = await GetAccountsAsync(http, jsonOptions, cID);
            var Customers = (IEnumerable<Customer>)jsonResponse.DataReturned;
        }
    }

}