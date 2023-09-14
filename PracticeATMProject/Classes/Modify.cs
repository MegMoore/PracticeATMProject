using PracticeATMProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace PracticeATMProject.Classes {
    public class Modify {
        const string baseurl = "http://localhost:5555";
        public static async Task<int> CreateTransaction(decimal D, JsonSerializerOptions JsonOptions, int CID, string DD, string Type, HttpClient http) {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, $"{baseurl}/api/transactions");
            Account acc = await Menu.DisplayAccounts(CID, JsonOptions, http);
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
        public static async Task<decimal> deposit(decimal IN, int AID, JsonSerializerOptions JsonOptions, HttpClient http) {
            var JR = await Get.AccountByAIDAsync(http, JsonOptions, AID);
            var acc = (Account)JR.DataReturned;
            acc.Balance += IN;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, $"{baseurl}/api/accounts/{AID}");
            var json = JsonSerializer.Serialize<Account>(acc, JsonOptions);
            req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage resp = await http.SendAsync(req);
            return acc.Balance;
        }
        public static async Task<decimal> withdraw(decimal IN, int AID, JsonSerializerOptions jsonOptions, HttpClient http) {
            var JR = await Get.AccountByAIDAsync(http, jsonOptions, AID);
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
