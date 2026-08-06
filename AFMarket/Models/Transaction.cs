using System;
using System.Collections.Generic;

namespace AFMarket.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }

    public virtual User User { get; set; } = null!;
}
