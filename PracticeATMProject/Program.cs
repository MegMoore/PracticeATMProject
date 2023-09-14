using PracticeATMProject.Classes;
using PracticeATMProject.Models;
using System.Security.Cryptography;
using System.Text.Json;
//using System.Transactions;

internal class Program {
    private static async Task Main(string[] args) {
        const string baseurl = "http://localhost:5555";
        JsonSerializerOptions joptions = new JsonSerializerOptions() {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        HttpClient http = new HttpClient();
        bool isActive = true;

        while (isActive == true) {
            bool Logged = false;
            int CID = await Login(http, joptions);
            if (CID != 0) {
                Logged = true;
            }
            int Pcode = 1;
            while (Logged) {
                Pcode = await Display(Pcode, CID, joptions);
                if (Pcode == -1) { Logged = false; };
            }
        }

        async Task<JsonResponse> GetCustomersAsync(HttpClient http, JsonSerializerOptions jsonOptions) {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/customers");//calling what you want
            HttpResponseMessage resp = await http.SendAsync(req);
            Console.WriteLine($"Http ErrorCode is {resp.StatusCode}");
            if (resp.StatusCode != System.Net.HttpStatusCode.OK) { }
            var json = await resp.Content.ReadAsStringAsync();
            var customers = (IEnumerable<Customer>?)JsonSerializer.Deserialize(json, typeof(IEnumerable<Customer>), jsonOptions);
            if (customers is null) {
                throw new Exception();
            }

            return new JsonResponse() {
                HttpStatusCode = (int)resp.StatusCode,
                DataReturned = customers
            };

        }

        async Task<JsonResponse> GetAccountsByCIDAsync(HttpClient http, JsonSerializerOptions jsonOptions, int CID) {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/accounts");
            HttpResponseMessage resp = await http.SendAsync(req);
            Console.WriteLine($"Http ErrorCode is {resp.StatusCode}");
            if (resp.StatusCode != System.Net.HttpStatusCode.OK) { }
            var json = await resp.Content.ReadAsStringAsync();
            List<Account> Accounts = (List<Account>)JsonSerializer.Deserialize(json, typeof(IEnumerable<Account>), jsonOptions);
            if (Accounts is null) { throw new Exception(); }

            List<Account> AccountsR = new();
            foreach (var a in Accounts) {
                if (a.CustomerID == CID) {
                    AccountsR.Add(a);
                }
            }

            return new JsonResponse() {
                HttpStatusCode = (int)resp.StatusCode,
                DataReturned = AccountsR
            };

        }

        async Task<JsonResponse> GetAccountByAIDAsync(HttpClient http, JsonSerializerOptions jsonOptions, int AID) {

            //Console.WriteLine($"DEBUG: int AID = {AID}");

            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/accounts/{AID}");
            HttpResponseMessage resp = await http.SendAsync(req);
            Console.WriteLine($"Http ErrorCode is {resp.StatusCode}");
            if (resp.StatusCode != System.Net.HttpStatusCode.OK) { }
            var json = await resp.Content.ReadAsStringAsync();
            object? Account = JsonSerializer.Deserialize(json, typeof(Account), jsonOptions);
            if (Account is null) { throw new Exception(); }
            return new JsonResponse() {
                HttpStatusCode = (int)resp.StatusCode,
                DataReturned = Account
            };

        }


        async Task<int> Login(HttpClient http, JsonSerializerOptions jsonOptions) {
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
            int AID = 0;
            decimal NewBalance = 0m;
            switch (pcode) {
                case 1:
                return await Menu.MainMenu();
                case 2:
                return await Menu.DisplayBalance(CID, jsonOptions, http);
                case 3:
                return await Menu.Deposit(jsonOptions, CID, http);
                case 4:
                return await Menu.Withdraw(jsonOptions, CID, http);
                case 5:
                return await Menu.Transfer(jsonOptions, CID, http);
                case 6:
                return await Menu.Transactions(jsonOptions, CID, http);
                case 7:
                return -1;
                default: return -1;

            }
        }


        async Task<decimal> GetBalanceByCID(int cID, JsonSerializerOptions jsonOptions) {
            Account JR = await DisplayAccounts(cID, jsonOptions);
            return JR.Balance;
        }

        async Task<Account> DisplayAccounts(int cID, JsonSerializerOptions jsonOptions) {
            var jsonResponse = await GetAccountsByCIDAsync(http, jsonOptions, cID);
            var Accounts = (IEnumerable<Account>)jsonResponse.DataReturned;
            foreach (var a in Accounts) {
                Console.WriteLine($"{a.ID}|{a.Description}|{a.Type}|{a.CreationDate}");
            }
            Console.WriteLine($"Select Account Number: ");
            int IN = Convert.ToInt32(Console.ReadLine());

            foreach (var a in Accounts) {
                if (a.ID == IN) {
                    //Console.WriteLine($"{a.ID}|{a.Description}|{a.Type}|{a.CreationDate}");//temp for debug
                    return a;
                }
            }
            return null;
        }

        async Task<int> CreateTransaction(decimal D, JsonSerializerOptions JsonOptions, int CID, string DD, string Type) {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, $"{baseurl}/api/transactions");
            Account acc = await DisplayAccounts(CID, JsonOptions);
            //Transaction T = new() {
            //  AccountID = acc.Id,
            //  PreviousBalance = acc.Result.Balance,
            //  TransactionType = Type,
            //  NewBalance = acc.Result.Balance + D,
            //  Description = DD
            // };

            Transaction T = new();


            switch (Type) {
                case "D":
                T = new() {
                    AccountID = acc.ID,
                    PreviousBalance = acc.Balance,
                    TransactionType = Type,
                    NewBalance = acc.Balance + D,
                    Description = DD
                };
                break;
                case "W":
                T = new() {
                    AccountID = acc.ID,
                    PreviousBalance = acc.Balance,
                    TransactionType = Type,
                    NewBalance = acc.Balance - D,
                    Description = DD
                };
                break;

            }


            var json = JsonSerializer.Serialize<Transaction>(T, JsonOptions);
            req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage resp = await http.SendAsync(req);
            return acc.ID;
        }

        async Task<decimal> deposit(decimal IN, int AID, JsonSerializerOptions JsonOptions) {
            var JR = await GetAccountByAIDAsync(http, JsonOptions, AID);
            var acc = (Account)JR.DataReturned;
            acc.Balance += IN;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/accounts/{AID}");
            var json = JsonSerializer.Serialize<Account>(acc, JsonOptions);
            req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage resp = await http.SendAsync(req);
            return acc.Balance;
        }

        ///Witdraw Method

        async Task<decimal> withdraw(decimal IN, int AID, JsonSerializerOptions jsonOptions) {
            var JR = await GetAccountByAIDAsync(http, jsonOptions, AID);
            var acc = (Account)JR.DataReturned;
            acc.Balance -= IN;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/accounts/{AID}");
            var json = JsonSerializer.Serialize<Account>(acc, jsonOptions);
            req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage resp = await http.SendAsync(req);
            return acc.Balance;
        }

        async Task<JsonResponse> GetAllTransactions(JsonSerializerOptions jsonOptions, int AID) {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/transactions");
            HttpResponseMessage resp = await http.SendAsync(req);
            Console.WriteLine($"Http ErrorCode is {resp.StatusCode}");
            if (resp.StatusCode != System.Net.HttpStatusCode.OK) { }
            var json = await resp.Content.ReadAsStringAsync();
            List<Transaction> Transactions = (List<Transaction>)JsonSerializer.Deserialize(json, typeof(IEnumerable<Transaction>), jsonOptions);
            if (Transactions is null) { throw new Exception(); }

            List<Transaction> TransactionsR = new();
            foreach (var T in Transactions) {
                if (T.AccountID == AID) {
                    TransactionsR.Add(T);
                }
            }

            return new JsonResponse() {
                HttpStatusCode = (int)resp.StatusCode,
                DataReturned = TransactionsR
            };
        }
    }
}