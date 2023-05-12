using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWeb.Models
{

    public class Article
    {
        public long Id { get; set; }

        [MaxLength(150)]
        [Required]
        public string Title { get; set; }
        public DateTime CreateTime { get; set; }
        [Required]
        [MinLength(50)]
        public string Texts { get; set; }
        public string Images { get; set; }
        public long ViewCount { get; set; }
        public long FavouriteCount { get; set; }
        public long VoteUpCount { get; set; }
        public long VoteDownCount { get; set; }
        public long CommentsCount { get; set; }
        public ArticleState State { get; set; }
        public bool IsPublic { get; set; }
        public long AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public MyUser Author { get; set; }

        public List<Comment> Comments { get; set; }
        public List<UserFavourite> FavouriteUser { get; set; }
        public List<CommentUser> UserAction { get; set; }
        public List<ArticleTag> Tags { get; set; }
        public List<ExamineRecord> BeExaminedRecords { get; set; }
    }
    public class ArticleTag
    {
        public long ArticleId { get; set; }
        public long TagId { get; set; }
    }
    public class Tag
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<ArticleTag> Articles { get; set; }

    }
    public class SearchRecord
    {
        [MaxLength(100)]
        public string Word { get; set; }
    }
    public enum ArticleState
    {
        Approved,
        Submit,
        Rejected,
    }
    public class UserView
    {
        public long UserId { get; set; }
        public long ArticleId { get; set; }
        public bool Unlogin { get; set; }
    }
    public class ChatMessage
    {
        public long Id { get; set; }
        public long SenderId { get; set; }
        public MyUser Sender { get; set; }
        public long RecieverId { get; set; }
        public MyUser Reciever { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
        public DateTime Time { get; set; }
        public bool Readed { get; set; }

    }
    [Index(nameof(ChatToUserId))]
    public class RecentChat
    {
        public long Id { get; set; }

        public long ChatToUserId { get; set; }
        public MyUser ChatToUser { get; set; }
        public DateTime LastChatTime { get; set; }
        public MyUser User { get; set; }
        public long UserId { get; set; }
    }
    public class SystemNotify
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public MyUser User { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Action { get; set; }
        public bool Readed { get; set; }
        public DateTime Time { get; set; }
    }
    public enum CommentState
    {
        Submit,
        Approved,
        Rejected,
    }
    public class Comment
    {
        public long Id { get; set; }

        [MaxLength(1000)]
        [Required]
        public string Text { get; set; }
        public string Images { get; set; }
        public DateTime Time { get; set; }

        public long UserId { get; set; }
        public MyUser User { get; set; }
        public CommentState State { get; set; }
        public long VoteUpCount { get; set; }
        public long VoteDownCount { get; set; }
        public long CommentsCount { get; set; }
        public long ReplyToArticleId { get; set; }
        public Article ReplyToArticle { get; set; }

        public long? ReplyToCommentId { get; set; }
        public Comment ReplyToComment { get; set; }

        public long? BelongToCommentId { get; set; }
        public Comment BelongToComment { get; set; }

        public List<Comment> UnderComments { get; set; }
        public List<Comment> Comments { get; set; }
        public List<CommentUser> UserActions { get; set; }
        public List<ExamineRecord> BeExaminedRecords { get; set; }
    }
    public class UserFavourite
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public MyUser User { get; set; }

        public long? FavouriteArticleId { get; set; }
        public Article FavouriteArticle { get; set; }
        public long? FavouriteUserId { get; set; }
        [ForeignKey(nameof(FavouriteUserId))]
        public MyUser FavouriteUser { get; set; }
    }
    public class CommentUser
    {
        public long Id { get; set; }
        public int Action { get; set; }

        public long UserId { get; set; }
        public MyUser User { get; set; }

        public long? CommentId { get; set; }
        public Comment Comment { get; set; }
        public long? ArticleId { get; set; }
        public Article Article { get; set; }


    }
    public class ExamineRecord
    {
        public long Id { get; set; }
        public long ExaminerId { get; set; }
        public MyUser Examiner { get; set; }
        public long? ArticleId { get; set; }
        public Article Article { get; set; }
        public long? CommentId { get; set; }
        public Comment Comment { get; set; }
        public long? UserId { get; set; }
        public MyUser User { get; set; }
        public int Action { get; set; }
        public DateTime Time { get; set; }
    }
    public class MyUser : IdentityUser<long>
    {
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string ShowText { get; set; }
        public string Avatar { get; set; }
        public string Background { get; set; }
        public long FollowsCount { get; set; }
        public long FansCount { get; set; }
        public UserState UserState { get; set; }
        public UserLastActivity LastActivity { get; set; }
        public List<CommentUser> CommentActions { get; set; }
        public List<UserFavourite> Favourites { get; set; }
        public List<AppFile> UploadFiles { get; set; }
        public List<Comment> Comments { get; set; }
        public List<SystemNotify> SystemNotify { get; set; }
        public List<ChatMessage> ChatMessages { get; set; }
        public List<ChatMessage> RecievedMessages { get; set; }
        public List<RecentChat> RecentChats { get; set; }
        public List<RecentChat> RecentBeChats { get; set; }
        public List<ExamineRecord> ExamineRecords { get; set; }
        public List<ExamineRecord> BeExaminedRecords { get; set; }

        [InverseProperty("FavouriteUser")]
        public List<UserFavourite> FavouriteUser { get; set; }

        [InverseProperty("Author")]
        public List<Article> Articles { get; set; }

    }
    public class UserLastActivity
    {
        public DateTime ArticleTime { get; set; }
        public DateTime ReplyTime { get; set; }
        public DateTime NotifyTime { get; set; }
    }
    public enum UserState
    {
        UnConfirmed,
        Suspaned,
        Baned,
        Confirmed,
    }
}
