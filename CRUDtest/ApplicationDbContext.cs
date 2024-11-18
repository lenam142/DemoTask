using CRUDtest.Models;
using Microsoft.EntityFrameworkCore;

namespace CRUDtest
{
    public class ApplicationDbContext:DbContext
    {
       public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Student> Student { get; set; }
        public DbSet<CapaPlan> CapaPlans { get; set; }
    }
}
