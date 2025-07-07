using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShorter.Context;
using UrlShorter.Models;
using UrlShorter.Requests;
using UrlShorter.Responses;
using UrlShorter.Responses.UrlResponses;
using UrlShorter.Services;

namespace UrlShorter.Controllers
{
    [Route("api")]
    [ApiController]
    public class UrlController : ControllerBase
    {
        private readonly UrlShortDBContext _context;
        private readonly UrlShortenSevice _urlShortenService;
        private readonly UserManager<User> _userManager;

        public UrlController(UrlShortDBContext context, UrlShortenSevice urlShortenService, UserManager<User> manager)
        {
            _context = context;
            _urlShortenService = urlShortenService;
            _userManager = manager;
        }

        [HttpPost("createshorturl")]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] CreateShortUrlRequest request)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            {
                return BadRequest(new BaseResponse
                {
                    Message = "Invalid URL request."
                });
            }

            var existingUrlEntry = await _context.URLModels
                .FirstOrDefaultAsync(u => u.OriginalUrl == request.Url);

            if (existingUrlEntry != null)
            {
                return Conflict(new BaseResponse
                {
                    Message = $"This URL has already been shortened.{existingUrlEntry.ShortenedUrl}",
                });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new BaseResponse 
                { 
                    Message = "User ID not found in token." 
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new BaseResponse
                {
                    Message = "User not found."
                });
            }

            var code = await _urlShortenService.GenerateShortUrl();
            var newUrl = new URLModel
            {
                Code = code,
                OriginalUrl = request.Url,
                ShortenedUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/url/{code}",
                CreatedAt = DateTime.Now,
                User = user,
            };

            _context.URLModels.Add(newUrl);
            await _context.SaveChangesAsync();

            return Ok(new NewUrlResponse
            {
                Success = true,
                UrlModel = newUrl
            });
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectByCode(string code)
        {
            var url = await _context.URLModels.FirstOrDefaultAsync(u => u.Code == code);
            if (url == null)
            {
                return NotFound(new BaseResponse
                {
                    Message = "URL not found."
                });
            }
            return Redirect(url.OriginalUrl);
        }

        [HttpGet("info/{id}")]
        [Authorize]
        public async Task<IActionResult> GetInfo(long id)
        {
            var url = await _context.URLModels.FirstOrDefaultAsync(u => u.Id == id);
            if (url == null)
            {
                return NotFound(new GetInfoResponse 
                { 
                    Message = "Not found"
                });
            }
            return Ok( new GetInfoResponse 
            { 
                Success = true,
                URLModel = url 
            });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetUrls(int count)
        {
            if (!User.Identity.IsAuthenticated)
            {
                var publicUrls = await _context.URLModels
                    .Take(count)
                    .ToListAsync();
                return Ok(publicUrls);
            }

            var userUrls = await _context.URLModels
                .Take(count)
                .ToListAsync();

            return Ok(new AllUrlsResponse
            {
                Success = true,
                URLModels = userUrls 
            });
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUrl(long id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return StatusCode(401);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return StatusCode(404);

            var url = await _context.URLModels
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (url == null)
                return StatusCode(404);

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && url.User?.Id != userId)
                return StatusCode(403);

            _context.URLModels.Remove(url);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse 
            {   
                Success = true,
                Message = "URL deleted successfully." 
            });
        }

        [HttpDelete("deleteall")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllRecords()
        {
            _context.URLModels.RemoveRange(_context.URLModels);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse
            {
                Success = true,
                Message = "All URL records deleted successfully." 
            });
        }

    }
}
