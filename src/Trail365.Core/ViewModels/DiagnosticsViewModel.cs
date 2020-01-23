using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace Trail365.ViewModels
{
    public class DiagnosticsViewModel
    {
        public DiagnosticsViewModel(AuthenticateResult result, AuthenticationService authService)
        {
            this.AuthenticateResult = result;
            AuthenticationService = authService;
            this.Id = System.Guid.NewGuid().ToString("N");
        }

        public AuthenticateResult AuthenticateResult { get; }

        private readonly AuthenticationService AuthenticationService;

        public IEnumerable<string> GetSchemes()
        {
            return AuthenticationService.Schemes.GetAllSchemesAsync().GetAwaiter().GetResult().Select<AuthenticationScheme, string>(sch => sch.Name);
        }

        public string Id { get; set; }
    }
}
