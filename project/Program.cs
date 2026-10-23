using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Budgeteer
{

    class GroupList
    {
        public List<Group> groups = new List<Group>();
    }

    class Group {
        public string Name { get; set; }
        public string[] Members { get; set; }
    }

    class ExpenseList {
        public List<Expense> expenses = new List<Expense>();
    }

    class Expense
    {
        public string expName { get; set; }
        public string[] expMembers { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
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

                MainMenu(groupList, expenseList);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }


        }
        static void MainMenu(
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

                if (groupList.groups.Count == 0)
                {
                    Console.WriteLine("No groups yet created. Press C to start creating!");
                }
                else
                {
                    for (int i = 0; i < groupList.groups.Count; i++)
                    {
                        var group = groupList.groups[i];
                        Console.WriteLine($"[{i}] {group.Name}");
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

                    if (groupIndex < groupList.groups.Count)
                    {
                        GroupMenu(
                            groupList.groups[groupIndex],
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
                            Console.WriteLine("Create Group");
                            Console.Write("Enter groupname: ");
                            string groupname = Console.ReadLine();

                            Console.Clear();
                            Console.WriteLine("Create Group");
                            Console.Write("Enter members (separate with commas): ");
                            string memberInput = Console.ReadLine();

                            if (!string.IsNullOrWhiteSpace(groupname) &&
                                !string.IsNullOrWhiteSpace(memberInput))
                            {
                                string[] members = memberInput.Split(',',
                                    StringSplitOptions.TrimEntries |
                                    StringSplitOptions.RemoveEmptyEntries
                                    );

                                Group newGroup = new Group
                                {
                                    Name = groupname,
                                    Members = members
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
        static void GroupMenu(
            Group group,
            ExpenseList expenseList)
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
                    Console.WriteLine("Invalid option");
                }
              
            }
        }

        static void ExpenseMenu(Expense expense) {
            Console.Clear();

            Console.WriteLine($"Expense: {expense.expName}");
            Console.WriteLine($"Amount: {expense.Amount}");
            Console.WriteLine($"Currency: {expense.Currency}");
            Console.WriteLine(
                $"Members: {string.Join(", ", expense.expMembers)}"
            );

            Console.WriteLine();
            Console.WriteLine("Press any key to return.");
            Console.ReadKey(true);
        }

    }
}




    
