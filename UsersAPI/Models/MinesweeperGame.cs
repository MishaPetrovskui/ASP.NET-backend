using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsersAPI.Models
{
    public class MinesweeperGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int MinesCount { get; set; }
        public string MinesJson { get; set; } = "[]";
        public string OpenedJson { get; set; } = "[]";
        public string FlagsJson { get; set; } = "[]";
        public string Status { get; set; } = "playing";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }
    }
}