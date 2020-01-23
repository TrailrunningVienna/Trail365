using System;
using System.Collections.Generic;
using System.Linq;
using Trail365.Entities;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests.Controllers
{
 

    [Trait("Category", "BuildVerification")]
    public class HomeControllerNewsTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public HomeControllerNewsTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void ShouldCreateController()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var hc = host.CreateHomeController();
            }
        }

        public static bool Unique(IEnumerable<NewsViewModel> list, ITestOutputHelper helper)
        {
            var origins = list.Select(n => n.OriginID);
            bool result = true;
            if (origins.Distinct().Count() != list.Count())
            {
                foreach (var g in origins.GroupBy(o => o))
                {
                    if (g.Count() > 1)
                    {
                        result = false;
                        var items = list.Where(l => l.OriginID == g.Key).ToList();
                        items.ForEach(i => helper.WriteLine(i.ItemName));
                    }
                }
            }
            return result;
        }

        private static readonly LoginViewModel CurrentLogin = LoginViewModel.CreateFromClaimsPrincipalOrDefault(null);

        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(10)]
        [InlineData(31)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-3)]
        [InlineData(-4)]
        public void ShouldTransformEvents(int addDays)
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var hc = host.CreateHomeController();
                List<Event> items = new List<Event>
                {
                    new Event
                    {
                        Name = "test",
                        StartTimeUtc = DateTime.UtcNow.AddDays(addDays)
                    }
                };
                var list = hc.Url.Transform(CurrentLogin, items).ToList();
                Assert.True(Unique(list, OutputHelper));
            }
        }

        [Fact]
        public void ShouldTransformEvents_findet_heute_statt()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var hc = host.CreateHomeController();
                List<Event> items = new List<Event>();
                var item = new Event
                {
                    Name = "test",
                    StartTimeUtc = DateTime.UtcNow.AddMinutes(100),
                    EndTimeUtc = DateTime.UtcNow.AddMinutes(200),
                };
                items.Add(item);
                var newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("findet heute statt", newsItem.ItemAction);
                item.ModifiedUtc = DateTime.UtcNow;
                newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("findet heute statt", newsItem.ItemAction);// "more important then change !? what about time change!?");
            }
        }

        [Fact]
        public void ShouldTransformEvents_hat_heute_statt_gefunden()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var hc = host.CreateHomeController();
                List<Event> items = new List<Event>();
                var item = new Event
                {
                    CreatedUtc = DateTime.UtcNow.AddDays(-12),
                    Name = "test",
                    StartTimeUtc = DateTime.UtcNow.AddMinutes(-180),
                    EndTimeUtc = DateTime.UtcNow.AddMinutes(-100),
                };
                items.Add(item);
                var newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("hat heute statt gefunden", newsItem.ItemAction);
                item.ModifiedUtc = DateTime.UtcNow;
                newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("hat heute statt gefunden", newsItem.ItemAction);// "more important then change !? what about time change!?");
            }
        }

        [Fact]
        public void ShouldTransformEvents_hat_gestern_statt_gefunden()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var hc = host.CreateHomeController();
                List<Event> items = new List<Event>();
                var item = new Event
                {
                    CreatedUtc = DateTime.UtcNow.AddDays(-12),
                    Name = "test",
                    StartTimeUtc = DateTime.UtcNow.AddDays(-1).AddMinutes(-100),
                    EndTimeUtc = DateTime.UtcNow.AddDays(-1).AddMinutes(-50)
                };
                items.Add(item);
                var newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("hat gestern statt gefunden", newsItem.ItemAction);
                item.ModifiedUtc = DateTime.UtcNow;
                newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("hat gestern statt gefunden", newsItem.ItemAction);// "more important then change !? what about time change!?");
            }
        }

        [Fact]
        public void ShouldTransformEvents_findet_morgen_statt()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var hc = host.CreateHomeController();
                List<Event> items = new List<Event>();
                var item = new Event
                {
                    Name = "test",
                    StartTimeUtc = DateTime.UtcNow.AddDays(1).AddMinutes(100),
                    EndTimeUtc = DateTime.UtcNow.AddDays(1).AddMinutes(200),
                };
                items.Add(item);
                var newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("findet morgen statt", newsItem.ItemAction);
                item.ModifiedUtc = DateTime.UtcNow;
                newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("findet morgen statt", newsItem.ItemAction);// "more important then change !? what about time change!?");
            }
        }

        [Fact]
        public void ShouldTransformEvents_in90days()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var hc = host.CreateHomeController();
                List<Event> items = new List<Event>();
                var item = new Event
                {
                    Name = "test",
                    StartTimeUtc = DateTime.UtcNow.AddDays(90).AddMinutes(100),
                    EndTimeUtc = DateTime.UtcNow.AddDays(90).AddMinutes(200),
                };
                items.Add(item);
                var newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("findet am", newsItem.ItemAction);
                item.ModifiedUtc = DateTime.UtcNow;
                newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("findet am", newsItem.ItemAction);// "more important then change !? what about time change!?");
            }
        }

        [Fact]
        public void ShouldTransformEvents_in90days_20daysago()
        {
            using (var host = TestHostBuilder.DefaultForFrontendAsAdmin().Build())
            {
                var hc = host.CreateHomeController();
                List<Event> items = new List<Event>();
                var item = new Event
                {
                    Name = "test",
                    StartTimeUtc = DateTime.UtcNow.AddDays(90).AddMinutes(100),
                    EndTimeUtc = DateTime.UtcNow.AddDays(90).AddMinutes(200),
                    CreatedUtc = DateTime.UtcNow.AddDays(-20),
                };
                items.Add(item);
                var newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("findet am", newsItem.ItemAction);
                item.ModifiedUtc = DateTime.UtcNow;
                newsItem = hc.Url.Transform(CurrentLogin, items).Single();
                Assert.Contains("findet am", newsItem.ItemAction);// "more important then change !? what about time change!?");
            }
        }
    }
}
