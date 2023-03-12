using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;
using RealTimeChatSignalRLab.Pagination;
using System.Security.Claims;

namespace RealTimeChatSignalRLab.Pages.Chat
{
    [Authorize]
    public class GroupChatMessagesModel : PageModel
    {
        private readonly IUserGroupRepository userGroupRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IMessageRepository messageRepository;

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public PaginatedList<Tuple<User, Message>> Messages { get; set; }

        public GroupChatMessagesModel(IGroupRepository groupRepository,
            IUserGroupRepository userGroupRepository,
            IMessageRepository messageRepository)
        {
            this.userGroupRepository = userGroupRepository;
            this.groupRepository = groupRepository;
            this.messageRepository = messageRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            Messages = await messageRepository.GetGroupMessages(1, Guid.Parse(userId), Id);
            return Page();
        }

        public async Task<ContentResult> OnPostAsync(int index)
        {
            var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            var messages = await messageRepository.GetGroupMessages(index, Guid.Parse(userId), Id);
            var content = JsonConvert.SerializeObject(messages);
            return Content(content);
        }
    }
}
