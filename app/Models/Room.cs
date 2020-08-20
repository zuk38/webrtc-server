namespace ST.Models
{
  using Newtonsoft.Json;
  using ST.WebApi.Models;
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class Room
  {
    public Room()
    {
      this.RoomFiles = new HashSet<RoomFiles>();
    }
    [Key]
    public int Id { get; set; }

    [Required]
    [MinLength(16)]
    public string Token { get; set; }

    [Required]
    [MinLength(1)]
    public string Name { get; set; }

    public string Descr { get; set; }

    public string OwnerId { get; set; }

    [JsonIgnore]
    public virtual ApplicationUser Owner { get; set; }

    public DateTime CreationDate { get; set; }

    [Required]
    public RoomStatus Status { get; set; }

    public PdfState PdfState { get; set; }

    public virtual ICollection<UserFile> Files { get; set; }

    [JsonIgnore]
    public ICollection<RoomFiles> RoomFiles { get; set; }

    public string SpaceId { get; set; }

    public string OwnerNote { get; set; }

    public MeetingTab SelectedTab { get; set; }

    public string ClientId { get; set; }

    [JsonIgnore]
    public virtual ApplicationUser Client { get; set; }

    public long? ImageSrc { get; set; }

    public long? VideoSrc { get; set; }

    public long? LinkSrc { get; set; }
  }
}