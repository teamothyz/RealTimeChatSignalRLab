using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;
using System.Security.Claims;

namespace RealTimeChatSignalRLab.Pages.Chat
{
    [Authorize]
    public class UserChatMessagesModel : PageModel
    {
        private readonly IUserGroupRepository userGroupRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IUserRepository userRepository;

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public User? Participant { get; set; }

        public List<Tuple<User, Message>> Messages { get; set; }

        public UserChatMessagesModel(IGroupRepository groupRepository,
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
            Participant = await userRepository.GetUserById(Id);
            if (Participant == null)
                return BadRequest();
            Messages = await messageRepository.GetUserMessages(1, Guid.Parse(userId), Id, null);
            return Page();
        }

        public async Task<ContentResult> OnPostAsync(int index, long? offsetTime)
        {
            var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            var messages = await messageRepository.GetUserMessages(index, Guid.Parse(userId), Id, offsetTime);
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var content = JsonConvert.SerializeObject(messages, settings);
            return Content(content);
        }
    }
}
