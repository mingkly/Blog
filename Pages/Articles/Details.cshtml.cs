using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MyWeb.Pages
{
    [AllowAnonymous]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly SecretProvider _secretProvider;
        private readonly UnloginUserManager _unloginUserManager;
        private readonly IAuthorizationService _service;
        private readonly ArticleServices _articleServices;
        public DetailsModel(ApplicationDbContext context,
            UserManager<MyUser> userManager,
            SecretProvider secretProvider,
            UnloginUserManager unloginUserManager,
            IAuthorizationService service,
            ArticleServices articleServices)
        {
            _context = context;
            _userManager = userManager;
            _secretProvider = secretProvider;
            _unloginUserManager = unloginUserManager;
            _service = service;
            _articleServices = articleServices;
        }

        public ArticleView ArticleView { get; set; }

        public Dictionary<CommentView, List<CommentView>> InnerComments { get; set; }
        [BindProperty]
        public Comment Comment { get; set; }
        public async Task<IActionResult> OnGetAsync(long id)
        {
            try
            {
                
                var Id = _secretProvider.UnProtectId(id);
                var loginUser = await _userManager.GetUserAsync(User);
                InnerComments = new Dictionary<CommentView, List<CommentView>>();
                ArticleView = await _articleServices.GetArticleWithAllById(id, User, InnerComments,3,3);

                if (ArticleView == null)
                {
                    return NotFound();
                }
                if(loginUser == null && !ArticleView.IsPublic)
                {
                    return NotFound();
                }
                if (!User.IsInRole(RoleNames.Administrator) && (ArticleView.State == ArticleState.Submit || ArticleView.State == ArticleState.Rejected))
                {
                    return Forbid();
                }
                if (loginUser != null)
                {
                    var view = await _context.UserViews.SingleOrDefaultAsync(a => a.UserId == loginUser.Id && a.ArticleId == Id );
                    if (view == null)
                    {
                        (await _context.Articles.FindAsync(Id)).ViewCount++;
                        var userView = new UserView()
                        {
                            UserId = loginUser.Id,
                            ArticleId = Id,
                            Unlogin = false,
                        };
                        await _context.UserViews.AddAsync(userView);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (CryptographicException)
            {
                return NotFound();
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(long? articleId, long? commentId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            Comment comment =null;
            var user = await _userManager.GetUserAsync(User);
            if (articleId is not null)
            {
                articleId = _secretProvider.UnProtectId(articleId.Value);
                comment = new Comment
                {
                    UserId = user.Id,
                    Text = Comment.Text,
                    ReplyToArticleId = articleId.Value,
                    Time = DateTime.Now
                };
            }           
            else if (commentId != null)
            {
                commentId = _secretProvider.UnProtectId(commentId.Value);
                var replyComment=await _context.Comments.FindAsync(commentId.Value);
                comment = new Comment
                {
                    UserId = user.Id,
                    Text = Comment.Text,
                    ReplyToArticleId = replyComment.ReplyToArticleId,
                    ReplyToCommentId = commentId,
                    Time = DateTime.Now
                };
                if (replyComment.BelongToCommentId != null)
                {
                    comment.BelongToCommentId= replyComment.BelongToCommentId.Value;
                }
                else
                {
                    comment.BelongToCommentId = replyComment.Id;
                }
                articleId = replyComment.ReplyToArticleId;
            }
            else
            {
                return NotFound();
            }

            try
            {
                if (comment != null)
                {
                    await _context.Comments.AddAsync(comment);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
            //return Page();
            return RedirectToPage(new { id = _secretProvider.ProtectId(articleId.Value) });
        }
        public async Task<IActionResult> OnPostReplyUserAsync()
        {

            var user = await _userManager.GetUserAsync(User);
            var commnet = new Comment
            {
                UserId = user.Id,

            };
            await _context.Comments.AddAsync(commnet);
            await _context.SaveChangesAsync();
            return Page();
        }
        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            try
            {
                id = _secretProvider.UnProtectId(id);
                var article = await _context.Articles.FindAsync(id);
                if (article == null)
                {
                    return NotFound();
                }
                OperationAuthorizationRequirement requirement = RoleActions.Delete;
                var isAuthorized = await _service.AuthorizeAsync(User, article, requirement);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
                return RedirectToPage("index");
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}

