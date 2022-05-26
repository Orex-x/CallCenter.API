using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyLongPolling.Models;

namespace MyLongPolling.Controllers;

[Route("api/[controller]")]
public class AccountController : Controller
{
    private ApplicationContext db;
    public AccountController(ApplicationContext context)
    {
        db = context;
    }

    [Route("Registration")]
    public async Task<IActionResult> Registration([FromBody] RegistrationModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                User user = new User()
                {
                    Login = model.Login,
                    Name = model.Name,
                    Password = model.Password,
                    Connections = new List<Connection>(),
                    Token = ""
                };
                var identity = await GetIdentity(user);
                var now = DateTime.UtcNow;
                // создаем JWT-токен
                var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                
                user.Token = encodedJwt;
                db.Users.Add(user);
                await db.SaveChangesAsync();
                var response = new
                {
                    access_token = encodedJwt,
                    username = identity.Name
                };
                return Json(response);
            }
            catch (Exception e)
            {
            
            }
        }
        return null;
    }
    
    
    [Route("token")] 
    public async Task<IActionResult> Token(string login, string password)
    {
        User person = await db.Users.FirstOrDefaultAsync(x => x.Login.Equals(login) && x.Password.Equals(password));
        var identity = await GetIdentity(person);
        if (identity == null)
        {
            return BadRequest("Invalid username or password.");
        }
  
        /*var now = DateTime.UtcNow;
        // создаем JWT-токен
        var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
 
        person.Token = encodedJwt;
        db.Users.Update(person);
        await db.SaveChangesAsync();*/
        var response = new
        {
            access_token = person.Token,
            username = identity.Name
        };
        return Json(response);
    }
 
   private async Task<ClaimsIdentity> GetIdentity(User person)
   {
     
       if (person != null)
       {
           var claims = new List<Claim>
           {
               new (ClaimsIdentity.DefaultNameClaimType, person.Login),
           };
           ClaimsIdentity claimsIdentity =
           new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
               ClaimsIdentity.DefaultRoleClaimType);
           return claimsIdentity;
       }
       // если пользователь не найден
       return null;
   }
}