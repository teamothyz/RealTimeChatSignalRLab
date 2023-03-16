using Microsoft.EntityFrameworkCore;
using RealTimeChatSignalRLab.Intentions;
using RealTimeChatSignalRLab.Models;

namespace RealTimeChatSignalRLab.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ChatDBContext dbcontext;

        public UserRepository(ChatDBContext context)
        {
            dbcontext = context;
        }

        public async Task<User> Create(User user)
        {
            var existed = await dbcontext.Users.AnyAsync(u => u.Email == user.Email);
            if (existed)
                throw new Exception("User already exist");
            await dbcontext.Users.AddAsync(user);
            await dbcontext.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetUserByEmails(List<string> emails)
        {
            var query = from user in dbcontext.Users
                        where emails.Contains(user.Email)
                        orderby user.Id
                        select user;
            return await query.ToListAsync();
        }

        public async Task<User?> SignIn(string email, string password)
        {
            return await dbcontext.Users
                .SingleOrDefaultAsync(user => user.Email == email 
                && user.Password == password);
        }

        public async Task<User?> GetUserById(Guid id)
        {
            var user = await dbcontext.Users.SingleOrDefaultAsync(user => user.Id == id);
            if (user != null)
            {
                return new User()
                {
                    Email = user.Email,
                    Password = null,
                    Fullname = user.Fullname,
                    Id = user.Id,
                };
            };
            return user;
        }
    }
}
