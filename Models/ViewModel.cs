using System;
using System.Collections.Generic;

namespace MyWeb.Models
{

    public class ArticleView
    {

        public string AuthorName { get; set; }
        public long Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string Title { get; set; }
        public string Texts { get; set; }
        public string Cover { get; set; }
        public long ViewCount { get; set; }
        internal ArticleState State { get; set; }
        public int CurrentUserAction
        {
            get
            {
                return UserAction == null ? 0 : UserAction.Action;
            }
            set
            {
                UserAction = new CommentUser
                {
                    Action = value,
                };
            }
        }
        internal CommentUser UserAction { get; set; }
        public long VoteUpNumber { get; set; }
        public long VoteDownNumber { get; set; }
        public bool NotReaded { get; set; }
        public string[] ImagePaths { get; set; }
        public long FavouriteNumber { get; set; }
        public bool Favourited { get; set; }
        public bool CurrentUserIsAuthor { get; set; }
        public bool IsDeleted { get; set; }
        public List<ExamineRecordView> ExamineRecords { get; set; }
        internal bool IsPublic { get; set; }
    }
    public class PageInfo
    {
        public int? TargetPage { get; set; }
        public int CurrentPage { get; set; }
        public bool PageUp { get; set; }
        public int? MaxPage { get; set; }
        public string SortWord { get; set; }
        public bool Descending { get; set; }
    }
    public class CommentView
    {
        public long Id { get; set; }
        public long VoteUpCount { get; set; }
        public long VoteDownCount { get; set; }
        public string Text { get; set; }
        public long ReplyToArticleId { get; set; }
        public long? ReplyToCommentId { get; set; }
        public long? BelongToCommentId { get; set; }
        public DateTime Time { get; set; }
        internal  CommentUser UserAction { get; set; }
        public int CurrentUserAction
        {
            get
            {
                return UserAction is null ? 0 : UserAction.Action;
            }
        }
        public int CommentCount { get; set; }
        public string UserAvatar { get; set; }
        public string UserName { get; set; }
        public CommentState State { get; set; }
        public string ReplyToCommentUserName { get; set; }
        public bool NotReaded { get; set; }
        public bool IsDeleted { get; set; }
        public List<CommentView> InnerComments { get; set; }
        public List<ExamineRecordView> ExamineRecords { get; set; }
    }
    public class UserInfoView
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        public bool ShowAvatar { get; set; }
        public string ShowText { set; get; }
        public string Background { get; set; }
        public string PhoneNumber { get; set; }
        public UserState UserState { get; set; }
        public List<ExamineRecordView> ExamineRecords { get; set; }
    }
    public class ExamineRecordView
    {
        public int ExamineType { get; set; }
        public int Action { get; set; }
        public UserInfoView Examiner { get; set; }

        public ArticleView Article { get; set; }
        public CommentView Comment { get; set; }
        public UserInfoView User { get; set; }
        public DateTime Time { set; get; }
    }
}
