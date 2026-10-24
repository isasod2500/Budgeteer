using System;
using System.Text.Json;
using System.Linq;

namespace Budgeteer
{

    //Deklarerar klasser

    //Lista för grupper
    class GroupList
    {
        public List<Group> groups = new List<Group>();
    }

    //Objekt för grupp, som i sin tur ska lagras i GroupList
    class Group
    {
        public int groupID { get; set; }
        public string Name { get; set; }
        public string[] Members { get; set; }
        public string Currency { get; set; }
    }

    //Lista för utgifter, expenses
    class ExpenseList
    {
        public List<Expense> expenses = new List<Expense>();
    }

    //Objekt för Expense, som i sin tur lagras i expenselist.
    class Expense
    {
        public int groupID { get; set; }
        public int expID { get; set; }
        public string expName { get; set; }
        public string[] expMembers { get; set; }
        public string expCurrency { get; set; }
        public decimal expAmount { get; set; }
    }

    class readEnv {
        public static void Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file '{filePath}' does not exist.");

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                var key = parts[0].Trim();
                var value = parts[1].Trim();
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        public static async Task fetchAPI()
        {
            string apiKey = Environment.GetEnvironmentVariable("API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("API_KEY MISSING");
            }

            using var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.fxratesapi.com/latest?api_key={apiKey}")
            };

            using var response = await client.SendAsync(request);
            
            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();

            Console.WriteLine(body);

            
        }

        class Program
    {

        static async void Main(string[] args)
        {


            //Skapa nya listor utefter klasserna ovan, och deras struktur.
            GroupList groupList = new GroupList();
            ExpenseList expenseList = new ExpenseList();




            //Försök läs in textfiler med information. Catch längst ned som fångar felet.
            try
            {
                if (File.Exists("groups.txt"))
                {
                    string json = File.ReadAllText("groups.txt");
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        groupList.groups = JsonSerializer.Deserialize<List<Group>>(json);
                    }
                }

                if (File.Exists("expenses.txt"))
                {
                    string json = File.ReadAllText("expenses.txt");
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        expenseList.expenses = JsonSerializer.Deserialize<List<Expense>>(json);
                    }
                }

                await MainMenu(groupList, expenseList);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }


        }

        //Main menu funktion. Denna kallas automatiskt, förutsatt att .txt filerna hittas.
        static async Task MainMenu(
            GroupList groupList,
            ExpenseList expenseList)
        {



            //True är alltid true, så detta körs hela tiden. 
            while (true)
            {
                Console.Clear();
                //text.
                Console.WriteLine();
                Console.WriteLine("B U D G E T E E R");
                Console.WriteLine("Manage your expenses with your friends; split, calculate and settle.");

                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("Use the numbers and letters indicated below to navigate the application.");

                Console.WriteLine();

                Console.WriteLine("Your groups:");

                Console.WriteLine();

                //Om inga grupper finns, skriv ut en kommentar. Annars ladda samtliga grupper.
                if (groupList.groups.Count == 0)
                {
                    Console.WriteLine("No groups yet created. Press C to start creating!");
                }
                else
                {
                    for (int i = 0; i < groupList.groups.Count; i++)
                    {
                        var group = groupList.groups[i];
                        Console.WriteLine($"[{group.groupID}] {group.Name}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("E. Edit group");
                Console.WriteLine("C. Create group");
                Console.WriteLine("D. Delete group");
                Console.WriteLine("ESC. Close application");

                Console.WriteLine();
                Console.WriteLine();

                //Tar användarens val
                Console.Write("Enter option here: ");
                var userInput = Console.ReadKey(true);

                //Gruppval, index 0-9
                if (char.IsDigit(userInput.KeyChar) &&
                    userInput.KeyChar >= '0' &&
                    userInput.KeyChar <= '9')
                {

                    int groupIndex = userInput.KeyChar - '0';

                    //Om siffran är 0-9 och inte är högre än gruppernas totala mängd, kallas funktionen GroupMenu och öppnar en meny med motsvarande grupp.
                    if (groupIndex < groupList.groups.Count)
                    {
                        await GroupMenu(
                            groupList.groups[groupIndex],
                            groupList,
                            expenseList);

                    }
                }
                else
                {
                    //Switch-case för redigering av grupper
                    switch (userInput.Key)
                    {

                        case ConsoleKey.E:
                            Console.WriteLine("Edit group option selected.");
                            break;
                        case ConsoleKey.C:
                            

                            string groupnameInput = string.Empty;
                            while (string.IsNullOrWhiteSpace(groupnameInput) ||
                                groupnameInput.Length < 3)
                            {
                                Console.Clear();
                                Console.WriteLine("Create Group");
                                Console.Write("Enter groupname (min 3 characters): ");
                                groupnameInput = Console.ReadLine();
                            }


                            string memberInput = string.Empty;
                            while (string.IsNullOrWhiteSpace(memberInput))
                            {
                                Console.Clear();
                                Console.WriteLine("Create Group");
                                Console.Write("Enter members (comma separated): ");
                                memberInput = Console.ReadLine();
                            }


                            string currencyInput = string.Empty;
                            while (string.IsNullOrWhiteSpace(currencyInput) ||
                                currencyInput.Length != 3) {

                                Console.Clear();
                                Console.WriteLine("Create Group");
                                Console.Write("Enter currency to be used (USD, EUR, GBP): ");
                                currencyInput = Console.ReadLine();
                            }

                            if (!string.IsNullOrWhiteSpace(groupnameInput) &&
                                !string.IsNullOrWhiteSpace(memberInput) &&
                                !string.IsNullOrWhiteSpace(currencyInput))
                            {
                                string[] members = memberInput.Split(',',
                                    StringSplitOptions.TrimEntries |
                                    StringSplitOptions.RemoveEmptyEntries
                                    );

                                Group newGroup = new Group
                                {
                                    groupID = groupList.groups
                                        .Select(e => e.groupID)
                                        .DefaultIfEmpty(0)
                                        .Max() + 1,
                                    Name = groupnameInput,
                                    Members = members,
                                    Currency = currencyInput
                                };

                                groupList.groups.Add(newGroup);

                                var jsonAdd = JsonSerializer.Serialize(groupList.groups);

                                File.WriteAllText("groups.txt", jsonAdd);
                            }

                            break;
                        case ConsoleKey.D:
                            Console.WriteLine("Delete group option selected.");
                            break;
                        case ConsoleKey.Escape:
                            return; // Exit the application
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
            }
        }
            //Undermeny för att se gruppen och dess medlemmar och utgifter.
            static async Task GroupMenu(
                Group group,
                GroupList groupList,
                ExpenseList expenseList
                )
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine();

                    Console.WriteLine("B U D G E T E E R");

                    Console.WriteLine();
                    Console.WriteLine();

                    Console.WriteLine($"Group: {group.Name}");

                    Console.WriteLine();

                    Console.WriteLine(
                        $"Members: {string.Join(", ", group.Members)}"
                        );

                    Console.WriteLine($"Currency: {group.Currency}");
                    for (var i = 0; i < expenseList.expenses.Count; i++)
                    {
                        Console.WriteLine(i);
                    }

                    Console.WriteLine();
                    Console.WriteLine("Expenses:");
                    Console.WriteLine("Enter number corresponding to expense to view expense in detail");


                    Console.WriteLine();
                    Console.WriteLine();


                    Console.WriteLine("C. Create expense");

                    Console.WriteLine("ESC. Back to start");

                    Console.WriteLine();
                    Console.WriteLine();

                    //Tar användarens val
                    Console.Write("Enter option here: ");
                    var userInput = Console.ReadKey(true);

                    if (char.IsDigit(userInput.KeyChar) &&
                        userInput.KeyChar >= '0' &&
                        userInput.KeyChar <= '9')
                    {
                        int expenseIndex = userInput.KeyChar - '0';

                        if (expenseIndex < expenseList.expenses.Count)
                        {
                            ExpenseMenu(expenseList.expenses[expenseIndex]);

                        }
                    }
                    else
                    {
                        //Switch-case för redigering av utgifter
                        switch (userInput.Key)
                        {

                            case ConsoleKey.C:

                                await readEnv.fetchAPI();
                                Console.Clear();

                                Console.WriteLine("Create new expense");

                                Console.WriteLine();

                                Console.Write("Enter expense name: ");
                                string expNameInput = Console.ReadLine();

                                Console.Clear();

                                Console.WriteLine("Create new expense");
                                Console.WriteLine();
                                Console.WriteLine("Enter members to share expense (comma separated): ");
                                string expMembersInput = Console.ReadLine();

                                Console.Clear();
                                Console.WriteLine("Create new expense");
                                Console.WriteLine();
                                Console.Write("Enter currency (USD, EUR, GBP etc): ");
                                string expCurrencyInput = Console.ReadLine();

                                Console.WriteLine($"Currencies not being the main currency of the group ({group.Currency}) will be automatically converted.");

                                Console.Write("Enter expense amount: ");

                                //Om talet kan omvandlas till decimal, sätt värdet till expAmountInput.
                                if (!decimal.TryParse(Console.ReadLine(), out decimal expAmountInput))
                                {
                                    Console.WriteLine("Invalid amount.");
                                    break;
                                }

                                //Kolla mot gruppens currency.
                                if (!string.Equals(
                                        expCurrencyInput,
                                        group.Currency,
                                        StringComparison.OrdinalIgnoreCase))
                                {

                                }

                                if (!string.IsNullOrWhiteSpace(expNameInput) &&
                                   !string.IsNullOrWhiteSpace(expMembersInput) &&
                                   !string.IsNullOrWhiteSpace(expCurrencyInput) &&
                                   expAmountInput != null)
                                {
                                    string[] expMembers = expMembersInput.Split(',',
                                        StringSplitOptions.TrimEntries |
                                        StringSplitOptions.RemoveEmptyEntries
                                        );

                                    Expense newExpense = new Expense
                                    {
                                        expID = expenseList.expenses
                                            .Select(e => e.expID)
                                            .DefaultIfEmpty(0)
                                            .Max() + 1,
                                        expName = expNameInput,
                                        expMembers = expMembers,
                                        expCurrency = expCurrencyInput,
                                        expAmount = expAmountInput
                                    };

                                    newExpense.groupID = group.groupID;
                                    expenseList.expenses.Add(newExpense);


                                }

                                break;
                            case ConsoleKey.Escape:
                                MainMenu(groupList, expenseList);
                                break;
                            default:
                                Console.WriteLine("Invalid option. Please try again.");
                                break;
                        }




                    }
                }

                static void ExpenseMenu(Expense expense)
                {
                    Console.Clear();

                    Console.WriteLine(
                        $"Members: {string.Join(", ", expense.expMembers)}"
                    );
                    Console.WriteLine($"Expense: {expense.expName}");
                    Console.WriteLine($"Amount: {expense.expAmount}");
                    Console.WriteLine($"Currency: {expense.expCurrency}");


                    Console.WriteLine();
                    Console.WriteLine("Press any key to return.");
                    Console.ReadKey(true);
                }


            }
        }
    }
}




    
