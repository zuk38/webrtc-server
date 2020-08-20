namespace ST.Web.Models.Users
{
  using System.ComponentModel.DataAnnotations;

  public class UserCreateRoomBindingModel
  {
    [Required]
    public string Name { get; set; }

    public string Descr { get; set; }

    [Required]
    public string SpaceId { get; set; }

    [Required]
    public string ClientId { get; set; }

    [Required]
    public string Token { get; set; }
  }
}