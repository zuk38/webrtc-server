namespace ST.Data
{
  using Microsoft.AspNetCore.Identity;
  using ST.Models;
  using ST.WebApi.Models;

  public interface IAppData
  {
    IRepository<ApplicationUser> Users { get; }

    IRepository<IdentityRole> UserRoles { get; }

    IRepository<Room> Rooms { get; }

    IRepository<UserFile> Files { get; }

    IRepository<UserLink> Links { get; }

    IRepository<RoomFiles> RoomFiles { get; }

    int SaveChanges();
  }
}