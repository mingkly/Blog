using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Services;
using MyWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.Articles
{
    public class FollowUserArticleModel : DI_Page
    {
        private readonly SecretProvider _secretProvider;
        private readonly ArticleServices _articleServices;
        public FollowUserArticleModel(UserManager<MyUser> userManager,
            ApplicationDbContext applicationDbContext,
            AppFileContext appFileContext,
            IAuthorizationService authorizationService,
            SecretProvider secretProvider,
            ArticleServices articleServices) : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
            _secretProvider = secretProvider;
            _articleServices = articleServices;
        }
        public static string RandNumber(string name)
        {
            return name + new Random().Next(100);
        }
        public IEnumerable<ArticleView> Articles { get; set; }
        public async Task OnGetAsync()
        {
            try
            {
                var user = await ApplicationDbContext
                .Users.SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user != null)
                {
                    Articles = await _articleServices.GetFollowUserArticles(10, user.UserName);
                    foreach (var article in Articles)
                    {
                        article.Id = _secretProvider.ProtectId(article.Id);
                    }
                    user.LastActivity.ArticleTime = DateTime.Now;
                    await ApplicationDbContext.SaveChangesAsync();
                }
            }
            catch
            {
                Articles = Array.Empty<ArticleView>();
                return;
            }

        }
    }
}
