namespace ST.Web.Controllers
{
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Threading.Tasks;
  using global::WebApi.Notifications;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.SignalR;
  using Microsoft.EntityFrameworkCore;
  using ST.Data;
  using ST.Models;
  using ST.WebApi.Controllers;

  [Authorize]
  [Route("api/storage")]
  public class StorageController : BaseApiController
  {
    private UserManager<ApplicationUser> _userManager;
    private readonly IHostingEnvironment _hostEnvironment;
    private readonly IHubContext<RoomsHub> _hubContext;

    public StorageController(IAppData data, IHostingEnvironment env, UserManager<ApplicationUser> userManager, IHubContext<RoomsHub> hubContext) : base(data)
    {
      this._hostEnvironment = env;

      _userManager = userManager;
      _hubContext = hubContext;
    }
    

    // GET api/Storage/{id}
    [HttpGet]
    [Route("{id:int}")]
    public IActionResult GetFileById(int id)
    {
      var file = this.Data.Files.GetById(id);
      if (file == null)
      {
        return this.BadRequest("File #" + id + " not found!");
      }

      // Validate the current user ownership over the ad
      var currentUserId = this.User.Identity.Name;
      if (file.OwnerId != currentUserId)
      {
        return this.Unauthorized();
      }

      return this.Ok(file);
    }

    // GET api/Storage
    [HttpGet]
    [Route("All")]
    public IActionResult GetAll()
    {
      var currentUserId = this.User.Identity.Name;
      var files = this.Data.Files.All().Where(f => f.OwnerId == currentUserId || f.Public);
     
      return this.Ok(files);
    }

    // GET api/Storage/Download/{id}
    [HttpGet]
    [Route("Download/{id:long}")]
    public async Task<IActionResult> Download(long id)
    {
      var file = this.Data.Files.GetById(id);
      if (file == null || !System.IO.File.Exists(file.FilePath))
      {
        return this.BadRequest("File #" + id + " not found!");
      }

      // Validate the current user ownership over the ad
      var currentUserId = this.User.Identity.Name;
      var dataStream = System.IO.File.OpenRead(file.FilePath);

      return new FileStreamResult(dataStream, "application/octet-stream");
    }

    // POST api/Storage/Upload
    [HttpPost]
    [Route("Upload")]
    public async Task<IActionResult> OnPostUploadAsync(IFormFile file, string owner)
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
      
      var postedFile = file;
      var storagePath = owner == "1" ? "PublicStorage/" : "Storage/";
      Directory.CreateDirectory(storagePath);
      var filePath = Path.Combine(_hostEnvironment.ContentRootPath, storagePath + Path.GetRandomFileName() + "." + file.FileName.Split('.').Last());

      using (var stream = System.IO.File.Create(filePath))
      {
        await postedFile.CopyToAsync(stream);
      }

      var userFile = new UserFile
      {
        FileType = GetFileType(postedFile.ContentType),
        Name = postedFile.FileName,
        MimeType = postedFile.ContentType,
        FilePath = filePath,
        OwnerId = currentUserId,
        Public = owner == "1"
      };

      this.Data.Files.Add(userFile);

      this.Data.SaveChanges();

      return Ok(userFile);
    }

    // DELETE api/Storage/{id}
    [HttpDelete]
    [Route("{id:long}")]
    public IActionResult DeleteFile(long id)
    {
      var file = this.Data.Files.GetById(id);
      if (file == null)
      {
        return this.BadRequest("File #" + id + " not found!");
      }

      // Validate the current user ownership over the add
      var currentUserId = this.User.Identity.Name;
      if (file.OwnerId != currentUserId)
      {
        return this.Unauthorized();
      }

      var rooms = this.Data.Rooms.All()
        .Include(r => r.RoomFiles)
        .Include(r => r.PdfState)
        .Include(r => r.PdfState.File)
        .Where(r => r.RoomFiles.Any(rf => rf.UserFileId == id) || r.PdfState.File.Id == id || r.ImageSrc == id || r.VideoSrc == id);

      if (rooms.Any())
      {
        return this.BadRequest("File is used in at least one room.");
      }

      this.Data.Rooms.SaveChanges();

      System.IO.File.Delete(file.FilePath);

      this.Data.Files.Delete(file);

      this.Data.Files.SaveChanges();

      return this.Ok(
         new
         {
           message = "File #" + id + " deleted successfully."
         }
     );
    }
  
    private FileType GetFileType(string contentType)
    {
      if (contentType.Contains("pdf"))
      {
        return FileType.Pdf;
      }

      if (contentType.Contains("image"))
      {
        return FileType.Image;
      }

      if (contentType.Contains("video"))
      {
        return FileType.Video;
      }

      return FileType.Other;
    }
  }

  public class FileResult : IActionResult
  {
    MemoryStream bookStuff;
    string fileName;
    HttpRequestMessage httpRequestMessage;
    HttpResponseMessage httpResponseMessage;
    public FileResult(MemoryStream data, HttpRequestMessage request, string filename)
    {
      bookStuff = data;
      httpRequestMessage = request;
      fileName = filename;
    }

    public Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
    {
      httpResponseMessage = httpRequestMessage.CreateResponse(HttpStatusCode.OK);
      httpResponseMessage.Content = new StreamContent(bookStuff);
      httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
      httpResponseMessage.Content.Headers.ContentDisposition.FileName = fileName;
      httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

      return Task.FromResult(httpResponseMessage);
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
      httpResponseMessage = httpRequestMessage.CreateResponse(HttpStatusCode.OK);
      httpResponseMessage.Content = new StreamContent(bookStuff);
      httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
      httpResponseMessage.Content.Headers.ContentDisposition.FileName = fileName;
      httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

      return Task.FromResult(httpResponseMessage);
    }
  }
 }
