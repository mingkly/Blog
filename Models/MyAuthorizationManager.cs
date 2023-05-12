using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using MyWeb.Data;
using System.Threading.Tasks;

namespace MyWeb.Models
{
    public class MyAuthorizationManager
    {
        private readonly UserManager<MyUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole<long>> _roleManager;

        public MyAuthorizationManager(UserManager<MyUser> userManager,
            ApplicationDbContext context,
            RoleManager<IdentityRole<long>> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }
        public async Task UpdateUserToAdmin(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return;
            }
            if (!await _roleManager.RoleExistsAsync(RoleNames.Administrator))
            {
                await _roleManager.CreateAsync(new IdentityRole<long>(RoleNames.Administrator));
            }
            await _userManager.AddToRoleAsync(user, RoleNames.Administrator);
        }
        public async Task UpdateUserToManager(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return;
            }
            if (!await _roleManager.RoleExistsAsync(RoleNames.Manager))
            {
                await _roleManager.CreateAsync(new IdentityRole<long>(RoleNames.Manager));
            }
            await _userManager.AddToRoleAsync(user, RoleNames.Manager);
        }

    }
    public class IsOwnerAuthorizationHandler
        : AuthorizationHandler<OperationAuthorizationRequirement, ArticleView>
    {
        private readonly UserManager<MyUser> _userManager;
        public IsOwnerAuthorizationHandler(UserManager<MyUser> userManager)
        {
            _userManager = userManager;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, ArticleView resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }
            if (requirement.Name != RoleActionNames.OperationCreate
                && requirement.Name != RoleActionNames.OperationRead
                && requirement.Name != RoleActionNames.OperationUpdate
                && requirement.Name != RoleActionNames.OperationDelete)
            {
                return Task.CompletedTask;
            }
            if (resource.AuthorName == _userManager.GetUserName(context.User))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
    public class IsManagerAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, Article>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Article resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }
            if (requirement.Name != RoleActionNames.OperationReject &&
                requirement.Name != RoleActionNames.OperationApprove)
            {
                return Task.CompletedTask;
            }
            if (context.User.IsInRole(RoleNames.Manager))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
    public class IsAdminAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, Article>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Article resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }
            if (context.User.IsInRole(RoleNames.Administrator))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
    public class IsAdminCommentAuthorizationHandler
           : AuthorizationHandler<OperationAuthorizationRequirement, Comment>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Comment resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }
            if (context.User.IsInRole(RoleNames.Administrator))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
    public static class RoleNames
    {
        public const string Administrator = "admin";
        public const string Manager = "manager";
        public const string NomalUser = "nomal";
    }
    public static class RoleActions
    {
        public static OperationAuthorizationRequirement Create =
            new OperationAuthorizationRequirement() { Name = RoleActionNames.OperationCreate };
        public static OperationAuthorizationRequirement Update =
            new OperationAuthorizationRequirement() { Name = RoleActionNames.OperationUpdate };
        public static OperationAuthorizationRequirement Delete =
            new OperationAuthorizationRequirement() { Name = RoleActionNames.OperationDelete };
        public static OperationAuthorizationRequirement Read =
            new OperationAuthorizationRequirement() { Name = RoleActionNames.OperationRead };
        public static OperationAuthorizationRequirement Reject =
            new OperationAuthorizationRequirement() { Name = RoleActionNames.OperationReject };
        public static OperationAuthorizationRequirement Approve =
            new OperationAuthorizationRequirement() { Name = RoleActionNames.OperationApprove };
    }
    public static class RoleActionNames
    {
        public const string OperationRead = "read";
        public const string OperationEdit = "edit";
        public const string OperationUpdate = "update";
        public const string OperationDelete = "delete";
        public const string OperationApprove = "approve";
        public const string OperationReject = "reject";
        public const string OperationCreate = "create";
    }
}
