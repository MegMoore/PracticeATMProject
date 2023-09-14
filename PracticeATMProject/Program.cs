using PracticeATMProject;
using System.Security.Cryptography;
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

        async Task<JsonResponse> GetAccountsByCIDAsync(HttpClient http, JsonSerializerOptions jsonOptions, int CID) {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/accounts");
            HttpResponseMessage resp = await http.SendAsync(req);
            Console.WriteLine($"Http ErrorCode is {resp.StatusCode}");
            if (resp.StatusCode != System.Net.HttpStatusCode.OK) {}
            var json = await resp.Content.ReadAsStringAsync();
            List<Account> Accounts = (List<Account>)JsonSerializer.Deserialize(json, typeof(IEnumerable<Account>), jsonOptions);
            if (Accounts is null) {throw new Exception();}

            List<Account> AccountsR = null;
            foreach ( var a in Accounts ) {
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
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{baseurl}/api/accounts/{AID}");
            HttpResponseMessage resp = await http.SendAsync(req);
            Console.WriteLine($"Http ErrorCode is {resp.StatusCode}");
            if (resp.StatusCode != System.Net.HttpStatusCode.OK) { }
            var json = await resp.Content.ReadAsStringAsync();
            Account Account = (Account)JsonSerializer.Deserialize(json, typeof(IEnumerable<Account>), jsonOptions);
            if (Account is null) { throw new Exception(); }
            return new JsonResponse() {
                HttpStatusCode = (int)resp.StatusCode,
                DataReturned = Account
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
            int AID = 0;
            decimal NewBalance = 0m;
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
                    var balance = await GetBalanceByCID(CID, jsonOptions);
                    Console.WriteLine($"Balance: {balance}");
                return 1;
                case 3:
                    Console.WriteLine($"Enter Deposit Amount: ");
                    var Deposit = Convert.ToDecimal(Console.ReadLine());
                    Console.WriteLine($"Enter Deposit Description: ");
                    var DepositDescription = Console.ReadLine();
                    AID = await CreateTransaction(Deposit, jsonOptions, CID, DepositDescription, "D");
                    NewBalance = Convert.ToDecimal(deposit(Deposit, AID, jsonOptions));
                    Console.WriteLine($"New Balance: {NewBalance}");
                return 1;
                    case 4:
                    Console.WriteLine($"Enter Withdraw Amount: ");
                    var Withdraw = Convert.ToDecimal(Console.ReadLine());
                    Console.WriteLine($"Enter Withdraw Description: ");
                    var WithdrawDescription = Console.ReadLine();
                    AID = await CreateTransaction(Withdraw, jsonOptions, CID, WithdrawDescription, "W");
                    NewBalance = Convert.ToDecimal(withdraw(Withdraw, AID, jsonOptions));
                    Console.WriteLine($"New Balance: {NewBalance}");
                return 1;
                case 5:
                    Console.WriteLine($"Enter Transfer Amount: ");
                    var Transfer = Convert.ToDecimal(Console.ReadLine());

                    Console.WriteLine($"Enter Transfer Description: ");
                    var TransferDescription = Console.ReadLine();

                    Console.WriteLine($"Select which account to transfer From: ");
                    AID = await CreateTransaction(Transfer, jsonOptions, CID, TransferDescription, "W");
                    NewBalance = Convert.ToDecimal(withdraw(Transfer, AID, jsonOptions));
                    Console.WriteLine($"New Balance: {NewBalance}");

                    Console.WriteLine($"Select which account to transfer To: ");
                    AID = await CreateTransaction(Transfer, jsonOptions, CID, TransferDescription, "D");
                    NewBalance = Convert.ToDecimal(deposit(Transfer, AID, jsonOptions));
                    Console.WriteLine($"New Balance: {NewBalance}");

                    return 1;

                default: return 1;

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
            
            foreach (var a in Accounts ) {
                if (a.ID == IN) {
                    return a;
                }
            }
            return null;
        }

        async Task<int> CreateTransaction(decimal D,JsonSerializerOptions JsonOptions, int CID, string DD, string Type) {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, $"{baseurl}/api/transactions");
            var acc = DisplayAccounts(CID, JsonOptions);
            //Transaction T = new() {
            //  AccountID = acc.Id,
            //  PreviousBalance = acc.Result.Balance,
            //  TransactionType = Type,
            //  NewBalance = acc.Result.Balance + D,
            //  Description = DD
            // };

            Transaction T = new();


            switch (Type)
            {
                case "D":
                    T = new()
                    {
                        AccountID = acc.Id,
                        PreviousBalance = acc.Result.Balance,
                        TransactionType = Type,
                        NewBalance = acc.Result.Balance + D,
                        Description = DD
                    };
                    break;
                case "W":
                    T = new()
                    {
                        AccountID = acc.Id,
                        PreviousBalance = acc.Result.Balance,
                        TransactionType = Type,
                        NewBalance = acc.Result.Balance - D,
                        Description = DD
                    };
                    break;
               
            }


            var json = JsonSerializer.Serialize<Transaction>(T, JsonOptions);
            req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage resp = await http.SendAsync(req);
            return acc.Id;
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

        async Task<decimal> withdraw(decimal IN, int AID, JsonSerializerOptions jsonOptions)
        {
            var JR = await GetAccountByAIDAsync(http, jsonOptions, AID);
            var acc = (Account)JR.DataReturned;
            acc.Balance -= IN;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/accounts/{AID}");
            var json = JsonSerializer.Serialize<Account>(acc, jsonOptions);
            req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage resp = await http.SendAsync(req);
            return acc.Balance;
        }
    }
}