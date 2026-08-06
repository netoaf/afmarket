using System;
using System.Collections.Generic;

namespace AFMarket.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Role { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public decimal Balance { get; set; }

    public DateTime RegistrationDate { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
