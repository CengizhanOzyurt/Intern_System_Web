using Microsoft.EntityFrameworkCore;

namespace ProjectForm.Models
{
    public class InternDBcontext : DbContext
    {
        public InternDBcontext(DbContextOptions<InternDBcontext> options) : base(options)
        {
            
        }

        public DbSet<UsersModel> Users { get; set; }
        public DbSet<UsersTaskModel> Users_Task { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsersModel>().ToTable("Users");
            modelBuilder.Entity<UsersTaskModel>().ToTable("Users_Task");
        }
    }
}