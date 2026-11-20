# Technical Decisions & Justifications

## ViewModel Usage in MVC Project

### Decision
MVC project uses ViewModels instead of domain entities for all data binding operations.

### Implementation
- All controller actions accept and return ViewModels
- All Razor views bind to ViewModels exclusively
- ViewModels contain presentation-specific properties and helper methods

### Justification
- **Separation of Concerns**: Prevents domain model leakage into presentation layer
- **Data Isolation**: MVC cannot directly access domain entities
- **API-First Design**: Forces all data operations through API endpoints
- **Security**: Reduces attack surface by limiting direct data access

### Evidence in Code
```csharp
// Controllers use ViewModels
public async Task<IActionResult> Index()
{
    var products = await _productService.GetAllProducts();
    return View(products); // Returns List<ProductViewModel>
}

// ViewModels contain presentation helpers
public class CartItemViewModel
{
    public decimal TotalPrice => Price * Quantity; // Presentation logic
}
```

## Domain Model Centralization in API

### Decision
All domain models, business logic, and data access are contained exclusively in the API project.

### Implementation
- API project contains all domain entities (9 models)
- API project implements all business rules and validation
- API project handles all database operations via Entity Framework Core

### Justification
- **Single Source of Truth**: Centralizes business logic and data rules
- **Maintainability**: Changes to business logic require updates in only one project
- **Security**: Database access restricted to API layer only
- **Scalability**: API can be independently scaled and secured

### Evidence in Code
```csharp
// API owns domain models
namespace WebApplication1.Models
{
    public class Product { /* Domain entity */ }
}

// API handles data access
public class ProductService
{
    private readonly ProductRepository _productRepository;
    // Business logic implementation
}
```

## DTO Usage at API Boundaries

### Decision
API controllers expose DTOs instead of domain entities to external consumers.

### Implementation
- All API controller actions return DTOs
- Domain-to-DTO mapping performed within API services
- MVC project consumes DTOs via HTTP calls

### Justification
- **Contract Stability**: DTOs provide stable API contracts
- **Data Hiding**: Internal domain structure not exposed externally
- **Versioning**: DTOs can evolve independently of domain models
- **Security**: Sensitive domain properties can be excluded from DTOs

### Evidence in Code
```csharp
// API controllers return DTOs
[HttpGet]
public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()

// Mapping occurs in API services
private ProductDTO MapToDTO(Product product)
{
    return new ProductDTO { /* Mapped properties */ };
}
```

## Functional Parity Preservation Strategy

### Decision
Maintain 100% functional parity during ViewModel refactor through compatibility aliases.

### Implementation
- ViewModels include compatibility properties that mirror old domain model structure
- Helper properties provide backward-compatible access patterns
- No changes required to existing Razor views

### Justification
- **Risk Mitigation**: Minimizes regression potential during architectural changes
- **Smooth Migration**: Allows gradual transition without breaking existing UI
- **Development Efficiency**: Preserves existing view code and logic

### Evidence in Code
```csharp
// Compatibility aliases in ViewModels
public class WeeklyMenuItemViewModel
{
    public DayOfWeek DayOfWeek 
    { 
        get => MenuDate.DayOfWeek;
        set { } // Empty setter for compatibility
    }
    
    public int MaxTickets 
    { 
        get => MaxReservations;
        set => MaxReservations = value;
    }
}
```

## HTTP-Only Communication Pattern

### Decision
MVC project communicates with API exclusively through HTTP calls, with no direct database access.

### Implementation
- All MVC services use HttpClient for API communication
- No Entity Framework or database connections in MVC project
- Session management handled in MVC, data persistence in API

### Justification
- **Clear Boundaries**: Enforces architectural separation
- **Scalability**: API can be deployed independently
- **Security**: Database access controlled through API layer
- **Flexibility**: Multiple front-end clients can consume same API

### Evidence in Code
```csharp
// MVC services use HTTP only
public async Task<List<ProductViewModel>> GetAllProducts()
{
    var response = await _httpClient.GetAsync($"{_baseUrl}/Products");
    return await response.Content.ReadFromJsonAsync<List<ProductViewModel>>();
}
```

## Trade-offs and Compromises

### Compatibility Aliases in ViewModels
**Trade-off**: Increased ViewModel complexity for backward compatibility
**Justification**: Prevents breaking changes in existing views during migration

### ErrorViewModel in MVC Project
**Trade-off**: Single domain model reference remains in MVC project
**Justification**: ASP.NET Core requires ErrorViewModel for system error handling

### Nullable Reference Warnings
**Trade-off**: 80 nullable warnings remain in codebase
**Justification**: Non-blocking warnings that don't affect functionality; would require extensive refactoring to resolve
