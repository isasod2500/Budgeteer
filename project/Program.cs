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
        public decimal convertedAmount { get; set; }
        public string convertedCurrency { get; set; }
    }

    class readEnv
    {
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

        public static async Task<string> fetchAPI()
        {

            readEnv.Load(".env");

            string apiKey = Environment.GetEnvironmentVariable("API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("API_KEY MISSING");
            }

            using var client = new HttpClient();

            string url = $"https://api.fxratesapi.com/latest?api_key={apiKey}";

            string body = await client.GetStringAsync(url);

            return body;

        }

        public static decimal ConvertCurrency(
            decimal amount,
            string fromCurrency,
            string toCurrency,
            Dictionary<string, decimal> rates)
        {
            if (fromCurrency == toCurrency)
                return amount;

            decimal fromRate = rates[fromCurrency];
            decimal toRate = rates[toCurrency];

            return amount / fromRate * toRate;
        }



        class Program
        {

            static async Task Main(string[] args)
            {


                //Skapa nya listor utefter klasserna ovan, och deras struktur.
                GroupList groupList = new GroupList();
                ExpenseList expenseList = new ExpenseList();




                //Försök läs in textfiler med information. Catch längst ned som fångar felet.
                try
                {

                    readEnv.Load(".env");


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
                    Console.ReadKey();
                }


            }

            static string ReadInputOrCancel(string prompt)
            {

                Console.Write(prompt);
                string input = null;

                while (true)
                {

                    ConsoleKeyInfo key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        return null;
                    }

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        return input;
                    }

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (input.Length > 0)
                        {
                            input = input[..^1];
                        }

                        continue;
                    }

                    if (!char.IsControl(key.KeyChar))
                    {
                        input += key.KeyChar;
                        Console.Write(key.KeyChar);
                    }
                }

            }

            public static async Task<bool> IsValidCurrency(string currency)
            {
                if (string.IsNullOrWhiteSpace(currency))

                    return false;

                currency = currency.Trim().ToUpperInvariant();

                try
                {
                    string body = await fetchAPI();

                    using JsonDocument doc = JsonDocument.Parse(body);

                    JsonElement rates = doc.RootElement.GetProperty("rates");

                    return rates.TryGetProperty(currency, out _);


                }
                catch (Exception)
                {
                    return false;
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

                        int groupNumber = userInput.KeyChar - '0';
                        int groupIndex = groupNumber - 1;
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

                                // Ange gruppnamn
                                string groupnameInput;

                                while (true)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Create Group");
                                    Console.WriteLine();

                                    groupnameInput = ReadInputOrCancel(
                                        "Enter groupname (min 3 characters): ");

                                    // ESC avbryter skapandet
                                    if (groupnameInput == null)
                                        break;

                                    if (groupnameInput.Trim().Length >= 3)
                                        break;

                                    Console.WriteLine("Group name must be at least 3 characters.");
                                    Console.ReadKey(true);
                                }

                                // Om användaren tryckte ESC, avbryt hela skapandet
                                if (groupnameInput == null)
                                    break;


                                // Ange medlemmar
                                string memberInput;

                                while (true)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Create Group");
                                    Console.WriteLine();

                                    memberInput = ReadInputOrCancel(
                                        "Enter members (comma separated): ");

                                    if (memberInput == null)
                                        break;

                                    if (!string.IsNullOrWhiteSpace(memberInput))
                                        break;

                                    Console.WriteLine("Enter at least one member.");
                                    Console.ReadKey(true);
                                }

                                if (memberInput == null)
                                    break;


                                // Ange valuta
                                string currencyInput;

                                while (true)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Create Group");
                                    Console.WriteLine();

                                    currencyInput = ReadInputOrCancel(
                                        "Enter currency to be used (USD, EUR, GBP): ");

                                    if (currencyInput == null)
                                        break;

                                    currencyInput = currencyInput.Trim().ToUpperInvariant();

                                    if (currencyInput.Length == 3 &&
                                        currencyInput.All(char.IsLetter))
                                    {
                                        break;
                                    }

                                    Console.WriteLine("Enter a valid three-letter currency code.");
                                    Console.ReadKey(true);
                                }

                                if (currencyInput == null)
                                    break;


                                // Skapa gruppen först när alla fält är godkända
                                string[] members = memberInput.Split(',',
                                    StringSplitOptions.TrimEntries |
                                    StringSplitOptions.RemoveEmptyEntries);

                                // Kontrollera att minst en medlem finns efter uppdelningen
                                if (members.Length == 0)
                                {
                                    Console.WriteLine("Enter at least one member.");
                                    Console.ReadKey(true);
                                    break;
                                }

                                Group newGroup = new Group
                                {
                                    groupID = groupList.groups
                                        .Select(g => g.groupID)
                                        .DefaultIfEmpty(0)
                                        .Max() + 1,

                                    Name = groupnameInput.Trim(),
                                    Members = members,
                                    Currency = currencyInput
                                };

                                groupList.groups.Add(newGroup);

                                // Spara grupperna till fil
                                string jsonAdd = JsonSerializer.Serialize(groupList.groups);
                                File.WriteAllText("groups.txt", jsonAdd);

                                Console.WriteLine();
                                Console.WriteLine($"Group '{newGroup.Name}' created successfully!");
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey(true);

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

                    Console.WriteLine(
                        $"Members: {string.Join(", ", group.Members)}"
                        );

                    Console.WriteLine($"Currency: {group.Currency}");

                    Console.WriteLine();
                    Console.WriteLine();
                    foreach (var expense in expenseList.expenses
                            .Where(e => e.groupID == group.groupID))
                    {
                        Console.WriteLine(
                            $"{expense.expID}: {expense.expName}, {Math.Round(expense.convertedAmount, 2)} {expense.convertedCurrency}");
                        Console.WriteLine();
                        Console.WriteLine();
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
                        int expenseNumber = userInput.KeyChar - '0';
                        int expenseIndex = expenseNumber - 1;

                        if (expenseIndex >= 0 && expenseList.expenses.Count < expenseList.expenses.Count)
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
                                {
                                    Console.Clear();
                                    Console.WriteLine("Create new expense");
                                    Console.WriteLine();

                                    // 1. Ange namn
                                    string? expNameInput = ReadInputOrCancel("Enter expense name: ");

                                    if (expNameInput == null)
                                        break;

                                    if (string.IsNullOrWhiteSpace(expNameInput))
                                    {
                                        Console.WriteLine("Expense name cannot be empty.");
                                        Console.ReadKey();
                                        break;
                                    }

                                    // 2. Ange medlemmar
                                    string? expMembersInput = ReadInputOrCancel(
                                        "Enter members to share expense (comma separated): ");

                                    if (expMembersInput == null)
                                        break;

                                    string[] expMembers = expMembersInput.Split(
                                        ',',
                                        StringSplitOptions.TrimEntries |
                                        StringSplitOptions.RemoveEmptyEntries
                                    );

                                    if (expMembers.Length == 0)
                                    {
                                        Console.WriteLine("You must enter at least one member.");
                                        Console.ReadKey();
                                        break;
                                    }

                                    // 3. Ange valuta
                                    string? expCurrencyInput = ReadInputOrCancel(
                                        "Enter currency (USD, EUR, GBP etc): ");

                                    if (expCurrencyInput == null)
                                        break;

                                    expCurrencyInput = expCurrencyInput.Trim().ToUpperInvariant();

                                    if (expCurrencyInput.Length != 3)
                                    {
                                        Console.WriteLine("Enter a valid three-letter currency code.");
                                        Console.ReadKey();
                                        break;
                                    }

                                    Console.WriteLine();
                                    Console.WriteLine(
                                        $"Expenses will be converted to the group's currency ({group.Currency})."
                                    );

                                    // 4. Ange belopp
                                    string? amountInput = ReadInputOrCancel("Enter expense amount: ");

                                    if (amountInput == null)
                                        break;

                                    if (!decimal.TryParse(amountInput, out decimal expAmountInput)
                                        || expAmountInput < 0)
                                    {
                                        Console.WriteLine("Invalid amount.");
                                        Console.ReadKey();
                                        break;
                                    }

                                    // Börja med originalbeloppet.
                                    // Om valutan är samma som gruppens behövs ingen konvertering.
                                    decimal convertedAmount = expAmountInput;

                                    // 5. Konvertera valutan om det behövs
                                    if (!string.Equals(
                                        expCurrencyInput,
                                        group.Currency,
                                        StringComparison.OrdinalIgnoreCase))
                                    {
                                        try
                                        {
                                            string body = await readEnv.fetchAPI();

                                            using JsonDocument doc = JsonDocument.Parse(body);

                                            JsonElement root = doc.RootElement;
                                            JsonElement rates = root.GetProperty("rates");

                                            string? baseCurrency = root
                                                .GetProperty("base")
                                                .GetString();

                                            if (!rates.TryGetProperty(
                                                    expCurrencyInput, out JsonElement fromRateElement) ||
                                                !rates.TryGetProperty(
                                                    group.Currency, out JsonElement toRateElement))
                                            {
                                                Console.WriteLine(
                                                    "One of the currencies was not found in the API."
                                                );
                                                Console.ReadKey();
                                                break;
                                            }

                                            decimal fromRate = fromRateElement.GetDecimal();
                                            decimal toRate = toRateElement.GetDecimal();

                                            convertedAmount = expAmountInput / fromRate * toRate;

                                            Console.WriteLine();
                                            Console.WriteLine($"Exchange rate base currency: {baseCurrency}");
                                            Console.WriteLine(
                                                $"Original amount: {expAmountInput:F2} {expCurrencyInput}"
                                            );
                                            Console.WriteLine(
                                                $"Converted amount: {convertedAmount:F2} {group.Currency}"
                                            );
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(
                                                $"Currency conversion failed: {ex.Message}"
                                            );
                                            Console.ReadKey();
                                            break;
                                        }
                                    }

                                    // 6. Bekräfta innan expense sparas
                                    Console.WriteLine();
                                    Console.WriteLine("Press ENTER to save this expense.");
                                    Console.WriteLine("Press ESC to cancel.");

                                    ConsoleKeyInfo confirmKey = Console.ReadKey(true);

                                    if (confirmKey.Key != ConsoleKey.Enter)
                                    {
                                        Console.WriteLine("Expense creation cancelled.");
                                        break;
                                    }

                                    // 7. Skapa expense-objektet
                                    Expense newExpense = new Expense
                                    {
                                        expID = expenseList.expenses
                                            .Select(e => e.expID)
                                            .DefaultIfEmpty(0)
                                            .Max() + 1,

                                        groupID = group.groupID,
                                        expName = expNameInput.Trim(),
                                        expMembers = expMembers,
                                        expCurrency = expCurrencyInput,
                                        expAmount = expAmountInput,
                                        convertedAmount = convertedAmount,
                                        convertedCurrency = group.Currency
                                    };

                                    // 8. Lägg till och spara
                                    expenseList.expenses.Add(newExpense);

                                    string jsonAdd = JsonSerializer.Serialize(expenseList.expenses);
                                    File.WriteAllText("expenses.txt", jsonAdd);

                                    Console.WriteLine("Expense saved successfully!");
                                    Console.ReadKey();

                                    break;
                                }
                            case ConsoleKey.Escape:
                                await MainMenu(groupList, expenseList);
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

                    Console.WriteLine();


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