using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ST.Models
{
  public class UserLink
  {
    [Key]
    public long Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Url { get; set; }

    public string OwnerId { get; set; }

    [JsonIgnore]
    public virtual ApplicationUser Owner { get; set; }

    public bool Public { get; set; }
  }
}