using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Trail365.UnitTests.TestContext
{
    public class TestApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            //https://github.com/aspnet/AspNetCore/blob/master/src/DefaultBuilder/src/WebHost.cs

            var builder = new WebHostBuilder().UseStartup<TEntryPoint>(); //avoid WebHost.CreateDefault because it loads to many config providers! => Providers.Clear should solve this!

            if (string.IsNullOrEmpty(builder.GetSetting(WebHostDefaults.ContentRootKey)))
            {
                builder.UseContentRoot(Directory.GetCurrentDirectory());
            }

            return builder;
        }
    }
}
