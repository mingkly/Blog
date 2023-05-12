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
    public class MyFollowModel : DI_Page
    {

        public List<UserViewModel> MyFollows { get; set; }
        private readonly SecretProvider _secretProvider;
        public MyFollowModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext
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
                var user0 = await ApplicationDbContext
                      .Users
                      .Select(u => new
                      {
                          UserName = u.UserName,
                          FavouriteUser = u.Favourites
                          .Where(u => u.FavouriteUserId != null)
                          .Select(f => new UserViewModel
                          {
                              Name = f.FavouriteUser.Name,
                              Avatar = String.IsNullOrEmpty(f.FavouriteUser.Avatar) ? "images/daxiong.gif" : f.User.Avatar,
                              Background = f.FavouriteUser.Background,
                              CurrentUserFollowed = true,
                          })
                      })
                      .AsNoTracking()
                      .SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user0 != null)
                {
                    MyFollows = user0.FavouriteUser.ToList();
                }
            }
            catch
            {
                MyFollows = new List<UserViewModel>();
            }

        }
    }
}
