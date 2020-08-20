namespace ST.Web.Controllers
{
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Text;
  using System.Threading.Tasks;
  using global::WebApi.Notifications;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.SignalR;
  using ST.Data;
  using ST.Models;
  using ST.WebApi.Controllers;

  [Authorize]
  [Route("api/link")]
  public class LinkController : BaseApiController
  {
    private UserManager<ApplicationUser> _userManager;
    private readonly IHostingEnvironment _hostEnvironment;
    private readonly IHubContext<RoomsHub> _hubContext;

    public LinkController(IAppData data, IHostingEnvironment env, UserManager<ApplicationUser> userManager, IHubContext<RoomsHub> hubContext) : base(data)
    {
      this._hostEnvironment = env;

      _userManager = userManager;
      _hubContext = hubContext;
    }


    // GET api/link/{id}
    [HttpGet]
    [Route("{id:int}")]
    public IActionResult GetLinkById(int id)
    {
      var link = this.Data.Links.GetById(id);
      if (link == null)
      {
        return this.BadRequest("Link #" + id + " not found!");
      }

      // Validate the current user ownership over the ad
      var currentUserId = this.User.Identity.Name;
      if (link.OwnerId != currentUserId)
      {
        return this.Unauthorized();
      }

      return this.Ok(link);
    }

    // GET api/link
    [HttpGet]
    [Route("All")]
    public IActionResult GetAll()
    {
      var currentUserId = this.User.Identity.Name;
      var links = this.Data.Links.All().Where(f => f.OwnerId == currentUserId || f.Public);

      return this.Ok(links);
    }

    // GET api/link/open
    [HttpGet]
    [AllowAnonymous]
    [Route("open/{id:int}")]
    public IActionResult GetPage(int id)
    {
      var link = this.Data.Links.GetById(id);
      if (link == null)
      {
        return this.BadRequest("Link #" + id + " not found!");
      }

      using (var client = new WebClient())
      using (var stream = client.OpenRead(link.Url))
      using (var textReader = new StreamReader(stream, Encoding.UTF8, true))
      {
        return this.Content(textReader.ReadToEnd(), "text/html");
      }
    }

    // POST api/link
    [HttpPost]
    public async Task<IActionResult> AddLink([FromBody] UserLink link)
    {
      var currentUserId = this.User.Identity.Name;
      var currentUser = this.Data.Users.All().FirstOrDefault(x => x.Id == currentUserId);
      if (currentUser == null)
      {
        return this.BadRequest("Invalid user token! Please login again!");
      }

      var userRoles = await this._userManager.GetRolesAsync(currentUser);

      if (!userRoles.Any(r => r == "Presenter" || r == "Admin"))
      {
        return this.BadRequest("User is not allowed to create room");
      }

      var userLink = new UserLink
      {
        Name = link.Name,
        Url = link.Url,
        OwnerId = currentUserId,
        Public = link.Public
      };

      this.Data.Links.Add(userLink);

      this.Data.SaveChanges();

      return Ok(userLink);
    }

    // DELETE api/link/{id}
    [HttpDelete]
    [Route("{id:long}")]
    public IActionResult DeleteLink(long id)
    {
      var link = this.Data.Links.GetById(id);
      var rooms = this.Data.Rooms.All().Where(r => r.LinkSrc == id);

      if (link == null)
      {
        return this.BadRequest("Link #" + id + " not found!");
      }

      if (rooms.Any())
      {
        return this.BadRequest("Link is used at least one room");
      }

      // Validate the current user ownership over the add
      var currentUserId = this.User.Identity.Name;
      if (link.OwnerId != currentUserId)
      {
        return this.Unauthorized();
      }

      this.Data.Links.Delete(link);

      this.Data.Links.SaveChanges();

      return this.Ok(
         new
         {
           message = "Link #" + id + " deleted successfully."
         }
     );
    }
  }
}