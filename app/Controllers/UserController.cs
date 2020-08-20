namespace ST.Web.Controllers
{
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using global::WebApi.Services;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using ST.Data;
  using ST.Models;
  using ST.Web.Models.Users;
  using ST.WebApi.Controllers;

  [Authorize]
  [ApiController]
  [Route("api/users")]
  public class UserController : BaseApiController
  {
    private UserManager<ApplicationUser> _userManager;
    private IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserController(IAppData data, IUserService userService, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor) : base(data)
    {
      _userService = userService;
      _httpContextAccessor = httpContextAccessor;
      _userManager = userManager;
    }

    // POST api/User/Register
    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> RegisterUser(RegisterUserBindingModel model)
    {
      var currentUser = this.Data.Users.GetById(User.Identity.Name);
      var userRoles = await this._userManager.GetRolesAsync(currentUser);

      if (!userRoles.Any(r => r == "Presenter" || r == "Admin"))
      {
        return this.BadRequest("User is not allowed to create room");
      }

      if (model == null)
      {
        return this.BadRequest("Invalid user data");
      }

      if (!ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var user = new ApplicationUser
      {
        UserName = model.Username,
        Name = model.Name,
        Email = model.Email,
        SpacedeckPass = model.SpacedeckPass,
        SpacedeckId = model.SpacedeckId
      };

      var identityResult = await this._userManager.CreateAsync(user, model.Password);

      if (!identityResult.Succeeded)
      {
        return this.GetErrorResult(identityResult);
      }

      var roleResult = await this._userManager.AddToRoleAsync(user, "Client");

      if (!roleResult.Succeeded)
      {
        return this.GetErrorResult(roleResult);
      }

      return Ok(new {
        this.Data.Users.All().FirstOrDefault(u => u.Email == model.Email).Id
      });
    }

    // POST api/User/Login
    [HttpPost]
    [AllowAnonymous]
    [Route("Login")]
    public async Task<IActionResult> LoginUserAsync([FromBody]LoginUserBindingModel model)
    {
      if (model == null)
      {
        return this.BadRequest("Invalid user data");
      }

      var user = this.Data.Users.All().FirstOrDefault(u => u.UserName == model.Username || u.Email == model.Username);

      if (user == null)
      {
        return this.BadRequest("Invalid username");
      }

      var passwordCheck = this._userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

      if (passwordCheck == PasswordVerificationResult.Success)
      {
        return this.Ok(new {
          token = this._userService.Authenticate(model.Username, model.Password)
        });
      } 
      
      return this.BadRequest("Invalid password");
    }

    // POST api/User/Check
    [HttpPost]
    [Route("Check")]
    public IActionResult CheckUser(CheckUserBindingModel model)
    {
      var user = this.Data.Users.All().Where(u => u.Email == model.Email).FirstOrDefault();

      if (user == null)
      {
       return this.BadRequest("User not found.");
      }

      return this.Ok(
          new
          {
            user.Id,
            user.SpacedeckId
          }
      );
    }

    // POST api/User/Logout
    [HttpPost]
    [Route("Logout")]
    public IActionResult Logout()
    {
      var currentUserId = this.User.Identity.Name;
      var currentUser = this.Data.Users.All().FirstOrDefault(x => x.Id == currentUserId);
      if (currentUser == null)
      {
        return this.BadRequest("Invalid user token! Please login again!");
      }

      currentUser.Token = null;

      Data.SaveChanges();

      return this.Ok(
          new
          {
            message = "Logout successful."
          }
      );
    }

    // PUT api/User/ChangePassword
    //[HttpPut]
    //[Route("ChangePassword")]
    //public async Task<IActionResult> ChangeUserPassword(ChangePasswordBindingModel model)
    //{
    //  if (!ModelState.IsValid)
    //  {
    //    return this.BadRequest(this.ModelState);
    //  }

    //  if (User.Identity.Name == "admin")
    //  {
    //    return this.BadRequest("Password change for user 'admin' is not allowed!");
    //  }

    //  var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

    //  IdentityResult result = await this._userManager.ChangePasswordAsync(model.OldPassword, model.NewPassword);

    //  if (!result.Succeeded)
    //  {
    //    return this.GetErrorResult(result);
    //  }

    //  return this.Ok(
    //      new
    //      {
    //        message = "Password changed successfully.",
    //      }
    //  );
    //}

    // GET api/Users/Profile
    [HttpGet]
    [Route("Profile")]
    public async Task<IActionResult> GetUserProfileAsync()
    {
      if (!ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      // Validate the current user exists in the database
      var currentUserId = this.User.Identity.Name;
      var currentUser = this.Data.Users
        .All()
        .Include(x => x.Roles)
        .FirstOrDefault(x => x.Id == currentUserId);
      if (currentUser == null)
      {
        return this.BadRequest("Invalid user token! Please login again!");
      }

      var roles = await this._userManager.GetRolesAsync(currentUser);

      var userToReturn = new
      {
        currentUser.Id,
        currentUser.Name,
        currentUser.Email,
        currentUser.SpacedeckPass,
        CanCreateRoom = roles.Any(r => r == "Presenter" || r == "Admin")
      };

      return this.Ok(userToReturn);
    }

    // PUT api/Users/Profile
    [HttpPut]
    [Route("Profile")]
    public IActionResult EditUserProfile(EditUserProfileBindingModel model)
    {
      if (!ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      // Validate the current user exists in the database
      var currentUserId = this.User.Identity.Name;
      var currentUser = this.Data.Users.All().FirstOrDefault(x => x.Id == currentUserId);
      if (currentUser == null)
      {
        return this.BadRequest("Invalid user token! Please login again!");
      }

      if (User.Identity.Name == "admin")
      {
        return this.BadRequest("Edit profile for user 'admin' is not allowed!");
      }

      var hasEmailTaken = this.Data.Users.All().Any(x => x.Email == model.Email);
      if (hasEmailTaken)
      {
        return this.BadRequest("Invalid email. The email is already taken!");
      }

      currentUser.Name = model.Name;
      currentUser.Email = model.Email;
      currentUser.PhoneNumber = model.PhoneNumber;

      this.Data.SaveChanges();

      return this.Ok(
          new
          {
            message = "User profile edited successfully.",
          });
    }
  }
}