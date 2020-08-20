namespace ST.Data
{
  using System;
  using System.Collections.Generic;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.AspNetCore.Identity;
  using ST.Models;
  using ST.WebApi.Repos;
  using ST.WebApi.Models;

  public class AppData : IAppData
  {
    private readonly DbContext context;

    private readonly IDictionary<Type, object> repositories;

    public AppData(AppDbContext context)
    {
      this.context = context;
      this.repositories = new Dictionary<Type, object>();
    }

    public IRepository<ApplicationUser> Users
    {
      get
      {
        return this.GetRepository<ApplicationUser>();
      }
    }

    public IRepository<IdentityRole> UserRoles
    {
      get
      {
        return this.GetRepository<IdentityRole>();
      }
    }

    public IRepository<Room> Rooms
    {
      get
      {
        return this.GetRepository<Room>();
      }
    }

    public IRepository<UserFile> Files
    {
      get
      {
        return this.GetRepository<UserFile>();
      }
    }

    public IRepository<RoomFiles> RoomFiles
    {
      get
      {
        return this.GetRepository<RoomFiles>();
      }
    }

    public IRepository<UserLink> Links
    {
      get
      {
        return this.GetRepository<UserLink>();
      }
    }

    public int SaveChanges()
    {
      return this.context.SaveChanges();
    }

    private IRepository<T> GetRepository<T>() where T : class
    {
      if (!this.repositories.ContainsKey(typeof(T)))
      {
        var type = typeof(EfRepository<T>);
        this.repositories.Add(typeof(T), Activator.CreateInstance(type, this.context));
      }

      return (IRepository<T>)this.repositories[typeof(T)];
    }
  }
}