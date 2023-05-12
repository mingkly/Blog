using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MyWeb.Pages.UserInfo
{
    public class UserSecurityModel : PageModel
    {
        [BindProperty]
        public UserInfo ThisUser { get; set; }
        private readonly SignInManager<MyUser> _signInManager;
        private readonly UserManager<MyUser> UserManager;
        public UserSecurityModel(SignInManager<MyUser> signInManager, UserManager<MyUser> userManager)
        {
            _signInManager = signInManager;
            UserManager = userManager;
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                var user = await UserManager.GetUserAsync(User);
                if (user is null)
                {
                    return NotFound();
                }
                var res = await UserManager.ChangePasswordAsync(user, ThisUser.CurrentPassword, ThisUser.Password);
                if (res.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                }
                else
                {
                    foreach (var error in res.Errors)
                    {
                        ModelState.AddModelError("error", error.Description);
                    }
                    return Page();
                }
            }
            catch
            {
                return BadRequest();
            }
            return RedirectToPage();
        }

    }
    public class UserInfo
    {
        [MaxLength(100)]
        public string Name { get; set; }

        public string Avatar { get; set; }
        public string Backgroud { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "原密码")]
        public string CurrentPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [Compare("Password", ErrorMessage = "必须一致")]
        public string ConfirmPassword { get; set; }
    }
}
