using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWeb.Models
{
    public class VipNumber
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public bool Used { get; set; }
    }
    public class VipNumberContext : DbContext
    {
        public VipNumberContext(DbContextOptions<VipNumberContext> options)
            : base(options) { }
        public DbSet<VipNumber> VipNumbers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

}
