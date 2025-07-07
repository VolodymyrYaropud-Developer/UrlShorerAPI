using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UrlShorter.Context;
using UrlShorter.Controllers;
using UrlShorter.Models;
using UrlShorter.Models.DTOs;
using UrlShorter.Services;
using Xunit;



namespace UrlShorter.Test
{
    public class UnitTests
    {
        

        [Fact]
        public async Task SingUp_ReturnsBadRequest_WhenDataMissing()
        {
            var mockUserManager = Mockets.MockUserManager();
            var controller = new LoginController();

            var result = await controller.SignUp(null, mockUserManager.Object);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteUrl_Returns403_IfUserNotOwnerOrAdmin()
        {
            var userId = "user1";
            var otherUser = new User { Id = "user2", UserName = "someone" };

            var db = new DbContextOptionsBuilder<UrlShortDBContext>()
                .UseInMemoryDatabase("DeleteUnauthorizedTest")
                .Options;

            using var context = new UrlShortDBContext(db);
            var url = new URLModel { Id = 1, OriginalUrl = "http://test.com", Code = "abc", User = otherUser };
            context.URLModels.Add(url);
            context.SaveChanges();

            var mockUserManager = Mockets.MockUserManager();
            mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
            mockUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), "Admin")).ReturnsAsync(false);

            var controller = new UrlController(context, new UrlShortenSevice(context), mockUserManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId)
                        
                    ]))
                }
            };

            var result = await controller.DeleteUrl(1);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(403, ((StatusCodeResult)result).StatusCode);
        }
    }
}