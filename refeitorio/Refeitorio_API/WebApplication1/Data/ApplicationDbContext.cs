using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<WeeklyMenu> WeeklyMenu { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração de relacionamentos e restrições

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Balance)
                .HasColumnType("decimal(10,2)");  // Explicit precision for currency

            // Category
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Product
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);

            // WeeklyMenu
            modelBuilder.Entity<WeeklyMenu>()
                .HasOne(wm => wm.MainDish)
                .WithMany()
                .HasForeignKey(wm => wm.MainDishId);

            modelBuilder.Entity<WeeklyMenu>()
                .HasOne(wm => wm.Soup)
                .WithMany()
                .HasForeignKey(wm => wm.SoupId);

            modelBuilder.Entity<WeeklyMenu>()
                .HasOne(wm => wm.Dessert)
                .WithMany()
                .HasForeignKey(wm => wm.DessertId);

            // Ticket
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Menu)
                .WithMany()
                .HasForeignKey(t => t.MenuId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.ValidatedByStaff)
                .WithMany()
                .HasForeignKey(t => t.ValidatedByStaffId);

            // Cart
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId);

            // Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.DeliveredByStaff)
                .WithMany()
                .HasForeignKey(o => o.DeliveredByStaffId);

            // OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            // Favorite
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.ProductId })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
