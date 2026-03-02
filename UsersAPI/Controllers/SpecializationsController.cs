using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsersAPI.Models;

namespace UsersAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpecializationsController : ControllerBase
    {
        private readonly UsersDbContext _ctx;
        public SpecializationsController(UsersDbContext ctx) { _ctx = ctx; }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int size = 10)
        {
            if (page < 1 || size < 1) return BadRequest("Invalid paging parameters");
            var total = await _ctx.Specializations.CountAsync();
            var items = await _ctx.Specializations.OrderBy(d => d.Id).Skip((page - 1) * size).Take(size).ToListAsync();
            return Ok(new { Items = items, TotalCount = total, Page = page, PagesCount = (int)Math.Ceiling(total / (double)size), PageSize = size });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var d = await _ctx.Specializations.FindAsync(id);
            if (d == null) return NotFound();
            return Ok(d);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Specialization dep)
        {
            _ctx.Specializations.Add(dep);
            await _ctx.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = dep.Id }, dep);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Specialization dep)
        {
            var ex = await _ctx.Specializations.FindAsync(id);
            if (ex == null) return NotFound();
            ex.Name = dep.Name;
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ex = await _ctx.Specializations.FindAsync(id);
            if (ex == null) return NotFound();
            _ctx.Specializations.Remove(ex);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
