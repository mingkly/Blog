using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyWeb.Data;
using MyWeb.Hubs;
using MyWeb.Middlewares;
using MyWeb.Models;
using MyWeb.Services;

namespace MyWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<VipNumberContext>(options =>
                options.UseSqlServer(
                     Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<IdProtectionContext>(options =>
                options.UseSqlServer(
                      Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<UnloginUserContext>(options =>
                options.UseSqlServer(
                      Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<MyUser>(options => options.SignIn.RequireConfirmedAccount = false)
               .AddRoles<IdentityRole<long>>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddDataProtection().PersistKeysToDbContext<IdProtectionContext>();
            services.AddSingleton<SecretProvider>();
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddScoped<UnloginUserManager>();
            services.AddDbContext<AppFileContext>(options =>
               options.UseSqlServer(
                      Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<ArticleServices>();
            services.AddSignalR();
            services.AddAntiforgery();
            services.AddSingleton<ChatHub>();
            services.AddSingleton<IUserIdProvider, UserNameProvider>();
            services.AddRazorPages().AddNewtonsoftJson();
            services.AddServerSideBlazor();
            services.AddControllersWithViews();
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            services.AddScoped<IAuthorizationHandler, IsOwnerAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, IsManagerAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, IsAdminAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, IsAdminCommentAuthorizationHandler>();
            services.AddScoped<MyAuthorizationManager>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString("/Articles/Create")),
                builder => builder.UseMiddleware<EnableRequestBuffering>());
          /*
            app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString("/Identity")),
                builder =>
                {
                    builder.UseMiddleware<BlockUnloginUser>();
                });
          */
           
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();

                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");

            });
        }
    }
}
