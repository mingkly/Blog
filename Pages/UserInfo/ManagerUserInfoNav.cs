using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace MyWeb.Pages.UserInfo
{
    public class ManagerUserInfoNav
    {
        public static readonly string UserInfo = nameof(UserInfo);
        public static readonly string MyArticles = nameof(MyArticles);
        public static readonly string MyFans = nameof(MyFans);
        public static readonly string MyComments = nameof(MyComments);
        public static readonly string MyFavourite = nameof(MyFavourite);
        public static readonly string MyFollow = nameof(MyFollow);
        public static readonly string MyHistory = nameof(MyHistory);
        public static readonly string MyReply = nameof(MyReply);
        public static readonly string SystemMessage = nameof(SystemMessage);
        public static readonly string Chats = nameof(Chats);
        public static readonly string UserSecurity = nameof(UserSecurity);
        public static readonly string UserArticles = nameof(UserArticles);
        public static readonly string UserMainPage = nameof(UserMainPage);
        public static string UserInfoActive(ViewContext viewContext)
        {
            return NavActive(viewContext, UserInfo);
        }
        public static string MyArticlesActive(ViewContext viewContext)
        {
            return NavActive(viewContext, MyArticles);
        }
        public static string MyFansActive(ViewContext viewContext)
        {
            return NavActive(viewContext, MyFans);
        }
        public static string MyCommentsActive(ViewContext viewContext)
        {
            return NavActive(viewContext, MyComments);
        }
        public static string MyFavouriteActive(ViewContext viewContext)
        {
            return NavActive(viewContext, MyFavourite);
        }
        public static string MyFollowActive(ViewContext viewContext)
        {
            return NavActive(viewContext, MyFollow);
        }
        public static string MyHistoryActive(ViewContext viewContext)
        {
            return NavActive(viewContext, MyHistory);
        }
        public static string MyReplyActive(ViewContext viewContext)
        {
            return NavActive(viewContext, MyReply);
        }
        public static string SystemMessageActive(ViewContext viewContext)
        {
            return NavActive(viewContext, SystemMessage);
        }
        public static string ChatsActive(ViewContext viewContext)
        {
            return NavActive(viewContext, Chats);
        }
        public static string UserSecurityActive(ViewContext viewContext)
        {
            return NavActive(viewContext, UserSecurity);
        }
        public static string UserArticlesActive(ViewContext viewContext)
        {
            return NavActive(viewContext, UserArticles);
        }
        public static string UserMainPageActive(ViewContext viewContext)
        {
            return NavActive(viewContext, UserMainPage);
        }
        public static string NavActive(ViewContext viewContext, string page)
        {
            var active = viewContext.ViewBag.ActiveUserPage as string;
            if (active is null)
            {
                active = UserInfo;
            }
            if (string.Equals(active, page, StringComparison.OrdinalIgnoreCase))
            {
                return "active";
            }
            return null;

        }
    }
}
