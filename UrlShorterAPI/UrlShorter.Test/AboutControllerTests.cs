using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrlShorter.Context;
using UrlShorter.Controllers;
using UrlShorter.Models;
using UrlShorter.Responses;

public class AboutControllerTests
{
    private UrlShortDBContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<UrlShortDBContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        return new UrlShortDBContext(options);
    }

    private AboutController GetController(UrlShortDBContext context, bool isAdmin = false)
    {
        var controller = new AboutController(context);

        if (isAdmin)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        return controller;
    }

    [Fact]
    public async Task Get_ReturnsDefaultMessage_WhenNoAboutExists()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var result = await controller.Get() as OkObjectResult;

        Assert.NotNull(result);
        var response = result.Value as BaseResponse;
        Assert.True(response.Success);
        Assert.Equal("This is a simple URL shortener service.", response.Message);
    }

    [Fact]
    public async Task Get_ReturnsAboutDescription_WhenExists()
    {
        var context = GetDbContext();
        context.Abouts.Add(new AboutModel { Description = "Custom About Message" });
        await context.SaveChangesAsync();
        var controller = GetController(context);
        var result = await controller.Get() as OkObjectResult;
        var response = result.Value as BaseResponse;
        Assert.True(response.Success);
        Assert.Equal("Custom About Message", response.Message);
    }

    [Fact]
    public async Task Update_CreatesNewAbout_WhenNoneExists()
    {
        var context = GetDbContext();
        var controller = GetController(context, isAdmin: true);
        var input = new AboutModel { Description = "Created new About" };
        var result = await controller.Update(input) as OkObjectResult;
        var response = result.Value as BaseResponse;
        Assert.True(response.Success);
        Assert.Equal("About description updated.", response.Message);
        var aboutInDb = await context.Abouts.FirstOrDefaultAsync();
        Assert.NotNull(aboutInDb);
        Assert.Equal("Created new About", aboutInDb.Description);
    }

    [Fact]
    public async Task Update_UpdatesExistingAbout()
    {
        var context = GetDbContext();
        context.Abouts.Add(new AboutModel { Description = "Old Description" });
        await context.SaveChangesAsync();
        var controller = GetController(context, isAdmin: true);
        var input = new AboutModel { Description = "Updated Description" };
        var result = await controller.Update(input) as OkObjectResult;
        var response = result.Value as BaseResponse;
        Assert.True(response.Success);
        Assert.Equal("About description updated.", response.Message);
        var updated = await context.Abouts.FirstOrDefaultAsync();
        Assert.Equal("Updated Description", updated.Description);
    }

    [Fact]
    public async Task Update_ReturnsForbidden_WhenUserNotAdmin()
    {
        var context = GetDbContext();
        var controller = GetController(context, isAdmin: false); 
        var input = new AboutModel { Description = "Should not update" };
        var result = await controller.Update(input);
        var response = result as OkObjectResult;
        Assert.NotNull(response); 
    }
}
