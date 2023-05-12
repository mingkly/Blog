using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class MyCommentsModel : DI_Page
    {
        public IEnumerable<CommentView> Comments { get; set; }
        private readonly SecretProvider _secretProvider;
        public MyCommentsModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext
            , AppFileContext appFileContext, IAuthorizationService authorizationService
            , SecretProvider secretProvider)
            : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
            _secretProvider = secretProvider;
        }
        public async Task OnGet()
        {
            try
            {
                var user = await ApplicationDbContext.Users.SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user != null)
                {
                    Comments = await ApplicationDbContext.Comments
                        .Where(b => b.UserId == user.Id)
                        .Take(10)
                        .Select(c => new CommentView
                        {
                            ReplyToArticleId = _secretProvider.ProtectId(c.ReplyToArticleId),
                            ReplyToCommentUserName = c.ReplyToCommentId == null ? "" : c.ReplyToComment.User.UserName,
                            Text = c.Text,
                            Time = c.Time,
                        })
                    .AsNoTracking()
                    .ToArrayAsync();
                }
            }
            catch
            {
                Comments = Array.Empty<CommentView>();
            }
        }
    }

}
