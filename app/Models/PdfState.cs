using System.ComponentModel.DataAnnotations;

namespace ST.Models
{
  public class PdfState
  {
    [Key]
    public int Id { get; set; }
    public int CurrentPage { get; set; }
    public int Rotation { get; set; }
    public float Zoom { get; set; }
    public UserFile File { get; set; }
  }
}