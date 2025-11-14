using WebApplication1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // (1) إضافة هذا
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Data
{
    // (2) تغيير "DbContext" إلى "IdentityDbContext"
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // جدول الموظفين الخاص بنا لا يزال موجوداً
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // (3) هام جداً: استدعاء الـ base أولاً
            base.OnModelCreating(modelBuilder);

            // الكود الخاص بنا لتحديد نوع الراتب
            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasColumnType("decimal(18, 2)");
        }
    }
}