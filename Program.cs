using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

class Program
{
    static void Main()
    {
        Console.WriteLine("Expense Tracker - enter a command or \"help\" to list the available commands or \"exit\" to quit: ");
        do
        {
            string? userInput = Console.ReadLine()?.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                Console.WriteLine("Please enter a command: ");
                continue;
            }

            if (userInput == "exit")
            {
                break;
            }

            try
            {
                if (userInput.StartsWith("add", StringComparison.OrdinalIgnoreCase))
                {
                    Expense newExpense = CommandParser.ParseAddCommand(userInput);
                    ExpensesService.AddExpense(newExpense);
                }
                else if (userInput == "list")
                {
                    List<Expense> existingExpenses = ExpensesService.ListExpenses();
                    Console.WriteLine($"{"ID",-5} {"Date",-12} {"Description",-30} {"Amount",10}");
                    Console.WriteLine(new string('-', 65));
                    foreach (Expense expense in existingExpenses)
                    {
                        Console.WriteLine($"{expense.Id,-5} {expense.Date.ToString("d"),-12} {expense.Description,-30} {expense.Amount.ToString("C", CultureInfo.CurrentCulture),10}");
                    }
                }
                else if (userInput.StartsWith("delete", StringComparison.OrdinalIgnoreCase))
                {
                    int id = CommandParser.ParseDeleteCommand(userInput);
                    ExpensesService.DeleteExpense(id);
                }
                else if (userInput.StartsWith("summary", StringComparison.OrdinalIgnoreCase))
                {
                    ExpensesService.SummaryExpenses(userInput);
                }
                else if (userInput == "clear")
                {
                    Console.Clear();
                    Console.WriteLine("Expense Tracker - enter a command or \"help\" to list the available commands or \"exit\" to quit: ");
                }
                else if (userInput == "help")
                {
                    Console.WriteLine("Here are the available commands:");
                    Console.WriteLine("help: Show available commands");
                    Console.WriteLine("add --description \"...\" --amount 30: Add a new expense");
                    Console.WriteLine("list: Lists all expenses");
                    Console.WriteLine("delete --id 1: Delete expense by ID");
                    Console.WriteLine("summary: Show total expenses");
                    Console.WriteLine("summary --month 8: Show total for a specific month (e.g., 8 = August)");
                    Console.WriteLine("clear: Clears the console");
                    Console.WriteLine("exit: Exit the app");
                }
                else
                {
                    Console.WriteLine("Invalid command. Please try again:");
                    continue;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON error: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"File read error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Input error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        } while (true);

    }
}
public class Expense
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = "";
    public int Amount { get; set; }
    // public string Category { get; set; }
}

public static class ExpensesService
{
    // TO DO: Remove debug option before release
    static bool isDebug = Debugger.IsAttached;
    static string filePath = isDebug ? Path.Combine(AppContext.BaseDirectory, "expenses.json") : "expenses.json";
    static List<Expense> expenses = new List<Expense>();
    static int id = 1;

    public static int AddExpense(Expense expense)
    {
        expense.Id = id++;
        expense.Date = DateTime.Now;
        expenses.Add(expense);
        SaveExpensesToFile();
        Console.WriteLine($"Expense added successfully ID: {expense.Id}");
        return expense.Id;
    }

    public static List<Expense> ListExpenses()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return new List<Expense>();
        }

        string json = File.ReadAllText(filePath);
        expenses = JsonSerializer.Deserialize<List<Expense>>(json) ?? new List<Expense>();
        return expenses;
    }

    public static void SummaryExpenses(string input)
    {
        var args = CommandParser.SplitCommandLine(input);
        var monthIndex = Array.IndexOf(args, "--month");
        List<Expense> expenses = ExpensesService.ListExpenses();
        int totalAmount = 0;

        // Overall summary
        if (args.Length == 1)
        {
            foreach (Expense expense in expenses)
            {
                totalAmount += expense.Amount;
            }

            Console.WriteLine($"Total expenses: {totalAmount.ToString("C", CultureInfo.CurrentCulture)}");
        }
        else
        {
            if (monthIndex == -1)
            {
                throw new ArgumentException("Missing required argument.");
            }

            int month = int.Parse(args[monthIndex + 1]);
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            foreach (Expense expense in expenses)
            {
                if (expense.Date.Month == month)
                {
                    totalAmount += expense.Amount;
                }
            }
            Console.WriteLine($"Total expenses for {monthName}: {totalAmount.ToString("C", CultureInfo.CurrentCulture)}");
        }
    }

    public static Expense? Get(int id) => expenses.FirstOrDefault(expense => expense.Id == id);

    public static void DeleteExpense(int id)
    {
        var expense = Get(id);
        if (expense is null)
        {
            return;
        }

        expenses.Remove(expense);
        SaveExpensesToFile();
        Console.WriteLine("Expense deleted successfully");
    }

    static void SaveExpensesToFile()
    {
        string updatedJson = JsonSerializer.Serialize(expenses, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, updatedJson);
    }

    // Comment-out for now
    // static void LoadExpensesFromFile()
    // {
    //     if (!File.Exists(filePath))
    //     {
    //         Console.WriteLine($"File not found: {filePath}");
    //         return; // Exit app
    //     }

    //     string json = File.ReadAllText(filePath);
    //     expenses = JsonSerializer.Deserialize<List<Expense>>(json) ?? new List<Expense>();
    // }
}

public static class CommandParser
{
    public static Expense ParseAddCommand(string input)
    {
        var args = SplitCommandLine(input);
        var descriptionIndex = Array.IndexOf(args, "--description");
        var amountIndex = Array.IndexOf(args, "--amount");

        if (descriptionIndex == -1 || amountIndex == -1)
        {
            throw new ArgumentException("Missing required arguments.");
        }

        string description = args[descriptionIndex + 1];
        int amount = int.Parse(args[amountIndex + 1]);

        // Note that only the description and amount are set here
        // Set the other required properties before adding to the expense list
        return new Expense { Description = description, Amount = amount };
    }

    public static int ParseDeleteCommand(string input)
    {
        var args = SplitCommandLine(input);
        var idIndex = Array.IndexOf(args, "--id");

        if (idIndex == -1)
        {
            throw new ArgumentException("Missing required argument.");
        }

        int id = int.Parse(args[idIndex + 1]);

        return id;
    }

    /*
    * Splits a command-line-style input string into an array of arguments.
    *
    * - Arguments are separated by whitespace
    * - Quoted substrings (enclosed in double quotes) are treated as single arguments.
    * 
    * Example:
    * Input: add --description "Lunch with coworkers" --amount 20
    * Output: ["add", "--description", "lunch with coworkers", "--amount", "20"]
    */
    public static string[] SplitCommandLine(string input)
    {
        var args = new List<string>();
        var currentArg = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (currentArg.Length > 0)
                {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
            }
            else
            {
                currentArg.Append(c);
            }
        }

        if (currentArg.Length > 0)
        {
            args.Add(currentArg.ToString());
        }

        return args.ToArray();
    }
}