using Plannify.Domain.Entities;

namespace Plannify.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<AppDbContext>();

        if (!dbContext.Departments.Any())   await SeedDepartments(dbContext);
        if (!dbContext.Rooms.Any())          await SeedRooms(dbContext);
        if (!dbContext.AcademicYears.Any())  await SeedAcademicYear(dbContext);
        if (!dbContext.Semesters.Any())      await SeedSemesters(dbContext);
        if (!dbContext.Teachers.Any())       await SeedTeachers(dbContext);
        if (!dbContext.Subjects.Any())       await SeedSubjects(dbContext);
        if (!dbContext.ClassBatches.Any())   await SeedClassBatches(dbContext);
        if (!dbContext.TimetableSlots.Any()) await SeedTimetableSlots(dbContext);
    }

    private static async Task SeedDepartments(AppDbContext dbContext)
    {
        var departments = new[]
        {
            new Department { Name = "Master of Computer Applications", Code = "MCA", ShortName = "MCA" },
            new Department { Name = "M.Sc. Software Technology", Code = "ST", ShortName = "ST" },
            new Department { Name = "M.Sc. Data Science", Code = "DS", ShortName = "DS" },
            new Department { Name = "M.Sc. Big Data Analytics", Code = "BDA", ShortName = "BDA" }
        };

        foreach (var dept in departments)
        {
            dbContext.Departments.Add(dept);
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedRooms(AppDbContext dbContext)
    {
        var rooms = new[]
        {
            // Lecture Rooms
            new Room { RoomNumber = "G03", BuildingName = "Ground Floor", RoomType = "Lecture", Capacity = 60, IsActive = true },
            new Room { RoomNumber = "G04", BuildingName = "Ground Floor", RoomType = "Lecture", Capacity = 60, IsActive = true },
            new Room { RoomNumber = "103", BuildingName = "First Floor", RoomType = "Lecture", Capacity = 50, IsActive = true },
            new Room { RoomNumber = "104", BuildingName = "First Floor", RoomType = "Lecture", Capacity = 50, IsActive = true },
            new Room { RoomNumber = "05", BuildingName = "Main Block", RoomType = "Lecture", Capacity = 50, IsActive = true },
            
            // Computer Labs
            new Room { RoomNumber = "Lab-01", BuildingName = "Computer Center", RoomType = "Lab", Capacity = 60, IsActive = true },
            new Room { RoomNumber = "Lab-02", BuildingName = "Computer Center", RoomType = "Lab", Capacity = 60, IsActive = true },
            new Room { RoomNumber = "Lab-03", BuildingName = "Computer Center", RoomType = "Lab", Capacity = 60, IsActive = true },
            new Room { RoomNumber = "Lab-04", BuildingName = "Computer Center", RoomType = "Lab", Capacity = 60, IsActive = true },
            new Room { RoomNumber = "Lab-05", BuildingName = "Computer Center", RoomType = "Lab", Capacity = 60, IsActive = true }
        };

        foreach (var room in rooms)
        {
            dbContext.Rooms.Add(room);
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedAcademicYear(AppDbContext dbContext)
    {
        var ay = new AcademicYear
        {
            YearLabel = "2025-26",
            StartDate = new DateTime(2025, 6, 1),
            EndDate = new DateTime(2026, 5, 31),
            IsActive = true
        };
        dbContext.AcademicYears.Add(ay);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedSemesters(AppDbContext dbContext)
    {
        var ayId = dbContext.AcademicYears.First().Id;

        var semEven = new Semester
        {
            Name = "Even Semester (Semester 2)",
            SemesterNumber = 2,
            AcademicYearId = ayId,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 5, 31),
            IsActive = true
        };
        dbContext.Semesters.Add(semEven);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedTeachers(AppDbContext dbContext)
    {
        var depts = dbContext.Departments.ToDictionary(d => d.Code, d => d);

        var teachers = new[]
        {
            new Teacher { FullName = "Dr. Hemalatha N", Initials = "HN", EmployeeCode = "FAC-001", Designation = "Associate Professor", MaxWeeklyHours = 24, DepartmentId = depts["BDA"].Id, IsActive = true },
            new Teacher { FullName = "Dr. Jeevan Pinto", Initials = "JP", EmployeeCode = "FAC-002", Designation = "Professor", MaxWeeklyHours = 25, DepartmentId = depts["MCA"].Id, IsActive = true },
            new Teacher { FullName = "Dr. Santhosh B", Initials = "SB", EmployeeCode = "FAC-003", Designation = "Associate Professor", MaxWeeklyHours = 24, DepartmentId = depts["MCA"].Id, IsActive = true },
            new Teacher { FullName = "Mrs. Suchetha V", Initials = "SV", EmployeeCode = "FAC-004", Designation = "Associate Professor", MaxWeeklyHours = 24, DepartmentId = depts["ST"].Id, IsActive = true },
            new Teacher { FullName = "Dr. Rakesh Kumar", Initials = "RK", EmployeeCode = "FAC-005", Designation = "Associate Professor", MaxWeeklyHours = 24, DepartmentId = depts["DS"].Id, IsActive = true },
            new Teacher { FullName = "Dr. Srinivas B.L", Initials = "SBL", EmployeeCode = "FAC-006", Designation = "Associate Professor", MaxWeeklyHours = 24, DepartmentId = depts["MCA"].Id, IsActive = true },
            new Teacher { FullName = "Mrs. Vanitha T", Initials = "VT", EmployeeCode = "FAC-007", Designation = "Assistant Professor", MaxWeeklyHours = 20, DepartmentId = depts["DS"].Id, IsActive = true },
            new Teacher { FullName = "Mrs. Nausheeda B S", Initials = "NBS", EmployeeCode = "FAC-008", Designation = "Assistant Professor", MaxWeeklyHours = 20, DepartmentId = depts["ST"].Id, IsActive = true },
            new Teacher { FullName = "Mr. S. Aravinda Prabhu", Initials = "SAP", EmployeeCode = "FAC-009", Designation = "Assistant Professor", MaxWeeklyHours = 20, DepartmentId = depts["DS"].Id, IsActive = true },
            new Teacher { FullName = "Mrs. Annapoorna Shetty", Initials = "AS", EmployeeCode = "FAC-010", Designation = "Assistant Professor", MaxWeeklyHours = 20, DepartmentId = depts["ST"].Id, IsActive = true },
            new Teacher { FullName = "Mr. Gana K V", Initials = "GKV", EmployeeCode = "FAC-011", Designation = "Assistant Professor", MaxWeeklyHours = 20, DepartmentId = depts["BDA"].Id, IsActive = true },
            new Teacher { FullName = "Ms. Anju M", Initials = "AM", EmployeeCode = "FAC-012", Designation = "Lecturer", MaxWeeklyHours = 18, DepartmentId = depts["DS"].Id, IsActive = true },
            new Teacher { FullName = "Mr. Brill Brenhil", Initials = "BB", EmployeeCode = "FAC-013", Designation = "Assistant Professor", MaxWeeklyHours = 20, DepartmentId = depts["MCA"].Id, IsActive = true },
            new Teacher { FullName = "Mrs. Amita Vakil", Initials = "AV", EmployeeCode = "FAC-014", Designation = "Assistant Professor", MaxWeeklyHours = 20, DepartmentId = depts["MCA"].Id, IsActive = true }
        };

        foreach (var teacher in teachers)
        {
            dbContext.Teachers.Add(teacher);
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedSubjects(AppDbContext dbContext)
    {
        var depts = dbContext.Departments.ToDictionary(d => d.Code, d => d);
        var mcaId = depts["MCA"].Id;
        var stId = depts["ST"].Id;
        var dsId = depts["DS"].Id;
        var bdaId = depts["BDA"].Id;

        var subjects = new[]
        {
            // ── MCA SUBJECTS (11) ──
            new Subject { Name = "Web Technologies and .NET Framework", Code = "IT3EPHC550", DepartmentId = mcaId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Object-Oriented Software Engineering", Code = "IT3EPHC551", DepartmentId = mcaId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Mobile Application Development using Android", Code = "IT3EPHC552", DepartmentId = mcaId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Operations Research", Code = "IT3EPSC571d", DepartmentId = mcaId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Optimization Techniques (Stream 1)", Code = "IT3EPSC572a", DepartmentId = mcaId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Marketing Analytics (Stream 2)", Code = "IT3EPSC572b", DepartmentId = mcaId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Web Technologies and .NET Framework Lab", Code = "IT3EPHP550", DepartmentId = mcaId, SubjectType = "Lab", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Mobile Application Development using Android Lab", Code = "IT3EPHP551", DepartmentId = mcaId, SubjectType = "Lab", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Operations Research Lab", Code = "IT3EPSP572", DepartmentId = mcaId, SubjectType = "Lab", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Research Methodology and Publication Ethics", Code = "IT3EPRM650", DepartmentId = mcaId, SubjectType = "Theory", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Research Methodology (MCA)", Code = "IT3EPPR586", DepartmentId = mcaId, SubjectType = "Theory", Credits = 2, SemesterNumber = 2 },

            // ── ST SUBJECTS (9) ──
            new Subject { Name = "Programming with Python", Code = "IT3FPHC550", DepartmentId = stId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Mobile Application Development with Android", Code = "IT3FPHC551", DepartmentId = stId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Python and Android Lab", Code = "IT3FPHP550", DepartmentId = stId, SubjectType = "Lab", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Enterprise level Data Warehousing and Data Mining", Code = "IT3FPSC571b", DepartmentId = stId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Foundations of Machine Learning", Code = "IT3FPSC572b", DepartmentId = stId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Machine Learning and Data Science Lab", Code = "IT3FPSP571", DepartmentId = stId, SubjectType = "Lab", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Decision making through Value Focused Thinking", Code = "IT3FPSC573", DepartmentId = stId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Enterprise Information System (OE)", Code = "IT3FPOE589", DepartmentId = stId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Research Methodology (ST)", Code = "IT3FPRM550", DepartmentId = stId, SubjectType = "Theory", Credits = 2, SemesterNumber = 2 },

            // ── DS SUBJECTS (9) ──
            new Subject { Name = "Multivariate Techniques for Data Science", Code = "IT3IPHC550", DepartmentId = dsId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Machine Learning Algorithms", Code = "IT3IPHC551", DepartmentId = dsId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Enabling Technologies for Data Science", Code = "IT3IPHC552", DepartmentId = dsId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Natural Language Processing (DS)", Code = "IT3IPSC571a", DepartmentId = dsId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Machine Learning and NLP Lab", Code = "IT3IPHP550", DepartmentId = dsId, SubjectType = "Lab", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Data Visualization using Tableau", Code = "IT3IPSP571", DepartmentId = dsId, SubjectType = "Lab", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Decision making through Value Focused (DS)", Code = "IT3IPSC572", DepartmentId = dsId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Statistical Data Analysis using R (DS OE)", Code = "IT3IPOE589", DepartmentId = dsId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Research Methodology (DS)", Code = "IT3IPRM550", DepartmentId = dsId, SubjectType = "Theory", Credits = 2, SemesterNumber = 2 },

            // ── BDA SUBJECTS (9) ──
            new Subject { Name = "Machine Learning – I", Code = "IT3HPHC550", DepartmentId = bdaId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Enabling Technologies for Data Science – I", Code = "IT3HPHC551", DepartmentId = bdaId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Natural Language Processing (BDA)", Code = "IT3HPHC552c", DepartmentId = bdaId, SubjectType = "Theory", Credits = 4, SemesterNumber = 2 },
            new Subject { Name = "Machine Learning and Data Science Lab – I", Code = "IT3HPHP550", DepartmentId = bdaId, SubjectType = "Lab", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Foundations of Data Science", Code = "IT3HPSC571", DepartmentId = bdaId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Advanced Statistical Methods", Code = "IT3HPSC572", DepartmentId = bdaId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 },
            new Subject { Name = "Research Methodology (BDA)", Code = "IT3HPRM550", DepartmentId = bdaId, SubjectType = "Theory", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Programming for Big Data and Advanced Statistical Methods Lab", Code = "IT3HPSP571", DepartmentId = bdaId, SubjectType = "Lab", Credits = 2, SemesterNumber = 2 },
            new Subject { Name = "Statistical Data Analysis using R (BDA OE)", Code = "IT3HPOE589", DepartmentId = bdaId, SubjectType = "Theory", Credits = 3, SemesterNumber = 2 }
        };

        foreach (var subject in subjects)
        {
            dbContext.Subjects.Add(subject);
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedClassBatches(AppDbContext dbContext)
    {
        var depts = dbContext.Departments.ToDictionary(d => d.Code, d => d);
        var ayId = dbContext.AcademicYears.First().Id;

        var batches = new[]
        {
            new ClassBatch { BatchName = "MCA 'A'", Strength = 60, Semester = 2, DepartmentId = depts["MCA"].Id, AcademicYearId = ayId },
            new ClassBatch { BatchName = "MCA 'B'", Strength = 60, Semester = 2, DepartmentId = depts["MCA"].Id, AcademicYearId = ayId },
            new ClassBatch { BatchName = "M.Sc. ST", Strength = 45, Semester = 2, DepartmentId = depts["ST"].Id, AcademicYearId = ayId },
            new ClassBatch { BatchName = "M.Sc. DS", Strength = 40, Semester = 2, DepartmentId = depts["DS"].Id, AcademicYearId = ayId },
            new ClassBatch { BatchName = "M.Sc. BDA", Strength = 38, Semester = 2, DepartmentId = depts["BDA"].Id, AcademicYearId = ayId }
        };

        foreach (var batch in batches)
        {
            dbContext.ClassBatches.Add(batch);
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedTimetableSlots(AppDbContext dbContext)
    {
        try
        {
            var semId = dbContext.Semesters.FirstOrDefault(s => s.SemesterNumber == 2)?.Id;
            if (!semId.HasValue) return;

            var teachers = dbContext.Teachers.ToDictionary(t => t.Initials, t => t);
            var subjects = dbContext.Subjects.ToDictionary(s => s.Code, s => s);
            var rooms = dbContext.Rooms.ToDictionary(r => r.RoomNumber, r => r);
            var batches = dbContext.ClassBatches.ToDictionary(b => b.BatchName, b => b);

            var slots = new List<TimetableSlot>();
            var now = DateTime.UtcNow;

            // Helper functions
            int GetTeacherId(string initial) => teachers.ContainsKey(initial) ? teachers[initial].Id : 0;
            int GetSubjectId(string code) => subjects.ContainsKey(code) ? subjects[code].Id : 0;
            int GetRoomId(string roomNum) => rooms.ContainsKey(roomNum) ? rooms[roomNum].Id : 0;
            int GetBatchId(string batchName) => batches.ContainsKey(batchName) ? batches[batchName].Id : 0;

            Action<string, string, string, string, string, int, int, int, int, bool> AddSlot =
                (batchName, teacherInitials, subjectCode, roomNumber, day, startHour, startMin, endHour, endMin, isLab) =>
            {
                int batchId = GetBatchId(batchName);
                int teacherId = GetTeacherId(teacherInitials);
                int subjectId = GetSubjectId(subjectCode);
                int roomId = GetRoomId(roomNumber);

                if (batchId > 0 && teacherId > 0 && subjectId > 0 && roomId > 0)
                {
                    slots.Add(new TimetableSlot
                    {
                        ClassBatchId = batchId,
                        TeacherId = teacherId,
                        SubjectId = subjectId,
                        RoomId = roomId,
                        Day = day,
                        StartTime = new TimeOnly(startHour, startMin),
                        EndTime = new TimeOnly(endHour, endMin),
                        SlotType = isLab ? "Lab" : "Theory",
                        IsLabSession = isLab,
                        CreatedAt = now,
                        CreatedBy = "DbSeeder",
                        SemesterId = semId.Value
                    });
                }
            };

            // ════════════════════════════════════════════════════════════════
            // MONDAY SLOTS
            // ════════════════════════════════════════════════════════════════
            AddSlot("MCA 'A'", "JP", "IT3EPSC571d", "G03", "Monday", 9, 0, 10, 0, false);
            AddSlot("MCA 'A'", "SB", "IT3EPSC572a", "G03", "Monday", 10, 0, 11, 0, false);
            AddSlot("MCA 'A'", "SBL", "IT3EPPR586", "G03", "Monday", 11, 10, 12, 0, false);
            AddSlot("MCA 'A'", "SB", "IT3EPHP550", "Lab-01", "Monday", 14, 0, 15, 0, true);
            AddSlot("MCA 'A'", "BB", "IT3EPHP550", "Lab-01", "Monday", 15, 0, 16, 0, true);
            
            AddSlot("MCA 'B'", "SB", "IT3EPHC550", "G04", "Monday", 9, 0, 10, 0, false);
            AddSlot("MCA 'B'", "RK", "IT3EPHC551", "G04", "Monday", 10, 0, 11, 0, false);
            AddSlot("MCA 'B'", "AV", "IT3EPSC572b", "G04", "Monday", 11, 10, 12, 0, false);
            AddSlot("MCA 'B'", "BB", "IT3EPHC552", "G04", "Monday", 12, 0, 13, 0, false);
            AddSlot("MCA 'B'", "AV", "IT3EPHP551", "Lab-05", "Monday", 14, 0, 15, 0, true);
            AddSlot("MCA 'B'", "AV", "IT3EPHP551", "Lab-05", "Monday", 15, 0, 16, 0, true);
            
            AddSlot("M.Sc. ST", "SV", "IT3FPHC550", "103", "Monday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. ST", "SBL", "IT3FPHC551", "103", "Monday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. ST", "NBS", "IT3FPSC572b", "103", "Monday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. ST", "AS", "IT3FPSC571b", "103", "Monday", 12, 0, 13, 0, false);
            AddSlot("M.Sc. ST", "SV", "IT3FPHP550", "Lab-03", "Monday", 14, 0, 15, 0, true);
            AddSlot("M.Sc. ST", "AS", "IT3FPHP550", "Lab-03", "Monday", 15, 0, 16, 0, true);
            
            AddSlot("M.Sc. DS", "SAP", "IT3IPHC551", "104", "Monday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. DS", "GKV", "IT3IPSC571a", "104", "Monday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. DS", "SV", "IT3IPOE589", "104", "Monday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. DS", "RK", "IT3IPHC552", "104", "Monday", 12, 0, 13, 0, false);
            AddSlot("M.Sc. DS", "AM", "IT3IPSP571", "Lab-04", "Monday", 14, 0, 15, 0, true);
            AddSlot("M.Sc. DS", "SAP", "IT3IPHC551", "104", "Monday", 15, 0, 16, 0, false);
            
            AddSlot("M.Sc. BDA", "AV", "IT3HPHC551", "05", "Monday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. BDA", "HN", "IT3HPHC550", "05", "Monday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. BDA", "VT", "IT3HPSC572", "05", "Monday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. BDA", "GKV", "IT3HPHC552c", "05", "Monday", 12, 0, 13, 0, false);

            // ════════════════════════════════════════════════════════════════
            // TUESDAY SLOTS
            // ════════════════════════════════════════════════════════════════
            AddSlot("MCA 'A'", "RK", "IT3EPHC551", "G03", "Tuesday", 9, 0, 10, 0, false);
            AddSlot("MCA 'A'", "JP", "IT3EPSC571d", "G03", "Tuesday", 10, 0, 11, 0, false);
            AddSlot("MCA 'A'", "BB", "IT3EPHC552", "G03", "Tuesday", 11, 10, 12, 0, false);
            AddSlot("MCA 'A'", "SB", "IT3EPSC572a", "G03", "Tuesday", 12, 0, 13, 0, false);
            AddSlot("MCA 'A'", "BB", "IT3EPHP551", "Lab-01", "Tuesday", 14, 0, 15, 0, true);
            AddSlot("MCA 'A'", "BB", "IT3EPHP551", "Lab-01", "Tuesday", 15, 0, 16, 0, true);
            
            AddSlot("MCA 'B'", "SB", "IT3EPHP550", "Lab-05", "Tuesday", 9, 0, 10, 0, true);
            AddSlot("MCA 'B'", "SBL", "IT3EPPR586", "G04", "Tuesday", 10, 0, 11, 0, false);
            AddSlot("MCA 'B'", "AV", "IT3EPSC572b", "G04", "Tuesday", 11, 10, 12, 0, false);
            AddSlot("MCA 'B'", "JP", "IT3EPSP572", "Lab-05", "Tuesday", 12, 0, 13, 0, true);
            AddSlot("MCA 'B'", "AV", "IT3EPSC572b", "G04", "Tuesday", 14, 0, 15, 0, false);
            
            AddSlot("M.Sc. ST", "NBS", "IT3FPSP571", "Lab-03", "Tuesday", 9, 0, 10, 0, true);
            AddSlot("M.Sc. ST", "AS", "IT3FPSC571b", "103", "Tuesday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. ST", "SV", "IT3FPHC550", "103", "Tuesday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. ST", "SBL", "IT3FPHP550", "Lab-03", "Tuesday", 12, 0, 13, 0, true);
            AddSlot("M.Sc. ST", "AS", "IT3FPSC571b", "103", "Tuesday", 14, 0, 15, 0, false);
            
            AddSlot("M.Sc. DS", "VT", "IT3IPHC550", "104", "Tuesday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. DS", "SAP", "IT3IPHC551", "104", "Tuesday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. DS", "RK", "IT3IPHC552", "104", "Tuesday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. DS", "SAP", "IT3IPHP550", "Lab-04", "Tuesday", 12, 0, 13, 0, true);
            AddSlot("M.Sc. DS", "AM", "IT3IPHC551", "104", "Tuesday", 14, 0, 15, 0, false);
            
            AddSlot("M.Sc. BDA", "GKV", "IT3HPHC552c", "05", "Tuesday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. BDA", "AM", "IT3HPSC571", "05", "Tuesday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. BDA", "AV", "IT3HPHC551", "05", "Tuesday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. BDA", "HN", "IT3HPHC550", "05", "Tuesday", 12, 0, 13, 0, false);
            AddSlot("M.Sc. BDA", "VT", "IT3HPSP571", "Lab-02", "Tuesday", 14, 0, 15, 0, true);
            AddSlot("M.Sc. BDA", "GKV", "IT3HPHC552c", "05", "Tuesday", 15, 0, 16, 0, false);

            // ════════════════════════════════════════════════════════════════
            // WEDNESDAY SLOTS
            // ════════════════════════════════════════════════════════════════
            AddSlot("MCA 'A'", "SB", "IT3EPHC550", "G03", "Wednesday", 9, 0, 10, 0, false);
            AddSlot("MCA 'A'", "RK", "IT3EPHC551", "G03", "Wednesday", 10, 0, 11, 0, false);
            AddSlot("MCA 'A'", "AV", "IT3EPRM650", "G03", "Wednesday", 11, 10, 12, 0, false);
            AddSlot("MCA 'A'", "BB", "IT3EPHC552", "G03", "Wednesday", 12, 0, 13, 0, false);
            AddSlot("MCA 'A'", "SV", "IT3EPRM650", "G03", "Wednesday", 14, 0, 15, 0, false);
            AddSlot("MCA 'A'", "BB", "IT3EPHC552", "G03", "Wednesday", 15, 0, 16, 0, false);
            
            AddSlot("MCA 'B'", "JP", "IT3EPSC571d", "G04", "Wednesday", 9, 0, 10, 0, false);
            AddSlot("MCA 'B'", "BB", "IT3EPHC552", "G04", "Wednesday", 10, 0, 11, 0, false);
            AddSlot("MCA 'B'", "RK", "IT3EPHC551", "G04", "Wednesday", 11, 10, 12, 0, false);
            AddSlot("MCA 'B'", "RK", "IT3EPRM650", "G04", "Wednesday", 12, 0, 13, 0, false);
            AddSlot("MCA 'B'", "AV", "IT3EPSC572b", "G04", "Wednesday", 14, 0, 15, 0, false);
            
            AddSlot("M.Sc. ST", "NBS", "IT3FPSC572b", "103", "Wednesday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. ST", "SV", "IT3FPHC550", "103", "Wednesday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. ST", "SBL", "IT3FPHC551", "103", "Wednesday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. ST", "AS", "IT3FPOE589", "103", "Wednesday", 12, 0, 13, 0, false);
            AddSlot("M.Sc. ST", "NBS", "IT3FPSC572b", "103", "Wednesday", 14, 0, 15, 0, false);
            AddSlot("M.Sc. ST", "NBS", "IT3FPSC572b", "103", "Wednesday", 15, 0, 16, 0, false);
            
            AddSlot("M.Sc. DS", "SAP", "IT3IPHC551", "104", "Wednesday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. DS", "GKV", "IT3IPSC571a", "104", "Wednesday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. DS", "SV", "IT3IPOE589", "104", "Wednesday", 12, 0, 13, 0, false);
            AddSlot("M.Sc. DS", "AS", "IT3IPHC551", "104", "Wednesday", 14, 0, 15, 0, false);
            AddSlot("M.Sc. DS", "AM", "IT3IPHC551", "104", "Wednesday", 15, 0, 16, 0, false);
            
            AddSlot("M.Sc. BDA", "VT", "IT3HPSC572", "05", "Wednesday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. BDA", "GKV", "IT3HPHC552c", "05", "Wednesday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. BDA", "GKV", "IT3HPHC552c", "05", "Wednesday", 12, 0, 13, 0, false);

            // ════════════════════════════════════════════════════════════════
            // THURSDAY SLOTS
            // ════════════════════════════════════════════════════════════════
            AddSlot("MCA 'A'", "SB", "IT3EPHC550", "G03", "Thursday", 9, 0, 10, 0, false);
            AddSlot("MCA 'A'", "BB", "IT3EPHC552", "G03", "Thursday", 10, 0, 11, 0, false);
            AddSlot("MCA 'A'", "JP", "IT3EPSC572a", "G03", "Thursday", 11, 10, 12, 0, false);
            AddSlot("MCA 'A'", "RK", "IT3EPHC551", "G03", "Thursday", 12, 0, 13, 0, false);
            AddSlot("MCA 'A'", "SB", "IT3EPHP550", "Lab-01", "Thursday", 14, 0, 15, 0, true);
            AddSlot("MCA 'A'", "BB", "IT3EPHP550", "Lab-01", "Thursday", 15, 0, 16, 0, true);
            
            AddSlot("MCA 'B'", "RK", "IT3EPHC551", "G04", "Thursday", 9, 0, 10, 0, false);
            AddSlot("MCA 'B'", "SB", "IT3EPHC550", "G04", "Thursday", 10, 0, 11, 0, false);
            AddSlot("MCA 'B'", "AV", "IT3EPSC572b", "G04", "Thursday", 11, 10, 12, 0, false);
            AddSlot("MCA 'B'", "BB", "IT3EPHC552", "G04", "Thursday", 12, 0, 13, 0, false);
            AddSlot("MCA 'B'", "BB", "IT3EPHP551", "Lab-05", "Thursday", 14, 0, 15, 0, true);
            AddSlot("MCA 'B'", "AV", "IT3EPHP551", "Lab-05", "Thursday", 15, 0, 16, 0, true);
            
            AddSlot("M.Sc. ST", "AS", "IT3FPSC571b", "103", "Thursday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. ST", "AM", "IT3FPSC573", "103", "Thursday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. ST", "NBS", "IT3FPSP571", "Lab-03", "Thursday", 11, 10, 12, 0, true);
            AddSlot("M.Sc. ST", "SBL", "IT3FPHP550", "Lab-03", "Thursday", 12, 0, 13, 0, true);
            AddSlot("M.Sc. ST", "NBS", "IT3FPSC572b", "103", "Thursday", 14, 0, 15, 0, false);
            
            AddSlot("M.Sc. DS", "GKV", "IT3IPSC571a", "104", "Thursday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. DS", "RK", "IT3IPHC552", "104", "Thursday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. DS", "SAP", "IT3IPRM550", "104", "Thursday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. DS", "VT", "IT3IPSP571", "Lab-04", "Thursday", 12, 0, 13, 0, true);
            AddSlot("M.Sc. DS", "AM", "IT3IPHC551", "104", "Thursday", 14, 0, 15, 0, false);
            
            AddSlot("M.Sc. BDA", "AV", "IT3HPHC551", "05", "Thursday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. BDA", "HN", "IT3HPHC550", "05", "Thursday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. BDA", "GKV", "IT3HPHC552c", "05", "Thursday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. BDA", "AM", "IT3HPSC571", "05", "Thursday", 12, 0, 13, 0, false);
            AddSlot("M.Sc. BDA", "AV", "IT3HPHP550", "Lab-02", "Thursday", 14, 0, 15, 0, true);
            AddSlot("M.Sc. BDA", "GKV", "IT3HPHC552c", "05", "Thursday", 15, 0, 16, 0, false);

            // ════════════════════════════════════════════════════════════════
            // FRIDAY SLOTS
            // ════════════════════════════════════════════════════════════════
            AddSlot("MCA 'A'", "RK", "IT3EPHC551", "G03", "Friday", 9, 0, 10, 0, false);
            AddSlot("MCA 'A'", "BB", "IT3EPHC552", "G03", "Friday", 10, 0, 11, 0, false);
            AddSlot("MCA 'A'", "SB", "IT3EPHC550", "G03", "Friday", 11, 10, 12, 0, false);
            AddSlot("MCA 'A'", "SBL", "IT3EPPR586", "G03", "Friday", 12, 0, 13, 0, false);
            AddSlot("MCA 'A'", "JP", "IT3EPSP572", "Lab-01", "Friday", 14, 0, 15, 0, true);
            AddSlot("MCA 'A'", "BB", "IT3EPSP572", "Lab-01", "Friday", 15, 0, 16, 0, true);
            
            AddSlot("MCA 'B'", "SB", "IT3EPHC550", "G04", "Friday", 9, 0, 10, 0, false);
            AddSlot("MCA 'B'", "SBL", "IT3EPPR586", "G04", "Friday", 10, 0, 11, 0, false);
            AddSlot("MCA 'B'", "BB", "IT3EPHC552", "G04", "Friday", 11, 10, 12, 0, false);
            AddSlot("MCA 'B'", "JP", "IT3EPSC571d", "G04", "Friday", 12, 0, 13, 0, false);
            AddSlot("MCA 'B'", "AV", "IT3EPHP550", "Lab-05", "Friday", 14, 0, 15, 0, true);
            AddSlot("MCA 'B'", "AV", "IT3EPHP550", "Lab-05", "Friday", 15, 0, 16, 0, true);
            
            AddSlot("M.Sc. ST", "SBL", "IT3FPHC551", "103", "Friday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. ST", "NBS", "IT3FPSC572b", "103", "Friday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. ST", "AV", "IT3FPRM550", "103", "Friday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. ST", "AS", "IT3FPOE589", "103", "Friday", 12, 0, 13, 0, false);
            AddSlot("M.Sc. ST", "AS", "IT3FPSP571", "Lab-03", "Friday", 14, 0, 15, 0, true);
            AddSlot("M.Sc. ST", "SAP", "IT3FPSC571b", "103", "Friday", 15, 0, 16, 0, false);
            
            AddSlot("M.Sc. DS", "VT", "IT3IPHC550", "104", "Friday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. DS", "RK", "IT3IPHC552", "104", "Friday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. DS", "SAP", "IT3IPHC551", "104", "Friday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. DS", "GKV", "IT3IPHP550", "Lab-04", "Friday", 12, 0, 13, 0, true);
            AddSlot("M.Sc. DS", "AM", "IT3IPHC551", "104", "Friday", 14, 0, 15, 0, false);
            
            AddSlot("M.Sc. BDA", "SAP", "IT3HPRM550", "05", "Friday", 9, 0, 10, 0, false);
            AddSlot("M.Sc. BDA", "AV", "IT3HPHC551", "05", "Friday", 10, 0, 11, 0, false);
            AddSlot("M.Sc. BDA", "AM", "IT3HPSC571", "05", "Friday", 11, 10, 12, 0, false);
            AddSlot("M.Sc. BDA", "VT", "IT3HPSC572", "05", "Friday", 12, 0, 13, 0, false);
            AddSlot("M.Sc. BDA", "HN", "IT3HPHP550", "Lab-02", "Friday", 14, 0, 15, 0, true);
            AddSlot("M.Sc. BDA", "GKV", "IT3HPHC552c", "05", "Friday", 15, 0, 16, 0, false);

            // ════════════════════════════════════════════════════════════════
            // SATURDAY SLOTS
            // ════════════════════════════════════════════════════════════════
            AddSlot("MCA 'A'", "BB", "IT3EPHP551", "Lab-01", "Saturday", 9, 0, 9, 45, true);
            AddSlot("MCA 'A'", "SB", "IT3EPHC550", "G03", "Saturday", 9, 50, 10, 35, false);
            
            AddSlot("MCA 'B'", "SB", "IT3EPHC550", "G04", "Saturday", 9, 0, 9, 45, false);
            AddSlot("MCA 'B'", "AV", "IT3EPRM650", "G04", "Saturday", 9, 50, 10, 35, false);
            AddSlot("MCA 'B'", "RK", "IT3EPHC551", "G04", "Saturday", 10, 45, 11, 30, false);
            
            AddSlot("M.Sc. ST", "SBL", "IT3FPHC551", "103", "Saturday", 9, 0, 9, 45, false);
            AddSlot("M.Sc. ST", "SV", "IT3FPHC550", "103", "Saturday", 9, 50, 10, 35, false);
            
            AddSlot("M.Sc. DS", "SAP", "IT3IPHP550", "Lab-04", "Saturday", 9, 0, 9, 45, true);
            
            AddSlot("M.Sc. BDA", "GKV", "IT3HPHC552c", "05", "Saturday", 9, 0, 9, 45, false);
            AddSlot("M.Sc. BDA", "HN", "IT3HPHC550", "05", "Saturday", 9, 50, 10, 35, false);

            // Save all slots in batches of 50
            for (int i = 0; i < slots.Count; i += 50)
            {
                var batch = slots.Skip(i).Take(50).ToList();
                foreach (var slot in batch)
                {
                    dbContext.TimetableSlots.Add(slot);
                }
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEED ERROR] TimetableSlots: {ex.Message}");
        }
    }
}

