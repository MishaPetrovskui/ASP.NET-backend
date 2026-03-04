using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;
using UsersAPI.Models;

namespace UsersAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MinesweeperController : ControllerBase
    {
        private readonly UsersDbContext _ctx;
        public MinesweeperController(UsersDbContext ctx) { _ctx = ctx; }

        [HttpPost("new")]
        public async Task<IActionResult> NewGame(int rows = 9, int cols = 9, int mines = 10)
        {
            if (rows < 2 || cols < 2 || mines < 1 || mines >= rows * cols)
                return BadRequest("Невірні параметри");

            var rnd = new Random();
            var allCells = new List<int[]>();
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    allCells.Add(new[] { r, c });

            var minesList = allCells.OrderBy(_ => rnd.Next()).Take(mines).ToList();

            var code = Guid.NewGuid().ToString("N")[..8].ToUpper();

            var game = new MinesweeperGame
            {
                Code = code,
                Rows = rows,
                Cols = cols,
                MinesCount = mines,
                MinesJson = JsonSerializer.Serialize(minesList),
                OpenedJson = "[]",
                FlagsJson = "[]",
                Status = "playing"
            };

            _ctx.MinesweeperGames.Add(game);
            await _ctx.SaveChangesAsync();

            return Ok(new { game.Id, game.Code, game.Rows, game.Cols, game.MinesCount, game.Status, game.CreatedAt });
        }
        [HttpPost("{code}/open")]
        public async Task<IActionResult> Open(string code, [FromBody] MoveRequest req)
        {
            var game = await _ctx.MinesweeperGames.FirstOrDefaultAsync(g => g.Code == code);
            if (game == null) return NotFound();
            if (game.Status != "playing") return BadRequest("Гра вже завершена");

            var mines = JsonSerializer.Deserialize<List<int[]>>(game.MinesJson)!;
            var opened = JsonSerializer.Deserialize<List<int[]>>(game.OpenedJson)!;
            var flags = JsonSerializer.Deserialize<List<int[]>>(game.FlagsJson)!;

            bool IsMine(int r, int c) => mines.Any(m => m[0] == r && m[1] == c);
            bool IsOpened(int r, int c) => opened.Any(o => o[0] == r && o[1] == c);
            bool IsFlag(int r, int c) => flags.Any(f => f[0] == r && f[1] == c);

            int CountAround(int r, int c)
            {
                int count = 0;
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                        if (dr != 0 || dc != 0)
                            if (r + dr >= 0 && r + dr < game.Rows && c + dc >= 0 && c + dc < game.Cols)
                                if (IsMine(r + dr, c + dc)) count++;
                return count;
            }
            if (IsMine(req.Row, req.Col))
            {
                game.Status = "lost";
                game.FinishedAt = DateTime.UtcNow;
                await _ctx.SaveChangesAsync();
                return Ok(new
                {
                    status = "lost",
                    newOpened = new List<object>(),
                    mines = mines
                });
            }
            var newOpened = new List<int[]>();
            var queue = new Queue<(int, int)>();

            if (!IsOpened(req.Row, req.Col) && !IsFlag(req.Row, req.Col))
                queue.Enqueue((req.Row, req.Col));

            while (queue.Count > 0)
            {
                var (r, c) = queue.Dequeue();
                if (IsOpened(r, c) || newOpened.Any(o => o[0] == r && o[1] == c)) continue;

                newOpened.Add(new[] { r, c });

                if (CountAround(r, c) == 0)
                {
                    for (int dr = -1; dr <= 1; dr++)
                        for (int dc = -1; dc <= 1; dc++)
                            if (dr != 0 || dc != 0)
                            {
                                int nr = r + dr, nc = c + dc;
                                if (nr >= 0 && nr < game.Rows && nc >= 0 && nc < game.Cols)
                                    if (!IsOpened(nr, nc) && !IsMine(nr, nc))
                                        queue.Enqueue((nr, nc));
                            }
                }
            }

            opened.AddRange(newOpened);
            game.OpenedJson = JsonSerializer.Serialize(opened);

            int totalSafe = game.Rows * game.Cols - game.MinesCount;
            if (opened.Count >= totalSafe)
            {
                game.Status = "won";
                game.FinishedAt = DateTime.UtcNow;
            }

            await _ctx.SaveChangesAsync();

            return Ok(new
            {
                status = game.Status,
                newOpened = newOpened.Select(o => new { row = o[0], col = o[1], number = CountAround(o[0], o[1]) }),
                mines = game.Status == "lost" ? mines : null
            });
        }
        [HttpPost("{code}/flag")]
        public async Task<IActionResult> Flag(string code, [FromBody] MoveRequest req)
        {
            var game = await _ctx.MinesweeperGames.FirstOrDefaultAsync(g => g.Code == code);
            if (game == null) return NotFound();
            if (game.Status != "playing") return BadRequest("Гра вже завершена");

            var flags = JsonSerializer.Deserialize<List<int[]>>(game.FlagsJson)!;
            var existing = flags.FirstOrDefault(f => f[0] == req.Row && f[1] == req.Col);

            if (existing != null)
                flags.Remove(existing);
            else
                flags.Add(new[] { req.Row, req.Col });

            game.FlagsJson = JsonSerializer.Serialize(flags);
            await _ctx.SaveChangesAsync();

            return Ok(new { flags = flags.Select(f => new { row = f[0], col = f[1] }) });
        }
        [HttpGet("{code}")]
        public async Task<IActionResult> GetGame(string code)
        {
            var game = await _ctx.MinesweeperGames.FirstOrDefaultAsync(g => g.Code == code);
            if (game == null) return NotFound();

            var opened = JsonSerializer.Deserialize<List<int[]>>(game.OpenedJson)!;
            var flags = JsonSerializer.Deserialize<List<int[]>>(game.FlagsJson)!;
            var mines = JsonSerializer.Deserialize<List<int[]>>(game.MinesJson)!;

            int CountAround(int r, int c)
            {
                int count = 0;
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                        if (dr != 0 || dc != 0)
                            if (r + dr >= 0 && r + dr < game.Rows && c + dc >= 0 && c + dc < game.Cols)
                                if (mines.Any(m => m[0] == r + dr && m[1] == c + dc)) count++;
                return count;
            }

            return Ok(new
            {
                game.Id,
                game.Code,
                game.Rows,
                game.Cols,
                game.MinesCount,
                game.Status,
                game.CreatedAt,
                game.FinishedAt,
                opened = opened.Select(o => new { row = o[0], col = o[1], number = CountAround(o[0], o[1]) }),
                flags = flags.Select(f => new { row = f[0], col = f[1] }),
                mines = game.Status != "playing" ? mines.Select(m => new { row = m[0], col = m[1] }) : null
            });
        }
        [HttpGet("history")]
        public async Task<IActionResult> History(int page = 1, int size = 10)
        {
            var total = await _ctx.MinesweeperGames.CountAsync();
            var items = await _ctx.MinesweeperGames
                .OrderByDescending(g => g.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(g => new {
                    g.Id,
                    g.Code,
                    g.Rows,
                    g.Cols,
                    g.MinesCount,
                    g.Status,
                    g.CreatedAt,
                    g.FinishedAt
                })
                .ToListAsync();

            return Ok(new
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PagesCount = (int)Math.Ceiling(total / (double)size),
                PageSize = size
            });
        }
    }
}