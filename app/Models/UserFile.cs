using Newtonsoft.Json;
using ST.WebApi.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ST.Models
{
  public class UserFile
  {
    public UserFile()
    {
      this.RoomFiles = new HashSet<RoomFiles>();
    }

    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public FileType FileType { get; set; }

    public string OwnerId { get; set; }

    [JsonIgnore]
    public string FilePath { get; set; }

    public string MimeType { get; set; }

    [JsonIgnore]
    public virtual ApplicationUser Owner { get; set; }

    [JsonIgnore]
    public ICollection<RoomFiles> RoomFiles { get; set; }

    public bool Public { get; set; }
  }
}