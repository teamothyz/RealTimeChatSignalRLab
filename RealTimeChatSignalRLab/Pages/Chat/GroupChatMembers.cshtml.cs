using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;
using System.Security.Claims;

namespace RealTimeChatSignalRLab.Pages.Chat
{
    [Authorize]
    public class GroupChatMembersModel : PageModel
    {
        private readonly IUserGroupRepository userGroupRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IMessageRepository messageRepository;

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public Group Group { get; set; }

        public List<User> Members { get; set; }

        public bool IsOwner { get; set; }

        public GroupChatMembersModel(IGroupRepository groupRepository,
            IUserGroupRepository userGroupRepository,
            IMessageRepository messageRepository)
        {
            this.userGroupRepository = userGroupRepository;
            this.groupRepository = groupRepository;
            this.messageRepository = messageRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Group = await groupRepository.GetGroup(Id);
            var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            Members = await userGroupRepository.GetGroupUsersByGroupId(null, Guid.Parse(userId), Id);
            IsOwner = await groupRepository.CheckOwner(Guid.Parse(userId), Id);
            return Page();
        }
    }
}
