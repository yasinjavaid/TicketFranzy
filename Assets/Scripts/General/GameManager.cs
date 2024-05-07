using System.Collections.Generic;
using System.Linq;

public class GameManager : SingletonMB<GameManager>
{
    public static int Tickets { get; protected set; }

    public static IEnumerable<Transaction> GetTransactions => transactions;

    protected static List<Transaction> transactions = new List<Transaction>();

    public static void AddTickets(string title, string description, int value, int quantity = 1)
    {
        Tickets += value * quantity;

        if (transactions.Count > 0)
        {
            Transaction lastTransaction = transactions[transactions.Count - 1];
            if (title == lastTransaction.Title && description == lastTransaction.Description && value == lastTransaction.Value)
                lastTransaction.Quantity += quantity;
            else transactions.Add(new Transaction(title, description, value, quantity));
        }
        else transactions.Add(new Transaction(title, description, value, quantity));
    }
}

public class Transaction
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int Value { get; set; }
    public int Quantity { get; set; }
    public int Subtotal => Value * Quantity;

    public Transaction(string title, string description, int value, int quantity)
    {
        Title = title;
        Description = description;
        Value = value;
        Quantity = quantity;
    }

    public bool Matches(Transaction other) => Title == other.Title && Description == other.Description && Value == other.Value;
}