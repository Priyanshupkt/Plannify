# Clean Architecture Quick Reference

## 🎯 Layer Responsibilities

```
┌─────────────────────────────────────────────────────────┐
│ PRESENTATION (Pages/)                                   │
│ • Razor Pages                                           │
│ • Accept DTOs from services                             │
│ • NO business logic, NO data access                     │
│                      ↓                                   │
├─────────────────────────────────────────────────────────┤
│ APPLICATION (Application/)                              │
│ • Business logic orchestration                          │
│ • DTOs & mapping                                        │
│ • Service interfaces & implementations                  │
│ • Uses repositories for data access                     │
│                      ↓                                   │
├─────────────────────────────────────────────────────────┤
│ DOMAIN (Domain/)                                        │
│ • Pure entities with domain methods                     │
│ • NO framework dependencies                             │
│ • Validation rules                                      │
│ • Core business logic                                   │
│                      ↓                                   │
├─────────────────────────────────────────────────────────┤
│ INFRASTRUCTURE (Infrastructure/)                        │
│ • EF Core DbContext                                     │
│ • Repository implementations                           │
│ • External services (email, PDF, etc.)                 │
│                      ↓                                   │
└─────────────────────────────────────────────────────────┘
            Only this layer touches the database
```

---

## 📝 Key Patterns

### 1. Result<T> Pattern

```csharp
// ✅ Instead of throwing exceptions for business errors
var result = await service.CreateAsync(request);

if (!result.IsSuccess)
{
    // Handle error (not an exception)
    return Page(result.ErrorMessage);
}

// Use successful value
var newId = result.Value;
```

### 2. Repository Pattern

```csharp
// ✅ Page doesn't know about DbContext
public class IndexModel : PageModel
{
    private readonly ITeacherRepository _repository;
    
    public async Task OnGetAsync()
    {
        var teachers = await _repository.GetAllAsync();
    }
}
```

### 3. Dependency Injection

```csharp
// ✅ In Program.cs
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();

// ✅ In Page/Service - receive dependencies
public class IndexModel
{
    private readonly ITeacherService _service; // ← Injected
    public IndexModel(ITeacherService service) => _service = service;
}
```

### 4. DTOs (Data Transfer Objects)

```csharp
// ✅ Transfer objects between layers (NOT entities)

// Request DTO
public class CreateTeacherRequest
{
    public string FullName { get; set; }
    public string Email { get; set; }
}

// Response DTO (UI only gets what it needs)
public class TeacherDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public decimal CurrentWeeklyHours { get; set; }
}
```

---

## 🗂️ File Organization

### When creating a new Feature: DO THIS

```
1. Create domain entity
   └─ Domain/Entities/YourEntity.cs

2. Create DTOs
   └─ Application/DTOs/YourEntityDtos.cs

3. Create repository interface
   └─ Application/Contracts/IYourRepository.cs

4. Implement repository
   └─ Infrastructure/Repositories/YourRepository.cs

5. Create service interface
   └─ Application/Contracts/IYourService.cs

6. Implement service
   └─ Application/Services/YourService.cs

7. Create AutoMapper profile
   └─ Application/Mappings/YourMappingProfile.cs

8. Refactor page model
   └─ Presentation/Pages/Admin/Your/Index.cshtml.cs

9. Register in DI
   └─ Update Program.cs

10. Write tests
    └─ Tests/YourServiceTests.cs
```

---

## 💻 Code Templates

### Domain Entity Template

```csharp
namespace Plannify.Domain.Entities;

public class MyEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // Domain business logic
    public static Result<MyEntity> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<MyEntity>.Failure("Name is required");
        
        return Result<MyEntity>.Success(new MyEntity { Name = name });
    }

    public bool CanPerformAction() => /* business rule */;
}
```

### Service Template

```csharp
namespace Plannify.Application.Services;

public interface IMyService
{
    Task<Result<MyDto>> GetByIdAsync(int id);
    Task<Result<int>> CreateAsync(CreateMyRequest request);
    Task<Result> UpdateAsync(UpdateMyRequest request);
    Task<Result> DeleteAsync(int id);
}

public class MyService : IMyService
{
    private readonly IMyRepository _repository;
    private readonly IMapper _mapper;

    public MyService(IMyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<int>> CreateAsync(CreateMyRequest request)
    {
        // 1. Validate
        if (await _repository.ExistsAsync(request.UniqueField))
            return Result<int>.Failure("Already exists");

        // 2. Create using domain method
        var result = MyEntity.Create(request.Name);
        if (!result.IsSuccess)
            return Result<int>.Failure(result.ErrorMessage);

        // 3. Persist
        await _repository.AddAsync(result.Value);
        await _repository.SaveChangesAsync();

        return Result<int>.Success(result.Value.Id);
    }
}
```

### Repository Template

```csharp
namespace Plannify.Infrastructure.Repositories;

public interface IMyRepository : IGenericRepository<MyEntity>
{
    Task<MyEntity?> GetByCodeAsync(string code);
}

public class MyRepository : GenericRepository<MyEntity>, IMyRepository
{
    public MyRepository(AppDbContext context) : base(context) { }

    public async Task<MyEntity?> GetByCodeAsync(string code)
        => await _dbSet.FirstOrDefaultAsync(x => x.Code == code);
}
```

### DTO Template

```csharp
namespace Plannify.Application.DTOs;

public class CreateMyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class MyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
```

### Page Model Template

```csharp
namespace Plannify.Pages.Admin.MyFeature;

public class IndexModel : PageModel
{
    private readonly IMyService _service;

    public List<MyDto> Items { get; set; } = new();

    [BindProperty]
    public CreateMyRequest NewItem { get; set; } = new();

    public IndexModel(IMyService service) => _service = service;

    public async Task OnGetAsync()
    {
        var result = await _service.GetAllAsync();
        Items = result.Value?.ToList() ?? new();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _service.CreateAsync(NewItem);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return Page();
        }

        TempData["Success"] = "Item created successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        
        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = "Item deleted successfully.";

        return RedirectToPage();
    }
}
```

### Program.cs Template (DI Registration)

```csharp
// ✅ Add these lines to Program.cs
builder.Services.AddScoped<IMyRepository, MyRepository>();
builder.Services.AddScoped<IMyService, MyService>();

// Don't forget to add AutoMapper profiles
builder.Services.AddAutoMapper(typeof(MyMappingProfile));
```

### Test Template

```csharp
namespace Plannify.Tests.Application.Services;

public class MyServiceTests
{
    private readonly Mock<IMyRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly MyService _service;

    public MyServiceTests()
    {
        _mockRepository = new Mock<IMyRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new MyService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateMyRequest { Name = "Test" };
        _mockRepository.Setup(r => r.ExistsAsync("test"))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<MyEntity>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsFailure()
    {
        // Arrange
        var request = new CreateMyRequest { Name = "Test" };
        _mockRepository.Setup(r => r.ExistsAsync("test"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
```

---

## ✅ Refactoring Checklist

For each feature, complete this checklist:

### Code Review Checklist
- [ ] Page model has NO `DbContext` injection
- [ ] Page model has NO direct EF queries
- [ ] Page model uses only DTOs, not entities
- [ ] Service is injected via DI
- [ ] Services use `Result<T>` pattern
- [ ] Domain entity has domain methods (not just getters)
- [ ] Repository has interface abstraction
- [ ] Repository doesn't leak IQueryable
- [ ] All CRUD in service, not page
- [ ] Unit tests written for service

### Before Committing
- [ ] Code compiles without warnings
- [ ] Page model is <100 lines
- [ ] Service is focused on one responsibility
- [ ] Repository only handles data access
- [ ] DTOs defined for I/O
- [ ] Tests pass
- [ ] No business logic in Pages
- [ ] All dependencies injected

---

## 🚫 Anti-Patterns to Avoid

```csharp
// ❌ BAD: DbContext in Page
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;  // ❌ NO!
}

// ❌ BAD: Leaking entities to UI
public List<Teacher> Teachers { get; set; }  // ❌ Use TeacherDto

// ❌ BAD: Business logic in Page
foreach (var teacher in Teachers)  // ❌ This belongs in Service
{
    // Calculate workload
}

// ❌ BAD: Throwing exceptions for business rules
throw new Exception("Code already exists");  // ❌ Use Result<T>

// ❌ BAD: Service without interface
public TeacherService { }  // ❌ Create ITeacherService interface

// ❌ BAD: Repository with complex logic
public class TeacherRepository
{
    public void CalculateWorkloadAndUpdate() { }  // ❌ Belongs in Service
}
```

---

## 🔗 File Reference

| File | Purpose |
|------|---------|
| `Application/Common/Result.cs` | Result<T> pattern |
| `Application/Contracts/IGenericRepository.cs` | Base repository interface |
| `Infrastructure/Repositories/GenericRepository.cs` | Base repository implementation |
| `Domain/Entities/Teacher.cs` | Example domain entity |
| `Application/Services/TeacherService.cs` | Example service |
| `Pages/Admin/Teachers/Index.cshtml.cs` | Example refactored page |
| `IMPLEMENTATION_GUIDE.md` | Detailed migration guide |
| `Plannify.csproj` | Check for AutoMapper package |
| `Program.cs` | DI configuration |

---

## 📞 Getting Help

1. **Study the Teacher feature** - It's fully documented
2. **Follow the templates above** - Copy-paste and adapt
3. **Refer to IMPLEMENTATION_GUIDE.md** - Step-by-step for new features
4. **Check existing tests** - TeacherServiceTests.cs shows patterns

---

**Key Principle: One layer never knows about implementation details of lower layers. All dependencies flow downward.**
