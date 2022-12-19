using JwtWebApiTemplate.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtWebApiTemplate.Data
{
    public class JwtWebApiTemplateContext: DbContext
    {
        public JwtWebApiTemplateContext(DbContextOptions<JwtWebApiTemplateContext> options): base(options) { 
        
        }
        public DbSet<User> Users { set; get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity => { entity.HasIndex(e => e.UserEmail).IsUnique(); });
        }
    }
}
