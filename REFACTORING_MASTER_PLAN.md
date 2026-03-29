# PLANNIFY: COMPLETE REFACTORING & CLEANUP MASTER PLAN

**Document Date:** March 29, 2026  
**Status:** Ready for Implementation  
**Estimated Timeline:** 4-6 weeks  
**Priority:** CRITICAL - Blocks scalability

---

## EXECUTIVE SUMMARY

The Plannify project has **half-implemented clean architecture**. The domain layer, services, and repositories are well-designed but **largely unused**. Instead, **18 out of 22 Pages bypass everything and directly access the database**.

This plan provides **step-by-step instructions** to complete the migration and achieve **85%+ architecture compliance**.

---

## PHASE 1: PREPARATION (Days 1-2)

### 1.1 Create Backup Branch
```bash
git checkout -b refactor/clean-architecture
git push --set-upstream origin refactor/clean-architecture
```

### 1.2 Document Current State
- Run audit: `find Plannify -name "*.cs" | wc -l` → 119 files
- Document all Pages currently using AppDbContext (already identified: 18 pages)
- Create checklist for tracking progress

### 1.3 Create Refactoring Template
Create file: `REFACTORING_TEMPLATE.md`

```markdown
# Page Refactoring Template

## Before Pattern
```csharp
public class [PageName]Model(AppDbContext dbContext) : PageModel
{
    private readonly AppDbContext _dbContext = dbContext;
    
    public async Task OnGetAsync()
    {
        Items = await _dbContext.[Table].ToListAsync();
    }
}
```

## After Pattern  
```csharp
public class [PageName]Model(I[Entity]Service service) : PageModel
{
    private readonly I[Entity]Service _service = service;
    
    public async Task OnGetAsync()
    {
        var result = await _service.GetAllAsync();
        if (result.IsSuccess)
            Items = result.Value?.ToList();
    }
}
```

## Common Replacements
| Old Pattern | New Pattern |
|---|---|
| `await _dbContext.Teachers.ToListAsync()` | `await _teacherService.GetAllAsync()` |
| `_dbContext.Teachers.Add(item)` | `await _teacherService.CreateAsync(request)` |
| `_dbContext.Teachers.Remove(item)` | `await _teacherService.DeleteAsync(id)` |
```

---

## PHASE 2: CREATE SERVICE INTERFACES (Days 3-4)

### 2.1 Create Missing Infrastructure Service Interfaces

**File:** `Application/Contracts/IAuditService.cs`
```csharp
using Plannify.Models;

namespace Plannify.Application.Contracts;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string entityId, 
        string? oldValues = null, string? newValues = null);
}
```

**File:** `Application/Contracts/IPdfExportService.cs`
```csharp
namespace Plannify.Application.Contracts;

public interface IPdfExportService
{
    byte[] GenerateClassTimetablePdf(int semesterId, int classBatchId);
    byte[] GenerateTeacherTimetablePdf(int semesterId, int teacherId);
}
```

**File:** `Application/Contracts/ITimetableReportService.cs`
```csharp
namespace Plannify.Application.Contracts;

public interface ITimetableReportService
{
    Task<byte[]> ExportTimetableAsync(int timetableId, string format);
}
```

### 2.2 Update Program.cs
```csharp
// Before
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<PdfExportService>();
builder.Services.AddScoped<SchedulingService>();

// After
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<ITimetableReportService, TimetableExportService>();
```

### 2.3 Update Service Implementations
```csharp
// Services/AuditService.cs
public class AuditService : IAuditService  // Add interface
{
    // ... existing code
}

// Services/PdfExportService.cs
public class PdfExportService : IPdfExportService  // Add interface
{
    // ... existing code
}
```

---

## PHASE 3: FIX SERVICE LAYER (Days 5-7)

### 3.1 Fix ConflictDetector

**File:** `Services/ConflictDetector.cs` (BEFORE)
```csharp
public class ConflictDetector : IConflictDetectorService
{
    private readonly AppDbContext _context;  // ❌ Direct context
    
    public async Task<ConflictResult> CheckTeacherConflictAsync(...)
    {
        var conflictingSlot = await _context.TimetableSlots
            .AsNoTracking()
            .Where(s => s.TeacherId == teacherId && ...)
            .FirstOrDefaultAsync();
    }
}
```

**File:** `Services/ConflictDetector.cs` (AFTER)
```csharp
public class ConflictDetector : IConflictDetectorService
{
    private readonly ITimetableSlotRepository _slotRepository;  // ✅ Repository
    
    public ConflictDetector(ITimetableSlotRepository slotRepository)
    {
        _slotRepository = slotRepository;
    }
    
    public async Task<ConflictResult> CheckTeacherConflictAsync(
        int teacherId, string day, TimeOnly startTime, TimeOnly endTime, int semesterId)
    {
        // Use repository method instead of direct query
        var conflicts = await _slotRepository.GetConflictingAsync(
            teacherId, day, startTime, endTime, semesterId);
            
        if (conflicts.Any())
            return ConflictResult.Conflict(conflicts);
            
        return ConflictResult.NoConflict();
    }
}
```

### 3.2 Update SchedulingService

**File:** `Services/SchedulingService.cs` (BEFORE)
```csharp
public class SchedulingService : ISchedulingService
{
    private readonly AppDbContext _dbContext;  // ❌ Direct context
    
    public async Task<SchedulingResult> GenerateTimetableAsync(SchedulingRequest request)
    {
        var semester = await _dbContext.Semesters
            .Include(s => s.AcademicYear)
            .FirstOrDefaultAsync(...);
    }
}
```

**File:** `Services/SchedulingService.cs` (AFTER)
```csharp
public class SchedulingService : ISchedulingService
{
    private readonly ISemesterRepository _semesterRepository;  // ✅ Repository
    private readonly IClassBatchRepository _classRepository;
    private readonly ITimetableSlotRepository _slotRepository;
    private readonly IConflictDetectorService _conflictDetector;
    
    public SchedulingService(
        ISemesterRepository semesterRepository,
        IClassBatchRepository classRepository,
        ITimetableSlotRepository slotRepository,
        IConflictDetectorService conflictDetector)
    {
        _semesterRepository = semesterRepository;
        _classRepository = classRepository;
        _slotRepository = slotRepository;
        _conflictDetector = conflictDetector;
    }
    
    public async Task<SchedulingResult> GenerateTimetableAsync(SchedulingRequest request)
    {
        var semester = await _semesterRepository.GetByIdAsync(request.SemesterId);
        if (semester == null)
            return SchedulingResult.Failure("Semester not found");
        
        var classes = await _classRepository.GetBySemesterAsync(request.SemesterId);
        // ... continue with repositories
    }
}
```

### 3.3 Add Missing Repository Methods

If not already present, add to `ITimetableSlotRepository`:
```csharp
public interface ITimetableSlotRepository
{
    // Existing methods...
    
    // New conflict detection methods
    Task<List<TimetableSlot>> GetConflictingAsync(
        int teacherId, string day, TimeOnly startTime, TimeOnly endTime, int semesterId);
    
    Task<List<TimetableSlot>> GetByTeacherAndSemesterAsync(int teacherId, int semesterId);
}
```

---

## PHASE 4: REFACTOR PAGES (Days 8-25) - THE LARGEST PHASE

This is the critical phase. Must convert 18 Pages to use services.

### 4.1 Priority Order for Page Refactoring

**HIGH PRIORITY (Master Data Pages):**
1. Pages/Admin/Teachers.cshtml.cs → Use ITeacherService
2. Pages/Admin/Subjects.cshtml.cs → Use ISubjectService
3. Pages/Admin/Departments.cshtml.cs → Use IDepartmentService
4. Pages/Admin/Rooms.cshtml.cs → Use IRoomService
5. Pages/Admin/AcademicYears/Index.cshtml.cs → Use IAcademicYearService

**MEDIUM PRIORITY (Timetable Pages):**
6. Pages/Admin/Timetable.cshtml.cs → Use ITimetableSlotService
7. Pages/Admin/Timetable/Create.cshtml.cs → Already partially done
8. Pages/Admin/Timetable/ByClass.cshtml.cs
9. Pages/Admin/Timetable/ByTeacher.cshtml.cs
10. Pages/Admin/Timetable/Conflicts.cshtml.cs

**OTHER:**
11-18. [Remaining 8 pages]

### 4.2 Detailed Refactoring Example: Teachers Page

**File:** `Pages/Admin/Teachers/Index.cshtml.cs` (BEFORE)
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Admin.Teachers;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _dbContext;
    private readonly AuditService _auditService;

    public IndexModel(AppDbContext dbContext, AuditService auditService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
    }

    [BindProperty]
    public Teacher NewTeacher { get; set; } = new();

    public List<SelectListItem> Departments { get; set; } = new();
    public List<Teacher> Teachers { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Load departments
        Departments = await _dbContext.Departments
            .OrderBy(d => d.Name)
            .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
            .ToListAsync();

        // Load teachers
        Teachers = await _dbContext.Teachers
            .Include(t => t.Department)
            .OrderBy(t => t.FullName)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        _dbContext.Teachers.Add(NewTeacher);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync(
            "CREATE",
            "Teacher",
            NewTeacher.Id.ToString(),
            null,
            $"Added teacher: {NewTeacher.FullName}");

        TempData["Success"] = "Teacher added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var teacher = await _dbContext.Teachers.FindAsync(id);
        if (teacher != null)
        {
            _dbContext.Teachers.Remove(teacher);
            await _dbContext.SaveChangesAsync();

            await _auditService.LogAsync(
                "DELETE",
                "Teacher",
                id.ToString());

            TempData["Success"] = "Teacher deleted.";
        }

        return RedirectToPage();
    }
}
```

**File:** `Pages/Admin/Teachers/Index.cshtml.cs` (AFTER)
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin.Teachers;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ITeacherService _teacherService;  // ✅ Service instead of context
    private readonly IDepartmentService _departmentService;

    public IndexModel(ITeacherService teacherService, IDepartmentService departmentService)
    {
        _teacherService = teacherService;
        _departmentService = departmentService;
    }

    [BindProperty]
    public CreateTeacherRequest NewTeacher { get; set; } = new();  // ✅ Use DTO

    public List<SelectListItem> Departments { get; set; } = new();
    public List<TeacherDto> Teachers { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Load departments
        var deptResult = await _departmentService.GetAllAsync();
        if (deptResult.IsSuccess)
        {
            Departments = deptResult.Value?
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
                .ToList() ?? new();
        }

        // Load teachers
        var teachersResult = await _teacherService.GetAllAsync();
        if (teachersResult.IsSuccess)
        {
            Teachers = teachersResult.Value?.ToList() ?? new();
        }
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _teacherService.CreateAsync(NewTeacher);  // ✅ Use service
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to add teacher");
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = "Teacher added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _teacherService.DeleteAsync(id);  // ✅ Use service
        if (result.IsSuccess)
        {
            TempData["Success"] = "Teacher deleted.";
        }
        else
        {
            TempData["Error"] = result.ErrorMessage ?? "Failed to delete teacher";
        }

        return RedirectToPage();
    }
}
```

**Key Changes:**
1. ✅ Removed `AppDbContext` injection
2. ✅ Added `ITeacherService` and `IDepartmentService` injection
3. ✅ Replaced direct DB queries with service calls
4. ✅ Replaced `Model.Teacher` with `TeacherDto`
5. ✅ Replaced create request with `CreateTeacherRequest` DTO
6. ✅ Used `Result<T>` pattern for error handling
7. ✅ No more direct `_dbContext.SaveChangesAsync()`

### 4.3 Refactoring Checklist for Each Page

Use this checklist for each page:

```markdown
## Page Refactor Checklist: [PageName]

- [ ] Remove `AppDbContext` parameter from constructor
- [ ] Add corresponding service parameters (IXxxService)
- [ ] Replace `_dbContext.Table.ToListAsync()` with `_service.GetAllAsync()`
- [ ] Replace `_dbContext.Table.FindAsync(id)` with `_service.GetByIdAsync(id)`
- [ ] Replace `_dbContext.Table.Add()` with `_service.CreateAsync()`
- [ ] Replace `_dbContext.Table.Remove()` with `_service.DeleteAsync()`
- [ ] Replace Model classes with corresponding DTOs
- [ ] Add Result<T> error handling
- [ ] Update ModelState errors to use Result?.ErrorMessage
- [ ] Test the page works correctly
- [ ] Verify audit logging still works
- [ ] Run build: `dotnet build` (0 errors expected)
```

---

## PHASE 5: DELETE MODELS FOLDER (Days 26-27)

### 5.1 Before Deletion Preparation

When all Pages have been refactored to use services:

1. Verify no `.cs` file still references `Plannify.Models` (except ApplicationUser, AuditLog)
   ```bash
   grep -r "using Plannify.Models" Plannify --include="*.cs" | grep -v ApplicationUser | grep -v AuditLog
   # Should return 0 results
   ```

2. Create backup of Models folder
   ```bash
   cp -r Plannify/Models Plannify/Models.backup
   git add Plannify/Models.backup
   ```

### 5.2 Update AppDbContext

Change DbSet references from Models to Domain.Entities:

**File:** `Data/AppDbContext.cs` (BEFORE)
```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Plannify.Models.Department> Departments { get; set; }  // ❌ Models
    public DbSet<Plannify.Models.Teacher> Teachers { get; set; }  // ❌ Models
    public DbSet<Plannify.Models.Subject> Subjects { get; set; }  // ❌ Models
    // ... more Models references
}
```

**File:** `Data/AppDbContext.cs` (AFTER)
```csharp
using Plannify.Domain.Entities;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Department> Departments { get; set; }  // ✅ Domain
    public DbSet<Teacher> Teachers { get; set; }  // ✅ Domain
    public DbSet<Subject> Subjects { get; set; }  // ✅ Domain
    // ... rest Domain entity references
}
```

### 5.3 Create ViewModels (if needed)

For Pages that need UI-specific models, create `Pages/Shared/ViewModels/`:

```csharp
// Pages/Shared/ViewModels/TeacherViewModel.cs
public class TeacherViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string DepartmentName { get; set; }  // Display name, not FK
    public string Email { get; set; }
    public int HoursAllocated { get; set; }
}
```

### 5.4 Delete Models Folder

```bash
# Only after all references are updated
rm -rf Plannify/Models
git add .
git commit -m "refactor: remove duplicate Models folder, use Domain.Entities exclusively"
```

---

## PHASE 6: DATABASE SCHEMA IMPROVEMENTS (Days 28-35)

### 6.1 Create EF Core Migration

**File:** `Data/Migrations/[Date]_ImproveSchemaConstraints.cs`

```csharp
public partial class ImproveSchemaConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add unique constraint on AcademicYear.YearLabel
        migrationBuilder.CreateIndex(
            name: "IX_AcademicYears_YearLabel_Unique",
            table: "AcademicYears",
            column: "YearLabel",
            unique: true);

        // Add unique constraint on Department.Code
        migrationBuilder.CreateIndex(
            name: "IX_Departments_Code_Unique",
            table: "Departments",
            column: "Code",
            unique: true);

        // Fix ClassBatch.Semester to be FK instead of just int
        // This is complex - may need to:
        // 1. Create Semester table properly (if not normalized)
        // 2. Add SemesterId FK to ClassBatch
        // 3. Populate existing data
        
        // Ensure room assignment constraints
        // ClassBatch can have default room, TimetableSlot can override
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reversal logic
    }
}
```

### 6.2 Update Database Fluent Configuration

**File:** `Data/AppDbContext.cs` (OnModelCreating)

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // Department uniqueness
    builder.Entity<Department>()
        .HasIndex(d => d.Code)
        .IsUnique();

    // Academic Year uniqueness
    builder.Entity<AcademicYear>()
        .HasIndex(a => a.YearLabel)
        .IsUnique();

    // Teacher constraints
    builder.Entity<Teacher>()
        .HasIndex(t => t.EmployeeCode)
        .IsUnique();

    // Subject constraints
    builder.Entity<Subject>()
        .HasIndex(s => s.Code)
        .IsUnique();

    // TimetableSlot relationships
    builder.Entity<TimetableSlot>()
        .HasOne(t => t.Teacher)
        .WithMany(te => te.TimetableSlots)
        .HasForeignKey(t => t.TeacherId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<TimetableSlot>()
        .HasOne(t => t.Subject)
        .WithMany(s => s.TimetableSlots)
        .HasForeignKey(t => t.SubjectId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<TimetableSlot>()
        .HasOne(t => t.ClassBatch)
        .WithMany(c => c.TimetableSlots)
        .HasForeignKey(t => t.ClassBatchId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<TimetableSlot>()
        .HasOne(t => t.Room)
        .WithMany(r => r.TimetableSlots)
        .HasForeignKey(t => t.RoomId)
        .OnDelete(DeleteBehavior.SetNull);
}
```

### 6.3 Apply Migration

```bash
cd Plannify
dotnet ef migrations add ImproveSchemaConstraints
dotnet ef database update
dotnet build  # Should compile without errors
```

---

## PHASE 7: TESTING & VALIDATION (Days 36-40)

### 7.1 Unit Tests for Services

Create test for one refactored service:

**File:** `Tests/TeacherServiceTests.cs`

```csharp
using Plannify.Application.Contracts;
using Plannify.Application.Services;
using Plannify.Infrastructure.Repositories;
using Xunit;
using Moq;

namespace Plannify.Tests;

public class TeacherServiceTests
{
    private readonly Mock<ITeacherRepository> _mockRepository;
    private readonly Mock<IAuditService> _mockAudit;
    private readonly ITeacherService _service;

    public TeacherServiceTests()
    {
        _mockRepository = new Mock<ITeacherRepository>();
        _mockAudit = new Mock<IAuditService>();
        _service = new TeacherService(_mockRepository.Object, _mockAudit.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsTeacher()
    {
        // Arrange
        var teacherId = 1;
        var teacher = new Teacher { Id = teacherId, FullName = "John Doe" };
        _mockRepository
            .Setup(r => r.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);

        // Act
        var result = await _service.GetByIdAsync(teacherId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("John Doe", result.Value.FullName);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesTeacher()
    {
        // Arrange
        var request = new CreateTeacherRequest
        {
            FullName = "Jane Smith",
            EmployeeCode = "EMP001",
            Email = "jane@school.edu"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Teacher>()), Times.Once);
    }
}
```

### 7.2 Integration Tests

```csharp
[Fact]
public async Task TeacherPage_CanLoadAndDisplay_AllTeachers()
{
    // Arrange: Setup test database with seed data
    // Act: Call page OnGetAsync()
    // Assert: Teachers list populated
}

[Fact]
public async Task TeacherPage_CanCreateTeacher_ViaService()
{
    // Arrange: Create teacher request
    // Act: Call OnPostAddAsync()
    // Assert: Teacher saved, audit logged, redirect triggered
}
```

### 7.3 Build Verification

```bash
# Full build check
dotnet clean
dotnet build
# Expected: 0 errors, warnings (if any) should be pre-existing

# Run tests
dotnet test
# Expected: All tests pass

# Check architecture
# 1. Verify no page references AppDbContext
grep -r "AppDbContext" Plannify/Pages --include="*.cs" | wc -l
# Expected: 0 (or only in accepted locations)

# 2. Verify all services have interfaces
grep -r "class.*Service" Plannify/Application/Services --include="*.cs" | head -10
# Check each has corresponding interface in Application/Contracts

# 3. Verify Models folder deleted (or only contains legacy models)
ls -la Plannify/Models 2>/dev/null || echo "Models folder successfully deleted"
```

---

## PHASE 8: CLEANUP & DOCUMENTATION (Days 41-42)

### 8.1 Remove Temporary Files

```bash
rm -f REFACTORING_TEMPLATE.md
rm -rf Plannify/Models.backup
git add .
```

### 8.2 Update Documentation

Create file: `ARCHITECTURE_COMPLETE.md`

```markdown
# Architecture Migration Complete ✅

**Date:** [Completion Date]
**Compliance Level:** 85%+

## What Changed

1. All Pages now use service layer
2. Models folder consolidated into Domain.Entities
3. All services have interfaces
4. Database schema improved with constraints
5. Removed direct AppDbContext access from Pages

## Key Files

- Domain entities: `Domain/Entities/*.cs`
- Application services: `Application/Services/*.cs`
- Razor Pages: `Pages/Admin/*.cshtml.cs`
- Repositories: `Infrastructure/Repositories/*.cs`

## Architecture Scoring

| Layer | Before | After |
|-------|--------|-------|
| Domain | 90% | 90% |
| Repository | 85% | 90% |
| Service | 50% | 95% |
| Presentation | 20% | 95% |
| **Overall** | **58%** | **92%** |
```

### 8.3 Final Checklist

```markdown
## Refactoring Completion Checklist

- [ ] All 18 Pages refactored to use services
- [ ] Models folder deleted
- [ ] No Page injects AppDbContext
- [ ] All services have interfaces
- [ ] ConflictDetector uses repositories
- [ ] SchedulingService uses repositories
- [ ] Database migrations applied
- [ ] All tests passing
- [ ] Code compiles: 0 errors
- [ ] Audit trail working
- [ ] Documentation updated
- [ ] Team trained on new architecture
- [ ] Code reviewed and approved
```

---

## FILE STRUCTURE AFTER REFACTORING

```
Plannify/
├── 📁 Domain/                          
│   └── Entities/
│       ├── Teacher.cs                  ✅
│       ├── Department.cs               ✅
│       ├── Subject.cs                  ✅
│       ├── ClassBatch.cs               ✅
│       ├── TimetableSlot.cs            ✅
│       ├── Semester.cs                 ✅
│       ├── AcademicYear.cs             ✅
│       ├── Room.cs                     ✅
│       ├── Substitution.cs             ✅
│       ├── Timetable.cs                ✅
│       └── [Single source of truth]
│
├── 📁 Application/                    
│   ├── Contracts/
│   │   ├── ITeacherService.cs          ✅
│   │   ├── IDepartmentService.cs       ✅
│   │   ├── [10 service interfaces]
│   │   ├── IAuditService.cs            ✅ New
│   │   ├── IPdfExportService.cs        ✅ New
│   │   └── ITimetableReportService.cs  ✅ New
│   ├── Services/
│   │   ├── TeacherService.cs           ✅
│   │   ├── [All services]              ✅
│   │   └── [All use repositories]
│   ├── DTOs/
│   │   ├── TeacherDto.cs               ✅
│   │   └── [All DTOs]                  ✅
│   └── Mappings/
│       ├── TeacherMappingProfile.cs    ✅
│       └── [All profiles]              ✅
│
├── 📁 Infrastructure/
│   ├── Repositories/
│   │   ├── TeacherRepository.cs        ✅
│   │   ├── [All repository implementations]
│   │   └── [All use Domain entities]
│   └── Data/
│       ├── AppDbContext.cs             ✅ Updated to use Domain.Entities
│       └── Migrations/
│           └── [Database constraints]  ✅
│
├── 📁 Pages/                           
│   └── Admin/
│       ├── Teachers.cshtml.cs          ✅ Uses ITeacherService
│       ├── Subjects.cshtml.cs          ✅ Uses ISubjectService
│       ├── [All 22 pages]              ✅ Use services, NOT AppDbContext
│       └── [NO DIRECT DB ACCESS]
│
├── 📁 Services/                        
│   ├── AuditService.cs                 ✅ Implements IAuditService
│   ├── PdfExportService.cs             ✅ Implements IPdfExportService
│   ├── ConflictDetector.cs             ✅ Uses ITimetableSlotRepository
│   └── SchedulingService.cs            ✅ Uses repositories
│
└── 📁 Tests/
    ├── TeacherServiceTests.cs          ✅
    ├── [Unit tests for services]       ✅
    └── [Integration tests for Pages]   ✅
```

---

## BENEFITS AFTER REFACTORING

### ✅ Clean Architecture
- Single entity source of truth (Domain.Entities)
- Proper layer separation
- No business logic in UI

### ✅ Testability
- Services mockable via interfaces
- Pages unit-testable without database
- Repositories independent

### ✅ Maintainability
- Changes to business logic in one place
- DTOs handle data transfer
- Audit trail automatic

### ✅ Scalability
- New features added via service layer
- Repositories abstract data access
- Easy to add: multi-database support, caching, logging

### ✅ Enforcement
- Compiler enforces layer violations
- No way to bypass audit logging
- DTOs ensure type safety

---

## ESTIMATED TIME BREAKDOWN

| Phase | Tasks | Days | Notes |
|-------|-------|------|-------|
| **1** | Preparation | 2 | Create branches, templates |
| **2** | Service Interfaces | 2 | Create IAuditService, etc |
| **3** | Fix Services | 3 | ConflictDetector, SchedulingService |
| **4** | Refactor Pages | 18 | 18 pages × 1 day ≈ 18 days |
| **5** | Delete Models | 2 | Cleanup & verification |
| **6** | Database | 8 | Migrations, constraints |
| **7** | Testing | 5 | Unit + integration tests |
| **8** | Cleanup | 2 | Documentation |
| **TOTAL** | **40 tasks** | **42 days** | **~6 weeks** |

---

## RISK MITIGATION

| Risk | Mitigation |
|------|-----------|
| Breaking existing Pages | Create feature branch, test each page independently |
| Database downtime | Run migrations in non-production first |
| Missing entity mappings | Create mapping tests before deletion |
| Page test failures | Write integration tests before refactoring |

---

## SUCCESS CRITERIA

✅ **Technical**
- [ ] 0 compilation errors
- [ ] All tests pass
- [ ] No direct DB access from Pages
- [ ] All services have interfaces
- [ ] Architecture score ≥85%

✅ **Code Quality**
- [ ] Code review approved
- [ ] No duplicate entities
- [ ] DTOs used consistently
- [ ] Audit logging works

✅ **Database**
- [ ] Migrations applied
- [ ] Constraints enforced
- [ ] Data integrity verified
- [ ] Backup exists

---

## NEXT IMMEDIATE STEPS

1. **Today:** Create branch `refactor/clean-architecture`
2. **Tomorrow:** Start Phase 1 (Preparation)
3. **Within 1 week:** Complete Phase 3 (Services)
4. **Within 3 weeks:** Complete Phase 4 (Pages refactoring 50%)
5. **Within 6 weeks:** Achieve 85%+ architecture compliance

---

**Document Version:** 1.0  
**Last Updated:** March 29, 2026  
**Status:** Ready for Implementation  
**Approved By:** [Your name]
