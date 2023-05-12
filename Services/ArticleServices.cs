using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyWeb.Services
{
    public class ArticleServices
    {
        private readonly ApplicationDbContext _appContext;
        private readonly SecretProvider _secretProvider;
        private readonly ILogger<ArticleServices> _logger;
        public ArticleServices(ApplicationDbContext context,
            SecretProvider secretProvider,
            ILogger<ArticleServices> logger)
        {
            _appContext = context;
            _secretProvider = secretProvider;
            _logger = logger;
        }
        public async Task<ArticleView> GetArticleById(long id)
        {
            var Id = _secretProvider.UnProtectId(id);
            var article = await _appContext.Articles
               .SelectArticleViewWithText()
               .SingleOrDefaultAsync(a => a.Id == Id);
            article.Id = _secretProvider.ProtectId(id);
            return article;
        }
        public async Task<ArticleView> GetArticleWithUserActionById(long id, ClaimsPrincipal userClaim)
        {
            var userId = (await _appContext.Users
                .Select(u => new { Id = u.Id, UserName = u.UserName })
                .FirstOrDefaultAsync(u => u.UserName == userClaim.Identity.Name))
                .Id;
            var Id = _secretProvider.UnProtectId(id);
            var article = await _appContext.Articles
               .SelectArticleViewWithAll(userId)
               .SingleOrDefaultAsync(a => a.Id == Id);
            article.Id = _secretProvider.ProtectId(id);
            return article;
        }
        public async Task<ArticleView> GetArticleWithAllById(long id, ClaimsPrincipal userClaim, Dictionary<CommentView, List<CommentView>> comments, int length, int innerLength)
        {
            var userId = (await _appContext.Users
                  .Select(u => new { Id = u.Id, UserName = u.UserName })
                  .FirstOrDefaultAsync(u => u.UserName == userClaim.Identity.Name))
                  ?.Id;
            var Id = _secretProvider.UnProtectId(id);
            var article = await _appContext.Articles
                 .Select(a => new
                 {
                     Id = a.Id,
                     CurrentUserAction = a.UserAction
                          .SingleOrDefault(u => u.UserId == userId),
                     Favourited = a.FavouriteUser
                          .SingleOrDefault(f => f.UserId == userId) == null ? false : true,
                     Title = a.Title,
                     Texts = a.Texts,
                     CreateTime = a.CreateTime,
                     AuthorName = a.Author.Name,
                     VoteUpNumber = a.VoteUpCount,
                     VoteDownNumber = a.VoteDownCount,
                     FavouriteCount = a.FavouriteCount,
                     ViewCount = a.ViewCount,
                     State = a.State,
                     IsPublic=a.IsPublic,
                     CurrentUserIsAuthor = a.AuthorId == userId,
                     Comments = a.Comments
                         .Where(c => c.ReplyToCommentId == null && c.BelongToCommentId == null)
                         .OrderByDescending(c => c.Id)
                         .Take(length)
                         .Select(c => new CommentView
                         {
                             Id = _secretProvider.ProtectId(c.Id),
                             Text = c.Text,
                             VoteUpCount = c.VoteUpCount,
                             VoteDownCount = c.VoteDownCount,
                             Time = c.Time,
                             UserName = c.User.Name,
                             UserAvatar = c.User.Avatar,
                             CommentCount = (int)c.CommentsCount,
                             ReplyToArticleId = _secretProvider.ProtectId(a.Id),
                             UserAction = c.UserActions
                                  .SingleOrDefault(c => c.UserId == userId),
                             InnerComments = c.UnderComments
                             .OrderByDescending(c => c.Id)
                                  .Take(innerLength)
                                  .Select(cc => new CommentView
                                  {
                                      Id = _secretProvider.ProtectId(cc.Id),
                                      Text = cc.Text,
                                      VoteUpCount = cc.VoteUpCount,
                                      VoteDownCount = cc.VoteDownCount,
                                      Time = cc.Time,
                                      UserName = cc.User.Name,
                                      UserAvatar = cc.User.Avatar,
                                      ReplyToArticleId = _secretProvider.ProtectId(a.Id),
                                      BelongToCommentId = _secretProvider.ProtectId(c.Id),
                                      ReplyToCommentId = cc.ReplyToCommentId == null ? null : _secretProvider.ProtectId(cc.ReplyToCommentId.Value),
                                      ReplyToCommentUserName = cc.ReplyToComment.User.Name,
                                      CommentCount = (int)cc.CommentsCount,
                                      UserAction = cc.UserActions
                                           .SingleOrDefault(c => c.UserId == userId),
                                  })
                                  .ToList()
                         })
                 })
                 .AsNoTracking()
                 .FirstOrDefaultAsync(m => m.Id == Id);
            var ArticleView = new ArticleView
            {
                Id = _secretProvider.ProtectId(article.Id),
                CurrentUserAction = article.CurrentUserAction is null ? 0 : article.CurrentUserAction.Action,
                Favourited = article.Favourited,
                Title = article.Title,
                Texts = article.Texts,
                CreateTime = article.CreateTime,
                AuthorName = article.AuthorName,
                VoteUpNumber = article.VoteUpNumber,
                VoteDownNumber = article.VoteDownNumber,
                FavouriteNumber = article.FavouriteCount,
                ViewCount = article.ViewCount,
                State = article.State,
                CurrentUserIsAuthor = article.CurrentUserIsAuthor,
                IsPublic= article.IsPublic,
            };
            foreach (var c in article.Comments)
            {
                comments.Add(c, c.InnerComments);
            }
            return ArticleView;
        }


        public async Task<List<ArticleView>> GetArticlesWithTextAndUserAction(int page, int pageSize, ClaimsPrincipal userClaim, string sortWord = "time", bool descending = false)
        {
            var userId = (await _appContext.Users
                .Select(u => new { Id = u.Id, UserName = u.UserName })
                .FirstOrDefaultAsync(u => u.UserName == userClaim.Identity.Name))
                .Id;
            var skipNumber = (page - 1) * pageSize;
            var articleViews = await _appContext.Articles
               .OrderBy(sortWord, descending)
               .SelectArticleViewWithAll(userId, _secretProvider)
               .Skip(skipNumber)
               .Take(pageSize)
               .ToListAsync();
            return articleViews;
        }
        public async Task<List<ArticleView>> GetArticlesWithUserAction(int page, int pageSize, ClaimsPrincipal userClaim, string sortWord = "time", bool descending = false)
        {
            var userId = (await _appContext.Users
                .Select(u => new { Id = u.Id, UserName = u.UserName })
                .FirstOrDefaultAsync(u => u.UserName == userClaim.Identity.Name))
                .Id;
            var skipNumber = (page - 1) * pageSize;
            var articleViews = await _appContext.Articles
               .OrderBy(sortWord, descending)
               .SelectArticleViewWithUserAction(userId, _secretProvider)
               .Skip(skipNumber)
               .Take(pageSize)
               .ToListAsync();
            return articleViews;
        }
        public async Task<List<ArticleView>> GetArticlesWithUserAction(int pageSize, ClaimsPrincipal userClaim, int pageJump, long property, string sortWord = "time", bool descending = false)
        {
            long p = property;
            if (sortWord == "time")
            {
                p = _secretProvider.UnProtectId(p);
            }
            var userId = (await _appContext.Users
                .Select(u => new { Id = u.Id, UserName = u.UserName })
                .FirstOrDefaultAsync(u => u.UserName == userClaim.Identity.Name))
                .Id;
            var articleViews = await _appContext.Articles
                .OrderByAndLimit(sortWord, pageJump > 0 ? 1 : -1, p, descending)
                .SelectArticleViewWithUserAction(userId, _secretProvider)
                .Take(pageSize)
                .ToListAsync();
            return articleViews;
        }
        public async Task<List<ArticleView>> GetArticlesWithTextAndUserAction(int pageSize, ClaimsPrincipal userClaim, int pageJump, long property, string sortWord = "time", bool descending = false)
        {
            long p = property;
            if (sortWord == "time")
            {
                p = _secretProvider.UnProtectId(p);
            }
            var userId = (await _appContext.Users
                .Select(u => new { Id = u.Id, UserName = u.UserName })
                .FirstOrDefaultAsync(u => u.UserName == userClaim.Identity.Name))
                .Id;
            var articleViews = await _appContext.Articles
                .OrderByAndLimit(sortWord, pageJump > 0 ? 1 : -1, p, descending)
                .SelectArticleViewWithAll(userId, _secretProvider)
                .Take(pageSize)
                .ToListAsync();
            return articleViews;
        }



        public async Task<List<ArticleView>> GetArticlesSimple(int page, int pageSize,ClaimsPrincipal user, string sortWord = "time", bool descending = false)
        {
            var skipNumber = (page - 1) * pageSize;
            if (user.IsInRole(RoleNames.Administrator))
            {
                var articleViews = await _appContext.Articles
                   .OrderBy(sortWord, descending)
                   .SelectArticleViewSimple(_secretProvider)
                   .Skip(skipNumber)
                   .Take(pageSize)
                   .ToListAsync();
                return articleViews;
            }
            else
            {
                var articleViews = await _appContext.Articles
                    .Where(a=>a.IsPublic)
                   .OrderBy(sortWord, descending)
                   .SelectArticleViewSimple(_secretProvider)
                   .Skip(skipNumber)
                   .Take(pageSize)
                   .ToListAsync();
                return articleViews;
            }

        }
        public async Task<List<ArticleView>> GetArticlesSimple(int pageSize, int pageJump, long property,ClaimsPrincipal user, string sortWord = "time", bool descending = false)
        {
            long p = property;
            if (sortWord == "time")
            {
                p = _secretProvider.UnProtectId(p);
            }
            if (user.IsInRole(RoleNames.Administrator))
            {
                var articleViews = await _appContext.Articles
                    .OrderByAndLimit(sortWord, pageJump > 0 ? 1 : -1, p, descending)
                    .SelectArticleViewSimple(_secretProvider)
                    .Take(pageSize)
                    .ToListAsync();
                return articleViews;
            }
            else
            {
                var articleViews = await _appContext.Articles
                    .Where(a => a.IsPublic)
                    .OrderByAndLimit(sortWord, pageJump > 0 ? 1 : -1, p, descending)
                    .SelectArticleViewSimple(_secretProvider)
                    .Take(pageSize)
                    .ToListAsync();
                return articleViews;
            }

        }
        public async Task<List<ArticleView>> GetArticleFromUser(int page, int pageSize, string userName, string sortWord = "time", bool descending = false)
        {
            var skipNumber = (page - 1) * pageSize;
            var articleViews = await _appContext.Articles
               .Where(a => a.Author.UserName == userName)
               .OrderBy(sortWord, descending)
               .SelectArticleViewSimple(_secretProvider)
               .Skip(skipNumber)
               .Take(pageSize)
               .ToListAsync();
            return articleViews;
        }
        public async Task<List<ArticleView>> GetUserFavouriteArticles(int pageSize, string userName, long property = long.MaxValue)
        {
            var userThings = await _appContext.Users
                .Select(u => new
                {
                    UserName = u.UserName,
                    Articles = u.Favourites
                    .Where(f => f.FavouriteArticleId != null)
                    .Where(f => f.FavouriteArticleId.Value < property)
                    .OrderByDescending(f => f.FavouriteArticleId.Value)
                    .Take(pageSize)
                    .Select(f => new ArticleView
                    {
                        Id = _secretProvider.ProtectId(f.FavouriteArticleId.Value),
                        CreateTime = f.FavouriteArticle.CreateTime,
                        Title = f.FavouriteArticle.Title,
                        Cover = FileHelpers.GetCoverPath(f.FavouriteArticle.Images),
                        ViewCount = f.FavouriteArticle.ViewCount,
                        State = f.FavouriteArticle.State,
                        VoteUpNumber = f.FavouriteArticle.VoteUpCount,
                        VoteDownNumber = f.FavouriteArticle.VoteDownCount,
                        AuthorName = f.FavouriteArticle.Author.Name
                    })
                })
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserName == userName);
            return userThings.Articles.ToList();
        }
        public async Task<List<ArticleView>> GetUserHistroyArticle(int pageSize, string userName, long property = long.MaxValue)
        {
            var user = await _appContext.Users
                .Select(u => new
                {
                    Id = u.Id,
                    UserName = u.UserName
                })
                .SingleOrDefaultAsync(u => u.UserName == userName);
            var articleView = await _appContext.UserViews
                .Where(u => u.UserId == user.Id && !u.Unlogin)
                .Select(v => v.ArticleId)
                .Join(
                _appContext.Articles.OrderByDescending(a => a.Id),
                a => a,
                b => b.Id,
                (a, article) => new ArticleView
                {
                    Id = article.Id,
                    CreateTime = article.CreateTime,
                    AuthorName = article.Author.UserName,
                    Title = article.Title,
                    Cover = FileHelpers.GetCoverPath(article.Images),
                    ViewCount = article.ViewCount,
                    State = article.State,
                    VoteUpNumber = article.VoteUpCount,
                    VoteDownNumber = article.VoteDownCount,
                })
                .Where(a => a.Id < property)
                .OrderByDescending(a => a.Id)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
            foreach (var a in articleView)
            {
                a.Id = _secretProvider.ProtectId(a.Id);
            }
            return articleView;
        }
        public async Task<List<ArticleView>> GetFollowUserArticles(int pageSize, string userName, long property = long.MaxValue)
        {
            var user = await _appContext
            .Users
            .Select(u => new
            {
                Id = u.Id,
                UserName = u.UserName,
                FollowUserArticles = u.Favourites
                .Where(f => f.FavouriteUserId != null)
                .SelectMany(f => f.FavouriteUser.Articles, (f, article) => new ArticleView
                {
                    Id = article.Id,
                    CreateTime = article.CreateTime,
                    AuthorName = article.Author.UserName,
                    Title = article.Title,
                    Texts = MingklyHelpers.RemoveTags(article.Texts, 100),
                    Cover = FileHelpers.GetCoverPath(article.Images),
                    ViewCount = article.ViewCount,
                    State = article.State,
                    VoteUpNumber = article.VoteUpCount,
                    VoteDownNumber = article.VoteDownCount,
                    UserAction = article.UserAction.SingleOrDefault(a => a.UserId == u.Id),
                    NotReaded = article.CreateTime > u.LastActivity.ArticleTime,
                })
                .Where(a => a.Id < property)
                .OrderByDescending(a => a.Id)
                .Take(pageSize)
                .ToList()
            })
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserName == userName);
            if (user != null)
            {
                return user.FollowUserArticles;
            }
            return null;
        }
        public async Task<List<ArticleView>> GetArticlesFromSearch(string keyword,int page, int pageSize, ClaimsPrincipal user, string sortWord = "time", bool descending = false)
        {
            var skipNumber = (page - 1) * pageSize;
            if (user.IsInRole(RoleNames.Administrator))
            {
                var articleViews = await _appContext.Articles
                    .Where(a=>a.Title.Contains(keyword)||a.Texts.Contains(keyword))
                   .OrderBy(sortWord, descending)
                   .SelectArticleViewSimple(_secretProvider)
                   .Skip(skipNumber)
                   .Take(pageSize)
                   .ToListAsync();
                return articleViews;
            }
            else
            {
                var articleViews = await _appContext.Articles
                    .Where(a => a.IsPublic)
                     .Where(a => a.Title.Contains(keyword) || a.Texts.Contains(keyword))
                   .OrderBy(sortWord, descending)
                   .SelectArticleViewSimple(_secretProvider)
                   .Skip(skipNumber)
                   .Take(pageSize)
                   .ToListAsync();
                return articleViews;
            }
        }


        public async Task<List<CommentView>> LoadMoreArticleComments(int length, int innerLength, long property, long articleId, string userName)
        {
            articleId = _secretProvider.UnProtectId(articleId);
            property = _secretProvider.UnProtectId(property);
            var userId = (await _appContext.Users.Select(u => new
            {
                Id = u.Id,
                UserName = u.UserName
            }).SingleOrDefaultAsync(u => u.UserName == userName)).Id;
            var commentViews = await _appContext.Comments
                .Where(a => a.ReplyToArticleId == articleId)
                .Where(a => a.BelongToCommentId == null)
                .Where(a => a.Id < property)
                .OrderByDescending(a => a.Id)
                .SelectCommentViewWithUserAction(userId, innerLength, _secretProvider)
                .Take(length)
                .AsNoTracking()
                .ToListAsync();
            return commentViews;
        }
        public async Task<List<CommentView>> LoadMoreInnerComments(int length, long property, long commentId, string userName)
        {
            commentId=_secretProvider.UnProtectId(commentId);
            property=_secretProvider.UnProtectId(property);
            var userId = (await _appContext.Users.Select(u => new
            {
                Id = u.Id,
                UserName = u.UserName
            }).SingleOrDefaultAsync(u => u.UserName == userName)).Id;
            var comments = await _appContext
                .Comments
                .Select(c => new
                {
                    Id = c.Id,
                    InnerComments = c.UnderComments
                                  .Where(cc=>cc.Id<property)
                                  .OrderByDescending(c => c.Id)
                                  .Take(length)
                                  .Select(cc => new CommentView
                                  {
                                      Id = _secretProvider.ProtectId(cc.Id),
                                      Text = cc.Text,
                                      VoteUpCount = cc.VoteUpCount,
                                      VoteDownCount = cc.VoteDownCount,
                                      Time = cc.Time,
                                      UserName = cc.User.Name,
                                      UserAvatar = cc.User.Avatar,
                                      ReplyToArticleId = _secretProvider.ProtectId(c.ReplyToArticleId),
                                      BelongToCommentId = _secretProvider.ProtectId(c.Id),
                                      ReplyToCommentId = cc.ReplyToCommentId == null ? null : _secretProvider.ProtectId(cc.ReplyToCommentId.Value),
                                      ReplyToCommentUserName = cc.ReplyToComment.User.Name,
                                      CommentCount = (int)cc.CommentsCount,
                                      UserAction = cc.UserActions
                                           .SingleOrDefault(c => c.UserId == userId),
                                  })
                                  .ToList()
                })
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == commentId);
            return comments.InnerComments;
        }

        public async Task<List<CommentView>> GetCommentcFromUser(int pageSize,string userName,long property=long.MaxValue)
        {
            var comments = await _appContext
                .Comments
                .Where(c => c.User.UserName == userName)
                .Where(c=>c.Id<property)
                .Take(pageSize)
                .AsNoTracking()
                .SelectCommentViewSimple(_secretProvider)
                .ToListAsync();
            return comments;
        }
        public async Task<List<CommentView>> GetCommentcFromUserReply(int pageSize, string userName, long property = long.MaxValue)
        {
            var comments = await _appContext
                .Comments
                .Where(c => c.ReplyToCommentId!=null&&c.ReplyToComment.User.UserName == userName)
                .Where(c => c.Id < property)
                .Take(pageSize)
                .AsNoTracking()
                .SelectCommentViewSimple(_secretProvider)
                .ToListAsync();
            return comments;
        }


    }
}
