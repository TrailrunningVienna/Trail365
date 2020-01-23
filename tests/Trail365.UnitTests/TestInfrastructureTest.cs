using System;
using Trail365.UnitTests.TestContext;
using Trail365.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class TestInfrastructureTest
    {
        private readonly ITestOutputHelper OutputHelper;

        public TestInfrastructureTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        [Fact]
        public void ShouldAssignStaticUserAsMember()
        {
            using (var host = TestHostBuilder.Empty().WithIdentityContext().UseStaticAuthenticationAsMember().WithTrailContext().UseTestOutputHelper(OutputHelper).Build())
            {
                var controller = host.CreateFrontendController();
                Assert.NotNull(controller.User);
                var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(controller.User);
                Assert.True(login.IsMember);
                Assert.True(login.IsLoggedIn);
                Assert.False(login.IsModeratorOrHigher);
            }
        }

        [Fact]
        public void ShouldAssignStaticUserAsUser()
        {
            using (var host = TestHostBuilder.Empty().WithIdentityContext().UseStaticAuthenticationAsUser().WithTrailContext().UseTestOutputHelper(OutputHelper).Build())
            {
                var controller = host.CreateFrontendController();
                Assert.NotNull(controller.User);
                var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(controller.User);
                Assert.False(login.IsMember);
                Assert.True(login.IsLoggedIn);
                Assert.False(login.IsModeratorOrHigher);
            }
        }

        [Fact]
        public void ShouldAssignStaticUserAsAdmin()
        {
            using (var host = TestHostBuilder.Empty().WithIdentityContext().UseStaticAuthenticationAsAdmin().WithTrailContext().UseTestOutputHelper(OutputHelper).Build())
            {
                var controller = host.CreateFrontendController();
                Assert.NotNull(controller.User);
                var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(controller.User);
                Assert.False(login.IsMember);
                Assert.True(login.IsLoggedIn);
                Assert.True(login.IsModeratorOrHigher);
                Assert.False(login.IsModerator);
                Assert.True(login.IsAdmin);
            }
        }

        [Fact]
        public void TestContextCoreTest_AuthDiag()
        {
            using (var host = TestHostBuilder.Empty().WithIdentityContext().UseTestOutputHelper(OutputHelper).Build())
            {
                var request = host.Server.CreateRequest(@"auth/diag");
                request.AddHeader("ContentType", "application/json");
                var response = request.GetAsync().GetAwaiter().GetResult();
                Assert.True(response.IsSuccessStatusCode);
                Assert.NotNull(Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()));
            }
        }

        [Fact]
        public void TestContextCoreTest_Health()
        {
            using (var host = TestHostBuilder.Empty().WithIdentityContext().UseTestOutputHelper(OutputHelper).Build())
            {
                var request = host.Server.CreateRequest(@"health");
                request.AddHeader("ContentType", "application/json");
                var response = request.GetAsync().GetAwaiter().GetResult();
                Assert.True(response.IsSuccessStatusCode);
                Assert.NotNull(Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()));
            }
        }
    }
}
