using PracticeATMProject.Classes;
using PracticeATMProject.Models;
using System.Security.Cryptography;
using System.Text.Json;
//using System.Transactions;

internal class Program {
    private static async Task Main(string[] args) {
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


        async Task<int> Login(HttpClient http, JsonSerializerOptions jsonOptions) {
            Console.WriteLine("CC: ");
            int CC = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("PC: ");
            int PC = Convert.ToInt32(Console.ReadLine());
            int CID = 0;
            var jsonResponse = await Get.CustomersAsync(http, jsonOptions);
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

    }
}