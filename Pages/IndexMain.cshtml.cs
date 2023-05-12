using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using System.Collections.Generic;
using System.Linq;

namespace MyWeb.Pages
{
    [AllowAnonymous]
    public class IndexMainModel : PageModel
    {


        public  IActionResult OnGet()
        {
            return RedirectToPage("/Articles/index");

        }
    }
    public class CountInfo
    {
        public string UserName { get; set; }
        public int UnreadReplyCount { get; set; }
        public IEnumerable<CountWithName> UnreadChatCount { get; set; }
        public IEnumerable<CountWithName> FavouriteArticleCount { get; set; }
        public int NotifyCount { get; set; }
    }
    public class CountWithName
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
