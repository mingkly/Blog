using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Services;
using MyWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class MyArticlesModel : DI_Page
    {

        public List<Article> Articles { get; set; }
        public IEnumerable<ArticleView> ArticleViews { get; set; }
        private readonly SecretProvider _secretProvider;
        private readonly ArticleServices _articleServices;
        public MyArticlesModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext
            , AppFileContext appFileContext, IAuthorizationService authorizationService
            , SecretProvider secretProvider,
            ArticleServices articleServices)
            : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
            _secretProvider = secretProvider;
            _articleServices = articleServices;
        }
        [BindProperty(SupportsGet = true)]
        public bool Descending { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SortWord { get; set; }
        [BindProperty(SupportsGet = true)]
        public long Property { get; set; }
        public int PageCount { get; set; } = 10;
        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(SortWord))
            {
                SortWord = "time";
            }
            try
            {
                ArticleViews = await _articleServices
                    .GetArticleFromUser(1, PageCount, User.Identity.Name, SortWord, Descending);
            }
            catch
            {
                ArticleViews = Array.Empty<ArticleView>();
            }
        }

    }
}
