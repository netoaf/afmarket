using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories;
using WebApplication1.Services;
using Xunit;

namespace WebApplication1.Tests;

public class OrderServiceTests
{
    private const int StudentId = 1;
    private const int StaffId = 2;

    private static (OrderService service, ApplicationDbContext db) CreateService(decimal balance = 20m)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);

        db.Users.AddRange(
            new User { Id = StudentId, UserNumber = "student", Password = "x", Email = "", Phone = "", ProfileImage = "", Name = "Student", Balance = balance, UserType = "Student" },
            new User { Id = StaffId, UserNumber = "func", Password = "x", Email = "", Phone = "", ProfileImage = "", Name = "Staff", Balance = 0m, UserType = "Staff" });
        db.Categories.Add(new Category { Id = 1, Name = "Snacks" });
        db.Products.AddRange(
            new Product { Id = 1, Name = "Coca-Cola", Description = "", ImageUrl = "", Allergens = "", CategoryId = 1, Price = 1.20m, Stock = 100, IsAvailable = true },
            new Product { Id = 2, Name = "Sandwich", Description = "", ImageUrl = "", Allergens = "", CategoryId = 1, Price = 3.50m, Stock = 50, IsAvailable = true });
        db.SaveChanges();

        var service = new OrderService(
            new OrderRepository(db),
            new CartRepository(db),
            new ProductRepository(db, NullLogger<ProductRepository>.Instance),
            new UserRepository(db));
        return (service, db);
    }

    private static void AddToCart(ApplicationDbContext db, int productId, int quantity)
    {
        db.Cart.Add(new Cart { UserId = StudentId, ProductId = productId, Quantity = quantity, CreatedAt = DateTime.Now });
        db.SaveChanges();
    }

    [Fact]
    public async Task CreateFromCart_DebitsBalanceAndCreatesPendingOrder()
    {
        var (service, db) = CreateService(balance: 20m);
        AddToCart(db, 1, 2);  // 2 x 1.20 = 2.40
        AddToCart(db, 2, 1);  // 1 x 3.50 = 3.50

        var order = await service.CreateFromCartAsync(StudentId);

        Assert.NotNull(order);
        Assert.Equal("Pending", order.Status);
        Assert.Equal(5.90m, order.TotalAmount);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(14.10m, db.Users.Find(StudentId).Balance);
    }

    [Fact]
    public async Task CreateFromCart_DecrementsStockAndClearsCart()
    {
        var (service, db) = CreateService();
        AddToCart(db, 1, 3);

        await service.CreateFromCartAsync(StudentId);

        Assert.Equal(97, db.Products.Find(1).Stock);
        Assert.Empty(db.Cart.Where(c => c.UserId == StudentId));
    }

    [Fact]
    public async Task CreateFromCart_WithInsufficientBalance_ChangesNothing()
    {
        var (service, db) = CreateService(balance: 1m);
        AddToCart(db, 2, 1);  // 3.50 > 1.00

        var order = await service.CreateFromCartAsync(StudentId);

        Assert.Null(order);
        Assert.Equal(1m, db.Users.Find(StudentId).Balance);
        Assert.Equal(50, db.Products.Find(2).Stock);
        Assert.Empty(db.Orders);
        Assert.Single(db.Cart);
    }

    [Fact]
    public async Task CreateFromCart_WithEmptyCart_ReturnsNull()
    {
        var (service, db) = CreateService();

        Assert.Null(await service.CreateFromCartAsync(StudentId));
        Assert.Equal(20m, db.Users.Find(StudentId).Balance);
    }

    [Fact]
    public async Task CreateFromCart_ForUnknownUser_ReturnsNull()
    {
        var (service, _) = CreateService();

        Assert.Null(await service.CreateFromCartAsync(999));
    }

    [Fact]
    public async Task CancelPendingOrder_RefundsTheFullAmount()
    {
        var (service, db) = CreateService(balance: 20m);
        AddToCart(db, 2, 2);  // 7.00
        var order = await service.CreateFromCartAsync(StudentId);
        Assert.Equal(13m, db.Users.Find(StudentId).Balance);

        var ok = await service.UpdateStatusAsync(order.Id, "Canceled");

        Assert.True(ok);
        Assert.Equal(20m, db.Users.Find(StudentId).Balance);
        Assert.Equal("Canceled", db.Orders.Find(order.Id).Status);
    }

    [Fact]
    public async Task OrderLifecycle_PendingReadyDelivered_RecordsTheStaffMember()
    {
        var (service, db) = CreateService();
        AddToCart(db, 1, 1);
        var order = await service.CreateFromCartAsync(StudentId);

        Assert.True(await service.UpdateStatusAsync(order.Id, "Ready"));
        Assert.True(await service.UpdateStatusAsync(order.Id, "Delivered", StaffId));

        var saved = db.Orders.Find(order.Id);
        Assert.Equal("Delivered", saved.Status);
        Assert.Equal(StaffId, saved.DeliveredByStaffId);
        Assert.NotNull(saved.DeliveredAt);
    }

    [Fact]
    public async Task Delivering_WithoutStaffId_IsRejected()
    {
        var (service, db) = CreateService();
        AddToCart(db, 1, 1);
        var order = await service.CreateFromCartAsync(StudentId);
        await service.UpdateStatusAsync(order.Id, "Ready");

        Assert.False(await service.UpdateStatusAsync(order.Id, "Delivered"));
        Assert.Equal("Ready", db.Orders.Find(order.Id).Status);
    }

    [Theory]
    [InlineData("Pending", "Delivered")]
    [InlineData("Delivered", "Canceled")]
    [InlineData("Canceled", "Pending")]
    [InlineData("Pending", "Unknown")]
    public async Task InvalidStatusChanges_AreRejected(string from, string to)
    {
        var (service, db) = CreateService();
        db.Orders.Add(new Order { Id = 1, UserId = StudentId, TotalAmount = 5m, Status = from, OrderDate = DateTime.Now });
        db.SaveChanges();

        Assert.False(await service.UpdateStatusAsync(1, to, StaffId));
        Assert.Equal(from, db.Orders.Find(1).Status);
    }

    [Fact]
    public async Task CancelingAReadyOrder_DoesNotRefundTwice()
    {
        // Documents current behaviour: only Pending orders are refunded on cancel.
        var (service, db) = CreateService(balance: 20m);
        AddToCart(db, 2, 1);  // 3.50
        var order = await service.CreateFromCartAsync(StudentId);
        await service.UpdateStatusAsync(order.Id, "Ready");

        await service.UpdateStatusAsync(order.Id, "Canceled");

        Assert.Equal(16.50m, db.Users.Find(StudentId).Balance);
    }
}
