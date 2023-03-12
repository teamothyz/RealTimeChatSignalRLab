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
    public class GroupChatModel : PageModel
    {
        private readonly IUserGroupRepository userGroupRepository;
        private readonly IGroupRepository groupRepository;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty]
        public string GroupName { get; set; }

        public PaginatedList<Tuple<Group, Message, bool>> UserGroups { get; set; }

        public GroupChatModel(IGroupRepository groupRepository, IUserGroupRepository userGroupRepository)
        {
            this.userGroupRepository = userGroupRepository;
            this.groupRepository = groupRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            UserGroups = await userGroupRepository.GetGroupsByUserId(PageIndex, Guid.Parse(userId));
            return Page();
        }

        public async Task<IActionResult> OnPostCreate()
        {
            var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = GroupName,
                Owner = Guid.Parse(userId),
            };
            await groupRepository.Create(group);
            return RedirectToPage("/Chat/GroupChat");
        }
    }
}
