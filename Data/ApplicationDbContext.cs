using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyWeb.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<MyUser, IdentityRole<long>, long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentUser> CommentUsers { get; set; }
        public DbSet<UserFavourite> UserFavourites { get; set; }
        public DbSet<UserView> UserViews { get; set; }
        public DbSet<AppFile> AppFiles { get; set; }
        public DbSet<SystemNotify> SystemNotifies { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<RecentChat> ChatInfos { get; set; }
        public DbSet<ArticleTag> ArticleTags { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<SearchRecord> SearchRecords { get; set; }
        public DbSet<ExamineRecord> ExamineRecords { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<MyUser>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Id);
                b.HasIndex(u => u.Name);
                b.HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
                b.HasMany(u => u.CommentActions)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);
                b.HasMany(u => u.Articles)
                .WithOne(a => a.Author)
                .HasForeignKey(a => a.AuthorId);
                b.HasMany(u => u.Favourites)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId);
                b.HasMany(u => u.SystemNotify)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId);
                b.HasMany(u => u.RecentChats)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
                b.HasMany(u => u.RecentBeChats)
                .WithOne(c => c.ChatToUser)
                .HasForeignKey(c => c.ChatToUserId);
                b.OwnsOne(u => u.LastActivity);
                b.HasQueryFilter(u => u.UserState == UserState.Confirmed);
            });
            builder.Entity<Comment>(b =>
            {
                b.HasKey(c => c.Id);
                b.HasMany(c => c.Comments)
                .WithOne(c => c.ReplyToComment)
                .HasForeignKey(c => c.ReplyToCommentId);
                b.HasOne(c => c.ReplyToArticle)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.ReplyToArticleId);
                b.HasMany(c => c.UserActions)
                .WithOne(a => a.Comment)
                .HasForeignKey(a => a.CommentId);
                b.HasOne(c => c.BelongToComment)
                .WithMany(b => b.UnderComments)
                .HasForeignKey(c => c.BelongToCommentId);
                b.Property<bool>("IsDeleted");
                b.HasQueryFilter(b => !EF.Property<bool>(b, "IsDeleted"));
            });
            builder.Entity<Article>(b =>
            {
                b.HasKey(a => a.Id);
                b.HasIndex(a => a.CreateTime);
                b.HasIndex(a => a.Title);
                b.HasMany(a => a.FavouriteUser)
                .WithOne(f => f.FavouriteArticle)
                .HasForeignKey(f => f.FavouriteArticleId);
                b.Property<bool>("IsDeleted");
                b.HasQueryFilter(b => !EF.Property<bool>(b, "IsDeleted")&&b.State==ArticleState.Approved);
                b.HasMany(a => a.Tags)
                .WithOne()
                .HasForeignKey(t => t.ArticleId);
            });
            builder.Entity<CommentUser>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Action)
                .HasDefaultValue(0);
            });
            builder.Entity<UserFavourite>()
                .HasKey(f => f.Id);
            builder.Entity<UserView>()
                .HasKey("ArticleId", "UserId");
            builder.Entity<ChatMessage>(b =>
            {
                b.HasOne(m => m.Sender)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(m => m.SenderId);
                b.HasOne(m => m.Reciever)
                .WithMany(u => u.RecievedMessages)
                .HasForeignKey(m => m.RecieverId);
                b.Property(m => m.Readed)
                .HasDefaultValue(false);
            });
            builder.Entity<Tag>(b =>
            {
                b.HasMany(t => t.Articles)
                .WithOne()
                .HasForeignKey(t => t.TagId);
                b.HasIndex(b => b.Name);
            });
            builder.Entity<ArticleTag>()
                .HasKey(a => new { a.ArticleId, a.TagId });
            builder.Entity<SearchRecord>()
                .HasKey(a => a.Word);
            builder.Entity<ExamineRecord>(b =>
            {
                b.HasOne(e => e.Examiner)
                .WithMany(u => u.ExamineRecords)
                .HasForeignKey(e => e.ExaminerId);
                b.HasOne(e => e.User)
                .WithMany(u => u.BeExaminedRecords)
                .HasForeignKey(b => b.UserId);
                b.HasOne(e => e.Article)
                .WithMany(a => a.BeExaminedRecords)
                .HasForeignKey(e => e.ArticleId);
                b.HasOne(e => e.Comment)
                .WithMany(c => c.BeExaminedRecords)
                .HasForeignKey(e => e.CommentId);
            });
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.DetectChanges();
            var articles = ChangeTracker.Entries<Article>()
                .Where(a => a.State == EntityState.Deleted).ToArray();
            foreach (var article in articles)
            {
                article.State = EntityState.Modified;
                article.CurrentValues["IsDeleted"] = true;
            }
            var comments = ChangeTracker.Entries<Comment>()
                     .Where(a => a.State == EntityState.Deleted).ToArray();
            foreach (var comment in comments)
            {
                comment.State = EntityState.Modified;
                comment.CurrentValues["IsDeleted"] = true;
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }

}
