using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;
using RealTimeChatSignalRLab.Pagination;
using System.Security.Claims;

namespace RealTimeChatSignalRLab.Pages.Chat
{
    [Authorize]
    public class UserChatModel : PageModel
    {
        private readonly IUserGroupRepository userGroupRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IUserRepository userRepository;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1; 
        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        public PaginatedList<Tuple<User, Message?, bool>> Users { get; set; }

        public UserChatModel(IGroupRepository groupRepository,
            IUserGroupRepository userGroupRepository,
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            this.userGroupRepository = userGroupRepository;
            this.groupRepository = groupRepository;
            this.messageRepository = messageRepository;
            this.userRepository = userRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            Users = await messageRepository.GetUserChat(PageIndex, Guid.Parse(userId));
            return Page();
        }

        public async Task<IActionResult> OnPostNewChat()
        {
            var users = await userRepository.GetUserByEmails(new List<string> { Email });
            return RedirectToPage("/Chat/UserChatMessages", new { users[0].Id });
        }
    }
}
