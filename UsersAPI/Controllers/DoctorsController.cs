using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsersAPI.Models;

namespace UsersAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly UsersDbContext _ctx;
        public DoctorsController(UsersDbContext ctx) { _ctx = ctx; }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int size = 10)
        {
            if (page < 1 || size < 1) return BadRequest("Invalid paging parameters");
            var total = await _ctx.Doctors.CountAsync();
            var items = await _ctx.Doctors
                .OrderBy(d => d.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            // load department name and specializations
            var result = new List<object>();
            foreach (var d in items)
            {
                var dept = await _ctx.Departments.FindAsync(d.DepartmentId);
                var specs = await (from ds in _ctx.DoctorsSpecializations
                                   join s in _ctx.Specializations on ds.SpecializationId equals s.Id
                                   where ds.DoctorId == d.Id
                                   select new { s.Id, s.Name }).ToListAsync();
                result.Add(new {
                    d.Id,
                    d.Name,
                    d.Surname,
                    d.Salary,
                    d.Premium,
                    departmentId = d.DepartmentId,
                    departmentName = dept?.Name,
                    specializations = specs
                });
            }

            return Ok(new {
                Items = result,
                TotalCount = total,
                Page = page,
                PagesCount = (int)Math.Ceiling(total / (double)size),
                PageSize = size
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var d = await _ctx.Doctors.FindAsync(id);
            if (d == null) return NotFound();
            var dept = await _ctx.Departments.FindAsync(d.DepartmentId);
            var specs = await (from ds in _ctx.DoctorsSpecializations
                               join s in _ctx.Specializations on ds.SpecializationId equals s.Id
                               where ds.DoctorId == d.Id
                               select new { s.Id, s.Name }).ToListAsync();
            return Ok(new {
                d.Id,
                d.Name,
                d.Surname,
                d.Salary,
                d.Premium,
                departmentId = d.DepartmentId,
                departmentName = dept?.Name,
                specializations = specs
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Doctor doctor)
        {
            _ctx.Doctors.Add(doctor);
            await _ctx.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = doctor.Id }, doctor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Doctor doctor)
        {
            var ex = await _ctx.Doctors.FindAsync(id);
            if (ex == null) return NotFound();
            ex.Name = doctor.Name;
            ex.Surname = doctor.Surname;
            ex.Salary = doctor.Salary;
            ex.Premium = doctor.Premium;
            ex.DepartmentId = doctor.DepartmentId;
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ex = await _ctx.Doctors.FindAsync(id);
            if (ex == null) return NotFound();
            // remove specializations links
            var links = _ctx.DoctorsSpecializations.Where(x => x.DoctorId == id);
            _ctx.DoctorsSpecializations.RemoveRange(links);
            _ctx.Doctors.Remove(ex);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/specializations/{specId}")]
        public async Task<IActionResult> AddSpecialization(int id, int specId)
        {
            var doc = await _ctx.Doctors.FindAsync(id);
            var spec = await _ctx.Specializations.FindAsync(specId);
            if (doc == null || spec == null) return NotFound();
            if (await _ctx.DoctorsSpecializations.AnyAsync(x => x.DoctorId == id && x.SpecializationId == specId))
                return BadRequest("Already exists");
            _ctx.DoctorsSpecializations.Add(new DoctorSpecialization { DoctorId = id, SpecializationId = specId });
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}/specializations/{specId}")]
        public async Task<IActionResult> RemoveSpecialization(int id, int specId)
        {
            var link = await _ctx.DoctorsSpecializations.FirstOrDefaultAsync(x => x.DoctorId == id && x.SpecializationId == specId);
            if (link == null) return NotFound();
            _ctx.DoctorsSpecializations.Remove(link);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
