using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ST.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebApi.Notifications
{
  [Authorize]
  public class RoomsHub : Hub
  {
    public override Task OnConnectedAsync()
    {
      Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.Name);

      return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
      Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.Identity.Name);

      return base.OnDisconnectedAsync(exception);
    }


  }
}
