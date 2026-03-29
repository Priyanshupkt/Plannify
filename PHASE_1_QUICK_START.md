# PHASE 1: QUICK START (Days 1-2)

**Objective:** Prepare refactoring environment and create templates  
**Time Estimate:** 2 days  
**Effort Level:** Low (Preparation only)

---

## STEP 1: Create Feature Branch (15 min)

```bash
cd /home/cy3pher/Documents/WorkSpace-Dev/Plannify
git status                              # Check current state
git checkout -b refactor/clean-architecture
git push --set-upstream origin refactor/clean-architecture
```

**Why?** Isolates your changes. You can revert if needed.

---

## STEP 2: Verify Current Build (5 min)

```bash
dotnet clean
dotnet build 2>&1 | tail -20
```

**Expected Output:**
```
Build succeeded. 0 error(s), 17 warning(s).
```

**Note:** If you get different numbers, note them down as your baseline.

---

## STEP 3: Generate Pages Report (10 min)

Create file: `PAGES_STATUS_REPORT.md`

**Command:**
```bash
grep -r "AppDbContext" Plannify/Pages --include="*.cshtml.cs" -l | sort > /tmp/pages_using_context.txt
wc -l /tmp/pages_using_context.txt
```

**Expected Output:** ~18 files

Now run:
```bash
cat > PAGES_STATUS_REPORT.md << 'EOF'
# Pages Status Report

## Need Refactoring (Using AppDbContext)

EOF

grep -r "AppDbContext" Plannify/Pages --include="*.cshtml.cs" -l | sort | while read file; do
  echo "- [ ] $file" >> PAGES_STATUS_REPORT.md
done

echo "" >> PAGES_STATUS_REPORT.md
echo "## Already Refactored (Using Services)" >> PAGES_STATUS_REPORT.md
echo "- [x] Pages/Admin/Dashboard.cshtml.cs (ISubstitutionService)" >> PAGES_STATUS_REPORT.md
echo "- [x] Pages/Admin/AutoGenerate.cshtml.cs (ISchedulingService)" >> PAGES_STATUS_REPORT.md
echo "- [x] Pages/Admin/Conflicts.cshtml.cs (IConflictDetectorService)" >> PAGES_STATUS_REPORT.md
echo "- [x] Pages/Admin/Timetable/Create.cshtml.cs (IConflictDetectorService)" >> PAGES_STATUS_REPORT.md
```

**Output will look like:**
```
- [ ] Pages/Admin/Teachers/Index.cshtml.cs
- [ ] Pages/Admin/Subjects/Index.cshtml.cs
- [ ] Pages/Admin/Departments/Index.cshtml.cs
[... more pages ...]

## Already Refactored (Using Services)
- [x] Pages/Admin/Dashboard.cshtml.cs (ISubstitutionService)
```

**Save this file** - you'll use it to track progress.

---

## STEP 4: Identify Required Services (20 min)

Create file: `SERVICES_NEEDED.md`

```bash
# Extract all pages that need services
cat > SERVICES_NEEDED.md << 'EOF'
# Services Needed for Refactoring

## By Service Type

### ITeacherService
- Pages/Admin/Teachers/Index.cshtml.cs
- Pages/Admin/TeacherAvailability.cshtml.cs

### ISubjectService
- Pages/Admin/Subjects/Index.cshtml.cs

### IDepartmentService
- Pages/Admin/Departments/Index.cshtml.cs

### IRoomService
- Pages/Admin/Rooms/Index.cshtml.cs

### IAcademicYearService
- Pages/Admin/AcademicYears/Index.cshtml.cs

### IClassBatchService
- Pages/Admin/Classes/Index.cshtml.cs

### ISemesterService
- Pages/Admin/Semesters/Index.cshtml.cs

### INFRA SERVICES NEEDED (Create Interfaces)
- [ ] **IAuditService** - needs to be created (extract from AuditService.cs)
- [ ] **IPdfExportService** - needs to be created
- [ ] **ITimetableReportService** - needs to be created

### SERVICE FIXES NEEDED
- [ ] **ConflictDetector** - Currently uses Models, should use ITimetableSlotRepository
- [ ] **SchedulingService** - Currently uses Models, should use repositories

EOF
cat SERVICES_NEEDED.md
```

---

## STEP 5: Document Current Architecture (15 min)

Create file: `ARCHITECTURE_BASELINE.md`

```bash
cat > ARCHITECTURE_BASELINE.md << 'EOF'
# Architecture Baseline (Pre-Refactoring)

**Date:** $(date)
**Build Status:** 0 errors, 17 warnings  
**Compliance Score:** 58%

## Entity Count
- Domain Entities: 10
- Domain Services: 10 (with Result<T>)
- Service Interfaces: 3 (ConflictDetector, SchedulingService, SubstitutionService)
- Repositories: 10+
- DTOs: 30+ (3 per entity average)

## Pages Status
- Total Pages: 22
- Using Services: 4
- Using AppDbContext: 18 ← **CRITICAL**

## Models Folder Status
- Duplicate Classes: 9 pairs
- Services use: Domain.Entities
- EF Core uses: Models.*
- → **ROOT CAUSE OF CONFUSION**

## Critical Issues
1. 18 Pages bypass service layer
2. 9 duplicate entity classes
3. Services lack interfaces (3 infrastructure services)
4. ConflictDetector/SchedulingService use Models instead of repositories
5. Database constraints missing

## Next Phase: Create Interfaces
- IAuditService
- IPdfExportService  
- ITimetableReportService
EOF
cat ARCHITECTURE_BASELINE.md
```

---

## STEP 6: Create a Master Service Interface Checklist (10 min)

```bash
cat > SERVICE_INTERFACE_CHECKLIST.md << 'EOF'
# Service Interface Audit Checklist

## Existing Service Interfaces ✅

- [x] ITeacherService - `Application/Contracts/ITeacherService.cs`
- [x] IDepartmentService - `Application/Contracts/IDepartmentService.cs`
- [x] IRoomService - `Application/Contracts/IRoomService.cs`
- [x] ISubjectService - `Application/Contracts/ISubjectService.cs`
- [x] ISemesterService - `Application/Contracts/ISemesterService.cs`
- [x] IClassBatchService - `Application/Contracts/IClassBatchService.cs`
- [x] IAcademicYearService - `Application/Contracts/IAcademicYearService.cs`
- [x] ITimetableSlotService - `Application/Contracts/ITimetableSlotService.cs`
- [x] ITimetableService - `Application/Contracts/ITimetableService.cs`
- [x] ISubstitutionService - `Application/Contracts/ISubstitutionService.cs`
- [x] IConflictDetectorService - `Application/Contracts/IConflictDetectorService.cs`
- [x] ISchedulingService - `Application/Contracts/ISchedulingService.cs`

## Infrastructure Services - NEED INTERFACES ❌

- [ ] **IAuditService** - needs `Application/Contracts/IAuditService.cs`
- [ ] **IPdfExportService** - needs `Application/Contracts/IPdfExportService.cs`
- [ ] **ITimetableReportService** - needs `Application/Contracts/ITimetableReportService.cs`

## Service Implementation Status

- [x] All 10 domain services implement their interfaces
- [ ] AuditService lacks interface
- [ ] PdfExportService lacks interface
- [ ] TimetableExportService lacks interface
- [ ] ConflictDetector needs to use repositories (currently uses Models)
- [ ] SchedulingService needs to use repositories (currently uses Models)
EOF
cat SERVICE_INTERFACE_CHECKLIST.md
```

---

## STEP 7: Create Pages Priority List (10 min)

```bash
cat > PAGES_REFACTORING_PRIORITY.md << 'EOF'
# Pages Refactoring Priority List

## TIER 1: Master Data Pages (Easiest - Start Here!)
Simple CRUD operations, good templates for other pages

- [ ] Pages/Admin/Teachers/Index.cshtml.cs → ITeacherService
- [ ] Pages/Admin/Subjects/Index.cshtml.cs → ISubjectService
- [ ] Pages/Admin/Departments/Index.cshtml.cs → IDepartmentService
- [ ] Pages/Admin/Rooms/Index.cshtml.cs → IRoomService
- [ ] Pages/Admin/AcademicYears/Index.cshtml.cs → IAcademicYearService

**Expected time:** 5 pages × 2 hours = 10 hours

## TIER 2: Complex Pages (Medium Difficulty)
More complex queries, relationships to manage

- [ ] Pages/Admin/Classes/Index.cshtml.cs → IClassBatchService
- [ ] Pages/Admin/Semesters/Index.cshtml.cs → ISemesterService
- [ ] Pages/Admin/Timetable.cshtml.cs → ITimetableSlotService
- [ ] Pages/Admin/Timetable/ByClass.cshtml.cs → ITimetableService
- [ ] Pages/Admin/Timetable/ByTeacher.cshtml.cs → ITimetableService

**Expected time:** 5 pages × 3 hours = 15 hours

## TIER 3: Advanced Pages (Most Complex)
Business logic heavy, conflict checking, reporting

- [ ] Pages/Admin/Substitutions/Index.cshtml.cs → ISubstitutionService
- [ ] Pages/Admin/Reports/.... → IRoomService/ITimetableReportService
- [ ] [Other advanced pages...]

**Expected time:** 8 pages × 4 hours = 32 hours

**TOTAL ESTIMATED:** ~57 hours ≈ 1.5 weeks
EOF
cat PAGES_REFACTORING_PRIORITY.md
```

---

## STEP 8: Backup and Commit (5 min)

```bash
# Back up original Pages folder
cp -r Plannify/Pages Plannify/Pages.backup
git add PAGES_STATUS_REPORT.md SERVICES_NEEDED.md ARCHITECTURE_BASELINE.md SERVICE_INTERFACE_CHECKLIST.md PAGES_REFACTORING_PRIORITY.md Plannify/Pages.backup
git commit -m "docs: Phase 1 preparation - baseline documentation and checklists"
```

---

## Summary of Phase 1 Deliverables

✅ **Created:**
1. Feature branch: `refactor/clean-architecture`
2. Pages tracking report: `PAGES_STATUS_REPORT.md` (18 pages need refactoring)
3. Services needed: `SERVICES_NEEDED.md` (3 interfaces to create)
4. Architecture baseline: `ARCHITECTURE_BASELINE.md` (58% compliance baseline)
5. Service checklist: `SERVICE_INTERFACE_CHECKLIST.md` (progress tracker)
6. Priority list: `PAGES_REFACTORING_PRIORITY.md` (phased approach)
7. Backup: `Plannify/Pages.backup` (rollback point)

✅ **Verified:**
- Current build: 0 errors ✓
- 18 Pages using AppDbContext identified ✓
- 4 Pages already using services confirmed ✓

---

## Phase 1 Completion Checklist

- [ ] Feature branch created and pushed
- [ ] Current build verified (0 errors)
- [ ] 6 documentation files created
- [ ] Pages backup created
- [ ] All files committed to git
- [ ] Team notified of branch

---

## Ready for Phase 2?

When you're ready to proceed to **Phase 2: Create Service Interfaces**, let me know and I'll:

1. Create `IAuditService` interface
2. Create `IPdfExportService` interface
3. Create `ITimetableReportService` interface
4. Update all implementations
5. Update Program.cs DI registrations
6. Verify build (still 0 errors)

**Estimated time for Phase 2:** 2-3 hours

---

## Commands for Phase 2 (Ready to Execute)

```bash
# These will be run in Phase 2:

# 1. Create IAuditService interface
cat > Plannify/Application/Contracts/IAuditService.cs << 'EOF'
using Plannify.Models;

namespace Plannify.Application.Contracts;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string entityId, 
        string? oldValues = null, string? newValues = null);
}
EOF

# 2. Create IPdfExportService interface
cat > Plannify/Application/Contracts/IPdfExportService.cs << 'EOF'
namespace Plannify.Application.Contracts;

public interface IPdfExportService
{
    byte[] GenerateClassTimetablePdf(int semesterId, int classBatchId);
    byte[] GenerateTeacherTimetablePdf(int semesterId, int teacherId);
}
EOF

# 3. Create ITimetableReportService interface
cat > Plannify/Application/Contracts/ITimetableReportService.cs << 'EOF'
namespace Plannify.Application.Contracts;

public interface ITimetableReportService
{
    Task<byte[]> ExportTimetableAsync(int timetableId, string format);
}
EOF
```

---

**STATUS:** ✅ Phase 1 Ready  
**NEXT:** Start Phase 2 when user is ready
