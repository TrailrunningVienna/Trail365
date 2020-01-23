using System.Linq;
using Microsoft.EntityFrameworkCore;
using Trail365.Data;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class IdentityContextTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public IdentityContextTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldCreateContext()
        {
            var builder = new DbContextOptionsBuilder<IdentityContext>();
            string dbFile = System.IO.Path.GetTempFileName();
            builder.UseSqlite($"Data Source={dbFile}");
            var context = new IdentityContext(builder.Options);
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            Assert.Empty(context.Identities.ToList());
        }
    }
}
