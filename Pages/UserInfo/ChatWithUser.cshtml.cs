using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class ChatWithUserModel : DI_Page
    {
        public ChatWithUserModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext, AppFileContext appFileContext, IAuthorizationService authorizationService) : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
        }
        public List<Messages> Msgs { get; set; }
        public UserView TalkTo { get; set; }
        public UserView Me { get; set; }
        [BindProperty]
        public Message MessageSend { get; set; }
        public record UserView(string UserName, string Avatar);

        public record Messages(bool Send, string message, string Iamge, DateTime Time);
        public record Message([Required] string Reciever, string message, string Iamge);
        public async Task<IActionResult> OnGetAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return NotFound();
            }
            try
            {
                var anotherUser = await UserManager.FindByNameAsync(name);
                var user = await ApplicationDbContext.Users
                    .Select(u => new
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Avatar = u.Avatar,
                        Messages = u.RecievedMessages.Where(m => m.SenderId == anotherUser.Id)
                        .Union(u.ChatMessages.Where(m => m.RecieverId == anotherUser.Id))
                        .OrderBy(m => m.Id)
                        .Take(100)
                        .Select(m => new Messages(
                           m.SenderId == u.Id,
                           m.Message,
                           m.Image,
                           m.Time
                            ))
                    })
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user == null || anotherUser == null)
                {
                    return NotFound();
                }
                var chat_1 = await ApplicationDbContext
                   .ChatInfos
                   .SingleOrDefaultAsync(c => c.UserId == user.Id && c.ChatToUserId == anotherUser.Id);
                if (chat_1 != null)
                {
                    chat_1.LastChatTime = DateTime.Now;
                }
                await ApplicationDbContext.SaveChangesAsync();
                TalkTo = new UserView(anotherUser.UserName, anotherUser.Avatar);
                Me = new UserView(user.UserName, user.Avatar);
                Msgs = user.Messages.ToList();

            }
            catch
            {
                Msgs = new List<Messages>();
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || MessageSend is null)
            {
                return Page();
            }

            var user = await UserManager.GetUserAsync(User);
            var anotherUser = await UserManager.FindByNameAsync(MessageSend.Reciever);
            if (user == null || anotherUser == null)
            {
                return NotFound();
            }
            var chat_1 = await ApplicationDbContext
               .ChatInfos
               .SingleOrDefaultAsync(c => c.UserId == user.Id && c.ChatToUserId == anotherUser.Id);
            if (chat_1 != null)
            {
                chat_1.LastChatTime = DateTime.Now;
            }
            else
            {
                var chat1 = new RecentChat
                {
                    UserId = user.Id,
                    ChatToUserId = anotherUser.Id,
                    LastChatTime = DateTime.Now,
                };
                await ApplicationDbContext.ChatInfos.AddRangeAsync(chat1);
            }
            var chat_2 = await ApplicationDbContext
                   .ChatInfos
                   .SingleOrDefaultAsync(c => c.UserId == anotherUser.Id && c.ChatToUserId == user.Id);
            if (chat_2 != null)
            {

            }
            else
            {
                var chat2 = new RecentChat
                {
                    UserId = anotherUser.Id,
                    ChatToUserId = user.Id,
                    LastChatTime = DateTime.Now,
                };
                await ApplicationDbContext.ChatInfos.AddRangeAsync(chat2);
            }

            var msg = new ChatMessage
            {
                SenderId = user.Id,
                RecieverId = anotherUser.Id,
                Time = DateTime.Now,
                Message = MessageSend.message,
                Image = MessageSend.Iamge,
            };

            await ApplicationDbContext.ChatMessages.AddAsync(msg);
            await ApplicationDbContext.SaveChangesAsync();
            return RedirectToPage(new
            {
                name = MessageSend.Reciever
            });
        }
    }
}
