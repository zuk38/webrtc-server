using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ST.Models;
using ST.WebApi.Repos;
using System;
using System.Threading.Tasks;

namespace ST.WebApi.Repositories
{
  public static class IdentitySeed
  {
    //public static async Task InitializeAsync(IServiceProvider serviceProvider)
    //{
    //  using (var context = new AppDbContext(serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
    //  {
    //    var _rolesManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    //    var _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    //    var exist = await _rolesManager.RoleExistsAsync("Admin");

    //    if (!exist)
    //    {
    //      var adminRole = "Admin";
    //      var roleNames = new String[] { adminRole, "Presenter", "Client" };

    //      foreach (var roleName in roleNames)
    //      {
    //        var role = await _rolesManager.RoleExistsAsync(roleName);
    //        if (!role)
    //        {
    //          var result = await _rolesManager.CreateAsync(new IdentityRole { Name = roleName });
    //        }
    //      }
    //      return;
    //    }


    //    // administrator
    //    var user = new ApplicationUser
    //    {
    //      UserName = "admin",
    //      Name = "PL",
    //      Email = "admin@macrix.pl",
    //      EmailConfirmed = true
    //    };

    //    var i = await _userManager.FindByEmailAsync(user.Email);

    //    if (i == null)
    //    {
    //      var adminUser = await _userManager.CreateAsync(user, "Admin_123");
    //      if (adminUser.Succeeded)
    //      {
    //        await _userManager.AddToRoleAsync(user, "Admin");
    //      }
    //    }

    //    // presenter
    //    var user2 = new ApplicationUser
    //    {
    //      UserName = "presenter",
    //      Name = "PL",
    //      Email = "presenter@macrix.pl",
    //      EmailConfirmed = true
    //    };

    //    var pExist = await _userManager.FindByEmailAsync(user2.Email);

    //    if (pExist == null)
    //    {
    //      var pUser = await _userManager.CreateAsync(user2, "Presenter_123");
    //      if (pUser.Succeeded)
    //      {
    //        await _userManager.AddToRoleAsync(user2, "Presenter");
    //      }
    //    }
    //  }
    //}

    public static async Task InitializeDbAsync(RoleManager<IdentityRole> _rolesManager, UserManager<ApplicationUser> _userManager)
    {
      var exist = await _rolesManager.RoleExistsAsync("Admin");

        if (!exist)
        {
          var adminRole = "Admin";
          var roleNames = new String[] { adminRole, "Presenter", "Client" };

          foreach (var roleName in roleNames)
          {
            var role = await _rolesManager.RoleExistsAsync(roleName);
            if (!role)
            {
              var result = await _rolesManager.CreateAsync(new IdentityRole { Name = roleName });
            }
          }
          return;
        }


        // administrator
        var user = new ApplicationUser
        {
          UserName = "admin",
          Name = "PL",
          Email = "admin@macrix.pl",
          EmailConfirmed = true
        };

        var i = await _userManager.FindByEmailAsync(user.Email);

        if (i == null)
        {
          var adminUser = await _userManager.CreateAsync(user, "Admin_123");
          if (adminUser.Succeeded)
          {
            await _userManager.AddToRoleAsync(user, "Admin");
          }
        }

        // presenter
        var user2 = new ApplicationUser
        {
          UserName = "presenter",
          Name = "PL",
          Email = "example@macrix.pl",
          EmailConfirmed = true,
          SpacedeckPass = "123"
        };

        var pExist = await _userManager.FindByEmailAsync(user2.Email);

        if (pExist == null)
        {
          var pUser = await _userManager.CreateAsync(user2, "Presenter_123");
          if (pUser.Succeeded)
          {
            await _userManager.AddToRoleAsync(user2, "Presenter");
          }
        }
      }    
  }
}
