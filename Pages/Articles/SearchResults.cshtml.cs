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
    [AllowAnonymous]
    public class SearchResultsModel : DI_Page
    {
        private readonly SecretProvider _secretProvider;
        private readonly ArticleServices _articleServices;
        public SearchResultsModel(UserManager<MyUser> userManager,
            ApplicationDbContext applicationDbContext,
            AppFileContext appFileContext,
            IAuthorizationService authorizationService,
            SecretProvider secretProvide,
            ArticleServices articleServices) : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
            _secretProvider = secretProvide;
            _articleServices = articleServices;
        }
        public int PageCount { get; set; } = 1;
        public int CurrentPage { get; set; }
        public int MaxPage { get; set; }
        public string Word { get; set; }
        public IEnumerable<ArticleView> Articles { get; set; }
        public async Task OnGetAsync(string word, int? pageNumber, int? pageAction, long? number, bool? usingKeyword)
        {
            Word = word;
            IQueryable<Article> articleQuery = ApplicationDbContext.Articles
                
                .Where(a => a.Title.Contains(word) || a.Texts.Contains(word));

            if (User.IsInRole(RoleNames.Administrator))
            {
                MaxPage = await articleQuery.CountAsync();
            }
            else
            {
                MaxPage = await articleQuery.Where(a => a.IsPublic).CountAsync();
            }
            if (MaxPage == 0)
            {
                Articles = Array.Empty<ArticleView>();
                return;
            }
            if (MaxPage % PageCount == 0)
            {
                MaxPage = MaxPage / PageCount;
            }
            else
            {
                MaxPage = MaxPage / PageCount + 1;
            }
            pageNumber = pageNumber ?? 1;
            pageNumber = Math.Clamp(pageNumber.Value, 1, MaxPage);
            int skipNumber = 0;
            if (pageAction == null)
            {
                skipNumber = (pageNumber.Value - 1) * PageCount;
                CurrentPage = pageNumber.Value;
                articleQuery = articleQuery.OrderByDescending(a => a.Id);
            }
            else
            {
                pageAction = pageAction.Value >= 0 ? 1 : -1;

                skipNumber = 0;
                if (!(pageNumber == 1 && pageAction < 0) && !(pageNumber == MaxPage && pageAction > 0))
                {
                    long p;
                    if (number == null)
                    {
                        return;
                    }
                    try
                    {
                        p = _secretProvider.UnProtectId(number.Value);
                    }
                    catch
                    {
                        return;
                    }
                    if (pageAction > 0)
                    {
                        articleQuery = articleQuery.OrderByDescending(a => a.Id)
                            .Where(a => a.Id < p);
                    }
                    else
                    {
                        articleQuery = articleQuery.OrderBy(a => a.Id)
                            .Where(a => a.Id > p);
                    }
                }
                else
                {
                    articleQuery = articleQuery.OrderByDescending(a => a.Id);
                }
                CurrentPage = Math.Clamp(pageNumber.Value + pageAction.Value, 1, MaxPage);
            }
            if (word.Length > 100)
            {
                word = word.Substring(0, 100);
            }
            try
            {
                var articles = await _articleServices.GetArticlesFromSearch(word,
                    pageNumber.Value, PageCount,
                    User);
                Articles = articles;
                usingKeyword = usingKeyword ?? false;
                if (!usingKeyword.Value)
                {
                    try
                    {
                        await ApplicationDbContext.SearchRecords.AddAsync(new SearchRecord
                        {
                            Word = word,
                        });
                        await ApplicationDbContext.SaveChangesAsync();
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {
                Articles = Array.Empty<ArticleView>();
            }

        }
    }
}
