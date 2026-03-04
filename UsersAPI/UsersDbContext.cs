using Microsoft.EntityFrameworkCore;
using UsersAPI.Models;

namespace UsersAPI
{
    public class UsersDbContext: DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) 
            : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<UsersAPI.Models.Doctor> Doctors { get; set; }
        public DbSet<UsersAPI.Models.Department> Departments { get; set; }
        public DbSet<UsersAPI.Models.Specialization> Specializations { get; set; }
        public DbSet<UsersAPI.Models.DoctorSpecialization> DoctorsSpecializations { get; set; }
        public DbSet<UsersAPI.Models.MinesweeperGame> MinesweeperGames { get; set; }
    }
}
