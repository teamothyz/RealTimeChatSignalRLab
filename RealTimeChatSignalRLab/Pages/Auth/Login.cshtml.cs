using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;
using System.Security.Claims;

namespace RealTimeChatSignalRLab.Pages.Auth
{
    [BindProperties]
    public class LoginModel : PageModel
    {
        private readonly ChatDBContext dBContext;
        private readonly IUserRepository userRepository;

        [BindProperty(SupportsGet = true)]
        public bool Error { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public string Message { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public LoginModel(ChatDBContext context, IUserRepository userRepository)
        {
            dBContext = context;
            this.userRepository = userRepository;
        }

        public IActionResult OnGetAsync()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToPage("/chat/userchat");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var user = await userRepository.SignIn(Email, Password);
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Fullname),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "login");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync("CookieAuthentication", claimsPrincipal);
                    return RedirectToPage("/chat/userchat");
                }
                return RedirectToPage("/auth/login", new { Error = true, Message = "Invalid email or password" });
            }
            catch
            {
                return RedirectToPage("/auth/login", new { Error = true, Message = "Something went wrong" });
            }
        }
    }
}
