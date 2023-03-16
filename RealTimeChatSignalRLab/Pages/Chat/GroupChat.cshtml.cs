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
    public class GroupChatModel : PageModel
    {
        private readonly IUserGroupRepository userGroupRepository;
        private readonly IGroupRepository groupRepository;

        [BindProperty]
        public string GroupName { get; set; }

        public List<Tuple<Group, Message?, bool>> UserGroups { get; set; }

        public GroupChatModel(IGroupRepository groupRepository, IUserGroupRepository userGroupRepository)
        {
            this.userGroupRepository = userGroupRepository;
            this.groupRepository = groupRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            UserGroups = await userGroupRepository.GetGroupsByUserId(Guid.Parse(userId), null);
            return Page();
        }

        public async Task<ContentResult> OnPostAsync(long offsetTime)
        {
            var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            var userGroups = await userGroupRepository.GetGroupsByUserId(Guid.Parse(userId), offsetTime);
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var content = JsonConvert.SerializeObject(userGroups, settings);
            return new ContentResult { Content = content };
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
            return RedirectToPage("/Chat/GroupChatMessages", new { group.Id });
        }
    }
}
