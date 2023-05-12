using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Utilities;
using System;
using System.Threading.Tasks;

namespace MyWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly SecretProvider _secretProvider;
        private readonly UnloginUserManager _unloginUserManager;
        public UserController(ApplicationDbContext context,
            UserManager<MyUser> userManager,
            SecretProvider secretProvider,
            UnloginUserManager unloginUserManager)
        {
            _context = context;
            _userManager = userManager;
            _secretProvider = secretProvider;
            _unloginUserManager = unloginUserManager;
        }
        [HttpPost(nameof(FollowUser))]
        public async Task<JsonResult> FollowUser([FromBody] UsersName name)
        {
            if (name == null)
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return new JsonResult(new
                    {
                        id = "error",
                        msg = "错误"
                    });
                }
                currentUser = await _context.Users.FindAsync(currentUser.Id);
                var followUser = await _context.Users.SingleOrDefaultAsync(u => u.Name == name.Name);
                if (followUser == null)
                {
                    return new JsonResult(new
                    {
                        id = "error",
                        msg = "错误"
                    });
                }
                var followAction = await _context.UserFavourites
                    .SingleOrDefaultAsync(f => f.UserId == currentUser.Id && f.FavouriteUserId != null && f.FavouriteUserId == followUser.Id);
                if (followAction == null)
                {
                    await _context.UserFavourites.AddAsync(new UserFavourite
                    {
                        UserId = currentUser.Id,
                        FavouriteUserId = followUser.Id
                    });
                    currentUser.FollowsCount++;
                    followUser.FansCount++;
                    await _context.SaveChangesAsync();
                    return new JsonResult(new
                    {
                        id = 1
                    });
                }
                else
                {
                    _context.UserFavourites.Remove(followAction);
                    currentUser.FollowsCount--;
                    followUser.FansCount--;
                    await _context.SaveChangesAsync();
                    return new JsonResult(new
                    {
                        id = 0
                    });
                }
            }
            catch
            {
                return new JsonResult(new
                {
                    id = "error",
                    msg = "错误"
                });
            }

        }
        [HttpPost(nameof(ExamineUser))]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<JsonResult> ExamineUser([FromBody] ExamineUserAction userAction)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == userAction.Name);
                var examiner = await _userManager.GetUserAsync(User);
                var action = userAction.Action;
                if (action == 0)
                {
                    user.UserState = UserState.Confirmed;
                }
                else if (action == 1)
                {
                    user.UserState = UserState.UnConfirmed;
                }
                else if (action == 2)
                {
                    user.UserState = UserState.Baned;
                    if (user.Articles != null)
                    {
                        _context.RemoveRange(user.Articles);
                    }
                    if (user.Comments != null)
                    {
                        _context.RemoveRange(user.Comments);
                    }
                }
                var record = new ExamineRecord
                {
                    ExaminerId = examiner.Id,
                    UserId = user.Id,
                    Action = action,
                    Time = DateTime.Now,
                };
                var notify = MingklyHelpers.GetNotify(action, user, userAction.Text, _secretProvider);
                await _context.ExamineRecords.AddAsync(record);
                await _context.SystemNotifies.AddAsync(notify);
                await _context.SaveChangesAsync();
                return new JsonResult(new
                {
                    id = 0
                });
            }

            catch (Exception)
            {
                return new JsonResult(new
                {
                    id = 1,
                });
            }
        }
    }
    public class UsersName
    {
        public string Name { get; set; }
    }
    public record ExamineUserAction(string Name, int Action, string Text);
}
