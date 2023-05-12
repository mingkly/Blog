using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.Articles
{
    [Authorize(Roles = RoleNames.Administrator)]
    public class ExamineTargetArticleModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly SecretProvider _secretProvider;
        public ExamineTargetArticleModel(ApplicationDbContext context,
            UserManager<MyUser> userManager,
            SecretProvider secretProvider)
        {
            _context = context;
            _userManager = userManager;
            _secretProvider = secretProvider;
        }
        public ArticleView ArticleView { get; set; }
        public async Task OnGetAsync(long id)
        {
            try
            {
                var Id = _secretProvider.UnProtectId(id);
                var article = await _context.Articles
                              .Select(a => new ArticleView
                              {
                                  Id = a.Id,
                                  Title = a.Title,
                                  Texts = a.Texts,
                                  Cover = FileHelpers.GetCoverPath(a.Images),
                                  CreateTime = a.CreateTime,
                                  AuthorName = a.Author.Name,
                                  State = a.State,
                              })
                              .AsNoTracking()
                              .SingleOrDefaultAsync(a => a.Id == Id);
                if (article == null)
                {
                    return;
                }
                article.Id = _secretProvider.ProtectId(Id);
                ArticleView = article;
            }
            catch
            {

            }
        }
    }
}
