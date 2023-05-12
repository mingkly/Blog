using Microsoft.EntityFrameworkCore;
using MyWeb.Models;

namespace MyWeb.Data
{
    public class AppFileContext : DbContext
    {
        public AppFileContext(DbContextOptions<AppFileContext> options)
            : base(options) { }
        public DbSet<AppFile> AppFiles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppFile>()
                .HasKey(f => f.Id);
            modelBuilder.Entity<AppFile>()
                .HasOne(f => f.Uploader)
                .WithMany(u => u.UploadFiles)
                .HasForeignKey(f => f.UploaderId);
        }
    }
}
