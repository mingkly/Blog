using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Services;
using MyWeb.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MyWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly SecretProvider _secretProvider;
        private readonly UnloginUserManager _unloginUserManager;
        private readonly IAuthorizationService _service;
        private readonly string[] _permittedExtensions;
        private readonly long _fileSizeLimit;
        private readonly string _targetStoredPath;
        private readonly FormOptions _formOptions = new FormOptions();
        private readonly ArticleServices _articleServices;
        public ArticlesController(ApplicationDbContext context,
            UserManager<MyUser> userManager,
            SecretProvider secretProvider,
            UnloginUserManager unloginUserManager,
            IAuthorizationService service,
            IConfiguration iconfig,
            ArticleServices articleServices)
        {
            _context = context;
            _userManager = userManager;
            _secretProvider = secretProvider;
            _unloginUserManager = unloginUserManager;
            _service = service;
            _permittedExtensions = iconfig.GetSection("PermittedExtensions").Get<string[]>();
            _fileSizeLimit = iconfig.GetValue<long>("FileSizeLimit");
            _targetStoredPath = iconfig.GetValue<string>("StoredFilePath");
            _articleServices = articleServices;
        }
        [HttpPost(nameof(VoteArticle))]
        public async Task<JsonResult> VoteArticle([FromBody] UserAction userAction)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }
            if (userAction == null)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }
            if (userAction.Action > 1 || userAction.Action < -1)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }
            var user = await _userManager.GetUserAsync(User);
            try
            {
                var unprotectId = _secretProvider.UnProtectId(userAction.Id);
                var id = unprotectId;
                var userActionExist = await _context.CommentUsers
                 .SingleOrDefaultAsync(c => c.ArticleId == id && user.Id == c.UserId);
                if (userActionExist != null && userActionExist.Action != 0)
                {
                    return new JsonResult(new
                    {
                        id = "doned",
                        msg = "已经赞过"
                    });
                }
                var ArticleAction = new CommentUser
                {
                    ArticleId = id,
                    Action = userAction.Action,
                    UserId = user.Id
                };
                var article = await _context.Articles.FindAsync(id);
                if (userAction.Action > 0)
                {
                    article.VoteUpCount++;
                }
                else if (userAction.Action < 0)
                {
                    article.VoteDownCount++;
                }

                await _context.CommentUsers.AddAsync(ArticleAction);
                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    id = "suc",
                    msg = "成功"
                });
            }
            catch (CryptographicException)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }
            catch (Exception)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }
        }
        [HttpPost(nameof(FavouriteArticle))]
        public async Task<JsonResult> FavouriteArticle([FromBody] FavouriteArticleId id)
        {
            if (id == null)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var realId = _secretProvider.UnProtectId(id.Id);
                var article = await _context.Articles.FindAsync(realId);
                if (user == null || article == null)
                {
                    return new JsonResult(new
                    {
                        id = "none",
                        msg = "无此文章"
                    });
                }
                var favourite = await _context.UserFavourites.SingleOrDefaultAsync(f => f.FavouriteArticleId == realId && f.UserId == user.Id);
                if (favourite != null)
                {
                    _context.UserFavourites.Remove(favourite);
                    article.FavouriteCount--;
                    await _context.SaveChangesAsync();
                    return new JsonResult(new
                    {
                        id = 0
                    });
                }
                var userFavourite = new UserFavourite
                {
                    FavouriteArticleId = realId,
                    UserId = user.Id
                };
                await _context.UserFavourites.AddAsync(userFavourite);
                article.FavouriteCount++;
                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    id = 1
                });
            }
            catch
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }
        }
        [HttpPost(nameof(ExamineArticle))]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<JsonResult> ExamineArticle([FromBody] UserAction userAction)
        {
            var id = userAction.Id;
            var action = userAction.Action;
            if (action < 0 || action > 2)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误动作"
                });
            }
            try
            {
                id = _secretProvider.UnProtectId(id);
                var article = await _context.Articles.FindAsync(id);
                if (article == null)
                {
                    return new JsonResult(new
                    {
                        id = "forbid",
                        msg = "无此文章"
                    });
                }
                int examineAction = 0;
                OperationAuthorizationRequirement requirement;
                if (action == 0)
                {
                    requirement = RoleActions.Approve;
                }
                else if (action == 1)
                {
                    requirement = RoleActions.Reject;
                    examineAction = 1;
                }
                else
                {
                    requirement = RoleActions.Delete;
                    examineAction = -1;
                }
                var isAuthorized = await _service.AuthorizeAsync(User, article, requirement);
                if (!isAuthorized.Succeeded)
                {
                    return new JsonResult(new
                    {
                        id = "forbid",
                        msg = "无权限"
                    });
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
                var notify = MingklyHelpers.GetNotify(action, article, userAction.Text, _secretProvider);
                await _context.SystemNotifies.AddAsync(notify);
                var user = await _userManager.GetUserAsync(User);
                var record = new ExamineRecord
                {
                    ExaminerId = user.Id,
                    ArticleId = article.Id,
                    Action = examineAction,
                    Time = DateTime.Now,
                };
                await _context.ExamineRecords.AddAsync(record);
                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    id = "suc",
                    msg = action
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = ex.Message
                });
            }
        }
        [HttpPost(nameof(VoteComment))]
        public async Task<JsonResult> VoteComment([FromBody] UserAction userAction)
        {
            if (userAction == null)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }
            if (userAction.Action > 1 || userAction.Action < -1)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "无此行为"
                });
            }
            try
            {
                var commentId = _secretProvider.UnProtectId(userAction.Id);
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return new JsonResult(new
                    {
                        id = "error",
                        msg = "无此用户"
                    });
                }
                var commentActionDb = await _context.CommentUsers
                    .SingleOrDefaultAsync(u => u.UserId == user.Id && u.CommentId != null && u.CommentId == commentId);
                var comment = await _context.Comments.FindAsync(commentId);
                int actionRes;
                if (commentActionDb == null)
                {
                    var commentAction = new CommentUser()
                    {
                        UserId = user.Id,
                        Action = userAction.Action,
                        CommentId = commentId,
                    };
                    await _context.CommentUsers.AddAsync(commentAction);
                    actionRes = userAction.Action;
                    if (userAction.Action > 0)
                    {
                        comment.VoteUpCount++;
                    }
                    else if (userAction.Action < 0)
                    {
                        comment.VoteDownCount++;
                    }

                }
                else
                {
                    if (commentActionDb.Action == userAction.Action)
                    {
                        commentActionDb.Action = 0;
                        if (userAction.Action > 0)
                        {
                            comment.VoteUpCount--;
                        }
                        else if (userAction.Action < 0)
                        {
                            comment.VoteDownCount--;
                        }
                    }
                    else
                    {
                        if (commentActionDb.Action == 0)
                        {
                            if (userAction.Action > 0)
                            {
                                comment.VoteUpCount++;
                            }
                            else if (userAction.Action < 0)
                            {
                                comment.VoteDownCount++;
                            }
                        }
                        else
                        {
                            if (userAction.Action > 0)
                            {
                                comment.VoteUpCount++;
                                comment.VoteDownCount--;
                            }
                            else if (userAction.Action < 0)
                            {
                                comment.VoteUpCount--;
                                comment.VoteDownCount++;
                            }
                        }
                        commentActionDb.Action = userAction.Action;
                    }

                    actionRes = commentActionDb.Action;
                }
                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    id = "suc",
                    msg = actionRes,
                    voteDownCount = comment.VoteDownCount,
                    voteUpCount = comment.VoteUpCount,
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = ex.Message
                });
            }
        }
        [HttpPost(nameof(LoadMore))]
        public async Task<JsonResult> LoadMore([FromForm] LoadMoreStart loadMoreStart)
        {
            if (loadMoreStart == null)
            {
                return new JsonResult(new
                {
                    id = -1,
                });
            }
            int pageJump = loadMoreStart.PageJump > 0 ? 1 : -1;
            if (loadMoreStart.PageCount <= 0 || loadMoreStart.PageCount > 20)
            {
                loadMoreStart.PageCount = 10;
            }
            var SortWord = String.IsNullOrEmpty(loadMoreStart.SortWord) ? "time" : loadMoreStart.SortWord;
            var Descending = loadMoreStart.Descending;

            var user = await _userManager.GetUserAsync(User);
            IQueryable<Article> articles;
            if (loadMoreStart.State == 0)
            {
                articles = _context.Articles
                  .Where(a => a.AuthorId == user.Id)
                  .Where(a => a.State == ArticleState.Approved);
            }
            else if (loadMoreStart.State == 1)
            {
                articles = _context.Articles
                  .Where(a => a.AuthorId == user.Id)
                  .Where(a => a.State == ArticleState.Submit);
            }
            else if (loadMoreStart.State == 1)
            {
                articles = _context.Articles
                  .Where(a => a.AuthorId == user.Id)
                  .Where(a => a.State == ArticleState.Rejected);
            }
            else
            {
                articles = _context.Articles
                  .Where(a => a.AuthorId == user.Id);
            }
            articles = MingklyHelpers.SortArticle(articles
                  , SortWord, Descending, pageJump);


            try
            {
                long p = loadMoreStart.Number;
                if (SortWord == "time")
                {
                    p = _secretProvider.UnProtectId(p);
                }
                articles = MingklyHelpers.SortArticlePageJump(articles, pageJump, SortWord, Descending, p);
                var articleViews = await articles
                     .Select(article => new
                     {
                         Id = _secretProvider.ProtectId(article.Id),
                         CreateTime = article.CreateTime,
                         AuthorName = article.Author.UserName,
                         Title = article.Title,
                         Texts = article.Texts,
                         ImagePaths = article.Images,
                         ViewCount = article.ViewCount,
                         State = article.State,
                         VoteUpNumber = article.VoteUpCount,
                         VoteDownNumber = article.VoteDownCount,
                         CurrentUserAction = article.UserAction.SingleOrDefault(a => a.UserId == user.Id),
                     })
                     .Take(loadMoreStart.PageCount)
                     .AsNoTracking()
                     .ToListAsync();

                var ArticleViews = new List<ArticleLoadMore>();
                foreach (var article in articleViews)
                {
                    ArticleViews.Add(new ArticleLoadMore
                    {
                        Id = article.Id,
                        CreateTime = article.CreateTime,
                        AuthorName = article.AuthorName,
                        Title = article.Title,
                        Texts = MingklyHelpers.RemoveTags(article.Texts, 100),
                        Cover = FileHelpers.GetCoverPath(article.ImagePaths),
                        ViewCount = article.ViewCount,
                        State = article.State,
                        VoteUpNumber = article.VoteUpNumber,
                        VoteDownNumber = article.VoteDownNumber,
                        CurrentUserAction = article.CurrentUserAction == null ? 0 : article.CurrentUserAction.Action,
                    });
                }
                var number = articleViews.Last().Id;
                if (SortWord == "vote")
                {
                    number = articleViews.Last().VoteUpNumber;
                }
                else if (SortWord == "view")
                {
                    number = articleViews.Last().ViewCount;
                }
                return new JsonResult(new
                {
                    id = 0,
                    articleViews = ArticleViews,
                    number = number,
                });
            }
            catch
            {
                return new JsonResult(new
                {
                    id = -1,
                });
            }
        }
        [HttpGet("DeleteComment/{id}")]
        public async Task<JsonResult> DeleteComment(long? id)
        {
            if (id == null)
            {
                return new JsonResult(new
                {
                    code = 0
                });
            }
            long Id = 0;
            try
            {
                Id = _secretProvider.UnProtectId(id.Value);
            }
            catch
            {
                return new JsonResult(new
                {
                    code = 1
                });
            }
            var comment = await _context.Comments.Include(u => u.User.UserName).FirstOrDefaultAsync(u => u.Id == Id);
            if (comment == null)
            {
                return new JsonResult(new
                {
                    code = 2
                });
            }
            var suc = await _service.AuthorizeAsync(User, comment, RoleActions.Delete);
            if (!suc.Succeeded)
            {
                return new JsonResult(new
                {
                    code = 3
                });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var record = new ExamineRecord
                {
                    ExaminerId = user.Id,
                    CommentId = comment.Id,
                    Action = -1,
                    Time = DateTime.Now,
                };
                await _context.ExamineRecords.AddAsync(record);
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    code = -1
                });
            }
            catch
            {
                return new JsonResult(new
                {
                    code = 4
                });
            }
        }
        [HttpPost(nameof(LoadMoreFavourite))]
        public async Task<JsonResult> LoadMoreFavourite([FromForm] LoadMoreStart loadMoreStart)
        {
            if (loadMoreStart == null)
            {
                return new JsonResult(new
                {
                    id = -1,
                });
            }
            int pageJump = loadMoreStart.PageJump > 0 ? 1 : -1;
            if (loadMoreStart.PageCount <= 0 || loadMoreStart.PageCount > 20)
            {
                loadMoreStart.PageCount = 10;
            }

            try
            {
                long p = loadMoreStart.Number;
                p = _secretProvider.UnProtectId(p);
                var user = await _context.Users
                    .Select(u => new
                    {
                        UserName = u.UserName,
                        FavouriteArticles = u.Favourites
                        .Where(f => f.FavouriteUserId != null)
                        .SelectMany(f => f.FavouriteUser.Articles, (f, article) => new
                        {
                            Id = article.Id,
                            CreateTime = article.CreateTime,
                            AuthorName = article.Author.UserName,
                            Title = article.Title,
                            Texts = article.Texts,
                            ImagePaths = article.Images,
                            ViewCount = article.ViewCount,
                            State = article.State,
                            VoteUpNumber = article.VoteUpCount,
                            VoteDownNumber = article.VoteDownCount,
                            CurrentUserAction = article.UserAction.SingleOrDefault(a => a.UserId == u.Id),
                        })
                        .OrderByDescending(a => a.Id)
                        .Where(a => a.Id < p)
                        .Take(loadMoreStart.PageCount)
                        .Select(article => new ArticleLoadMore
                        {
                            Id = _secretProvider.ProtectId(article.Id),
                            CreateTime = article.CreateTime,
                            AuthorName = article.AuthorName,
                            Title = article.Title,
                            Texts = MingklyHelpers.RemoveTags(article.Texts, 100),
                            Cover = FileHelpers.GetCoverPath(article.ImagePaths),
                            ViewCount = article.ViewCount,
                            State = article.State,
                            VoteUpNumber = article.VoteUpNumber,
                            VoteDownNumber = article.VoteDownNumber,
                            CurrentUserAction = article.CurrentUserAction == null ? 0 : article.CurrentUserAction.Action,
                        })
                    })
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);


                if (user == null)
                {
                    return new JsonResult(new
                    {
                        id = -1,
                    });
                }
                var ArticleViews = user.FavouriteArticles;
                var number = ArticleViews.Last().Id;
                return new JsonResult(new
                {
                    id = 0,
                    articleViews = ArticleViews,
                    number = number,
                });
            }
            catch
            {
                return new JsonResult(new
                {
                    id = -1,
                });
            }
        }
        [HttpGet("GetSearchRecord/{word}")]
        public async Task<JsonResult> GetSearchRecord([MaxLength(100)] string word)
        {
            try
            {
                var records = await _context.SearchRecords
                .Where(a => a.Word.StartsWith(word))
                .Take(10).ToArrayAsync();
                if (records.Length > 0)
                {
                    return new JsonResult(new
                    {
                        id = 1,
                        records = records
                    });
                }
            }
            catch
            {
                return new JsonResult(new
                {
                    id = 0
                });
            }

            return new JsonResult(new
            {
                id = 0
            });
        }
        [HttpPost(nameof(LoadMoreArticleComment))]
        public async Task<JsonResult> LoadMoreArticleComment([FromBody]  LoadMoreComment loadMoreComment)
        {
            try
            {
                Debug.WriteLine($"articleid : {loadMoreComment.ArticleId}");
                var comments = await _articleServices.LoadMoreArticleComments(10, 3, loadMoreComment.CommentId, loadMoreComment.ArticleId, User.Identity.Name);
                if (comments == null)
                {
                    return new JsonResult(new
                    {
                        id = -1,
                    });
                }
                return new JsonResult(comments);
            }
            catch(Exception e)
            {
                return new JsonResult(new
                {
                    id = e.Message,
                });
            }

        }
       [HttpPost(nameof(LoadMoreInnerComment))]
        public async Task<JsonResult> LoadMoreInnerComment([FromBody] LoadMoreComment loadMoreComment)
        {
            try
            {

                var comments = await _articleServices.LoadMoreInnerComments( 3,loadMoreComment.InnerCommentId.Value, loadMoreComment.CommentId,  User.Identity.Name);
                if (comments == null)
                {
                    return new JsonResult(new
                    {
                        id = -1,
                    });
                }
                return new JsonResult(comments);
            }
            catch (Exception e)
            {
                return new JsonResult(new
                {
                    id = e.Message,
                });
            }

        }
        [HttpGet("LoadMoreArticleViews/{property}")]
        public async Task<JsonResult> LoadMoreArticleViews(long? property)
        {
            try
            {
                if (property != null)
                {
                    var articles = await _articleServices.GetArticlesWithUserAction(10, User, 1, property.Value);
                    return new JsonResult(articles);
                }
                else
                {
                    return new JsonResult(new
                    {
                        id = -1,
                    });
                }
            }
            catch
            {
                return new JsonResult(new
                {
                    id = -1,
                });
            }

        }
    }
    public class ArticleLoadMore
    {
        public string AuthorName { get; set; }
        public long Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string Title { get; set; }
        public string Texts { get; set; }
        public string Cover { get; set; }
        public long ViewCount { get; set; }
        public ArticleState State { get; set; }
        public int CurrentUserAction { get; set; }
        public long VoteUpNumber { get; set; }
        public long VoteDownNumber { get; set; }
    }
    public class UserAction
    {
        [Required]
        public int Action { get; set; }
        [Required]
        public long Id { get; set; }
        public string Text { get; set; }
    }
    public class UserCommentAction
    {
        [Required]
        public int Action { get; set; }
        [Required]
        public long Id { get; set; }
        public long ArticleId { get; set; }
    }
    public class FavouriteArticleId
    {
        public long Id { get; set; }
    }
    public class LoadMoreStart
    {
        public int PageJump { get; set; }
        public string SortWord { get; set; }
        public long Number { get; set; }
        public bool Descending { get; set; }
        public int PageCount { get; set; }
        public int State { get; set; }
    }
    public class LoadMoreComment
    {
        public long ArticleId { get; set; }
        public long CommentId { get; set; }
        public long? InnerCommentId { get; set; }

    }
}
