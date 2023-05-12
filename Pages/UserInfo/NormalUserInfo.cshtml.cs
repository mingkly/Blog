using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class NormalUserInfoModel : PageModel
    {
        private readonly UserManager<MyUser> _userManager;
        private readonly ApplicationDbContext _context;
        public NormalUserInfoModel(UserManager<MyUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public MyUser MyUser { get; set; }
        [BindProperty]
        public string UserName { get; set; }
        public async Task<IActionResult> OnGet(string userName)
        {
            if (userName == null)
            {
                return NotFound();
            }
            if (User.Identity.Name == userName)
            {
                return RedirectToPage("UserInfo");
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }
            var thisUser = await _userManager.GetUserAsync(User);

            var followed = await _context.UserFavourites
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == thisUser.Id && u.FavouriteUserId == user.Id);
            if (followed != null)
            {
                ViewData["Followed"] = "ÒÑ¹Ø×¢";
            }
            else
            {
                ViewData["Followed"] = "¹Ø×¢";
            }
            MyUser = user;
            if (string.IsNullOrEmpty(user.Avatar))
            {
                MyUser.Avatar = "images/daxiong.gif";
            }

            if (string.IsNullOrEmpty(user.Background))
            {
                MyUser.Background = "images/avatar.jpg";
            }

            return Page();
        }
        /*
        public async Task<IActionResult> OnPostAsync()
        {
            if (UserName == null)
            {
                return NotFound() ;
            }
            var user = await _userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                return NotFound();
            }
            var thisUser = await _userManager.GetUserAsync(User);
            var followed = await _context.UserFavourites.SingleOrDefaultAsync(u => u.UserId == thisUser.Id && u.FavouriteUserId == user.Id);
            if (followed != null)
            {
                 _context.UserFavourites.Remove(followed);
                await _context.SaveChangesAsync();
                return RedirectToPage(new { userName=user.Name});
            }
            var userFavourite = new UserFavourite
            {
                UserId = thisUser.Id,
                FavouriteUserId = user.Id
            };
            await _context.UserFavourites.AddAsync(userFavourite);
            await _context.SaveChangesAsync();
            return RedirectToPage(new { userName = user.Name });
        }
        */
        public async Task<IActionResult> OnPostFollowUserAsync([FromBody] UsersName name)
        {
            if (name == null)
            {
                return BadRequest();
            }
            var user = await _userManager.FindByNameAsync(name.Name);
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                var followAction = await _context.UserFavourites
                    .SingleOrDefaultAsync(f => f.User.UserName == User.Identity.Name && f.FavouriteUserId != null && f.FavouriteUser.UserName == name.Name);
                if (followAction == null)
                {

                    var thisUser = await _userManager.GetUserAsync(User);
                    await _context.UserFavourites.AddAsync(new UserFavourite
                    {
                        UserId = thisUser.Id,
                        FavouriteUserId = user.Id
                    });
                    await _context.SaveChangesAsync();
                    return new JsonResult(new
                    {
                        follow = 1
                    });
                }
                else
                {
                    _context.UserFavourites.Remove(followAction);
                    await _context.SaveChangesAsync();
                    return new JsonResult(new
                    {
                        follow = 0
                    });
                }
            }
            catch
            {
                return BadRequest();
            }

        }
        public class UsersName
        {
            public string Name { get; set; }
        }
    }

}
