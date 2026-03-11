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
        public User? GetUserById(int id) => _context.Users.FirstOrDefault(u => u.Id == id);
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
        public void Update(User user)
        {
            var existing = _context.Users.Find(user.Id);
            if (existing == null) return;
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Birthday = user.Birthday;
            existing.Gender = user.Gender;
            existing.Role = user.Role;
            _context.SaveChanges();
        }

    }
}
