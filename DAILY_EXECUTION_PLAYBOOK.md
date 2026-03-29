# DAILY EXECUTION PLAYBOOK

**6-Week Refactoring & Cleanup Implementation Guide**  
**Follow this day-by-day to transform Plannify into production-ready architecture**

---

## 📅 WEEK 1: PREPARATION & SERVICE INTERFACES

### DAY 1 (Phase 1a: Preparation)
**Goal:** Set up environment and create tracking documents  
**Time:** 1-2 hours

**Morning (9:00-10:00 AM):**
```bash
# 1. Create and checkout feature branch
cd /home/cy3pher/Documents/WorkSpace-Dev/Plannify
git checkout -b refactor/clean-architecture
git push --set-upstream origin refactor/clean-architecture
echo "✅ Branch created: refactor/clean-architecture"

# 2. Verify baseline build
dotnet clean
dotnet build 2>&1 | grep -E "Build|Error|Warning" | tail -5
# Expected: "Build succeeded. 0 error(s), 17 warning(s)."
```

**Afternoon (2:00-4:00 PM):**
```bash
# 3. Generate Pages report
grep -r "AppDbContext" Plannify/Pages --include="*.cshtml.cs" -l | sort > /tmp/pages_context.txt
echo "Pages using AppDbContext: $(wc -l < /tmp/pages_context.txt)"
# Expected: 18 pages

# 4. Create backup
cp -r Plannify/Pages Plannify/Pages.backup
cp -r Plannify/Models Plannify/Models.backup

# 5. Create documentation (from PHASE_1_QUICK_START.md Steps 3-7)
# Run all the commands to generate:
# - PAGES_STATUS_REPORT.md
# - SERVICES_NEEDED.md  
# - ARCHITECTURE_BASELINE.md
# - SERVICE_INTERFACE_CHECKLIST.md
# - PAGES_REFACTORING_PRIORITY.md

# 6. Commit preparation work
git add PAGES_STATUS_REPORT.md SERVICES_NEEDED.md ARCHITECTURE_BASELINE.md \
  SERVICE_INTERFACE_CHECKLIST.md PAGES_REFACTORING_PRIORITY.md Plannify/*.backup
git commit -m "docs: Phase 1 - baseline documentation and backups"
```

**Checklist:**
- [ ] Feature branch created and pushed
- [ ] Build verified (0 errors)
- [ ] Pages backup created
- [ ] Models backup created
- [ ] 5 documentation files generated
- [ ] All committed to git

---

### DAY 2 (Phase 2: Create Service Interfaces)
**Goal:** Create 3 infrastructure service interfaces  
**Time:** 2-3 hours

**Morning (9:00-11:00 AM):**

**Step 1: Create IAuditService Interface**
```bash
# File: Plannify/Application/Contracts/IAuditService.cs
cat > Plannify/Application/Contracts/IAuditService.cs << 'EOF'
using Plannify.Models;

namespace Plannify.Application.Contracts;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string entityId, 
        string? oldValues = null, string? newValues = null);
}
EOF
echo "✅ Created IAuditService.cs"

# File: Plannify/Services/AuditService.cs (UPDATE - add interface)
# Open in VS Code and change:
# FROM: public class AuditService
# TO:   public class AuditService : IAuditService
```

**Action:** Open `Plannify/Services/AuditService.cs` in VS Code and:
1. Add interface declaration: `public class AuditService : IAuditService`
2. Verify methods match interface signature

**Step 2: Create IPdfExportService Interface**
```bash
# File: Plannify/Application/Contracts/IPdfExportService.cs
cat > Plannify/Application/Contracts/IPdfExportService.cs << 'EOF'
namespace Plannify.Application.Contracts;

public interface IPdfExportService
{
    byte[] GenerateClassTimetablePdf(int semesterId, int classBatchId);
    byte[] GenerateTeacherTimetablePdf(int semesterId, int teacherId);
}
EOF
echo "✅ Created IPdfExportService.cs"

# File: Plannify/Services/PdfExportService.cs (UPDATE - add interface)
# In VS Code: Change "public class PdfExportService" → "public class PdfExportService : IPdfExportService"
```

**Action:** Open `Plannify/Services/PdfExportService.cs` and:
1. Add interface: `public class PdfExportService : IPdfExportService`
2. Verify methods exist and match

**Step 3: Create ITimetableReportService Interface**
```bash
# File: Plannify/Application/Contracts/ITimetableReportService.cs
cat > Plannify/Application/Contracts/ITimetableReportService.cs << 'EOF'
namespace Plannify.Application.Contracts;

public interface ITimetableReportService
{
    Task<byte[]> ExportTimetableAsync(int timetableId, string format);
    Task<byte[]> ExportTimetableToExcelAsync(int timetableId);
}
EOF
echo "✅ Created ITimetableReportService.cs"

# If TimetableExportService exists, update it:
# FROM: public class TimetableExportService
# TO:   public class TimetableExportService : ITimetableReportService
```

**Action:** Find TimetableExportService in Services folder and update similarly

**Afternoon (2:00-3:00 PM):**

**Step 4: Update Program.cs**
```bash
# File: Plannify/Program.cs
# Find these lines and update them:

# OLD:
# builder.Services.AddScoped<AuditService>();
# builder.Services.AddScoped<PdfExportService>();
# builder.Services.AddScoped<SchedulingService>();

# NEW:
# builder.Services.AddScoped<IAuditService, AuditService>();
# builder.Services.AddScoped<IPdfExportService, PdfExportService>();
# builder.Services.AddScoped<ITimetableReportService, TimetableExportService>();
```

**Action:** Open `Plannify/Program.cs` and:
1. Find the service registration section
2. Change: `builder.Services.AddScoped<AuditService>();` → `builder.Services.AddScoped<IAuditService, AuditService>();`
3. Change: `builder.Services.AddScoped<PdfExportService>();` → `builder.Services.AddScoped<IPdfExportService, PdfExportService>();`
4. Add: `builder.Services.AddScoped<ITimetableReportService, TimetableExportService>();`

**Step 5: Build & Verify**
```bash
dotnet build 2>&1 | tail -5
# Expected: Build succeeded. 0 error(s), 17 warning(s).

# If errors, check:
# 1. Interface file syntax correct
# 2. Service class has ":" and interface name
# 3. No typos in Program.cs

git add Plannify/Application/Contracts/IAuditService.cs \
  Plannify/Application/Contracts/IPdfExportService.cs \
  Plannify/Application/Contracts/ITimetableReportService.cs \
  Plannify/Services/AuditService.cs \
  Plannify/Services/PdfExportService.cs \
  Plannify/Program.cs

git commit -m "feat: add infrastructure service interfaces (IAuditService, IPdfExportService, ITimetableReportService)"
```

**Checklist:**
- [ ] IAuditService.cs created
- [ ] IPdfExportService.cs created
- [ ] ITimetableReportService.cs created
- [ ] AuditService implements interface
- [ ] PdfExportService implements interface
- [ ] TimetableExportService implements interface (if exists)
- [ ] Program.cs DI registrations updated
- [ ] Build succeeds (0 errors)
- [ ] Commit pushed

---

## 📅 WEEK 2: FIX SERVICE LAYER & START PAGE REFACTORING

### DAY 3-4 (Phase 3: Fix Service Layer)
**Goal:** Update ConflictDetector & SchedulingService to use repositories  
**Time:** 4-6 hours

---

### DAY 5 (Phase 4: Start Page Refactoring - Tier 1)
**Goal:** Refactor first 2-3 master data pages  
**Time:** 4-6 hours

**Morning (9:00-12:30 PM):**

**Target:** Pages/Admin/Teachers/Index.cshtml.cs

1. **Analyze Current Code**
```bash
# See what needs changing:
grep -A 50 "public class IndexModel(AppDbContext" Plannify/Pages/Admin/Teachers/Index.cshtml.cs
```

2. **Refactor Step-by-Step** (Use template from REFACTORING_MASTER_PLAN.md)
   - Remove: `AppDbContext dbContext` parameter
   - Add: `ITeacherService teacherService, IDepartmentService departmentService`
   - Replace queries with service calls
   - Use DTOs instead of Models
   - Add Result<T> error handling

3. **Test Page**
   - Run build
   - Visit page in browser
   - Try CRUD operations (Create, Read, Update, Delete)

4. **Commit**
```bash
git add Plannify/Pages/Admin/Teachers/Index.cshtml.cs
git commit -m "refactor(pages): Teachers page - use ITeacherService instead of AppDbContext"
```

**Afternoon (2:00-4:30 PM):**

**Targets:** Pages/Admin/Subjects/Index.cshtml.cs + Pages/Admin/Departments/Index.cshtml.cs

Repeat same process for each page.

**Checklist:**
- [ ] Teachers page refactored (use ITeacherService)
- [ ] Subjects page refactored (use ISubjectService)
- [ ] Departments page refactored (use IDepartmentService)
- [ ] All 3 pages tested in browser
- [ ] Build still 0 errors
- [ ] Each page committed separately

---

### DAY 6 (Phase 4 Continued: More Pages)
**Goal:** Refactor Rooms & AcademicYears pages  
**Time:** 4 hours

**Targets:** 
- Pages/Admin/Rooms/Index.cshtml.cs → Use IRoomService
- Pages/Admin/AcademicYears/Index.cshtml.cs → Use IAcademicYearService

**Same process as Day 5:**
1. Remove AppDbContext
2. Add service interface
3. Replace queries with service calls
4. Test in browser
5. Commit with clear message

---

## 📅 WEEK 3-4: MASS PAGE REFACTORING (Phase 4 Main)

### DAY 7-18 (Refactor Remaining Pages)

**This is the longest phase. ~1 page per day.**

**Daily Pattern:**
```bash
# Morning: Pick one page from PAGES_REFACTORING_PRIORITY.md

# 1. Analyze current code
grep -A 30 "public class.*Model.*AppDbContext" [PageName].cshtml.cs

# 2. Refactor (copy template from MASTER_PLAN.md)
#    - Remove AppDbContext
#    - Add service(s)
#    - Replace queries
#    - Use DTOs
#    - Add Result<T> error handling

# 3. Build & verify
dotnet build

# 4. Test in browser
# Go to page URL, test CRUD if applicable

# 5. Commit
git add Plannify/Pages/.../[PageName].cshtml.cs
git commit -m "refactor(pages): [PageName] - use I[Service] instead of AppDbContext"
```

**Progress Tracking:**
Update `PAGES_STATUS_REPORT.md` as you go:
```bash
# For each completed page:
sed -i 's/- \[ \] Pages\/Admin\/Teachers\/Index.cshtml.cs/- [x] Pages\/Admin\/Teachers\/Index.cshtml.cs/' PAGES_STATUS_REPORT.md
```

**Tier 1 (Days 7-11):** Master data pages (5 pages × 4 hours)
- Rooms, Teachers, Subjects, Departments, AcademicYears

**Tier 2 (Days 12-16):** Complex pages (5 pages × 6 hours)
- Classes, Semesters, advanced timetable pages

**Tier 3 (Days 17-22):** Most complex (6+ pages)
- Reports, Substitutions, advanced queries

---

## 📅 WEEK 5: DELETE MODELS FOLDER (Phase 5)

### DAY 26-27

**Prep (Day 25 evening):**
```bash
# Verify NO page still references Models
grep -r "using Plannify.Models" Plannify/Pages --include="*.cshtml.cs" | wc -l
# Expected: 0 (or only ApplicationUser/AuditLog in specific contexts)

# Verify Models backup exists
ls -la Plannify/Models.backup/
```

**DAY 26 (Morning):**

**Step 1: Update AppDbContext**
```bash
# File: Plannify/Data/AppDbContext.cs
# Find all DbSet<> references
# Change: public DbSet<Plannify.Models.Teacher> Teachers { get; set; }
# TO: public DbSet<Teacher> Teachers { get; set; }
# Add at top: using Plannify.Domain.Entities;
```

**Action:** Open `AppDbContext.cs` and:
1. Add `using Plannify.Domain.Entities;` at top
2. Find all `public DbSet<Plannify.Models.XYZ>` lines
3. Replace with `public DbSet<XYZ>` (remove "Plannify.Models.")
4. Verify build

**Step 2: Update EF Core Fluent Configurations**
```bash
# Find all DbContext.OnModelCreating configurations
# Replace any Model references with Entity references
grep -n "Models\." Plannify/Data/AppDbContext.cs
# Update any found
```

**Step 3: Delete Models Folder**
```bash
# Only after verification above!
rm -rf Plannify/Models

# Verify deletion
ls -la Plannify/Models 2>&1 | grep -i "cannot access"
# Expected: "cannot access 'Plannify/Models': No such file or directory"

# Build verification
dotnet build
# Expected: 0 errors, same 17 warnings

git add -A
git commit -m "refactor: remove duplicate Models folder, consolidate to Domain.Entities"
```

**Checklist:**
- [ ] All Pages verified (0 Models references)
- [ ] AppDbContext updated to use Domain.Entities
- [ ] EF configurations updated
- [ ] Models folder backup verified
- [ ] Models folder deleted
- [ ] Build succeeds (0 errors)
- [ ] Commit pushed

---

## 📅 WEEK 6: DATABASE & TESTING (Phases 6-7)

### DAY 28-35 (Database Schema Improvements)

**Create migration file (EF Core):**
```bash
cd Plannify
dotnet ef migrations add ImproveSchemaConstraints -o Data/Migrations -p Plannify.csproj
# Review:
cat Data/Migrations/*_ImproveSchemaConstraints.cs

# Apply migration
dotnet ef database update
```

### DAY 36-40 (Testing)

**Unit Tests Example:**
```bash
# Create test file
cat > Tests/TeacherServiceTests.cs << 'EOF'
[TestFixture]
public class TeacherServiceTests
{
    [Test]
    public async Task GetByIdAsync_WithValidId_ReturnsTeacher()
    {
        // Arrange
        // Act
        // Assert
    }
}
EOF

# Run tests
dotnet test
```

### DAY 41-42 (Cleanup & Documentation)

**Final Build Verification:**
```bash
dotnet clean
dotnet build 2>&1 | tail -5
# Expected: Build succeeded. 0 error(s), [warnings]

# No "Models" references
grep -r "Plannify.Models" Plannify --include="*.cs" | wc -l
# Expected: 0
```

**Create Summary:**
```bash
cat > REFACTORING_COMPLETE_REPORT.md << 'EOF'
# Refactoring Complete! 🎉

**Date:** $(date)
**Build Status:** 0 Errors

## Summary
- [ ] All 18 Pages refactored (use services)
- [ ] Models folder deleted
- [ ] 3 service interfaces created
- [ ] ConflictDetector fixed
- [ ] SchedulingService fixed
- [ ] Database constraints added
- [ ] All tests passing

## Architecture Score
Before: 58%
After: 92%+ ✅

## Key Metrics
- Pages using AppDbContext: 0 → 0 ✅
- Duplicate entities: 9 → 0 ✅
- Services with interfaces: 3 → 12 ✅
EOF
```

---

## 🚨 TROUBLESHOOTING

### Build Fails After Page Refactoring
```bash
# Check 1: Missing using statement
grep "ITeacherService" Plannify/Pages/Admin/Teachers/Index.cshtml.cs
# Should see: using Plannify.Application.Contracts;

# Check 2: Service registered in Program.cs
grep "ITeacherService" Plannify/Program.cs
# Should see: builder.Services.AddScoped<ITeacherService, TeacherService>();

# Check 3: Parameter order in constructor
# Constructor params must match DI registrations

# Check 4: Clean rebuild
dotnet clean && dotnet build
```

### Page Still Using AppDbContext
```bash
# Find it:
grep -r "AppDbContext" Plannify/Pages/Admin/Teachers/ --include="*.cshtml.cs"

# Remove all references, replace with service calls
```

### Database Migration Fails
```bash
# Rollback last migration
dotnet ef migrations remove
# Fix migration file
dotnet ef migrations add [Name]
dotnet ef database update
```

---

## ✅ SUCCESS CRITERIA (Day 42)

**Code Quality:**
- [ ] 0 compilation errors
- [ ] All tests passing  
- [ ] No direct AppDbContext in Pages
- [ ] All services have interfaces
- [ ] DTOs used consistently

**Architecture:**
- [ ] Architecture score: 85%+ (from 58%)
- [ ] Domain layer: 90%+
- [ ] Repository pattern: 90%+
- [ ] Service layer: 95%+
- [ ] Presentation layer: 95%+

**Database:**
- [ ] Migrations applied
- [ ] Constraints enforced
- [ ] Data integrity verified

**Documentation:**
- [ ] ARCHITECTURE_COMPLETE.md created
- [ ] All changes documented
- [ ] Team trained on patterns

---

## 📊 PROGRESS TRACKING

Print this and track daily:
```
Week 1 (Days 1-6): Preparation & Interfaces
☐ Day 1: Setup & documentation
☐ Day 2: Create 3 service interfaces
☐ Day 3-4: Fix ConflictDetector & SchedulingService
☐ Day 5: Refactor Tier 1 pages (Teachers, Subjects)
☐ Day 6: Refactor Tier 1 pages (Departments, Rooms, AcademicYears)

Week 2 (Days 7-12): Complex Pages
☐ Days 7-12: Refactor 6 Tier 2 pages (~1 per day)

Week 3 (Days 13-22): Remaining Pages
☐ Days 13-22: Refactor 10 Tier 3 pages (~1 per day)

Week 4 (Days 23-27): Models Deletion & Verification
☐ Days 23-25: Final page cleanup & verification
☐ Days 26-27: Delete Models folder

Week 5 (Days 28-35): Database & Testing
☐ Days 28-35: Database improvements & tests

Week 6 (Days 36-42): Final Testing & Cleanup
☐ Days 36-40: Comprehensive testing
☐ Days 41-42: Cleanup & documentation
```

---

## 🎯 FINAL COMMIT MESSAGE

When complete:
```bash
git commit -m "refactor: complete clean architecture migration

✅ All 22 pages refactored to use service layer
✅ Models folder consolidated to Domain.Entities
✅ All services have interfaces
✅ Database constraints added
✅ Architecture compliance: 58% → 92%

BREAKING: Direct AppDbContext access no longer available from Pages"
```

---

**Ready to start? Day 1 commands are above. Execute them today!**
