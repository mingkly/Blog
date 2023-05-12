using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class UserArticlesModel : DI_Page
    {
        public IEnumerable<ArticleView> ArticleViews { get; set; }
        private readonly SecretProvider _secretProvider;
        public UserArticlesModel(UserManager<Models.MyUser> userManager, ApplicationDbContext applicationDbContext, AppFileContext appFileContext, IAuthorizationService authorizationService, SecretProvider secretProvider)
            : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
            _secretProvider = secretProvider;
        }

        public async Task OnGetAsync(string userName)
        {
            try
            {
                var anoherUser = await ApplicationDbContext.Users.SingleOrDefaultAsync(u => u.UserName == userName);
                if (anoherUser == null)
                {
                    ArticleViews = new List<ArticleView>();
                    return;
                }
                var user = await UserManager.GetUserAsync(User);
                if (user != null)
                {
                    var articles = ApplicationDbContext.Articles
                        .Where(a => a.AuthorId == anoherUser.Id);

                    articles = MingklyHelpers.SortArticle(articles, "time", false);
                    var articleViews = await articles.Select(article => new ArticleView
                    {
                        Id = _secretProvider.ProtectId(article.Id),
                        CreateTime = article.CreateTime,
                        AuthorName = article.Author.UserName,
                        Title = article.Title,
                        Texts = MingklyHelpers.RemoveTags(article.Texts, 100),
                        Cover = FileHelpers.GetCoverPath(article.Images),
                        ViewCount = article.ViewCount,
                        State = article.State,
                        VoteUpNumber = article.VoteUpCount,
                        VoteDownNumber = article.VoteDownCount,
                        UserAction = article.UserAction.SingleOrDefault(a => a.UserId == user.Id),
                    })
                        .Take(10)
                        .AsNoTracking()
                        .ToListAsync(); ;
                    ArticleViews = articleViews;
                }
            }
            catch
            {
                ArticleViews = new List<ArticleView>();
            }

        }
    }
}
