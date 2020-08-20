using ST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ST.WebApi.Models
{
  public class RoomFiles
  {
    public int RoomId { get; set; }

    public Room Room { get; set; }

    public int UserFileId { get; set; }

    public UserFile File { get; set; }
  }
}
