using UsersAPI.Models;

namespace UsersAPI.Services
{
    public class UserService
    {
        private readonly UsersDbContext _context;

        public UserService(UsersDbContext context)
        {
            _context = context;
        }

        public List<User> GetAll() => _context.Users.ToList();
        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
    }
}
