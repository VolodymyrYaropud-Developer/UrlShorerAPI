using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UrlShorter.Models;
using UrlShorter.Models.DTOs;
using UrlShorter.Responses;
using UrlShorter.Responses.LoginResponses;

namespace UrlShorter.Controllers
{
    [ApiController]
    [Route("api")]
    public class LoginController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO login,
            [FromServices] SignInManager<User> signInManager,
            [FromServices] UserManager<User> userManager,
            [FromServices] IConfiguration config)
        {
            var result = await signInManager.PasswordSignInAsync(login.Username, login.Password, isPersistent: true, lockoutOnFailure: false);
            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            var user = await userManager.FindByNameAsync(login.Username);
            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(config["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new LoginResponse
            {
                Success = true,
                TokenString = tokenString,
                Expires = token.ValidTo
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task Logout([FromServices] SignInManager<User> signInManager)
        {
            await signInManager.SignOutAsync();
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] UserSingUpDTO userForSingUp, [FromServices] UserManager<User> manager)
        {
            if (userForSingUp == null || string.IsNullOrEmpty(userForSingUp.UserName) || string.IsNullOrEmpty(userForSingUp.Password))
            {
                return BadRequest(new BaseResponse
                {
                    Message = "Username and password are required."
                });
            }


            var user = new User { UserName = userForSingUp.UserName };
            var result = await manager.CreateAsync(user, userForSingUp.Password);
            if (result.Succeeded)
            {
                await manager.AddToRoleAsync(user, "Member");
                var roles = await manager.GetRolesAsync(user);
                return Ok(new BaseResponse
                {
                    Success = true,
                    Message = "User created successfully"
                });
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new BaseResponse
                {
                    Message = $"User creation failed: {errors}"
                });
            }
        }
    }
}

