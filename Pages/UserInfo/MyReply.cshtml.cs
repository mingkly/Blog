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
    public class MyReplyModel : DI_Page
    {
        public IEnumerable<CommentView> Replys { get; set; }
        [BindProperty]
        public Comment Comment { get; set; }
        private readonly SecretProvider _secretProvider;

        public MyReplyModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext
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
                var user = await ApplicationDbContext.Users
                    .Select(u => new
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Replys = ApplicationDbContext.Comments
                              .Where(c => c.ReplyToArticle.AuthorId == u.Id ||
                              c.BelongToComment.UserId == u.Id ||
                              c.ReplyToComment.UserId == u.Id)
                              .OrderByDescending(y => y.Id).
                              Select(y => new CommentView
                              {
                                  ReplyToArticleId = _secretProvider.ProtectId(y.ReplyToArticleId),
                                  Id = _secretProvider.ProtectId(y.Id),
                                  Text = y.Text,
                                  Time = y.Time,
                                  UserName = y.User.UserName,
                                  NotReaded = y.Time > u.LastActivity.ReplyTime,
                                  BelongToCommentId = y.BelongToCommentId == null ? null : _secretProvider.ProtectId(y.BelongToCommentId.Value),
                              }).AsEnumerable()
                    }
                    )
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user != null)
                {
                    Replys = user.Replys;
                    var userUpdate = await ApplicationDbContext.Users.FindAsync(user.Id);
                    userUpdate.LastActivity.ReplyTime = DateTime.Now;
                    await ApplicationDbContext.SaveChangesAsync();
                }
            }
            catch
            {
                Replys = new List<CommentView>();
            }

        }
        public async Task<IActionResult> OnPostReplyAsync(long? id, long? belongToId)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            try
            {
                id = _secretProvider.UnProtectId(id.Value);
                if (belongToId != null)
                {
                    belongToId = _secretProvider.UnProtectId(belongToId.Value);
                }
            }
            catch
            {
                return BadRequest();
            }

            var comment = await ApplicationDbContext.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            var user = await UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            Comment.Time = DateTime.Now;
            Comment.ReplyToCommentId = id;
            Comment.ReplyToArticleId = comment.ReplyToArticleId;
            Comment.UserId = user.Id;
            Comment.BelongToCommentId = belongToId;
            try
            {
                await ApplicationDbContext.Comments.AddAsync(Comment);
                await ApplicationDbContext.SaveChangesAsync();
                return RedirectToPage();
            }
            catch
            {
                return BadRequest();
            }


        }
    }
    public class CommentReplyModel
    {
        public long CommentId { get; set; }
        public long ArticleId { get; set; }
        public long? BelongToId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public bool NotReaded { get; set; }
    }
}
