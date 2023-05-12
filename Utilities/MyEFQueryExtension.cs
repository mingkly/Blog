using Microsoft.EntityFrameworkCore;
using MyWeb.Models;
using System.Collections.Generic;
using System.Linq;

namespace MyWeb.Utilities
{
    public static class MyEFQueryExtension
    {
        public static IQueryable<ArticleView> SelectArticleViewWithAll(this IQueryable<Article> article, long userId)
        {
            return article.Select(a => new ArticleView
            {
                Id = a.Id,
                Title = a.Title,
                Texts = a.Texts,
                Cover = FileHelpers.GetCoverPath(a.Images),
                CreateTime = a.CreateTime,
                AuthorName = a.Author.Name,
                VoteUpNumber = a.VoteUpCount,
                VoteDownNumber = a.VoteDownCount,
                FavouriteNumber = a.FavouriteCount,
                ViewCount = a.ViewCount,
                State = a.State,
                UserAction = a.UserAction
                        .SingleOrDefault(u => u.UserId == userId),
                Favourited = a.FavouriteUser
                        .SingleOrDefault(f => f.UserId == userId) == null ? false : true,
                CurrentUserIsAuthor = a.AuthorId == userId,
            })
            .AsNoTracking();
        }
        public static IQueryable<ArticleView> SelectArticleViewWithUserAction(this IQueryable<Article> article, long userId, SecretProvider secretProvider)
        {
            return article.Select(a => new ArticleView
            {
                Id = secretProvider.ProtectId(a.Id),
                Title = a.Title,
                Cover = FileHelpers.GetCoverPath(a.Images),
                CreateTime = a.CreateTime,
                AuthorName = a.Author.Name,
                VoteUpNumber = a.VoteUpCount,
                VoteDownNumber = a.VoteDownCount,
                FavouriteNumber = a.FavouriteCount,
                ViewCount = a.ViewCount,
                State = a.State,
                UserAction = a.UserAction
                        .SingleOrDefault(u => u.UserId == userId),
                Favourited = a.FavouriteUser
                        .SingleOrDefault(f => f.UserId == userId) == null ? false : true,
                CurrentUserIsAuthor = a.AuthorId == userId,
            })
            .AsNoTracking();
        }
        public static IQueryable<ArticleView> SelectArticleViewWithAll(this IQueryable<Article> article, long userId, SecretProvider secretProvider)
        {
            return article.Select(a => new ArticleView
            {
                Id = secretProvider.ProtectId(a.Id),
                Title = a.Title,
                Texts = a.Texts,
                Cover = FileHelpers.GetCoverPath(a.Images),
                CreateTime = a.CreateTime,
                AuthorName = a.Author.Name,
                VoteUpNumber = a.VoteUpCount,
                VoteDownNumber = a.VoteDownCount,
                FavouriteNumber = a.FavouriteCount,
                ViewCount = a.ViewCount,
                State = a.State,
                UserAction = a.UserAction
                        .SingleOrDefault(u => u.UserId == userId),
                Favourited = a.FavouriteUser
                        .SingleOrDefault(f => f.UserId == userId) == null ? false : true,
                CurrentUserIsAuthor = a.AuthorId == userId,
            })
            .AsNoTracking();
        }
        public static IQueryable<ArticleView> SelectArticleViewSimple(this IQueryable<Article> article)
        {
            return article.Select(a => new ArticleView
            {
                Id = a.Id,
                Title = a.Title,
                Cover = FileHelpers.GetCoverPath(a.Images),
                CreateTime = a.CreateTime,
                AuthorName = a.Author.Name,
                VoteUpNumber = a.VoteUpCount,
                VoteDownNumber = a.VoteDownCount,
                FavouriteNumber = a.FavouriteCount,
                ViewCount = a.ViewCount,
                State = a.State,
            })
            .AsNoTracking();
        }
        public static IQueryable<ArticleView> SelectArticleViewSimple(this IQueryable<Article> article, SecretProvider secretProvider)
        {
            return article.Select(a => new ArticleView
            {
                Id = secretProvider.ProtectId(a.Id),
                Title = a.Title,
                Cover = FileHelpers.GetCoverPath(a.Images),
                CreateTime = a.CreateTime,
                AuthorName = a.Author.Name,
                VoteUpNumber = a.VoteUpCount,
                VoteDownNumber = a.VoteDownCount,
                FavouriteNumber = a.FavouriteCount,
                ViewCount = a.ViewCount,
                State = a.State,
            })
            .AsNoTracking();
        }
        public static IQueryable<ArticleView> SelectArticleViewWithText(this IQueryable<Article> article)
        {
            return article.Select(a => new ArticleView
            {
                Id = a.Id,
                Title = a.Title,
                Texts = a.Texts,
                Cover = FileHelpers.GetCoverPath(a.Images),
                CreateTime = a.CreateTime,
                AuthorName = a.Author.Name,
                VoteUpNumber = a.VoteUpCount,
                VoteDownNumber = a.VoteDownCount,
                FavouriteNumber = a.FavouriteCount,
                ViewCount = a.ViewCount,
                State = a.State,
            })
            .AsNoTracking();
        }
        public static IQueryable<ArticleView> SelectArticleViewWithText(this IQueryable<Article> article, SecretProvider secretProvider)
        {
            return article.Select(a => new ArticleView
            {
                Id = secretProvider.ProtectId(a.Id),
                Title = a.Title,
                Texts = a.Texts,
                Cover = FileHelpers.GetCoverPath(a.Images),
                CreateTime = a.CreateTime,
                AuthorName = a.Author.Name,
                VoteUpNumber = a.VoteUpCount,
                VoteDownNumber = a.VoteDownCount,
                FavouriteNumber = a.FavouriteCount,
                ViewCount = a.ViewCount,
                State = a.State,
            })
            .AsNoTracking();
        }
        public static IEnumerable<ArticleView> SelectArticleFromFavourite(this IEnumerable<UserFavourite> favourites, SecretProvider secretProvider)
        {
            return favourites
                .Select(f => new ArticleView
                {
                    Id = f.FavouriteArticle.Id,
                    CreateTime = f.FavouriteArticle.CreateTime,
                    Title = f.FavouriteArticle.Title,
                    Cover = FileHelpers.GetCoverPath(f.FavouriteArticle.Images),
                    ViewCount = f.FavouriteArticle.ViewCount,
                    State = f.FavouriteArticle.State,
                    VoteUpNumber = f.FavouriteArticle.VoteUpCount,
                    VoteDownNumber = f.FavouriteArticle.VoteDownCount,
                    AuthorName = f.FavouriteArticle.Author.Name
                });
        }
        public static IOrderedQueryable<Article> OrderBy(this IQueryable<Article> articles, string propertyName, bool descending = false)
        {
            IOrderedQueryable<Article> articleOrdered = articles.OrderByDescending(a => a.Id);
            if (!string.IsNullOrEmpty(propertyName))
            {
                propertyName = propertyName.ToLower();
                if (propertyName == "vote")
                {
                    if (!descending)
                    {
                        articleOrdered = articles.OrderByDescending(a => a.VoteUpCount);
                    }
                    else
                    {
                        articleOrdered = articles.OrderBy(a => a.VoteUpCount);
                    }
                }
                else if (propertyName == "view")
                {
                    if (!descending)
                    {
                        articleOrdered = articles.OrderByDescending(a => a.ViewCount);
                    }
                    else
                    {
                        articleOrdered = articles.OrderBy(a => a.ViewCount);
                    }
                }
                else if (propertyName == "time")
                {
                    if (!descending)
                    {
                        articleOrdered = articles.OrderByDescending(a => a.Id);
                    }
                    else
                    {
                        articleOrdered = articles.OrderBy(a => a.Id);
                    }
                }
            }
            return articleOrdered;
        }
        public static IOrderedQueryable<Article> OrderByAndLimit(this IQueryable<Article> articles, string propertyName, int pageJump, long property, bool descending = false)
        {
            IOrderedQueryable<Article> articleOrdered;
            if (string.IsNullOrEmpty(propertyName))
            {
                propertyName = "time";
            }
            propertyName = propertyName.ToLower();

            int order;
            if (descending)
            {
                order = pageJump * -1;
            }
            else
            {
                order = pageJump;
            }

            if (propertyName == "time")
            {
                articles = articles.Where(a => a.Id * order < property * order);
            }
            else if (propertyName == "vote")
            {
                articles = articles.Where(a => a.VoteUpCount * order < property * order);
            }
            else if (propertyName == "view")
            {
                articles = articles.Where(a => a.ViewCount * order < property * order);
            }
            articleOrdered = articles.OrderBy(propertyName, descending);
            return articleOrdered;
        }
        public static IOrderedEnumerable<UserFavourite> OrderBy(this IEnumerable<UserFavourite> favourites, string propertyName, bool descending = false)
        {
            IOrderedEnumerable<UserFavourite> articleOrdered = favourites.OrderBy(f => f.FavouriteArticle.Id);
            if (!string.IsNullOrEmpty(propertyName))
            {
                propertyName = propertyName.ToLower();
                if (propertyName == "vote")
                {
                    if (!descending)
                    {
                        articleOrdered = favourites.OrderByDescending(a => a.FavouriteArticle.VoteUpCount);
                    }
                    else
                    {
                        articleOrdered = favourites.OrderBy(a => a.FavouriteArticle.VoteUpCount);
                    }
                }
                else if (propertyName == "view")
                {
                    if (!descending)
                    {
                        articleOrdered = favourites.OrderByDescending(a => a.FavouriteArticle.ViewCount);
                    }
                    else
                    {
                        articleOrdered = favourites.OrderBy(a => a.FavouriteArticle.ViewCount);
                    }
                }
                else if (propertyName == "time")
                {
                    if (!descending)
                    {
                        articleOrdered = favourites.OrderByDescending(a => a.FavouriteArticleId);
                    }
                    else
                    {
                        articleOrdered = favourites.OrderBy(a => a.FavouriteArticleId);
                    }
                }
            }
            return articleOrdered;
        }
        public static IOrderedQueryable<ArticleView> OrderBy(this IQueryable<ArticleView> articles, string propertyName, bool descending = false)
        {
            IOrderedQueryable<ArticleView> articleOrdered = articles.OrderByDescending(a => a.Id);
            if (!string.IsNullOrEmpty(propertyName))
            {
                propertyName = propertyName.ToLower();
                if (propertyName == "vote")
                {
                    if (!descending)
                    {
                        articleOrdered = articles.OrderByDescending(a => a.VoteUpNumber);
                    }
                    else
                    {
                        articleOrdered = articles.OrderBy(a => a.VoteUpNumber);
                    }
                }
                else if (propertyName == "view")
                {
                    if (!descending)
                    {
                        articleOrdered = articles.OrderByDescending(a => a.ViewCount);
                    }
                    else
                    {
                        articleOrdered = articles.OrderBy(a => a.ViewCount);
                    }
                }
                else if (propertyName == "time")
                {
                    if (!descending)
                    {
                        articleOrdered = articles.OrderByDescending(a => a.Id);
                    }
                    else
                    {
                        articleOrdered = articles.OrderBy(a => a.Id);
                    }
                }
            }
            return articleOrdered;
        }

        public static  IQueryable<CommentView> SelectCommentViewWithUserAction(this IQueryable<Comment> comments, long userId,int innerLength,SecretProvider secretProvider)
        {
            return comments
                  .Select(c => new CommentView
                  {
                      Id = secretProvider.ProtectId(c.Id),
                      Text = c.Text,
                      VoteUpCount = c.VoteUpCount,
                      VoteDownCount = c.VoteDownCount,
                      Time = c.Time,
                      UserName = c.User.Name,
                      UserAvatar = c.User.Avatar,
                      CommentCount = (int)c.CommentsCount,
                      ReplyToArticleId = secretProvider.ProtectId(c.ReplyToArticleId),
                      UserAction = c.UserActions
                                  .SingleOrDefault(c => c.UserId == userId),
                      InnerComments = c.UnderComments
                                  .OrderByDescending(c => c.Id)
                                  .Take(innerLength)
                                  .Select(cc => new CommentView
                                  {
                                      Id = secretProvider.ProtectId(cc.Id),
                                      Text = cc.Text,
                                      VoteUpCount = cc.VoteUpCount,
                                      VoteDownCount = cc.VoteDownCount,
                                      Time = cc.Time,
                                      UserName = cc.User.Name,
                                      UserAvatar = cc.User.Avatar,
                                      ReplyToArticleId = secretProvider.ProtectId(c.ReplyToArticleId),
                                      BelongToCommentId = secretProvider.ProtectId(c.Id),
                                      ReplyToCommentId = cc.ReplyToCommentId == null ? null : secretProvider.ProtectId(cc.ReplyToCommentId.Value),
                                      ReplyToCommentUserName = cc.ReplyToComment.User.Name,
                                      CommentCount = (int)cc.CommentsCount,
                                      UserAction = cc.UserActions
                                           .SingleOrDefault(c => c.UserId == userId),
                                  })
                                  .ToList()
                  });
        }
        public static IQueryable<CommentView> SelectCommentViewSimple(this IQueryable<Comment> comments,SecretProvider secretProvider)
        {
            return comments
                  .Select(c => new CommentView
                  {
                      Id = secretProvider.ProtectId(c.Id),
                      Text = c.Text,
                      Time = c.Time,
                      UserName = c.User.Name,
                      UserAvatar = c.User.Avatar,
                      CommentCount = (int)c.CommentsCount,
                      ReplyToArticleId = secretProvider.ProtectId(c.ReplyToArticleId),
                  });
        }
    }
    }
