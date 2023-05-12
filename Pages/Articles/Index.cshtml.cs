using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Services;
using MyWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly SecretProvider _secretProvider;
        private readonly IAuthorizationService _service;
        private readonly ArticleServices _articleServices;
        public IndexModel(ApplicationDbContext context, UserManager<MyUser> userManager,
            SecretProvider secretProvider,
            IAuthorizationService service,
            ArticleServices articleServices)
        {
            _context = context;
            _userManager = userManager;
            _secretProvider = secretProvider;
            _service = service;
            _articleServices = articleServices;
        }
        public bool Descending { get; set; }
        public string SortWord { get; set; }
        public IList<Article> Article { get; set; }
        public IEnumerable<ArticleView> ArticleViews { get; set; }
        [BindProperty]
        public long? ArticleId { get; set; }
        [BindProperty]
        public int? ActionId { get; set; }
        public string CommentText { get; set; }
        public int PageCount { get; set; } = 10;
        public int CurrentPageNumber { get; set; }
        public int MaxPageNumber { get; set; }
        public async Task OnGetAsync(int? pageNumber, string sortWord, bool? descending, int? pageJump, long? property)
        {
            if (pageNumber == null)
            {
                pageNumber = 1;
            }
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            pageJump = pageJump ?? 0;
            SortWord = String.IsNullOrEmpty(sortWord) ? "time" : sortWord;
            Descending = descending is null ? false : descending.Value;

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole(RoleNames.Administrator))
            {
                MaxPageNumber = await _context.Articles.CountAsync();
            }
            else
            {
                MaxPageNumber = await _context.Articles.Where(a => a.IsPublic).CountAsync();
            }

            if (MaxPageNumber % PageCount == 0)
            {
                MaxPageNumber = MaxPageNumber / PageCount;
            }
            else
            {
                MaxPageNumber = MaxPageNumber / PageCount + 1;
            }
            long userId = user is null ? -1 : user.Id;
            try
            {
                if (pageJump.Value == 0)
                {
                    if (pageNumber > MaxPageNumber)
                    {
                        pageNumber = MaxPageNumber;
                    }
                    CurrentPageNumber = pageNumber.Value;
                    ArticleViews = await _articleServices.GetArticlesSimple(pageNumber.Value, PageCount,User, sortWord, Descending);
                }
                else
                {
                    long p = property ?? -1;
                    if (sortWord == "time")
                    {
                        p = _secretProvider.UnProtectId(p);
                    }
                    CurrentPageNumber = pageNumber.Value + (pageJump.Value > 0 ? 1 : -1);
                    ArticleViews = await _articleServices.GetArticlesSimple( PageCount,pageJump.Value,p,User, sortWord, Descending);
                }
            }
            catch
            {
                ArticleViews = new List<ArticleView>();
                return;
            }
        }
        public async Task<IActionResult> OnPostApproveAsync()
        {
            var id = ArticleId;
            var action = ActionId;
            if (id is null || action == null)
            {
                return NotFound();
            }
            if (action < 0 || action > 2)
            {
                return NotFound();
            }
            id = _secretProvider.UnProtectId(id.Value);
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            OperationAuthorizationRequirement requirement;
            if (action == 0)
            {
                requirement = RoleActions.Approve;
            }
            else if (action == 1)
            {
                requirement = RoleActions.Reject;
            }
            else
            {
                requirement = RoleActions.Delete;
            }
            var isAuthorized = await _service.AuthorizeAsync(User, article, requirement);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            if (action == 0)
            {
                article.State = ArticleState.Approved;
            }
            else if (action == 1)
            {
                article.State = ArticleState.Rejected;
            }
            else
            {
                _context.Articles.Remove(article);
            }
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }


    }


}
