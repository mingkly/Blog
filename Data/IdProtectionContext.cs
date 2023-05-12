using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyWeb.Data
{
    public class IdProtectionContext : DbContext, IDataProtectionKeyContext
    {
        public IdProtectionContext(DbContextOptions<IdProtectionContext> options) : base(options) { }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}
