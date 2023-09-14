using PracticeATMProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace PracticeATMProject.Classes {
    public class Get {
        const string baseurl = "http://localhost:5555";
        public static async Task<decimal> BalanceByCID(int CID, JsonSerializerOptions jsonOptions, HttpClient http) {
            Account JR = await Menu.DisplayAccounts(CID, jsonOptions, http);
            return JR.Balance;
        }
        public static async Task<JsonResponse> AccountsByCIDAsync(HttpClient http, JsonSerializerOptions jsonOptions, int CID) {
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
        public static async Task<JsonResponse> AccountByAIDAsync(HttpClient http, JsonSerializerOptions jsonOptions, int AID) {

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
        public static async Task<JsonResponse> AllTransactions(JsonSerializerOptions jsonOptions, int AID, HttpClient http) {
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
