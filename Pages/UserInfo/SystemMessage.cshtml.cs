using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class SystemMessageModel : DI_Page
    {
        public SystemMessageModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext, AppFileContext appFileContext, IAuthorizationService authorizationService) : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
        }
        public List<SystemNotify> Notifies { get; set; }
        public DateTime Time { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var user = await ApplicationDbContext.Users
                    .Include(u => u.SystemNotify.OrderByDescending(s => s.Id))
                    .SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user == null)
                {
                    return Forbid();
                }
                Time = user.LastActivity.NotifyTime;
                Notifies = user.SystemNotify;
                user.LastActivity.NotifyTime = DateTime.Now;
                await ApplicationDbContext.SaveChangesAsync();
            }
            catch
            {
                Notifies = new List<SystemNotify>();
            }
            return Page();
        }
    }
}
