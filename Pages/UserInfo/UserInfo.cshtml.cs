using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class UserInfoModel : DI_Page
    {


        [BindProperty]
        public ImgFile ImgFiles { get; set; }
        private readonly SecretProvider _secretProvider;
        private readonly string _targetStoredPath;
        private readonly long _fileSizeLimit;
        private readonly SignInManager<MyUser> _signInManager;
        private readonly string[] _permittedExtensions = new string[] { ".jpg", ".png", ".JPG", ".PNG", ".gif" };
        public UserInfo ThisUser { get; set; }

        public UserInfoModel(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext
            , AppFileContext appFileContext, IAuthorizationService authorizationService
            , SecretProvider secretProvider
            , IConfiguration iconfig
            , SignInManager<MyUser> signInManager)
            : base(userManager, applicationDbContext, appFileContext, authorizationService)
        {
            _secretProvider = secretProvider;
            _fileSizeLimit = iconfig.GetValue<long>("FileSizeLimit");
            _targetStoredPath = iconfig.GetValue<string>("StoredFilePath");
            _signInManager = signInManager;
        }
        public async Task OnGetAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            if (user != null)
            {
                ThisUser = new UserInfo();
                ImgFiles = new ImgFile();
                ThisUser.Name = user.Name;
                ImgFiles.ShowText = user.ShowText;
                if (string.IsNullOrEmpty(user.Avatar))
                {
                    ThisUser.Avatar = "images/daxiong.gif";
                }
                else
                {
                    ThisUser.Avatar = user.Avatar;
                }
                if (string.IsNullOrEmpty(user.Background))
                {
                    ThisUser.Backgroud = "images/01131376-8acd-45c1-9e99-a2a6e5f866d3.jpg";
                }
                else
                {
                    ThisUser.Backgroud = user.Background;
                }

            }
        }
        public async Task<IActionResult> OnPostUpdateUserInfoAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest();
            }
            if (ImgFiles == null)
            {
                return NotFound();
            }
            try
            {
                if (ImgFiles.Avatar != null)
                {
                    var data = await FileHelpers.ProcessFormFileAsync<ImgFile>(ImgFiles.Avatar, ModelState, _permittedExtensions, _fileSizeLimit);
                    if (!ModelState.IsValid)
                    {
                        await OnGetAsync();
                        return Page();
                    }
                    var targetStoredPath = _targetStoredPath + "\\" + user.UserName;
                    if (!System.IO.Directory.Exists(targetStoredPath))
                    {
                        Directory.CreateDirectory(targetStoredPath);
                    }
                    var fileName = Guid.NewGuid().ToString() + FileHelpers.GetImgExtesions(ImgFiles.Avatar.FileName, _permittedExtensions);
                    var path = System.IO.Path.Combine(targetStoredPath, fileName);

                    using (var stream = System.IO.File.Create(path))
                    {
                        await stream.WriteAsync(data);
                    }
                    var relativePath = Path.GetRelativePath(_targetStoredPath.Replace("images", ""), path);
                    user.Avatar = relativePath;
                }
                if (ImgFiles.Background != null)
                {
                    var data = await FileHelpers.ProcessFormFileAsync<ImgFile>(ImgFiles.Background, ModelState, _permittedExtensions, _fileSizeLimit);
                    if (!ModelState.IsValid)
                    {
                        await OnGetAsync();
                        return Page();
                    }
                    Debug.WriteLine("background");
                    var targetStoredPath = _targetStoredPath + "\\" + user.UserName;
                    if (!System.IO.Directory.Exists(targetStoredPath))
                    {
                        Directory.CreateDirectory(targetStoredPath);
                    }
                    var fileName = Guid.NewGuid().ToString() + FileHelpers.GetImgExtesions(ImgFiles.Background.FileName, _permittedExtensions);
                    var path = System.IO.Path.Combine(targetStoredPath, fileName);

                    using (var stream = System.IO.File.Create(path))
                    {
                        await stream.WriteAsync(data);
                    }
                    var relativePath = Path.GetRelativePath(_targetStoredPath.Replace("images", ""), path);
                    user.Background = relativePath;
                }
                user.ShowText = ImgFiles.ShowText;
                await ApplicationDbContext.SaveChangesAsync();
            }
            catch
            {
                return BadRequest();
            }

            return RedirectToPage();
        }

        public class ImgFile
        {
            public IFormFile Avatar { get; set; }
            public IFormFile Background { get; set; }
            [MaxLength(500)]
            [Display(Name = "Ç©Ãû")]
            public string ShowText { get; set; }
        }

    }

}
