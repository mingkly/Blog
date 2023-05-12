using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MyWeb.Pages
{
    public class EditModel : DI_Page
    {
        private readonly string[] _permittedTag;
        private readonly string[] _permittedAttr;
        private readonly string[] _permittedHost;
        private readonly string _targetStoredPath;
        private readonly SecretProvider _secretProvider;
        public EditModel(ApplicationDbContext context, UserManager<MyUser> userManager
            , IConfiguration iconfig
            , AppFileContext fileContext
            , IAuthorizationService service
            , SecretProvider secretProvider)
            : base(userManager, context, fileContext, service)
        {
            _permittedAttr = iconfig.GetSection("PermittedAttr").Get<string[]>();
            _permittedTag = iconfig.GetSection("PermittedTag").Get<string[]>();
            _permittedHost = iconfig.GetSection("PermittedHost").Get<string[]>();
            _targetStoredPath = iconfig.GetValue<string>("StoredFilePath");
            _secretProvider = secretProvider;
        }


        public Article Article { get; set; }
        [BindProperty]
        public ArticleUpdate ArticleToUpdate { get; set; }

        public class ArticleUpdate
        {
            [Required]
            public long Id { get; set; }
            [Required]
            [MaxLength(150)]
            public string Title { get; set; }
            [Required]
            [MinLength(50)]
            public string Texts { get; set; }
            public bool IsPublic { get; set; }
        }
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            long realId = -1;
            try
            {
                realId = _secretProvider.UnProtectId(id.Value);
            }
            catch (Exception)
            {
                return BadRequest();
            }
            Article = await ApplicationDbContext.Articles
                .FirstOrDefaultAsync(m => m.Id == realId);
            if (Article == null)
            {
                return NotFound();
            }
            OperationAuthorizationRequirement requirement = RoleActions.Update;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, Article, requirement);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            Article.Id = _secretProvider.ProtectId(Article.Id);
            ArticleToUpdate = new ArticleUpdate
            {
                Id = Article.Id,
                Texts = Article.Texts,
                Title = Article.Title,
            };
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                ArticleToUpdate.Id = _secretProvider.UnProtectId(ArticleToUpdate.Id);
            }
            catch
            {
                return NotFound();
            }
            Article = await ApplicationDbContext.Articles.FindAsync(ArticleToUpdate.Id);
            if (Article == null)
            {
                return NotFound();
            }
            var user = await UserManager.GetUserAsync(User);
            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, Article, RoleActions.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            var text = MingklyHelpers.TagFiliter(ArticleToUpdate.Texts, _permittedTag, _permittedAttr);
            Article.State = ArticleState.Submit;
            Article.Title = WebUtility.HtmlDecode(ArticleToUpdate.Title);
            Article.IsPublic=ArticleToUpdate.IsPublic;
            try
            {
                text = MingklyHelpers.ProcessImageSrc(text
                    , _targetStoredPath.Replace("images", "")
                    , _targetStoredPath + "\\" + _secretProvider.ProtectId(Article.Id)
                    , out var paths);
                text = MingklyHelpers.ProcessHref(text, _permittedHost.ToList());
                Article.Texts = text;
                Article.Images = String.Join(";", paths);
                await ApplicationDbContext.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("error", "something wwrong");
                return Page();
            }

        }


    }
}
