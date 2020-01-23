using System;
using System.Net.Http;

namespace Trail365
{
    public static class HttpResponseMessageExtension
    {
        public static string EnsureRedirectStatusCode(this HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
            {
                var redirectUri = response.Headers.Location;
                if (!redirectUri.IsAbsoluteUri)
                {
                    //redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                }
                return redirectUri.ToString();
            }

            throw new InvalidOperationException("Unexpected Statuscode");
        }
    }
}
