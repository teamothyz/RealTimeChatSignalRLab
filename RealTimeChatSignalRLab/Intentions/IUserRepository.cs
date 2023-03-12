using RealTimeChatSignalRLab.Models;
using RealTimeChatSignalRLab.Pagination;

namespace RealTimeChatSignalRLab.Intentions
{
    public interface IUserRepository
    {
        Task<User?> GetUserById(Guid id);
        Task<User?> SignIn(string email, string password);
        Task<User> Create(User user);
        Task<List<User>> GetUserByEmails(List<string> emails);
    }
}
