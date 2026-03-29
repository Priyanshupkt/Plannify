# Clean Architecture Implementation Guide

## 📋 Overview

This guide explains the complete refactoring of Plannify from a basic layered structure to a production-ready Clean Architecture implementation.

---

## 🎯 What's Been Completed

### ✅ Teacher Management Feature (Complete Example)

The Teacher feature has been fully refactored to demonstrate the clean architecture pattern:

```
Domain Layer (Pure business logic)
    ↓
Application Layer (DTOs, Services, Contracts)
    ↓
Infrastructure Layer (Repositories, EF Core)
    ↓
Presentation Layer (Razor Pages)
```

---

## 📂 New Folder Structure

```
Plannify/
├── Domain/
│   ├── Entities/
│   │   └── Teacher.cs ✨ NEW (clean entity with domain methods)
│   └── ValueObjects/
│
├── Application/
│   ├── Common/
│   │   └── Result.cs ✨ NEW (Result<T> pattern for error handling)
│   ├── Contracts/
│   │   ├── IGenericRepository.cs ✨ NEW (base repo interface)
│   │   ├── ITeacherRepository.cs ✨ NEW (teacher-specific repo)
│   │   ├── ITeacherService.cs ✨ NEW (teacher service interface)
│   │   └── IDepartmentService.cs ✨ NEW
│   ├── DTOs/
│   │   ├── TeacherDtos.cs ✨ NEW (CreateTeacherRequest, TeacherDto, etc.)
│   │   └── DepartmentDtos.cs ✨ NEW
│   ├── Services/
│   │   ├── TeacherService.cs ✨ NEW (business logic implementation)
│   │   └── DepartmentService.cs ✨ NEW
│   └── Mappings/
│       └── TeacherMappingProfile.cs ✨ NEW (AutoMapper config)
│
├── Infrastructure/
│   └── Repositories/
│       ├── GenericRepository.cs ✨ NEW (base CRUD implementation)
│       └── TeacherRepository.cs ✨ NEW (teacher data access)
│
├── Presentation/ (renamed from Pages/ for clarity)
│   └── Pages/Admin/Teachers/
│       └── Index.cshtml.cs ✨ REFACTORED (uses ITeacherService)
│
├── Tests/
│   ├── TeacherServiceTests.cs ✨ NEW (unit test examples)
│   └── TeacherEntityTests.cs ✨ NEW (domain test examples)
│
├── Program.cs ✨ UPDATED (clean DI setup)
├── Plannify.csproj ✨ UPDATED (AutoMapper added)
└── Models/ (old, keep for now - gradually migrate)
```

---

## 🔄 BEFORE vs AFTER Comparison

### ❌ BEFORE (Problems)

**Teachers/Index.cshtml.cs:**
```csharp
public class IndexModel : PageModel
{
    private readonly AppDbContext _dbContext;  // ❌ Direct DB access

    public async Task OnGetAsync()
    {
        // ❌ Raw queries in Page layer
        Teachers = await _dbContext.Teachers.Include(t => t.Department).ToListAsync();
        
        // ❌ Business logic in Page
        foreach (var teacher in Teachers)
        {
            var slots = await _dbContext.TimetableSlots
                .Where(t => t.TeacherId == teacher.Id && t.SemesterId == activeSemester.Id)
                .ToListAsync();
            // Calculate hours manually
        }
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        // ❌ CRUD logic mixed with validation
        var codeExists = await _dbContext.Teachers.AnyAsync(...);
        _dbContext.Teachers.Add(NewTeacher);  // ❌ Directly modifying DB
        await _dbContext.SaveChangesAsync();
    }
}
```

**Issues:**
- Hard to test (can't mock DbContext)
- Business logic scattered across layers
- Direct EF Core coupling
- Code reuse impossible

---

### ✅ AFTER (Clean)

**Application Layer Service:**
```csharp
public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _repository;
    
    public async Task<Result<int>> CreateTeacherAsync(CreateTeacherRequest request)
    {
        // ✅ Validation
        if (await _repository.EmployeeCodeExistsAsync(request.EmployeeCode))
            return Result<int>.Failure("Employee code already exists");

        // ✅ Domain logic
        var result = Teacher.Create(request.FullName, request.EmployeeCode, ...);
        if (!result.IsSuccess)
            return Result<int>.Failure(result.ErrorMessage);

        // ✅ Persist through repository
        await _repository.AddAsync(result.Value);
        await _repository.SaveChangesAsync();

        return Result<int>.Success(result.Value.Id);
    }
}
```

**Domain Entity:**
```csharp
public class Teacher
{
    // ✅ Domain business logic
    public static Result<Teacher> Create(string fullName, string employeeCode, ...)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result<Teacher>.Failure("Full name is required");
        // ... validation
        return Result<Teacher>.Success(new Teacher { ... });
    }

    public bool CanAcceptMoreHours(decimal allocatedHours) 
        => allocatedHours < MaxWeeklyHours;
}
```

**Refactored Page:**
```csharp
public class IndexModel : PageModel
{
    private readonly ITeacherService _teacherService;  // ✅ Service abstraction

    public async Task OnGetAsync()
    {
        // ✅ Simple, readable, testable
        var result = await _teacherService.GetAllAsync();
        Teachers = result.Value?.ToList() ?? new();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        // ✅ Delegates all logic to service
        var result = await _teacherService.CreateTeacherAsync(NewTeacher);
        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        return RedirectToPage();
    }
}
```

---

## 🚀 Key Improvements

| Aspect | Before | After |
|--------|--------|-------|
| **Testing** | Hard - direct DB dependency | Easy - mock services & repositories |
| **Code Reuse** | Scattered logic, copy-paste | Centralized in services |
| **Maintainability** | Files with 200+ lines | Single responsibility files |
| **Error Handling** | Try-catch everywhere | Result<T> pattern |
| **Dependencies** | Hard-coupled | Injected, abstractable |
| **Testability** | ~10% coverage possible | ~90% coverage possible |
| **Validation** | Mixed with DB queries | Centralized in services |

---

## 📝 How to Migrate Other Features

### Step 1: Create Domain Entity (if needed)

**Example: Department**

```csharp
// Domain/Entities/Department.cs
public class Department
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }

    public static Result<Department> Create(string name, string code)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(name))
            return Result<Department>.Failure("Name required");

        return Result<Department>.Success(new Department 
        { 
            Name = name, 
            Code = code 
        });
    }
}
```

### Step 2: Create DTOs

```csharp
// Application/DTOs/DepartmentDtos.cs
public class CreateDepartmentRequest
{
    public string Name { get; set; }
    public string Code { get; set; }
}

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
}
```

### Step 3: Create Repository Interface

```csharp
// Application/Contracts/IDepartmentRepository.cs
public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<Department?> GetByCodeAsync(string code);
    Task<bool> CodeExistsAsync(string code);
}
```

### Step 4: Implement Repository

```csharp
// Infrastructure/Repositories/DepartmentRepository.cs
public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context) { }

    public async Task<Department?> GetByCodeAsync(string code)
        => await _dbSet.FirstOrDefaultAsync(d => d.Code == code);

    public async Task<bool> CodeExistsAsync(string code)
        => await _dbSet.AnyAsync(d => d.Code == code);
}
```

### Step 5: Create Service

```csharp
// Application/Services/DepartmentService.cs
public interface IDepartmentService
{
    Task<Result<int>> CreateAsync(CreateDepartmentRequest request);
    Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync();
}

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    
    public async Task<Result<int>> CreateAsync(CreateDepartmentRequest request)
    {
        if (await _repository.CodeExistsAsync(request.Code))
            return Result<int>.Failure("Code already exists");

        var result = Department.Create(request.Name, request.Code);
        if (!result.IsSuccess) return Result<int>.Failure(result.ErrorMessage);

        await _repository.AddAsync(result.Value);
        await _repository.SaveChangesAsync();

        return Result<int>.Success(result.Value.Id);
    }

    public async Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync()
    {
        var departments = await _repository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<DepartmentDto>>(departments);
        return Result<IEnumerable<DepartmentDto>>.Success(dtos);
    }
}
```

### Step 6: Register in DI (Program.cs)

```csharp
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
```

### Step 7: Use in Page Model

```csharp
public class DepartmentsModel : PageModel
{
    private readonly IDepartmentService _service;

    public DepartmentsModel(IDepartmentService service) => _service = service;

    public async Task OnGetAsync()
    {
        var result = await _service.GetAllAsync();
        Departments = result.Value?.ToList() ?? new();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var result = await _service.CreateAsync(NewDepartment);
        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        return RedirectToPage();
    }
}
```

---

## 🧪 Testing Pattern

### Service Tests

```csharp
[Fact]
public async Task CreateAsync_WithValidData_ReturnsSuccess()
{
    // Arrange
    var mockRepo = new Mock<IDepartmentRepository>();
    var service = new DepartmentService(mockRepo.Object);
    var request = new CreateDepartmentRequest { Name = "CSE", Code = "CSE" };

    mockRepo.Setup(r => r.CodeExistsAsync("CSE"))
        .ReturnsAsync(false);

    // Act
    var result = await service.CreateAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    mockRepo.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Once);
}
```

### Domain Entity Tests

```csharp
[Fact]
public void Create_WithValidData_ReturnsSuccess()
{
    // Act
    var result = Department.Create("Computer Science", "CSE");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Computer Science", result.Value!.Name);
}
```

---

## 🔁 Migration Roadmap

### Phase 1: ✅ COMPLETE
- [x] Teacher Management (all layers)
- [x] Result pattern base classes
- [x] Generic & Teacher repositories
- [x] DI setup updated
- [x] Unit test examples

### Phase 2: IN PROGRESS (Next)
- [ ] Department Management (same pattern as Teacher)
- [ ] Subject Management
- [ ] Room Management
- [ ] Academic Year Management

### Phase 3: TODO
- [ ] Timetable feature (leverage existing logic)
- [ ] Conflict Detection service
- [ ] Scheduling service refactor
- [ ] Report services

### Phase 4: TODO (Optional)
- [ ] Split into separate projects:
  - Plannify.Domain
  - Plannify.Application
  - Plannify.Infrastructure
  - Plannify.Web
- [ ] Add comprehensive integration tests
- [ ] API layer (if needed)

---

## 🚨 Important Notes

### ⚠️ Gradual Migration
- **Don't rush**: Migrate one feature at a time
- Keep old `Models/` folder during transition
- Test each layer independently

### 📦 NuGet Packages Added
```xml
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
```

### 🧬 Key Patterns Used
1. **Result<T>**: Eliminates exception handling for business errors
2. **Repository Pattern**: Abstracts data access
3. **Dependency Injection**: Enables testing & loose coupling
4. **DTOs**: Prevents entity exposure between layers
5. **Domain Methods**: Business logic in entities

### 🎓 Learning Resources
- Clean Architecture: Robert C. Martin
- Domain-Driven Design: Eric Evans
- Dependency Injection Principles, Practices, and Patterns: Steven van Deursen

---

## 💡 Quick Checklist for New Features

For each new feature, use this checklist:

- [ ] Create domain entity (if applicable)
- [ ] Create DTOs (Request & Response)
- [ ] Create repository interface (extends IGenericRepository)
- [ ] Implement repository
- [ ] Create service interface
- [ ] Implement service (with Result<T> pattern)
- [ ] Create AutoMapper profile
- [ ] Register in DI (Program.cs)
- [ ] Refactor page model to use service
- [ ] Write unit tests
- [ ] Test end-to-end

---

## 📞 Questions?

Refer to the Teacher feature implementation as a template - all files are documented with comments explaining each layer's responsibilities.

**Key files to study in order:**
1. `Domain/Entities/Teacher.cs` - Domain logic
2. `Application/DTOs/TeacherDtos.cs` - Data transfer objects
3. `Application/Contracts/ITeacherRepository.cs` - Abstractions
4. `Infrastructure/Repositories/TeacherRepository.cs` - Data access
5. `Application/Services/TeacherService.cs` - Business orchestration
6. `Pages/Admin/Teachers/Index.cshtml.cs` - Presentation
7. `Program.cs` - Dependency injection
8. `Tests/TeacherServiceTests.cs` - Testing patterns
