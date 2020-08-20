namespace ST.Web.Models.Users
{
  using ST.Models;
  using System.Collections.Generic;

  public class UserUpdateRoomBindingModel
  {
    public PdfState PdfState { get; set; }

    public List<UserFile> Files { get; set; }

    public string OwnerNote { get; set; }

    public MeetingTab SelectedTab { get; set; }

    public long? ImageSrc { get; set; }

    public long? VideoSrc { get; set; }

    public long? LinkSrc { get; set; }
  }
}
