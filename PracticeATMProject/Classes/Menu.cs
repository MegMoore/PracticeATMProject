using PracticeATMProject.Models;
using System.Text.Json;

namespace PracticeATMProject.Classes
{
    public class Menu
    {
        public static async Task<int> MainMenu()
        {
            Console.WriteLine($"(B)Balance\n(D)Deposit\n(W)Withdraw\n(T)Transfer\n(S)Show Transactions\n(X)Logout");
            Console.WriteLine("Enter Menu Option: ");
            var Input1 = Console.ReadLine();
            var Output = 0;
            switch (Input1)
            {
                case "B":
                    Output = 2;
                    break;
                case "D":
                    Output = 3;
                    break;
                case "W":
                    Output = 4;
                    break;
                case "T":
                    Output = 5;
                    break;
                case "S":
                    Output = 6;
                    break;
                case "X":
                    Output = 7;
                    break;
                default:
                    Output = 1;
                    break;
            }
            return Output;
        }
        public static async Task<Account> DisplayAccounts(int cID, JsonSerializerOptions jsonOptions, HttpClient http)
        {
            var jsonResponse = await Get.AccountsByCIDAsync(http, jsonOptions, cID);
            var Accounts = (IEnumerable<Account>)jsonResponse.DataReturned;
            foreach (var a in Accounts)
            {
                Console.WriteLine($"{a.ID}|{a.Description}|{a.Type}|{a.CreationDate}");
            }
            Console.WriteLine($"Select Account Number: ");
            int IN = Convert.ToInt32(Console.ReadLine());

            foreach (var a in Accounts)
            {
                if (a.ID == IN)
                {
                    //Console.WriteLine($"{a.ID}|{a.Description}|{a.Type}|{a.CreationDate}");//temp for debug
                    return a;
                }
            }
            return null;
        }
        public static async Task<int> DisplayBalance(int CID, JsonSerializerOptions jsonOptions, HttpClient http)
        {
            var balance = await Get.BalanceByCID(CID, jsonOptions, http);
            Console.WriteLine($"Balance: {balance}");
            return 1;
        }
        public static async Task<int> Deposit(JsonSerializerOptions jsonOptions, int CID, HttpClient http)
        {
            Console.WriteLine($"Enter Deposit Amount: ");
            var Deposit = Convert.ToDecimal(Console.ReadLine());
            if(Deposit < 1)
            {
                Console.WriteLine($"Deposit amount must be greater than 0. Please enter a new amount.");
                return 3;
            }
            Console.WriteLine($"Enter Deposit Description: ");
            var DepositDescription = Console.ReadLine();
            var AID = await Modify.CreateTransaction(Deposit, jsonOptions, CID, DepositDescription, "D", http);
            var NewBalance = await Modify.deposit(Deposit, AID, jsonOptions, http);
            Console.WriteLine($"New Balance: {NewBalance}");
            return 1;
        }
        public static async Task<int> Withdraw(JsonSerializerOptions jsonOptions, int CID, HttpClient http)
        {
            Console.WriteLine($"Enter Withdraw Amount: ");
            var Withdraw = Convert.ToDecimal(Console.ReadLine());
            if (Withdraw < 1)
            {
                Console.WriteLine($"Withdraw amount must be greater than 0. Please enter a new amount.");
                return 4;
            }
            Console.WriteLine($"Enter Withdraw Description: ");
            var WithdrawDescription = Console.ReadLine();
            var AID = await Modify.CreateTransaction(Withdraw, jsonOptions, CID, WithdrawDescription, "W", http);
            var NewBalance = await Modify.withdraw(Withdraw, AID, jsonOptions, http);
            Console.WriteLine($"New Balance: {NewBalance}");
            return 1;
        }
        public static async Task<int> Transfer(JsonSerializerOptions jsonOptions, int CID, HttpClient http)
        {
            Console.WriteLine($"Enter Transfer Amount: ");
            var Transfer = Convert.ToDecimal(Console.ReadLine());

            Console.WriteLine($"Enter Transfer Description: ");
            var TransferDescription = Console.ReadLine();

            Console.WriteLine($"Select which account to transfer From: ");
            var AID = await Modify.CreateTransaction(Transfer, jsonOptions, CID, TransferDescription, "W", http);
            //creating new var for GetByAIDAsync. retreiving balance before if statement.
            //if (Transfer > )
            var NewBalance = await Modify.withdraw(Transfer, AID, jsonOptions, http);
            
            Console.WriteLine($"New Balance: {NewBalance}");

            Console.WriteLine($"Select which account to transfer To: ");
            AID = await Modify.CreateTransaction(Transfer, jsonOptions, CID, TransferDescription, "D", http);
            NewBalance = await Modify.deposit(Transfer, AID, jsonOptions, http);
            Console.WriteLine($"New Balance: {NewBalance}");
            return 1;
        }
        public static async Task<int> Transactions(JsonSerializerOptions jsonOptions, int CID, HttpClient http)
        {
            var tempacc = await DisplayAccounts(CID, jsonOptions, http);
            var AID = tempacc.ID;
            var jsonResponse = await Get.AllTransactions(jsonOptions, AID, http);
            var Transactions = (IEnumerable<Transaction>)jsonResponse.DataReturned;
            foreach (Transaction T in Transactions)
            {
                Console.WriteLine($"{T.ID}|{T.PreviousBalance}|{T.TransactionType}|{T.NewBalance}|{T.Description}|{T.CreationDate}");
            }
            return 1;
        }
    }
}
