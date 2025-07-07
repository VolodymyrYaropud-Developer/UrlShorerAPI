using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShorter.Context;
using UrlShorter.Models;
using UrlShorter.Responses;

namespace UrlShorter.Controllers
{
    [Route("api")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        private readonly UrlShortDBContext _context;

        public AboutController(UrlShortDBContext context)
        {
            _context = context;
        }

        [HttpGet("about")]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var about = await _context.Abouts.FirstOrDefaultAsync();
            return Ok(new BaseResponse
            { 
                Success = true,
                Message = about?.Description ?? "This is a simple URL shortener service." 
            });
        }

        [HttpPost("edit/about")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] AboutModel model)
        {
            var about = await _context.Abouts.FirstOrDefaultAsync();
            if (about == null)
            {
                about = new AboutModel { Description = model.Description };
                _context.Abouts.Add(about);
            }
            else
            {
                about.Description = model.Description;
            }

            await _context.SaveChangesAsync();
            return Ok(new BaseResponse 
            { 
                Success = true,
                Message = "About description updated." 
            });
        }

    }
}
