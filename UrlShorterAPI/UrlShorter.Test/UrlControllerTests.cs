using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using UrlShorter.Context;
using UrlShorter.Controllers;
using UrlShorter.Models;
using UrlShorter.Requests;
using UrlShorter.Services;

public class UrlControllerTests
{
    private UrlShortDBContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<UrlShortDBContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        return new UrlShortDBContext(options);
    }

    private ClaimsPrincipal GetUser(string userId, bool isAdmin = false)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        if (isAdmin) claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        var identity = new ClaimsIdentity(claims, "mock");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenUrlIsInvalid()
    {
        var context = GetDbContext();
        var userManager = MockUserManager();
        var service = new UrlShortenSevice(context);
        var controller = new UrlController(context, service, userManager);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = GetUser("1") } };

        var result = await controller.Post(new CreateShortUrlRequest { Url = "not-a-valid-url" });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Post_ReturnsConflict_WhenUrlExists()
    {
        var context = GetDbContext();
        var url = new URLModel { OriginalUrl = "https://test.com", Code = "abc", ShortenedUrl = "short", CreatedAt = System.DateTime.Now };
        context.URLModels.Add(url);
        await context.SaveChangesAsync();

        var userManager = MockUserManager();
        var service = new UrlShortenSevice(context);
        var controller = new UrlController(context, service, userManager);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = GetUser("1") } };

        var result = await controller.Post(new CreateShortUrlRequest { Url = "https://test.com" });
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RedirectByCode_ReturnsRedirect_WhenFound()
    {
        var context = GetDbContext();
        var url = new URLModel { OriginalUrl = "https://example.com", Code = "xyz" };
        context.URLModels.Add(url);
        await context.SaveChangesAsync();

        var controller = new UrlController(context, null, null);
        var result = await controller.RedirectByCode("xyz") as RedirectResult;
        Assert.Equal("https://example.com", result.Url);
    }

    [Fact]
    public async Task RedirectByCode_ReturnsNotFound_WhenNotFound()
    {
        var context = GetDbContext();
        var controller = new UrlController(context, null, null);
        var result = await controller.RedirectByCode("notfound");
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetInfo_ReturnsOk_WhenFound()
    {
        var context = GetDbContext();
        context.URLModels.Add(new URLModel { Id = 1, OriginalUrl = "https://x.com" });
        await context.SaveChangesAsync();
        var controller = new UrlController(context, null, null);
        var result = await controller.GetInfo(1);
        Assert.IsType<OkObjectResult>(result);
    }

    private UserManager<User> MockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        var mgr = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new User { Id = "1" });
        mgr.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), "Admin")).ReturnsAsync(true);
        return mgr.Object;
    }
}
