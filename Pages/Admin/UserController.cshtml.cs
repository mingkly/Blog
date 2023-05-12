using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyWeb.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Pages.Admin
{
    public class UserControllerModel : PageModel
    {
        private readonly VipNumberContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly MyAuthorizationManager _manager;
        public UserControllerModel(VipNumberContext context,
            UserManager<MyUser> userManager,
            MyAuthorizationManager manager)
        {
            _context = context;
            _manager = manager;
            _userManager = userManager;
        }
        [TempData]
        public string Message { get; set; }
        [BindProperty]
        public VirtualUser[] Users { get; set; }
        public int Number { get; private set; }
        public string Type { get; set; }
        public void OnGet()
        {
            if (User.IsInRole(RoleNames.Administrator))
            {
                Type = "管理员";
            }
            else
            {
                Type = "no";
            }

        }
        public async Task<IActionResult> OnPostAsync()
        {
            int number = new Random().Next(20000);
            await _context.VipNumbers.AddAsync(new VipNumber
            {
                Id = number,
                Used = false
            });
            await _context.SaveChangesAsync();
            Number = number;
            return Page();
        }
        public async Task<IActionResult> OnPostUpdateToAdminAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            await _manager.UpdateUserToAdmin(user.Id);
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostAddUsersAsync(VirtualUser[] users)
        {
            var password = @"Ming1998@";
            foreach (var user in users)
            {
                var u = new MyUser
                {
                    Name = user.Name,
                    UserName = user.Name,
                    ShowText = user.ShowText
                };
                var res = await _userManager.CreateAsync(u, password);
                if (res.Succeeded)
                {
                    Message = "成功";
                }
                else
                {
                    foreach (var x in res.Errors.ToList())
                    {
                        Message += x.Description;
                    }
                }
            }

            return Page();
        }
        public class VirtualUser
        {
            public string Name { get; set; }
            public string ShowText { get; set; }
        }
    }
}
