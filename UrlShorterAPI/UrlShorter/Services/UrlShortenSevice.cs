using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using UrlShorter.Context;

namespace UrlShorter.Services
{
    public class UrlShortenSevice
    {
        public const int UrlLength = 10;
        private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly Random _random = new();
        private readonly UrlShortDBContext _context;

        public UrlShortenSevice(UrlShortDBContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateShortUrl()
        {
            var shortUrl = new char[UrlLength];
            for (int i = 0; i < UrlLength; i++)
            {
                shortUrl[i] = Characters[_random.Next(Characters.Length - 1)];
            }
            var code = new string(shortUrl);
            return code;
        }
    }
}
