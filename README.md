# AFMarket

AFMarket is an academic marketplace prototype built with ASP.NET Core MVC and
SQL Server. It covers the complete flow from browsing a catalogue to placing a
simulated order.

I developed this project independently, using AI tools as development support.
It is presented as a learning and portfolio project, not as a production
e-commerce platform.

## What is implemented

- Product catalogue with search and category filters
- User registration, login, profile editing and session management
- Password hashing with ASP.NET Core's `PasswordHasher`
- Favourites and a per-user shopping cart
- Quantity and stock validation
- Simulated account balance and transaction history
- Transactional checkout, order history and stock updates
- Admin-only product and user management screens
- SQL Server relational model for users, products, categories, carts, orders,
  order items, transactions, favourites and promotions

## Technology

- C# and .NET 8
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server / SQL LocalDB
- Razor, HTML, CSS and JavaScript

## Run locally

Requirements:

- .NET 8 SDK
- SQL Server LocalDB or another SQL Server instance

1. Run `database/reset-demo-database.sql` against a local SQL Server instance.
   The script is intentionally destructive **only for the database named
   `AFMarketDB`**.
2. If you are not using LocalDB, override the `DefaultConnection` connection
   string with user secrets, an environment variable, or
   `appsettings.Local.json`.
3. Start the application:

   ```powershell
   dotnet restore
   dotnet run --project AFMarket/AFMarket.csproj
   ```

4. Register a user through the web interface. New demo users receive a
   simulated starting balance of 100.

To test the admin screens locally, promote a registered account using the
commented query at the end of the database script.

## Security and scope

The public version removes machine-specific credentials from the original
school prototype, hashes passwords, validates anti-forgery tokens on state
changes, restricts cart operations to their owner and limits management actions
to an admin session.

Authentication is still a small custom session-based implementation. Payments
and balance top-ups are simulations, concurrent checkout has not been
stress-tested, and the project has no automated test suite yet. A production
version should use ASP.NET Core Identity, migrations, stronger authorization,
structured logging and integration tests.
