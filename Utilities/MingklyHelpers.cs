using MyWeb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MyWeb.Utilities
{
    public class MingklyHelpers
    {
        public static string GetDeltaTime(DateTime currenttime)
        {
            var timeNow = DateTime.Now;
            (int Year, int Month, int Day, int Hour, int Minute, int Second) time =
                (timeNow.Year - currenttime.Year,
                timeNow.Month - currenttime.Month,
                timeNow.Day - currenttime.Day,
                timeNow.Hour - currenttime.Hour,
                timeNow.Minute - currenttime.Minute,
                timeNow.Second - currenttime.Second
                );
            if (time.Year > 0)
            {
                return time.Year + "年前";
            }
            else if (time.Month > 0)
            {
                return time.Month + "月前";
            }
            else if (time.Day > 0)
            {
                return time.Day + "天前";
            }
            else if (time.Hour > 0)
            {
                return time.Hour + "小时前";
            }
            else if (time.Minute > 0)
            {
                return time.Minute + "分钟前";
            }
            else
            {
                return "刚刚";
            }
        }
        public static bool GetArticleTexts(string content, out string[] texts, out string[] styles)
        {
            var segments = content.Split("-imageboundary-", StringSplitOptions.RemoveEmptyEntries);
            texts = null;
            styles = null;
            if (segments.Length > 0)
            {
                texts = segments.Where(t => !t.StartsWith("-style-")).ToArray();
                styles = segments.Where(t => t.StartsWith("-style-")).Select(t => t.Substring("-style-".Length).Replace("\"", "").Replace("'", "")).ToArray();
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string[] GetImgStylesInArticle(string content)
        {
            var segments = content.Split("-imageboundary-", StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0)
            {
                return segments.Where(t => t.StartsWith("-style-")).Select(t => t.Substring("-style-".Length).Replace("\"", "").Replace("'", "")).ToArray();
            }
            return null;
        }
        public static string GetTextsInArticle(string content, int length)
        {
            var segments = content.Split("-imageboundary-", StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0)
            {
                var texts = segments.Where(t => !t.StartsWith("-style-") && t != "-null-").Select(t => t.Replace("-br-", "")).ToArray();
                var s = string.Join("", texts);
                if (s.Length > length)
                {
                    s = s.Substring(0, length);
                }
                return s;
            }
            return null;
        }
        public static string GetArticleViewProperty(ArticleView article, string property)
        {
            if (article == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(property))
            {
                return null;
            }
            var sortWord = property.ToLower();
            string p = null;
            if (sortWord == "vote")
            {
                p = (article.VoteUpNumber - article.VoteDownNumber).ToString();
            }
            else if (sortWord == "view")
            {
                p = article.ViewCount.ToString();
            }
            else if (sortWord == "time")
            {
                p = article.Id.ToString();
            }
            return p;
        }
        public static IQueryable<Article> SortArticle(IQueryable<Article> articles, string sortWord, bool descending, int pageJump = 0)
        {
            if (!string.IsNullOrEmpty(sortWord))
            {
                if (pageJump < 0)
                {
                    descending = !descending;
                }
                sortWord = sortWord.ToLower();
                if (sortWord == "vote")
                {
                    if (!descending)
                    {
                        articles = articles.OrderByDescending(a => a.VoteUpCount);
                    }
                    else
                    {
                        articles = articles.OrderBy(a => a.VoteUpCount);
                    }
                }
                else if (sortWord == "view")
                {
                    if (!descending)
                    {
                        articles = articles.OrderByDescending(a => a.ViewCount);
                    }
                    else
                    {
                        articles = articles.OrderBy(a => a.ViewCount);
                    }
                }
                else if (sortWord == "time")
                {
                    if (!descending)
                    {
                        articles = articles.OrderByDescending(a => a.Id);
                    }
                    else
                    {
                        articles = articles.OrderBy(a => a.Id);
                    }
                }
            }
            return articles;
        }
        public static IQueryable<Article> SortArticlePageJump(IQueryable<Article> articles, int up, string sortWord, bool descending, long number)
        {
            if (number < 0)
            {
                return articles;
            }
            if (string.IsNullOrEmpty(sortWord))
            {
                sortWord = "time";
            }
            sortWord = sortWord.ToLower();

            int order;
            if (descending)
            {
                order = up * -1;
            }
            else
            {
                order = up;
            }
            if (sortWord == "time")
            {
                articles = articles.Where(a => a.Id * order < number * order);
            }
            else if (sortWord == "vote")
            {
                articles = articles.Where(a => a.VoteUpCount * order < number * order);
            }
            else if (sortWord == "view")
            {
                articles = articles.Where(a => a.ViewCount * order < number * order);
            }

            return articles;
        }
        public static List<ArticleView> ProcessArticles(IEnumerable<Article> articles, SecretProvider secretProvider, long? userId)
        {
            var ArticleViews = new List<ArticleView>();
            foreach (var a in articles)
            {

                a.Id = secretProvider.ProtectId(a.Id);
                int currentUserAction = 0;
                var processedTexts = RemoveTags(a.Texts, 100);
                var view = new ArticleView
                {
                    Id = a.Id,
                    CreateTime = a.CreateTime,
                    AuthorName = a.Author.Name,
                    Title = a.Title,
                    Texts = processedTexts,
                    Cover = FileHelpers.GetFilePaths(a.Images)[0],
                    ViewCount = a.ViewCount,
                    State = a.State,
                    VoteUpNumber = a.VoteUpCount,
                    VoteDownNumber = a.VoteDownCount,
                    CurrentUserAction = currentUserAction,
                };
                ArticleViews.Add(view);
            }
            return ArticleViews;
        }
        public static Dictionary<CommentView, List<CommentView>> ProcessCommentsAsync(IEnumerable<Comment> articleComments,
            long? userId)
        {
            var InnerComments = new Dictionary<CommentView, List<CommentView>>();
            foreach (var r in articleComments)
            {
                var rs = new List<CommentView>();
                GetAll(r.Comments, rs);
                var v = new CommentView()
                {
                    Id = r.Id,
                    Text = r.Text,
                    ReplyToArticleId = r.ReplyToArticleId,
                    Time = r.Time,
                    VoteUpCount = r.VoteUpCount,
                    VoteDownCount = r.VoteDownCount,
                    UserName = r.User.Name,
                    UserAvatar = r.User.Avatar
                };
                InnerComments.Add(v, rs);
            }
            return InnerComments;
            void GetAll(List<Comment> comments, List<CommentView> container)
            {
                if (comments == null)
                {
                    return;
                }
                foreach (var c in comments)
                {
                    c.Id = c.Id;
                    var v = new CommentView()
                    {
                        Id = c.Id,
                        Text = c.Text,
                        ReplyToArticleId = c.ReplyToArticleId,
                        ReplyToCommentId = c.ReplyToCommentId,
                        Time = c.Time,
                        VoteDownCount = c.VoteDownCount,
                        VoteUpCount = c.VoteUpCount,
                        ReplyToCommentUserName = c.ReplyToComment.User.Name,
                        UserName = c.User.Name,
                        UserAvatar = c.User.Avatar
                    };
                    container.Add(v);
                    if (c.Comments != null && c.Comments.Count != 0)
                    {
                        GetAll(c.Comments, container);
                    }
                }
            }
        }
        public static string TagFiliter(string html, string[] permittedTag, string[] permittedAttr)
        {
            var htmlEncode = WebUtility.HtmlEncode(html);
            var quto = WebUtility.HtmlEncode("\"");
            var left = WebUtility.HtmlEncode("<");
            var right = WebUtility.HtmlEncode(">");
            string startPart = String.Join("|", permittedAttr);
            var regx = new Regex($"({startPart})"
                + "=" + quto + @"[^&]*" + $"{quto}");
            htmlEncode = regx.Replace(htmlEncode, (s) => s.Value.Replace(quto, "\""));
            var regexTag = new Regex($"{left}"
                + $"/?({String.Join("|", permittedTag)})"
                + @"[^&]*"
                + $"{right}");
            htmlEncode = regexTag.Replace(htmlEncode, (s) => s.Value.Replace(left, "<").Replace(right, ">"));
            regx = new Regex(@"<code>[^<>]*</code>");
            htmlEncode = regx.Replace(htmlEncode, (s) => s.Value.Replace("&amp;", "&"));
            htmlEncode = htmlEncode.Replace("&amp;nbsp;", "&nbsp;");
            return htmlEncode;
        }
        public static string ProcessImageSrc(string html, string oldRootPath, string newRootPath, out List<string> paths)
        {
            var p = new List<string>();
            var regex = new Regex(@"<img[^>]*>");
            html = regex.Replace(html, (s) =>
             {
                 var srcRegex = new Regex("src=\"[^\"]*\"");
                 Debug.WriteLine("imgtag: " + s.Value);
                 return srcRegex.Replace(s.Value, (x) =>
                 {
                     var oldPath = x.Value.Substring("src=".Length).Replace("\"", "").Replace("..\\", "").Replace(".\\", "");
                     Debug.WriteLine("src : " + x.Value);
                     Debug.WriteLine("odlpath : " + oldPath);
                     oldPath = Path.Combine(oldRootPath, oldPath);

                     Debug.WriteLine("odlpath : " + oldPath);
                     if (File.Exists(oldPath))
                     {
                         if (!Directory.Exists(newRootPath))
                         {
                             Directory.CreateDirectory(newRootPath);
                         }
                         var fileName = Path.GetFileName(oldPath);
                         var newPath = Path.Combine(newRootPath, fileName);
                         if (oldPath != newPath)
                         {
                             File.Move(oldPath, newPath, true);
                         }
                         var relativePath = Path.GetRelativePath(oldRootPath, newPath);
                         p.Add(relativePath);
                         return "src=" + relativePath;
                     }
                     else
                     {
                         return "src=/images/nullImage.png";
                     }
                 });
             });
            paths = p;
            return html;
        }
        public static string ProcessHref(string html, List<string> permittedHost)
        {
            var hrefRegex = new Regex("href=\"[^\"]*\"");
            html = hrefRegex.Replace(html, (s) =>
            {
                var href = s.Value.Substring("href=".Length).Replace("\"", "");
                try
                {
                    var url = new Uri(href);
                    Debug.WriteLine(url.Host);
                    if (!permittedHost.Contains(url.Host))
                    {
                        return "href=\"\"";
                    }
                }
                catch (Exception)
                {
                    return "href=\"\"";
                }

                return s.Value;
            });
            return html;
        }
        public static string RemoveTags(string html, int length)
        {
            var regex = new Regex(@"<[^>]*>");
            var res = regex.Replace(html, "");
            res = res.Replace("&emp;nbsp;", " ");
            res = res.Replace("&nbsp;", " ");
            if (res.Length > length)
            {
                res = res.Substring(0, length);
            }
            return res;
        }
        public static SystemNotify GetNotify(int action, Article article, string text, SecretProvider secretProvider)
        {
            var notify = new SystemNotify
            {
                UserId = article.AuthorId,
                Title = "稿件退回通知",
                Text = text,
                Action = $"<a href=" +
            $"/articles/details?id={secretProvider.ProtectId(article.Id)}" +
            $"  >点击此处修改</a>",
                Readed = false,
                Time = DateTime.Now,
            };
            if (action == 0)
            {
                notify.Action = $"<a href=" +
                   $"/articles/details?id={secretProvider.ProtectId(article.Id)}" +
                   $"  >点击此处浏览</a>";
                notify.Title = "稿件通过通知";
            }
            else if (action == 1)
            {
                notify.Action = $"<a href=" +
                   $"/articles/edit?id={secretProvider.ProtectId(article.Id)}" +
                   $"  >点击此处修改</a>";
                notify.Title = "稿件退回通知";
            }
            else if (action == 2)
            {
                notify.Title = "稿件删除通知";
            }
            return notify;
        }
        public static SystemNotify GetNotify(int action, Comment comment, string text, SecretProvider secretProvider)
        {
            var notify = new SystemNotify
            {
                UserId = comment.UserId,
                Title = "稿件退回通知",
                Text = text,
                Action = comment.Text,
                Readed = false,
                Time = DateTime.Now,
            };
            if (action == 0)
            {
                notify.Title = "评论通过通知";
            }
            else if (action == 1)
            {
                notify.Title = "评论退回通知";
            }
            else if (action == 2)
            {
                notify.Title = "pl删除通知";
            }
            return notify;
        }
        public static SystemNotify GetNotify(int action, MyUser user, string text, SecretProvider secretProvider)
        {
            var notify = new SystemNotify
            {
                UserId = user.Id,
                Title = "稿件退回通知",
                Text = text,
                Action = "",
                Readed = false,
                Time = DateTime.Now,
            };
            if (action == 0)
            {
                notify.Title = "用户验证通过通知";
            }
            else if (action == 1)
            {
                notify.Title = "用户验证失败通知";
            }
            else if (action == 2)
            {
                notify.Title = "用户封禁通知";
            }
            return notify;
        }
    }
}
