using Microsoft.AspNetCore.Mvc;
using UsersAPI.Services;
using UsersAPI.Models;

namespace UsersAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController: ControllerBase
    {
        private readonly UserService _userService;
        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public ActionResult<List<User>> GetUsers()
        {
            return Ok(_userService.GetAll());
        }

        [HttpGet("paged/{page?}")]
        public ActionResult<PagedResult<User>> GetUsersPaged(int page = 1, int size = 5)
        {
            List<User> users = _userService.GetAll();
            return Ok(new PagedResult<User>()
            {
                Items = users.GetRange((page - 1) * size,
                    Math.Min(users.Count - (page - 1) * size, size)
                ),
                TotalCount = users.Count,
                Page = page,
                PagesCount = Convert.ToInt32(Math.Ceiling((double)users.Count / size)),
                PageSize = size
            });
        }

        [HttpPost]
        public ActionResult AddUser([FromBody]User user)
        {
            _userService.Add(user);
            return Ok();
        }
    }
}
