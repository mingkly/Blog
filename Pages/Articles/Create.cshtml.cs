using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyWeb.Pages
{


    public class CreateModel : DI_Page
    {
        private readonly string[] _permittedExtensions;
        private readonly long _fileSizeLimit;
        private readonly string _targetStoredPath;
        private readonly FormOptions _formOptions = new FormOptions();
        private readonly SecretProvider _secretProvider;
        private readonly string[] _permittedTag;
        private readonly string[] _permittedAttr;
        private readonly string[] _permittedHost;
        public CreateModel(ApplicationDbContext context, UserManager<MyUser> userManager
            , IConfiguration iconfig
            , AppFileContext fileContext
            , IAuthorizationService service
            , SecretProvider secretProvider)
            : base(userManager, context, fileContext, service)
        {
            _permittedExtensions = iconfig.GetSection("PermittedExtensions").Get<string[]>();
            _fileSizeLimit = iconfig.GetValue<long>("FileSizeLimit");
            _targetStoredPath = iconfig.GetValue<string>("StoredFilePath");
            _secretProvider = secretProvider;
            _permittedAttr = iconfig.GetSection("PermittedAttr").Get<string[]>();
            _permittedTag = iconfig.GetSection("PermittedTag").Get<string[]>();
            _permittedHost = iconfig.GetSection("PermittedHost").Get<string[]>();
        }

        public IActionResult OnGet()
        {
            return Page();
        }
        public long FileSizeLimit => _fileSizeLimit;
        public string[] PermittedExtensions => _permittedExtensions;


        public Article Article { get; set; }
        [BindProperty]
        public ArticleCreate ArticleToCreate { get; set; }
        public class ArticleCreate
        {
            [Required]
            [MaxLength(150)]
            public string Title { get; set; }
            [Required]
            [MinLength(50)]
            public string Texts { get; set; }
            public IFormFile Cover { get; set; }
            public bool IsPublic { get; set; }
        }
        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await UserManager.GetUserAsync(User);
            Article = new Article();
            Article.AuthorId = user.Id;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, Article, RoleActions.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            byte[] cover = null;
            if (ArticleToCreate.Cover != null)
            {
                cover = await FileHelpers.ProcessFormFileAsync<ArticleCreate>(
                    ArticleToCreate.Cover,
                    ModelState,
                    _permittedExtensions,
                    _fileSizeLimit);
                if (!ModelState.IsValid)
                {
                    return Page();
                }
            }
            Article.Title = ArticleToCreate.Title;
            var text = MingklyHelpers.TagFiliter(ArticleToCreate.Texts, _permittedTag, _permittedAttr);
            Article.CreateTime = DateTime.Now;
            Article.Texts = String.Empty;
            Article.State = ArticleState.Submit;
            Article.Title = WebUtility.HtmlDecode(Article.Title);
            Article.IsPublic = ArticleToCreate.IsPublic;
            try
            {
                await ApplicationDbContext.Articles.AddAsync(Article);
                await ApplicationDbContext.SaveChangesAsync();
                var targetPath = _targetStoredPath + "\\" + _secretProvider.ProtectId(Article.Id);
                var rootPath = _targetStoredPath.Replace("images", "");
                text = MingklyHelpers.ProcessImageSrc(text
                        , rootPath
                        , targetPath
                        , out var paths);
                text = MingklyHelpers.ProcessHref(text, _permittedHost.ToList());
                Article.Texts = text;

                if (cover != null)
                {
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }
                    var fileName = Guid.NewGuid().ToString() + FileHelpers.GetImgExtesions(ArticleToCreate.Cover.FileName, _permittedExtensions);
                    targetPath = Path.Combine(targetPath, fileName);
                    using (var fileStream = System.IO.File.Create(targetPath))
                    {
                        await fileStream.WriteAsync(cover);
                    }
                    var appFile = new AppFile()
                    {
                        CreateTime = DateTime.Now,
                        UploaderId = Article.AuthorId,
                        FileType = "image",
                    };
                    await ApplicationDbContext.AppFiles.AddAsync(appFile);
                    var rPath = Path.GetRelativePath(rootPath, targetPath);
                    paths.Insert(0, rPath);
                }
                Article.Images = String.Join(";", paths);
                await ApplicationDbContext.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("error", "something wwrong");
                return Page();
            }
        }
        /*
        public async Task<IActionResult> OnPostUploadFileAsync()
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
                {
                    id = "Article",
                    msg = "格式错误"
                });
            }
            var user = await UserManager.GetUserAsync(User);
            Article.AuthorId = user.Id;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, Article, RoleActions.Create);
            if (!isAuthorized.Succeeded)
            {
                return new JsonResult(new
                {
                    id = "Article",
                    msg = "请登录"
                });
            }
            Article.CreateTime = DateTime.Now;
            Article.State = ArticleState.Submit;
            if (!MultipartRequestHelper.IsMultipart(Request.ContentType))
            {
                return new JsonResult(new
                {
                    id = "Article",
                    msg = "格式错误"
                });
            }
            HttpContext.Request.Body.Position = 0;
            if (HttpContext.Request.ContentLength > 1024 * 1024 * 25)
            {
                return new JsonResult(new
                {
                    id="Article",
                    msg="内容太大，尝试删减图片"
                });
            }
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType),
                _formOptions.MultipartBoundaryLengthLimit);            
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            var filePaths = new StringBuilder();
            var formData = new KeyValueAccumulator();
            var fileList = new Dictionary<string,byte[]>();
            while (section != null)
            {
                var hasDisposition = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var disposition);
                if (hasDisposition)
                {
                    if (MultipartRequestHelper.HasFileDispostion(disposition))
                    {
                        var data = await FileHelpers.ProcessStreamedFile(section, disposition,
                            ModelState, _permittedExtensions, _fileSizeLimit);
                        if (!ModelState.IsValid)
                        {
                            return new JsonResult(new
                            {
                                id = "File",
                                msg = "图片错误"
                            });
                        }
                        var fileName = Guid.NewGuid().ToString() + FileHelpers.GetImgExtesions(disposition.FileName.Value, _permittedExtensions);
                        fileList.Add(fileName,data);
                    }
                    else if (MultipartRequestHelper.HasFormDataDispostion(disposition))
                    {
                        var key =HeaderUtilities.RemoveQuotes(disposition.Name).Value;
                        var encoding = GetEncoding(section);
                        if (encoding == null)
                        {
                            ModelState.AddModelError("error", $"unknownEncoding for {key}");
                            return new JsonResult(new
                            {
                                id = "Title",
                                msg = "标题错误"
                            });
                        }
                        using (var streamReader=new StreamReader(
                            section.Body,encoding,
                            detectEncodingFromByteOrderMarks:true,
                            bufferSize:1024,
                            leaveOpen: true))
                        {
                            var value = await streamReader.ReadToEndAsync();
                            if (string.Equals(value, "undefine", StringComparison.OrdinalIgnoreCase))
                            {
                                value = string.Empty;
                            }
                            formData.Append(key, value);
                            if (formData.ValueCount > _formOptions.ValueCountLimit)
                            {
                                ModelState.AddModelError("error", $"exceed formdata valueLImit {_formOptions.ValueCountLimit}");
                                return new JsonResult(new
                                {
                                    id = "File",
                                    msg = "图片过大"
                                });
                            }
                        }
                    }
                }
                section = await reader.ReadNextSectionAsync();
            }
            HttpContext.Request.Body.Position = 0;
            var results = formData.GetResults();
            if (!results.TryGetValue("Title", out var title))
            {
                ModelState.AddModelError("error", "title bindinf failed");
                return new JsonResult(new
                {
                    id = "Title",
                    msg = "标题不能为空"
                });
            }
            if (!results.TryGetValue("Texts", out var texts))
            {
                ModelState.AddModelError("error", "texts bindinf failed");
                return new JsonResult(new
                {
                    id = "Texts",
                    msg = "内容不能为空"
                });
            }
            if (results.TryGetValue("file0", out var cover))
            {
                if (!string.IsNullOrEmpty(cover))
                {
                    filePaths.Insert(0, "images\\cover.jpg");
                }
            }
            if (title.ToString().Length > 70)
            {
                ModelState.AddModelError("Title", "标题过长，不得超过70字");
                return new JsonResult(new
                {
                    id = "Title",
                    msg = "标题过长，不得超过70字"
                });
            }
            Article.Texts = WebUtility.HtmlEncode(texts);    
            Article.Title = WebUtility.HtmlEncode( title);
            await ApplicationDbContext.Articles.AddAsync(Article);
            await ApplicationDbContext.SaveChangesAsync();
            foreach(var file in fileList)
            {
                var data = file.Value;
                var targetStoredPath = _targetStoredPath + "\\" +_secretProvider.ProtectId(Article.Id);
                if (!System.IO.Directory.Exists(targetStoredPath))
                {
                    Directory.CreateDirectory(targetStoredPath);
                }

                var fileName = file.Key;
                var path = System.IO.Path.Combine(targetStoredPath, fileName);

                using (var stream = System.IO.File.Create(path))
                {
                    await stream.WriteAsync(data);
                }
                var relativePath = Path.GetRelativePath(targetStoredPath.Replace("images", ""), path);
                filePaths.Append(relativePath + ";");
            }

            Article.Images = filePaths.ToString();
            await ApplicationDbContext.SaveChangesAsync();
            return new JsonResult(new
            {
                id = "Suc",
            });
        }
        public async Task<JsonResult> OnPostUpdateArticleAsync()
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
                {
                    id = "Article",
                    msg = "格式错误"
                });
            }
            var user = await UserManager.GetUserAsync(User);
            var Article = new Article();
            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, Article, RoleActions.Create);
            if (!isAuthorized.Succeeded)
            {
                return new JsonResult(new
                {
                    id = "Article",
                    msg = "请登录"
                });
            }
            if (!MultipartRequestHelper.IsMultipart(Request.ContentType))
            {
                return new JsonResult(new
                {
                    id = "Article",
                    msg = "格式错误"
                });
            }
            HttpContext.Request.Body.Position = 0;
            if (HttpContext.Request.ContentLength > 1024 * 1024 * 25)
            {
                return new JsonResult(new
                {
                    id = "Article",
                    msg = "内容太大，尝试删减图片"
                });
            }
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType),
                _formOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            var filePaths = new List<string>();
            var formData = new KeyValueAccumulator();
            var fileList = new Dictionary<string, byte[]>();
            while (section != null)
            {
                var hasDisposition = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var disposition);
                if (hasDisposition)
                {
                    if (MultipartRequestHelper.HasFileDispostion(disposition))
                    {
                        var data = await FileHelpers.ProcessStreamedFile(section, disposition,
                            ModelState, _permittedExtensions, _fileSizeLimit);
                        if (!ModelState.IsValid)
                        {
                            return new JsonResult(new
                            {
                                id = "File",
                                msg = "图片错误"
                            });
                        }
                        var fileName = Guid.NewGuid().ToString() + FileHelpers.GetImgExtesions(disposition.FileName.Value, _permittedExtensions);
                        fileList.Add(fileName, data);
                        filePaths.Add(fileName);
                    }
                    else if (MultipartRequestHelper.HasFormDataDispostion(disposition))
                    {
                        var key = HeaderUtilities.RemoveQuotes(disposition.Name).Value;
                        var encoding = GetEncoding(section);
                        if (encoding == null)
                        {
                            ModelState.AddModelError("error", $"unknownEncoding for {key}");
                            return new JsonResult(new
                            {
                                id = "Title",
                                msg = "标题错误"
                            });
                        }
                        using (var streamReader = new StreamReader(
                            section.Body, encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        {
                            var value = await streamReader.ReadToEndAsync();
                            if (string.Equals(value, "undefine", StringComparison.OrdinalIgnoreCase))
                            {
                                value = string.Empty;
                            }
                            if (key.StartsWith("file")&&key!="file0")
                            {
                                filePaths.Add("path"+value);
                            }
                            formData.Append(key, value);
                            if (formData.ValueCount > _formOptions.ValueCountLimit)
                            {
                                ModelState.AddModelError("error", $"exceed formdata valueLImit {_formOptions.ValueCountLimit}");
                                return new JsonResult(new
                                {
                                    id = "File",
                                    msg = "内容过多"
                                });
                            }
                        }
                    }
                }
                section = await reader.ReadNextSectionAsync();
            }
            HttpContext.Request.Body.Position = 0;
            var results = formData.GetResults();
            if (!results.TryGetValue("Title", out var title))
            {
                ModelState.AddModelError("error", "title bindinf failed");
                return new JsonResult(new
                {
                    id = "Title",
                    msg = "标题不能为空"
                });
            }
            if (!results.TryGetValue("Texts", out var texts))
            {
                ModelState.AddModelError("error", "texts bindinf failed");
                return new JsonResult(new
                {
                    id = "Texts",
                    msg = "内容不能为空"
                });
            }
            if (title.ToString().Length > 70)
            {
                ModelState.AddModelError("Title", "标题过长，不得超过70字");
                return new JsonResult(new
                {
                    id = "Title",
                    msg = "标题过长，不得超过70字"
                });
            }
            if (!results.TryGetValue("articleId", out var articleId))
            {
                return new JsonResult(new
                {
                    id = "Title",
                    msg = "未知错误"
                });
            }
            if (!long.TryParse(articleId.ToString(), out var id))
            {
                return new JsonResult(new
                {
                    id = "Title",
                    msg = "未知错误"
                });
            }
            try
            {
              id = _secretProvider.UnProtectId(id);
            }
            catch(Exception)
            {
                return new JsonResult(new
                {
                    id = "Title",
                    msg = "未知错误"
                });
            }
            var article = await ApplicationDbContext.Articles.FindAsync(id);
            if (article == null)
            {
                return new JsonResult(new
                {
                    id = "Title",
                    msg = "未知错误"
                });
            }
            var isAuthorized2 = await AuthorizationService.AuthorizeAsync(User, article, RoleActions.Update);
            if (!isAuthorized2.Succeeded)
            {
                return new JsonResult(new
                {
                    id = "Article",
                    msg = "无权编辑"
                });
            }
            var paths = FileHelpers.GetFilePaths(article.Images);
            var pathsUsed = new List<int>();
            for(int i = 0; i <filePaths.Count; i++)
            {
                if (filePaths[i].StartsWith("path"))
                {                   
                    if(int.TryParse(filePaths[i].Substring("path".Length),out var pathId)
                        &&pathId>0&&pathId<paths.Length)
                    {
                        filePaths[i] = paths[pathId];
                        pathsUsed.Add(pathId);
                    }
                }
                else
                {
                    var fileName =filePaths[i];
                    var data = fileList[fileName];
                    var targetStoredPath = _targetStoredPath + "\\" + _secretProvider.ProtectId(article.Id);
                    if (!System.IO.Directory.Exists(targetStoredPath))
                    {
                        Directory.CreateDirectory(targetStoredPath);
                    }
                    var path = System.IO.Path.Combine(targetStoredPath, fileName);

                    using (var stream = System.IO.File.Create(path))
                    {
                        await stream.WriteAsync(data);
                    }
                    var relativePath = Path.GetRelativePath(targetStoredPath.Replace("images", ""), path);
                    filePaths[i]=relativePath;
                }
            }
            if (results.TryGetValue("file0", out var cover))
            {
                if (!string.IsNullOrEmpty(cover))
                {
                    filePaths.Insert(0, paths[0]);
                    pathsUsed.Add(0);
                }
            }
            for(int i = 0; i < paths.Length; i++)
            {
                if (!pathsUsed.Contains(i))
                {
                    System.IO.File.Delete(Path.Combine(_targetStoredPath.Replace("images",""),filePaths[i]));
                }
            }
            article.Texts = WebUtility.HtmlEncode(texts);
            article.Title = WebUtility.HtmlEncode(title);
            article.Images = String.Join(";", filePaths);
            await ApplicationDbContext.SaveChangesAsync();
            return new JsonResult(new
            {
                id = "Suc",
            });
        }
        */
        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader =
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF8.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }
    }

    public class DI_Page : PageModel
    {
        protected UserManager<MyUser> UserManager { get; }
        protected ApplicationDbContext ApplicationDbContext { get; }
        protected AppFileContext AppFileContext { get; }
        protected IAuthorizationService AuthorizationService { get; }
        //protected SignInManager<MyUser> SignInManager { get; }
        public DI_Page(UserManager<MyUser> userManager, ApplicationDbContext applicationDbContext
            , AppFileContext appFileContext, IAuthorizationService authorizationService)
        {
            UserManager = userManager;
            ApplicationDbContext = applicationDbContext;
            AppFileContext = appFileContext;
            AuthorizationService = authorizationService;
        }
    }

}
