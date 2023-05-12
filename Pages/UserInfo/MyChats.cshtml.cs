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
    public class MyChatsModel : DI_Page
    {
        public MyChatsModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext, AppFileContext appFileContext, IAuthorizationService authorizationService) : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
        }

        public List<RecentChatView> Chats { get; set; }
        public record RecentChatView(string firstMsg, string userName, string Avatar, DateTime LastTime);
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var user = await ApplicationDbContext.Users
                    .Select(u => new
                    {
                        UserName = u.UserName,
                        RecentChats = u.RecentChats.Select(
                            s => new
                            {
                                chat = s,
                                firstMsg = u.RecievedMessages.Where(m => m.SenderId == s.ChatToUserId)
                              .Union(u.ChatMessages.Where(m => m.RecieverId == s.ChatToUserId))
                              .OrderByDescending(m => m.Time).FirstOrDefault()
                            })
                        .Select(s => new RecentChatView(
                               s.firstMsg.Message,
                              s.chat.ChatToUser.UserName,
                              s.firstMsg.Sender.UserName == u.UserName ? s.firstMsg.Reciever.Avatar : s.firstMsg.Sender.Avatar
                              ,
                              s.chat.LastChatTime
                              ))
                    })
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user != null)
                {
                    Chats = user.RecentChats.ToList();
                }
            }
            catch
            {
                Chats = new List<RecentChatView>();
            }

            return Page();
        }
    }
}
