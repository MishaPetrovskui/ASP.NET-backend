using Microsoft.AspNetCore.Mvc;
using UsersAPI.Services;
using UsersAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UsersAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController: ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        public UsersController(UserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<List<User>> GetUsers()
        {
            return Ok(_userService.GetAll());
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public ActionResult UpdateUser(int id, [FromBody] User user)
        {
            user.Id = id;
            _userService.Update(user);
            return Ok();
        }
        [Authorize]
        [HttpGet("me")]
        public ActionResult<List<User>> GetMe()
        {
            string NameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int id = Convert.ToInt32(NameIdentifier);
            return Ok(_userService.GetUserById(id));
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

        [HttpPost("register")]
        public ActionResult RegisterUser([FromBody] User user)
        {
            _userService.RegisterUser(user);
            return Ok();
        }
        [HttpPost("login")]
        public ActionResult LoginUser([FromBody] UserLoginDTO credentials)
        {
            var user = _userService.Validate(credentials);
            if (user == null)
                return Unauthorized();
            var token = _tokenService.GenerateToken(user);
            return Ok(token);
        }
    }
}
