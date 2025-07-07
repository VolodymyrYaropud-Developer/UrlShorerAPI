using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using UrlShorter.Models;

namespace UrlShorter.Test
{
    internal static class Mockets
    {
        public static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        public static Mock<SignInManager<User>> MockSignInManager()
        {
            var userManager = MockUserManager();
            var context = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            return new Mock<SignInManager<User>>(userManager.Object, context.Object, claimsFactory.Object, null, null, null, null);
        }

    }
}
