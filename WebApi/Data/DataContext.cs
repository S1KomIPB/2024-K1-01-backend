using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WebApi.Models;

namespace WebApi.Data
{
    public partial class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseType> CourseTypes { get; set; }
        public DbSet<CourseClass> CourseClasses { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.InitialChar)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasMany(c => c.CourseTypes)
                .WithOne(ct => ct.Courses)
                .HasForeignKey(ct => ct.CourseId);

            modelBuilder.Entity<CourseType>()
                .HasOne(ct => ct.Courses)
                .WithMany(c => c.CourseTypes)
                .HasForeignKey(ct => ct.CourseId);

            modelBuilder.Entity<CourseType>()
                .HasMany(ct => ct.CourseClasses)
                .WithOne(cc => cc.CourseTypes)
                .HasForeignKey(cc => cc.CourseTypeId);

            modelBuilder.Entity<Semester>()
                .HasKey(x => x.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}