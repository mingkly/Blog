using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly string[] _permittedExtensions;
        private readonly long _fileSizeLimit;
        private readonly string _targetStoredPath;
        private readonly ApplicationDbContext _context;
        protected UserManager<MyUser> UserManager { get; }
        protected IAuthorizationService AuthorizationService { get; }
        public FileController(IConfiguration iconfig
            , UserManager<MyUser> userManager
            , IAuthorizationService service
            , ApplicationDbContext applicationDbContext)
        {
            _permittedExtensions = iconfig.GetSection("PermittedExtensions").Get<string[]>();
            _fileSizeLimit = iconfig.GetValue<long>("FileSizeLimit");
            _targetStoredPath = iconfig.GetValue<string>("StoredFilePath");
            UserManager = userManager;
            AuthorizationService = service;
            _context = applicationDbContext;
        }
        public long FileSizeLimit => _fileSizeLimit;
        public string[] PermittedExtensions => _permittedExtensions;
        [HttpPost(nameof(UploadImage))]
        public async Task<JsonResult> UploadImage([FromForm] UploadImg img)
        {
            try
            {
                if (img == null)
                {
                    return new JsonResult(new
                    {
                        errno = 1,
                        message = "空d"
                    });
                }
                var user = await UserManager.GetUserAsync(User);
                var article = new Article();
                var isAuthorized = await AuthorizationService.AuthorizeAsync(User, article, RoleActions.Create);
                if (!isAuthorized.Succeeded)
                {
                    return new JsonResult(new
                    {
                        errno = 1,
                        message = "无权"
                    });
                }
                var appFiles = await _context.AppFiles.Where(a => a.UploaderId == user.Id).AsNoTracking().ToArrayAsync();
                if (appFiles != null && appFiles.Length > 0)
                {
                    var uploadCount = appFiles.Count(a => DateTime.Now - a.CreateTime < TimeSpan.FromHours(1));
                    if (uploadCount > 10)
                    {
                        return new JsonResult(new
                        {
                            errno = 1,
                            message = "传文件过多，请稍后再试"
                        });
                    }
                }
                if (img.mingklyImage.Length == 0)
                {
                    return new JsonResult(new
                    {
                        errno = 1,
                        message = "无图片"
                    });
                }
                if (img.mingklyImage.Length > FileSizeLimit)
                {
                    return new JsonResult(new
                    {
                        errno = 1,
                        message = "图片过大"
                    });
                }
                var data = await FileHelpers.ProcessFormFileAsync<UploadImg>(img.mingklyImage
                    , ModelState, PermittedExtensions, FileSizeLimit);
                if (!ModelState.IsValid)
                {
                    return new JsonResult(new
                    {
                        errno = 1,
                        message = "错误"
                    }); ;
                }

                var fileName = Guid.NewGuid().ToString() + FileHelpers.GetImgExtesions(img.mingklyImage.FileName, _permittedExtensions);
                var targetStoredPath = _targetStoredPath;
                if (!System.IO.Directory.Exists(targetStoredPath))
                {
                    Directory.CreateDirectory(targetStoredPath);
                }
                var path = Path.Combine(targetStoredPath, fileName);
                using (var fs = System.IO.File.Create(path))
                {
                    await fs.WriteAsync(data);
                }
                var relativePath = Path.GetRelativePath(targetStoredPath.Replace("images", ""), path);
                var appFile = new AppFile()
                {
                    CreateTime = DateTime.Now,
                    UploaderId = user.Id,
                    FileType = "image",
                };
                await _context.AppFiles.AddAsync(appFile);
                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    errno = 0,
                    data = new
                    {
                        url = relativePath,
                        alt = "图片"
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    errno = 1,
                    message = ex.Message
                });
            }
        }
        public class UploadImg
        {
            [Display(Name = "图片")]
            public IFormFile mingklyImage { get; set; }
        }
    }
}
