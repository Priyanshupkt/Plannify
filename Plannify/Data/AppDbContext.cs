using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Plannify.Models;

namespace Plannify.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Department> Departments { get; set; }
    public DbSet<AcademicYear> AcademicYears { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<ClassBatch> ClassBatches { get; set; }
    public DbSet<TimetableSlot> TimetableSlots { get; set; }
    public DbSet<SubstitutionRecord> SubstitutionRecords { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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

        builder.Entity<SubstitutionRecord>()
            .HasOne(s => s.OriginalTeacher)
            .WithMany()
            .HasForeignKey(s => s.OriginalTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SubstitutionRecord>()
            .HasOne(s => s.SubstituteTeacher)
            .WithMany()
            .HasForeignKey(s => s.SubstituteTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Teacher>()
            .HasIndex(t => t.EmployeeCode)
            .IsUnique();

        builder.Entity<Subject>()
            .HasIndex(s => s.Code)
            .IsUnique();

        builder.Entity<AcademicYear>()
            .HasIndex(a => a.IsActive);
    }
}

