using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShorter.Models;
using UrlShorter.Services;

namespace UrlShorter.Context
{
    public class UrlShortDBContext: IdentityDbContext<User>
    {
        public UrlShortDBContext(DbContextOptions options) :base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<URLModel>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd();
                b.Property(p => p.Code).HasMaxLength(UrlShortenSevice.UrlLength);
                b.HasIndex(p => p.Code).IsUnique();
            });
            builder.Entity<AboutModel>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd();
            });
        }
        public DbSet<AboutModel> Abouts { get; set; }
        public DbSet<URLModel> URLModels { get; set; }
    }
}
