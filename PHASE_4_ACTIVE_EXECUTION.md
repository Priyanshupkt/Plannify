# PHASE 4: ACTIVE PAGE REFACTORING EXECUTION

**Status:** Mid-Phase (3 of 19 pages refactored, 16 remaining)  
**Build Status:** ✅ 0 Errors, 0 Warnings  
**Time Estimate:** 2-3 weeks to complete all 16 pages  

---

## 📋 REMAINING PAGES (16) BY PRIORITY

### TIER 1: CRITICAL PAGES (Use First) - 3 pages
These are frequently used and should be done first to validate pattern

- [ ] **Plannify/Pages/Admin/Dashboard.cshtml.cs** → `ISubstitutionService`, `IConflictDetectorService`
- [ ] **Plannify/Pages/Admin/Timetable/Create.cshtml.cs** → `IConflictDetectorService`, `ITimetableSlotService`
- [ ] **Plannify/Pages/Admin/Teachers.cshtml.cs** → `ITeacherService`

### TIER 2: MASTER DATA PAGES (Easy Pattern) - 5 pages
Simple CRUD, same pattern for all

- [ ] **Plannify/Pages/Admin/Subjects.cshtml.cs** → `ISubjectService`
- [ ] **Plannify/Pages/Admin/Subjects/Index.cshtml.cs** → `ISubjectService`
- [ ] **Plannify/Pages/Admin/Classes/Index.cshtml.cs** → `IClassBatchService`
- [ ] **Plannify/Pages/Admin/AcademicYears/Index.cshtml.cs** → `IAcademicYearService`
- [ ] **Plannify/Pages/Admin/AcademicYears/Semesters.cshtml.cs** → `ISemesterService`

### TIER 3: COMPLEX PAGES (Medium Difficulty) - 5 pages
Complex queries and relationships

- [ ] **Plannify/Pages/Admin/Teachers/Profile.cshtml.cs** → `ITeacherService`, `ITimetableSlotService`
- [ ] **Plannify/Pages/Admin/Timetable.cshtml.cs** → `ITimetableSlotService`, `IClassBatchService`
- [ ] **Plannify/Pages/Admin/Timetable/ByClass.cshtml.cs** → `ITimetableService`, `IClassBatchService`
- [ ] **Plannify/Pages/Admin/Timetable/ByTeacher.cshtml.cs** → `ITimetableService`, `ITeacherService`
- [ ] **Plannify/Pages/Admin/Substitutions/Index.cshtml.cs** → `ISubstitutionService`, `ITeacherService`

### TIER 4: ADVANCED PAGES (Most Complex) - 3 pages
Advanced logic and business constraints

- [ ] **Plannify/Pages/Admin/Timetable/Conflicts.cshtml.cs** → `IConflictDetectorService`, `ITimetableSlotService`
- [ ] **Plannify/Pages/Admin/Timetable/AutoGenerate.cshtml.cs** → `ISchedulingService`, `IConflictDetectorService`
- [ ] **Plannify/Pages/Admin/Timetable/MasterTimetable.cshtml.cs** → `ITimetableService`, `ITimetableSlotService`

---

## 🔄 REFACTORING WORKFLOW (Repeat for Each Page)

### STEP 1: ANALYZE
```bash
# View current page model
code Plannify/Pages/Admin/[PageName].cshtml.cs

# Look for:
# 1. Constructor parameters (remove AppDbContext, add services)
# 2. Properties (Models.* → DTOs)
# 3. OnGet/OnPost methods (queries → service calls)
# 4. OnPost handlers (SaveChanges → service calls)
```

### STEP 2: REFACTOR (Using Template Below)
Follow the before/after pattern exactly

### STEP 3: BUILD & VERIFY
```bash
# Build
dotnet build 2>&1 | grep -E "Build|Error"

# Test in browser
# Goto: https://localhost:5001/Admin/[Page]
# Try: Create, Read, Update, Delete operations
```

### STEP 4: COMMIT
```bash
git add Plannify/Pages/Admin/[PageName].cshtml.cs
git commit -m "refactor(pages): [PageName] - use I[Service] instead of AppDbContext

- Removed AppDbContext injection
- Added I[Service] injection
- Replaced DB queries with service calls
- Replaced Models with DTOs
- Added Result<T> error handling"
```

---

## 💡 UNIVERSAL REFACTORING TEMPLATE

**BEFORE:**
```csharp
public class IndexModel(AppDbContext dbContext, AuditService auditService) : PageModel
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly AuditService _auditService = auditService;

    [BindProperty]
    public Teacher NewTeacher { get; set; } = new();  // ❌ Model

    public List<Teacher> Teachers { get; set; } = new();  // ❌ Model list

    public async Task OnGetAsync()
    {
        // ❌ Direct DB access
        Teachers = await _dbContext.Teachers
            .AsNoTracking()
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

        // ❌ Direct DB manipulation
        _dbContext.Teachers.Add(NewTeacher);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("CREATE", "Teacher", NewTeacher.Id.ToString());

        TempData["Success"] = "Added";
        return RedirectToPage();
    }
}
```

**AFTER:**
```csharp
public class IndexModel(ITeacherService teacherService) : PageModel  // ✅ Service only
{
    private readonly ITeacherService _teacherService = teacherService;

    [BindProperty]
    public CreateTeacherRequest NewTeacher { get; set; } = new();  // ✅ DTO

    public List<TeacherDto> Teachers { get; set; } = new();  // ✅ DTO list

    public async Task OnGetAsync()
    {
        // ✅ Service call
        var result = await _teacherService.GetAllAsync();
        if (result.IsSuccess)
            Teachers = result.Value?.ToList() ?? new();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        // ✅ Service call with Result<T>
        var result = await _teacherService.CreateAsync(NewTeacher);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Error");
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = "Added";
        return RedirectToPage();
    }
}
```

**Key Changes:**
1. ✅ Remove `AppDbContext dbContext` parameter
2. ✅ Remove other service parameters (only if now injected via main service)
3. ✅ Add `IXxxService` parameter
4. ✅ Change Model properties to DTO properties
5. ✅ Replace `await _dbContext.Table.**` with `await _service.Get**Async()`
6. ✅ Add `if (result.HasErrors)` checks
7. ✅ Remove `await _dbContext.SaveChangesAsync()` calls

---

## 🎯 QUICK REFERENCE: Service Method Mapping

| Old Code | New Code |
|----------|----------|
| `await _dbContext.Teachers.ToListAsync()` | `await _teacherService.GetAllAsync()` |
| `await _dbContext.Teachers.FindAsync(id)` | `await _teacherService.GetByIdAsync(id)` |
| `_dbContext.Teachers.Add(item)` | `await _teacherService.CreateAsync(request)` |
| `_dbContext.Teachers.Update(item)` | `await _teacherService.UpdateAsync(id, request)` |
| `_dbContext.Teachers.Remove(item)` | `await _teacherService.DeleteAsync(id)` |
| `_dbContext.Teachers.AsNoTracking().Where(...).ToListAsync()` | `(await _service.GetAllAsync())?.Where(...)` |
| `_dbContext.SaveChangesAsync()` | *(done automatically by service)* |

---

## 📊 PROGRESS TRACKING

Use this checklist daily:

```markdown
## Refactoring Progress Checklist

### TIER 1: CRITICAL (3 Pages)
- [ ] Dashboard.cshtml.cs
- [ ] Timetable/Create.cshtml.cs  
- [ ] Teachers.cshtml.cs

### TIER 2: MASTER DATA (5 Pages)
- [ ] Subjects.cshtml.cs
- [ ] Subjects/Index.cshtml.cs
- [ ] Classes/Index.cshtml.cs
- [ ] AcademicYears/Index.cshtml.cs
- [ ] AcademicYears/Semesters.cshtml.cs

### TIER 3: COMPLEX (5 Pages)
- [ ] Teachers/Profile.cshtml.cs
- [ ] Timetable.cshtml.cs
- [ ] Timetable/ByClass.cshtml.cs
- [ ] Timetable/ByTeacher.cshtml.cs
- [ ] Substitutions/Index.cshtml.cs

### TIER 4: ADVANCED (3 Pages)
- [ ] Timetable/Conflicts.cshtml.cs
- [ ] Timetable/AutoGenerate.cshtml.cs
- [ ] Timetable/MasterTimetable.cshtml.cs

**TOTAL: 0/16 Complete**
```

---

## 🔧 REFACTORING COMMAND REFERENCE

### Check current state
```bash
# Count pages still using AppDbContext
grep -r "AppDbContext" Plannify/Pages --include="*.cshtml.cs" | wc -l

# Find specific page issues
grep -A 5 "AppDbContext" Plannify/Pages/Admin/Teachers.cshtml.cs | head -10

# List all pages needing refactoring
grep -r "AppDbContext" Plannify/Pages --include="*.cshtml.cs" -l | sort
```

### Validate after refactoring
```bash
# Build check
dotnet build 2>&1 | grep -E "Build|Error"

# Verify page removed AppDbContext reference
grep "AppDbContext" Plannify/Pages/Admin/Teachers/Index.cshtml.cs
# Should return: (nothing)

# Verify page uses service
grep "ITeacherService" Plannify/Pages/Admin/Teachers/Index.cshtml.cs
# Should return: using... | _teacherService | constructor param
```

### Progress commit
```bash
# See what's staged
git status

# Commit all completed pages
git add Plannify/Pages/Admin/*.cshtml.cs Plannify/Pages/Admin/*/*.cshtml.cs
git commit -m "refactor(pages): complete Tier 1 pages - use services instead of AppDbContext"
```

---

## ⚠️ COMMON ISSUES & SOLUTIONS

### Issue: `Service method doesn't exist`
**Solution:** Check service interface for available methods
```bash
grep "public" Plannify/Application/Contracts/ITeacherService.cs
# Use similar method name in page
```

### Issue: `Model binder error - wrong type`
**Solution:** Use DTO instead of Model
```bash
# WRONG: public Teacher NewTeacher { get; set; }
# RIGHT: public CreateTeacherRequest NewTeacher { get; set; }
```

### Issue: `Build fails - missing using statement`
**Solution:** Add using for service interface
```csharp
using Plannify.Application.Contracts;  // Add this
```

### Issue: `Cannot access private field`
**Solution:** Use service methods, not direct property access
```bash
# WRONG: if (_dbContext.Teachers.Any(t => t.Id == id))
# RIGHT: var teacher = await _teacherService.GetByIdAsync(id);
#        if (teacher.IsSuccess && teacher.Value != null)
```

### Issue: `Page shows empty or errors after refactoring`
**Solution:** Check Result<T> error handling
```csharp
// Add this after service call
if (!result.IsSuccess)
{
    ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Operation failed");
}
```

---

## 📈 EXPECTED DAILY PACE

| Day | Pages | Status | Build |
|-----|-------|--------|-------|
| 1-2 | Tier 1 (3) | Active | ✅ 0 Errors |
| 3-7 | Tier 2 (5) | Active | ✅ 0 Errors |
| 8-12 | Tier 3 (5) | Active | ✅ 0 Errors |
| 13-15 | Tier 4 (3) | Active | ✅ 0 Errors |
| 16 | Verification | Complete | ✅ 0 Errors |

**If taking 1 page/day:** ~16 days to complete all pages  
**If taking 2 pages/day:** ~8 days to complete all pages  
**Recommended pace:** 1-2 pages/day

---

## ✅ COMPLETION CRITERIA

All pages must:
- [ ] Remove `AppDbContext` injection
- [ ] Add required `I[Service]Service` injection
- [ ] Replace all `_dbContext` queries with service calls
- [ ] Use DTOs instead of Models
- [ ] Add `Result<T>` error handling
- [ ] Build with 0 errors
- [ ] Manually tested in browser (CRUD operations)
- [ ] Commit with clear message

---

## 🎯 NEXT IMMEDIATE STEP

**Start with Tier 1 pages (easiest, most critical):**

1. Open `Plannify/Pages/Admin/Teachers.cshtml.cs`
2. Compare against template above
3. Make changes systematically
4. Build & test
5. Commit
6. Repeat for next page

**Recommended:** Start with Teachers.cshtml.cs (simple CRUD, clear pattern)

---

**Ready to start? Pick the first page and begin refactoring!**
