using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class MyFansModel : DI_Page
    {
        public List<UserViewModel> MyFans { get; set; }
        private readonly SecretProvider _secretProvider;
        public MyFansModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext
            , AppFileContext appFileContext, IAuthorizationService authorizationService
            , SecretProvider secretProvider)
            : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
            _secretProvider = secretProvider;
        }
        public async Task OnGet()
        {
            try
            {
                var user = await ApplicationDbContext
                    .Users
                    .Select(u => new
                    {
                        UserName = u.UserName,
                        FavouriteUser = u.FavouriteUser
                        .Select(f => new UserViewModel
                        {
                            Name = f.User.Name,
                            Avatar = String.IsNullOrEmpty(f.User.Avatar) ? "images/daxiong.gif" : f.User.Avatar,
                            Background = f.User.Background,
                            CurrentUserFollowed =
                            f.User.FavouriteUser.Select(fu => fu.UserId).Contains(u.Id),
                        })
                    })
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
                MyFans = user?.FavouriteUser.ToList();
            }
            catch
            {
                MyFans = new List<UserViewModel>();
            }
        }
    }
    public class UserViewModel
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Background { get; set; }
        public string ShowText { get; set; }
        public bool CurrentUserFollowed { get; set; }
    }
}
