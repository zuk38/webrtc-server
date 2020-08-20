namespace ST.Web.Controllers
{
  using System.Linq;
  using System.Threading.Tasks;
  using ST.Models;
  using ST.Web.Models.Admin;
  using ST.Data;
  using Microsoft.AspNetCore.Authorization;
  using ST.WebApi.Controllers;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using global::WebApi.Services;
  using Microsoft.AspNetCore.Identity;
  using ST.WebApi.Repositories;

  [Authorize(Roles = "Admin")]
  [Route("api/admin")]
  public class AdminController : BaseApiController
  {
    private IUserService _userService;
    private UserManager<ApplicationUser> _userManager;
    private RoleManager<IdentityRole> _rolesManager;

    public AdminController(IAppData data, IUserService userService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> rolesManager) : base(data)
    {
      _userService = userService;
      _userManager = userManager;
      _rolesManager = rolesManager;
    }

    // GET api/Admin/InitDb
    [HttpGet]
    [Route("init")]
    [AllowAnonymous]
    public async Task<IActionResult> InitDbAsync()
    {

      await IdentitySeed.InitializeDbAsync(_rolesManager, _userManager);

      return this.Ok("Ok");
    }

    // GET api/Admin/Rooms
    [HttpGet]
    [Route("Rooms")]
    public IActionResult GetRooms([FromQuery]AdminGetRoomsBindingModel model)
    {
      if (model == null)
      {
        // When no parameters are passed, the model is null, so we create an empty model
        model = new AdminGetRoomsBindingModel();
      }

      // Validate the input parameters
      if (!ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var rooms = this.Data.Rooms.All().Include(r => r.Owner);

      return this.Ok(rooms);
    }   

    // GET api/Admin/Users
    [HttpGet]
    [Route("Users")]
    public IActionResult GetUsers([FromQuery]AdminGetUsersBindingModel model)
    {
      if (model == null)
      {
        // When no parameters are passed, the model is null, so we create an empty model
        model = new AdminGetUsersBindingModel();
      }

      // Validate the input parameters
      if (!ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      // Select all users along with their roles
      var users = this.Data.Users.All().Include(u => u.Roles).Include(u => u.Rooms);

      // Select the admin role ID
      var adminRoleId = this.Data.UserRoles.All().First(r => r.Name == "Admin").Id;

      // Select the columns to be returned 
      var usersToReturn = users.ToList().Select(u => new
      {
        id = u.Id,
        username = u.UserName,
        name = u.Name,
        email = u.Email,
        isAdmin = u.Roles.Any(r => r.Id == adminRoleId)
      });

      return this.Ok(usersToReturn);
    }

    // GET api/Admin/Users/id
    [HttpGet]
    [Route("Users/{id}")]
    public IActionResult GetUserProfileById(string id)
    {
      var user = this.Data.Users
          .All()
          .Include(x => x.Rooms)
          .Include(x => x.Roles)
          .FirstOrDefault(x => x.Id == id);

      if (user == null)
      {
        return this.BadRequest(string.Format("User # {0} not found: ", id));
      }

      var adminRoleId = this.Data.UserRoles.All().First(r => r.Name == "Admin").Id;
      var isAdmin = user.Roles.Any(r => r.Id == adminRoleId);

      var userToReturn = new
      {
        isAdmin,
        user.Id,
        user.UserName,
        user.Name,
        user.Email
      };

      return this.Ok(userToReturn);
    }

    // PUT api/Admin/User/{username}
    [HttpPut]
    [Route("User/{username}")]
    public IActionResult EditUserProfile(string username,
        [FromBody]AdminUpateUserBindingModel model)
    {
      if (!ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      // Find the user in the database
      var user = this.Data.Users.All().FirstOrDefault(x => x.UserName == username);
      if (user == null)
      {
        return this.BadRequest("User not found: username = " + username);
      }

      if (user.UserName == "admin")
      {
        return this.BadRequest("Edit profile for user 'admin' is not allowed!");
      }

      user.Name = model.Name;
      user.Email = model.Email;

      if (model.IsAdmin.HasValue)
      {
        if (model.IsAdmin.Value)
        {
          // Make the user administrator
          this._userManager.AddToRoleAsync(user, "Admin");
        }
        else
        {
          // Make the user non-administrator
          this._userManager.RemoveFromRoleAsync(user, "Admin");
        }
      }

      this.Data.SaveChanges();

      return this.Ok(
          new
          {
            message = "User " + user.UserName + " edited successfully.",
          }
      );
    }

    // PUT api/Admin/SetPassword
    [HttpPut]
    [Route("SetPassword")]
    public async Task<IActionResult> SetUserPassword(AdminSetPasswordBindingModel model)
    {
      if (!ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var user = await this.Data.Users.All().FirstOrDefaultAsync(u => u.UserName == model.Username);
      if (user == null)
      {
        return this.BadRequest("User not found: " + model.Username);
      }

      if (user.UserName == "admin")
      {
        return this.BadRequest("Password change for user 'admin' is not allowed!");
      }

      var removePassResult = await this._userManager.RemovePasswordAsync(user);
      if (!removePassResult.Succeeded)
      {
        return this.GetErrorResult(removePassResult);
      }

      var addPassResult = await this._userManager.AddPasswordAsync(user, model.NewPassword);
      if (!addPassResult.Succeeded)
      {
        return this.GetErrorResult(addPassResult);
      }

      return this.Ok(
          new
          {
            message = "Password for user " + user.UserName + " changed successfully.",
          }
      );
    }

    // DELETE /api/Admin/User/{username}
    /// <summary>
    /// Deletes user by username.
    /// </summary>
    [HttpDelete]
    [Route("User/{username}")]
    public async Task<IActionResult> DeleteUserAsync(string username)
    {
      var user = await this._userManager.FindByNameAsync(username);
      if (user == null)
      {
        return this.BadRequest("User not found: " + username);
      }

      if (user.UserName == "admin")
      {
        return this.BadRequest("Deleting user 'admin' is not allowed!");
      }

      var currentUserId = User.Identity.Name;
      if (user.Name == currentUserId)
      {
        return this.BadRequest("User cannot delete himself: " + username);
      }

      await this._userManager.DeleteAsync(user);

      return this.Ok(
         new
         {
           message = "User " + username + " deleted successfully."
         }
     );
    }
  }
}