using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Services;
using MyWeb.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class MyFavouriteModel : DI_Page
    {

        public List<Article> MyFavourite { get; set; }
        public IEnumerable<ArticleView> ArticleViews { get; set; }
        private readonly SecretProvider _secretProvider;
        private readonly ArticleServices _articleServices;
        public MyFavouriteModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext
            , AppFileContext appFileContext, IAuthorizationService authorizationService
            , SecretProvider secretProvider,
            ArticleServices articleServices)
            : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
            _secretProvider = secretProvider;
            _articleServices = articleServices;
        }
        public async Task OnGet()
        {
            try
            {
                ArticleViews = await _articleServices
                    .GetUserFavouriteArticles(10, User.Identity.Name);

            }
            catch
            {
                ArticleViews = new List<ArticleView>();
            }

        }
    }
}
