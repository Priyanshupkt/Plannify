using Microsoft.EntityFrameworkCore;
using Plannify.Models;

namespace Plannify.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<TimetableSlot> TimetableSlots { get; set; }
}
