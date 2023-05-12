using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Models
{
    public class NumberRecount
    {
        private readonly ApplicationDbContext context;
        public NumberRecount(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task ReCountAsync()
        {
            var articles = await context.Articles
                .Include(a => a.Comments)
                .Include(a => a.FavouriteUser)
                .Include(a => a.UserAction).ToListAsync();
            foreach (var article in articles)
            {
                article.FavouriteCount = article.FavouriteUser
                    .Count;
                article.CommentsCount = article.Comments.Count;
                article.VoteUpCount = article.UserAction
                    .LongCount(a => a.Action > 0);
                article.VoteDownCount = article.UserAction
                    .LongCount(article => article.Action < 0);
            }
            await context.SaveChangesAsync();
            var comments = await context.Comments
                .Include(c => c.UserActions)
                .ToArrayAsync();
            foreach (var comment in comments)
            {
                comment.VoteUpCount = comment.UserActions
                    .LongCount(a => a.Action > 0);
                comment.VoteDownCount = comment.UserActions
                    .LongCount(a => a.Action < 0);
            }
            await context.SaveChangesAsync();
            var users = await context.Users
                .Include(u => u.FavouriteUser)
                .Include(u => u.Favourites)
                .ToArrayAsync();
            foreach (var user in users)
            {
                user.FansCount = user.FavouriteUser.LongCount();
                user.FollowsCount = user.Favourites
                    .LongCount(f => f.FavouriteUserId != null);
            }
            await context.SaveChangesAsync();
        }
    }
}
