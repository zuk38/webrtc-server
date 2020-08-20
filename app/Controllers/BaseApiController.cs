namespace ST.WebApi.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using ST.Data;
  using Microsoft.AspNetCore.Identity;

  public class BaseApiController : ControllerBase
  {
    public BaseApiController(IAppData data)
    {
      this.Data = data;
    }

    protected IAppData Data { get; private set; }

    protected IActionResult GetErrorResult(IdentityResult result)
    {
      if (result == null)
      {
        return this.BadRequest();
      }

      if (!result.Succeeded)
      {
        if (result.Errors != null)
        {
          foreach (var error in result.Errors)
          {
            this.ModelState.AddModelError(string.Empty, error.Description);
          }
        }

        if (ModelState.IsValid)
        {
          // No ModelState errors are available to send, so just return an empty BadRequest.
          return this.BadRequest();
        }

        return this.BadRequest(this.ModelState);
      }

      return null;
    }
  }
}