using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ST.Models;
using ST.WebApi.Models;
using System.Data;

namespace ST.WebApi.Repos
{
  public class AppDbContext : IdentityDbContext<ApplicationUser>
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<ApplicationUser>().HasMany(u => u.Rooms);
      modelBuilder.Entity<ApplicationUser>().HasMany(x => x.Roles).WithOne();
      modelBuilder.Entity<Room>().HasOne(u => u.Owner);
      modelBuilder.Entity<Room>().HasOne(u => u.Client);
      modelBuilder.Entity<Room>().HasOne(u => u.PdfState);
      modelBuilder.Entity<Room>(r =>
      {
        r.HasMany<UserFile>();
      });

      modelBuilder.Entity<UserFile>(r =>
      {
        r.HasMany<Room>();
      });

      modelBuilder.Entity<RoomFiles>()
          .HasKey(t => new { t.RoomId, t.UserFileId });

      modelBuilder.Entity<RoomFiles>()
          .HasOne(pt => pt.Room)
          .WithMany(p => p.RoomFiles)
          .HasForeignKey(pt => pt.RoomId);

      modelBuilder.Entity<RoomFiles>()
          .HasOne(pt => pt.File)
          .WithMany(t => t.RoomFiles)
          .HasForeignKey(pt => pt.UserFileId);
    }

    public DbSet<Room> Rooms { get; set; }

    public DbSet<ApplicationUser> Users { get; set; }

    public DbSet<UserFile> Files { get; set; }

    public DbSet<UserLink> Links { get; set; }
  }
}
