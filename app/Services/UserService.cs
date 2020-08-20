using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ST.Data;
using WebApi.Helpers;

namespace WebApi.Services
{
  public interface IUserService
  {
    string Authenticate(string username, string password);
  }

  public class UserService : IUserService
  {
    private readonly AppSettings _appSettings;
    private readonly IAppData _data;

    public UserService(IOptions<AppSettings> appSettings, IAppData data)
    {
      _appSettings = appSettings.Value;
      _data = data;
    }

    public string Authenticate(string username, string password)
    {
      var user = this._data.Users.All().SingleOrDefault(x => x.UserName == username);

      // return null if user not found
      if (user == null)
        return null;

      // authentication successful so generate jwt token
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new Claim[]
          {
            new Claim(ClaimTypes.Name, user.Id.ToString())
          }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };
      var token = tokenHandler.CreateToken(tokenDescriptor);
      user.Token = tokenHandler.WriteToken(token);

      this._data.SaveChanges();

      return user.Token;
    }
  }
}