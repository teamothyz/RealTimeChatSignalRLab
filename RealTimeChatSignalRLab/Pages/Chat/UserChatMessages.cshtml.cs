using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RealTimeChatSignalRLab.Pages.Chat
{
    [Authorize]
    public class UserChatMessagesModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        public UserChatMessagesModel() 
        {
        }

        public void OnGet()
        {
        }
    }
}
