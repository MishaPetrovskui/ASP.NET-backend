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

        public void RegisterUser(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            Add(user);
        }

        public User? Validate(UserLoginDTO credentials)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == credentials.Email);
            if (user == null)
                return null;
            if (!BCrypt.Net.BCrypt.Verify(credentials.Password, user.PasswordHash))
                return null;
            return user;
        }
    }
}
