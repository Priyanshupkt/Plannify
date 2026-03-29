using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Plannify.Data;
using Plannify.Services;
using Plannify.Application.Contracts;
using Plannify.Application.Services;
using Plannify.Application.Mappings;
using Plannify.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ============════════════════════════════════════════════════
// CLEAN ARCHITECTURE DEPENDENCY INJECTION SETUP
// ============════════════════════════════════════════════════

// Set QuestPDF community license
QuestPDF.Settings.License = LicenseType.Community;

// ✅ INFRASTRUCTURE LAYER: Database & Persistence
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// ✅ REPOSITORIES: Data access abstractions
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<IClassBatchRepository, ClassBatchRepository>();
builder.Services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
builder.Services.AddScoped<ITimetableSlotRepository, TimetableSlotRepository>();
builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();
builder.Services.AddScoped<ISubstitutionRepository, SubstitutionRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// ✅ APPLICATION LAYER: Business logic services
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<IClassBatchService, ClassBatchService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<ITimetableSlotService, TimetableSlotService>();
builder.Services.AddScoped<ITimetableService, TimetableService>();
builder.Services.AddScoped<ISubstitutionService, SubstitutionService>();
// TODO: Add other services here as you migrate them

// ✅ INFRASTRUCTURE SERVICES: External services (still being refactored)
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<IConflictDetectorService, ConflictDetector>();
builder.Services.AddScoped<PdfExportService>();
builder.Services.AddScoped<ISchedulingService, SchedulingService>();

// ✅ AUTOMAPPER: For DTO mapping
builder.Services.AddAutoMapper(
    typeof(TeacherMappingProfile),
    typeof(DepartmentMappingProfile),
    typeof(RoomMappingProfile),
    typeof(SubjectMappingProfile),
    typeof(SemesterMappingProfile),
    typeof(ClassBatchMappingProfile),
    typeof(AcademicYearMappingProfile),
    typeof(TimetableSlotMappingProfile),
    typeof(TimetableMappingProfile),
    typeof(SubstitutionMappingProfile)
);

// ✅ AUTHENTICATION & AUTHORIZATION
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("User", policy => policy.RequireAuthenticatedUser());

// ✅ PRESENTATION LAYER
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// ============════════════════════════════════════════════════

var app = builder.Build();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
