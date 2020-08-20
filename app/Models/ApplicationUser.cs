namespace ST.Models
{
  using Microsoft.AspNetCore.Identity;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class ApplicationUser : IdentityUser
  {
    private ICollection<Room> rooms;

    private ICollection<IdentityRole> roles;

    public ApplicationUser()
    {
      this.rooms = new HashSet<Room>();
      this.roles = new HashSet<IdentityRole>();
    }

    [Required]
    public string Name { get; set; }

    public string SpacedeckId { get; set; }

    public string SpacedeckPass { get; set; }

    public virtual ICollection<Room> Rooms
    {
      get { return this.rooms; }
      set { this.rooms = value; }
    }

    public virtual ICollection<IdentityRole> Roles
    {
      get { return this.roles; }
      set { this.roles = value; }
    }

    public string Token { get; set; }
  }
}