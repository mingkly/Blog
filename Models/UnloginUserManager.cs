using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace MyWeb.Models
{
    public class UnloginUserManager
    {
        private readonly IHttpContextAccessor _httpAccessor;

        private readonly UnloginUserContext _context;
        private readonly SecretProvider _secretProvider;
        public UnloginUserManager(IHttpContextAccessor httpAccessor,
            UnloginUserContext context,
            SecretProvider secretProvider)
        {
            _httpAccessor = httpAccessor;
            _secretProvider = secretProvider;
            _context = context;
        }
        public async Task<UnloginUser> CreateCookie()
        {

            var unloginUser = new UnloginUser();
            await _context.AddAsync(unloginUser);
            await _context.SaveChangesAsync();
            _httpAccessor.HttpContext.Response.Cookies.Append("UnloginUser", _secretProvider.ProtectId(unloginUser.Id.ToString()));
            return unloginUser;
        }
        public async Task<UnloginUser> GetUserAsync()
        {
            try
            {
                var cookie = _httpAccessor.HttpContext.Request.Cookies["UnloginUser"];
                cookie = _secretProvider.UnProtectId(cookie);
                if (!long.TryParse(cookie, out long id))
                {
                    return null;
                }
                return await _context.UnloginUsers.FindAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
    public class UnloginUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
    }
    public class UnloginUserContext : DbContext
    {
        public UnloginUserContext(DbContextOptions<UnloginUserContext> options)
            : base(options) { }
        public DbSet<UnloginUser> UnloginUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UnloginUser>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
