namespace ST.Web.Controllers
{
  using System;
  using System.Collections.Generic;

  using System.Linq;
  using global::WebApi.Notifications;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.SignalR;
  using Microsoft.EntityFrameworkCore;
  using ST.Data;
  using ST.Models;
  using ST.Web.Models.Users;
  using ST.WebApi.Controllers;
  using ST.WebApi.Models;

  [Authorize]
  [Route("api/rooms")]
  public class RoomsController : BaseApiController
  {
    private UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<RoomsHub> _hubContext;

    public RoomsController(IAppData data, IHubContext<RoomsHub> hubContext, UserManager<ApplicationUser> userManager)
        : base(data)
    {
      _hubContext = hubContext;
      _userManager = userManager;
    }

    // GET api/Rooms
    [HttpGet]
    public async System.Threading.Tasks.Task<IActionResult> GetRoomsAsync()
    {
      var currentUserId = this.User.Identity.Name;
      var rooms = Data.Rooms.All()
        .Where(r => r.Status != RoomStatus.Removed)
        .Where(r => r.ClientId == currentUserId || r.OwnerId == currentUserId);

      var user = Data.Users.GetById(currentUserId);
      var userRoles = await _userManager.GetRolesAsync(user);

      if (!userRoles.Contains("Admin") && !userRoles.Contains("Presenter"))
      {
        rooms = rooms.Where(ad => ad.Status == RoomStatus.Active);
      }

      rooms = rooms
        .OrderByDescending(r => r.Status == RoomStatus.Active)
        .ThenBy(r => r.CreationDate)
        .ThenBy(r => r.Id);

      return this.Ok(rooms);
    }

    // GET api/Rooms/{token}
    [HttpGet]
    [Route("GetByToken/{token}")]
    public IActionResult GetRoomByToken(string token)
    {
      var room = this.Data.Rooms.All()
        .Include(r => r.RoomFiles)
        .Include(r => r.PdfState)
        .Include(r => r.PdfState.File)
        .FirstOrDefault(d => d.Token == token);
      if (room == null)
      {
        return this.BadRequest("Room #" + token + " not found!");
      }

      // Validate the current user ownership over the room
      var currentUserId = this.User.Identity.Name;
      if (room.OwnerId != currentUserId && room.ClientId != currentUserId)
      {
        return this.Unauthorized();
      }

      room.Files = Data.Files.All().Where(r => room.RoomFiles.Any(f => f.UserFileId == r.Id)).ToList();

      return this.Ok(room);
    }

    // POST api/Rooms/Create
    [HttpPost]
    [Route("Create")]
    public async System.Threading.Tasks.Task<IActionResult> CreateNewRoomAsync([FromBody]UserCreateRoomBindingModel model)
    {
      // Validate the input parameters
      if (!ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      // Validate that the current user exists in the database
      var currentUser = this.Data.Users.GetById(this.User.Identity.Name);
      if (currentUser == null)
      {
        return this.BadRequest("Invalid user token! Please login again!");
      }

      var userRoles = await this._userManager.GetRolesAsync(currentUser);

      if (!userRoles.Any(r => r == "Presenter" || r == "Admin"))
      {
        return this.BadRequest("User is not allowed to create room");
      }

      var room = new Room()
      {
        Name = model.Name,
        Descr = model.Descr,
        CreationDate = DateTime.Now,
        Status = RoomStatus.Active,
        OwnerId = this.User.Identity.Name,
        ClientId = model.ClientId,
        SpaceId = model.SpaceId,
        Token = model.Token,
        SelectedTab = MeetingTab.WhiteBoard
      };

      this.Data.Rooms.Add(room);

      this.Data.SaveChanges();

      await _hubContext.Clients.Group(room.ClientId).SendAsync("RoomUpdate", room);

      return this.Ok(
          new
          {
            message = "Room created successfully.",
            roomId = room.Id
          }
      );
    }

    // PUT api/Rooms/{id}
    [HttpPut]
    [Route("{id:int}")]
    public IActionResult UpdateRoom(int id, [FromBody]UserUpdateRoomBindingModel model)
    {
      // Validate the input parameters
      if (!ModelState.IsValid)
      {
        return this.BadRequest(this.ModelState);
      }

      var room = this.Data.Rooms.All()
        .Include(r => r.RoomFiles)
        .Include(r => r.PdfState)
        .Include(r => r.PdfState.File)
        .FirstOrDefault(d => d.Id == id);
      if (room == null)
      {
        return this.BadRequest("Room #" + id + " not found!");
      }

      // Validate the current user ownership over the ad
      var currentUserId = this.User.Identity.Name;
      if (room.OwnerId != currentUserId)
      {
        return this.Unauthorized();
      }

      room.SelectedTab = model.SelectedTab;
      room.OwnerNote = model.OwnerNote;
      room.ImageSrc = model.ImageSrc;
      room.VideoSrc = model.VideoSrc;
      room.LinkSrc = model.LinkSrc;

      if (model.Files != null)
      {
        if (room.RoomFiles == null)
        {
          room.RoomFiles = new List<RoomFiles>();
        }

        foreach(var file in model.Files)
        {
          if (!room.RoomFiles.Any(f => f.UserFileId == file.Id))
          {
            Data.RoomFiles.Add(new RoomFiles
            {
              RoomId = room.Id,
              UserFileId = file.Id
            });
          }          
        }
      }

      if (model.PdfState != null)
      {
        if (room.PdfState == null)
        {
          room.PdfState = new PdfState
          {
            CurrentPage = model.PdfState.CurrentPage,
            Rotation = model.PdfState.Rotation,
            Zoom = model.PdfState.Zoom,
            File = this.Data.Files.GetById(model.PdfState.File.Id)
          };
        }
        else
        {
          room.PdfState.CurrentPage = model.PdfState.CurrentPage;
          room.PdfState.Rotation = model.PdfState.Rotation;
          room.PdfState.Zoom = model.PdfState.Zoom;
          room.PdfState.File = this.Data.Files.GetById(model.PdfState.File.Id);
        }
      } else
      {
        room.PdfState = null;
      }

      this.Data.Rooms.SaveChanges();
      this.Data.RoomFiles.SaveChanges();

      room.Files = Data.Files.All().Where(r => room.RoomFiles.Any(f => f.UserFileId == r.Id)).ToList();

      _hubContext.Clients.Group(room.ClientId).SendAsync("RoomUpdate", room);

      return this.Ok(
          new
          {
            message = "Room #" + id + " edited successfully."
          }
      );
    }

    // PUT api/Rooms/Close/{token}
    [HttpPut]
    [Route("Close/{token}")]
    public IActionResult CloseRoom(string token)
    {
      var room = this.Data.Rooms.All()
       .FirstOrDefault(d => d.Token == token);
      if (room == null)
      {
        return this.BadRequest("Room #" + token + " not found!");
      }

      // Validate the current user ownership over the room
      var currentUserId = this.User.Identity.Name;
      if (room.OwnerId != currentUserId)
      {
        return this.Unauthorized();
      }

      room.Status = RoomStatus.Closed;

      this.Data.Rooms.SaveChanges();

      _hubContext.Clients.Group(room.ClientId).SendAsync("RoomUpdate", room);

      return this.Ok();
    }

    // PUT api/Rooms/Open/{token}
    [HttpPut]
    [Route("Open/{token}")]
    public IActionResult OpenRoom(string token)
    {
      var room = this.Data.Rooms.All()
       .FirstOrDefault(d => d.Token == token);
      if (room == null)
      {
        return this.BadRequest("Room #" + token + " not found!");
      }

      // Validate the current user ownership over the room
      var currentUserId = this.User.Identity.Name;
      if (room.OwnerId != currentUserId)
      {
        return this.Unauthorized();
      }

      room.Status = RoomStatus.Active;

      this.Data.Rooms.SaveChanges();

      _hubContext.Clients.Group(room.ClientId).SendAsync("RoomUpdate", room);

      return this.Ok();
    }

    // PUT api/Rooms/Close/{token}
    [HttpPut]
    [Route("Remove/{token}")]
    public IActionResult RemoveRoom(string token)
    {
      var room = this.Data.Rooms.All()
       .FirstOrDefault(d => d.Token == token);
      if (room == null)
      {
        return this.BadRequest("Room #" + token + " not found!");
      }

      // Validate the current user ownership over the room
      var currentUserId = this.User.Identity.Name;
      if (room.OwnerId != currentUserId)
      {
        return this.Unauthorized();
      }

      room.Status = RoomStatus.Removed;

      this.Data.Rooms.SaveChanges();

      _hubContext.Clients.Group(room.ClientId).SendAsync("RoomUpdate", room);

      return this.Ok();
    }

    // DELETE api/Room/{roomId}/{id}
    [HttpDelete]
    [Route("File/{roomId:int}/{id:int}")]
    public IActionResult DeleteFileFromRoom(int roomId, int id)
    {
      var file = this.Data.Files.GetById(id);
      var room = this.Data.Rooms.All()
        .Include(r => r.RoomFiles)
        .Include(r => r.PdfState)
        .Include(r => r.PdfState.File)
        .FirstOrDefault(r => r.Id == roomId);

      if (room == null)
      {
        return this.BadRequest("Room #" + id + " not found!");
      }

      if (file == null)
      {
        return this.BadRequest("File #" + id + " not found!");
      }

      // Validate the current user ownership over the add
      var currentUserId = this.User.Identity.Name;

      if (room.OwnerId != currentUserId)
      {
        return this.Unauthorized();
      }

      room.RoomFiles.Remove(room.RoomFiles.FirstOrDefault(rf => rf.UserFileId == file.Id));

      this.Data.Rooms.SaveChanges();

      room.Files = Data.Files.All().Where(r => room.RoomFiles.Any(f => f.UserFileId == r.Id)).ToList();

      _hubContext.Clients.Group(room.ClientId).SendAsync("RoomUpdate", room);

      return this.Ok(
         new
         {
           message = "File #" + id + " removed from room #" + roomId + " successfully."
         }
     );
    }

  }
}
