using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using NetTopologySuite.Geometries;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.ViewModels;

namespace Trail365
{
    public static class HelperExtensions
    {
        public static IEnumerable<NewsViewModel> Transform(this IUrlHelper helper, LoginViewModel login, IEnumerable<Event> items, int priority = 0)
        {
            var converted = items.Select(e => new Tuple<Event, int>(e, priority));
            return Transform(helper, login, converted);
        }

        public static IEnumerable<NewsViewModel> Transform(this IUrlHelper helper, LoginViewModel login, IEnumerable<Tuple<Event, int>> items)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (login == null) throw new ArgumentNullException(nameof(login));

            string prefix = "Die Veranstaltung";

            DateTime today00 = DateTime.UtcNow.Date;
            DateTime tomorrow00 = DateTime.UtcNow.AddDays(1).Date;
            DateTime afterTomorrow00 = DateTime.UtcNow.AddDays(2).Date;
            DateTime endofRollingWeek = DateTime.UtcNow.AddDays(6).Date;
            DateTime endOfRollingQuarter = DateTime.UtcNow.AddMonths(3 * 2).Date;
            DateTime now = DateTime.UtcNow;
            bool trailPermission = true; //TODO refine!
            foreach (var t in items)
            {
                var e = t.Item1;
                var priority = t.Item2;
                if (e.StartTimeUtc.HasValue == false)
                {
                    continue;
                }

                if (e.StartTimeUtc.Value.Date == today00)
                {
                    if (e.StartTimeUtc.Value > now)
                    {
                        var diff = e.StartTimeUtc.Value.Subtract(now);
                        if (diff.TotalMinutes > (10)) //print out events that are at least 6hour in the future
                        {
                            yield return new NewsViewModel(e.ID, prefix, e.Name, e.StartTimeUtc.Value, null, "findet heute statt", priority)
                            {
                                DetailsUrl = helper.GetEventUrl(e.ID),
                                ImageUrl = e.CoverImage?.Url,
                                EventItem = e.ToEventViewModel(helper, login, null, trailPermission),
                            };
                            continue;
                        }
                    }
                    else
                    {
                        //event is over, but today
                        yield return new NewsViewModel(e.ID, prefix, e.Name, e.StartTimeUtc.Value, null, "hat heute statt gefunden", priority)
                        {
                            DetailsUrl = helper.GetEventUrl(e.ID),
                            ImageUrl = e.CoverImage?.Url,
                            EventItem = e.ToEventViewModel(helper, login, null, trailPermission),
                        };
                        continue;
                    }
                }

                if (e.StartTimeUtc.Value.Date == tomorrow00)
                {
                    yield return new NewsViewModel(e.ID, prefix, e.Name, e.StartTimeUtc.Value, null, "findet morgen statt", priority)
                    {
                        DetailsUrl = helper.GetEventUrl(e.ID),
                        ImageUrl = e.CoverImage?.Url,
                        EventItem = e.ToEventViewModel(helper, login, null, trailPermission),
                    };
                    continue;
                }

                if (e.StartTimeUtc.Value.Date == afterTomorrow00)
                {
                    yield return new NewsViewModel(e.ID, prefix, e.Name, e.StartTimeUtc.Value, null, "findet übermorgen statt", priority)
                    {
                        DetailsUrl = helper.GetEventUrl(e.ID),
                        ImageUrl = e.CoverImage?.Url,
                        EventItem = e.ToEventViewModel(helper, login, null, trailPermission),
                    };
                    continue;
                }

                if (e.StartTimeUtc.Value.Date > afterTomorrow00 && e.StartTimeUtc.Value.Date < endofRollingWeek)
                {
                    //string day = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName(e.StartTimeUtc.Value.DayOfWeek);

                    var r1 = new NewsViewModel(e.ID, prefix, e.Name, e.StartTimeUtc.Value, null, "findet {0} statt", priority)
                    {
                        DetailsUrl = helper.GetEventUrl(e.ID),
                        ImageUrl = e.CoverImage?.Url,
                        EventItem = e.ToEventViewModel(helper, login, null, trailPermission),
                    };
                    yield return r1;
                    continue;
                }

                if (e.StartTimeUtc.Value.Date > afterTomorrow00 && e.StartTimeUtc.Value.Date < endOfRollingQuarter)
                {
                    ///string day = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName(e.StartTimeUtc.Value.DayOfWeek);
                    //string day = e.StartTimeUtc.Value.ToShortDateString();
                    var r1 = new NewsViewModel(e.ID, prefix, e.Name, e.StartTimeUtc.Value, null, "findet {0} statt", priority)
                    {
                        DetailsUrl = helper.GetEventUrl(e.ID),
                        ImageUrl = e.CoverImage?.Url,
                        EventItem = e.ToEventViewModel(helper, login, null, trailPermission),
                    };
                    yield return r1;
                    continue;
                }

                if (e.StartTimeUtc.Value.Date < today00)
                {
                    var r3 = new NewsViewModel(e.ID, prefix, e.Name, e.StartTimeUtc.Value, null, "hat {0} statt gefunden", priority)
                    {
                        DetailsUrl = helper.GetEventUrl(e.ID),
                        ImageUrl = e.CoverImage?.Url,
                        EventItem = e.ToEventViewModel(helper, login, null, trailPermission),
                    };

                    yield return r3;
                    continue;
                }

                var r2 = new NewsViewModel(e.ID, prefix, e.Name, e.CreatedUtc, e.ModifiedUtc, null, priority)
                {
                    DetailsUrl = helper.GetEventUrl(e.ID),
                    ImageUrl = e.CoverImage?.Url,
                    EventItem = e.ToEventViewModel(helper, login, null, trailPermission),
                };
                yield return r2;
            }
        }

        static HelperExtensions()
        {
            EmptyUrlHelper = new UrlHelper(new ActionContext { RouteData = new RouteData() });
        }

        /// <summary>
        /// the Urlhelper on the controller requires httpcontext
        /// we try to copy out the basic settings from that and use them in a isolated instance, injected into background services
        /// most important useCase: app url!
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IUrlHelper CreateDecoupledUrlHelper(IUrlHelper source)
        {
            bool call = UrlHelperFactory.TryGetStaticUrlHelper(source.AbsoluteUrl(string.Empty), out var result);
            if (call == false)
            {
                throw new InvalidOperationException("Could not create a Urlhelper that is available for the Background Service");
            }
            return result;
        }

        public static IUrlHelper EmptyUrlHelper { get; private set; }

        public static string AbsoluteUrl(this IUrlHelper url, string relativePath)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                relativePath = string.Empty;
            }

            if (url.ActionContext.HttpContext == null)
            {
                return string.Concat(
                "https", "://",
                "localhost:5000",
                "",
                "/", relativePath);
            }

            var request = url.ActionContext.HttpContext.Request;

            string sep = @"/";
            string pathBase = request.PathBase.ToUriComponent();

            if (pathBase.EndsWith(sep))
            {
                sep = string.Empty;
            }

            var host = request.Host.ToUriComponent();

            if (string.IsNullOrEmpty(host))
            {
                throw new InvalidOperationException("Value for 'host' cannot be retrieved");
            }

            var result = string.Concat(
                request.Scheme, "://",
                host,
                pathBase,
                sep, relativePath);

            if (result.LastIndexOf("//") != result.IndexOf("//"))
            {
                Guard.Assert(result.LastIndexOf("//") == result.IndexOf("//"));
            }
            return result;
        }

        public static string ApplicationBase(this IUrlHelper url) => url.AbsoluteUrl(string.Empty);

        public static Uri BuildFileSystemUri(this IUrlHelper url, Uri fileUri, string directoryPath, string requestPath)
        {
            if (string.IsNullOrEmpty(requestPath)) throw new ArgumentNullException(nameof(requestPath));
            string fn = Path.Combine(directoryPath, "dummy.txt"); //uri.makerelative works better with file resource => dummy
            var rootUri = new Uri(fn, UriKind.Absolute);
            Guard.Assert(fileUri.IsFile == rootUri.IsFile);
            var rel2 = rootUri.MakeRelativeUri(fileUri);
            string s = rel2.OriginalString;
            Guard.Assert(requestPath.StartsWith("/"));
            string striped = requestPath.Substring(1);
            Guard.Assert(string.IsNullOrEmpty(striped) == false);
            var s2 = striped + @"/" + s;
            var webPath = url.AbsoluteUrl(s2);
            var result = new UriBuilder(webPath).Uri;
            return result;
        }

        public static string GetFacebookSharerLink(string urlToShare)
        {
            //https://stackoverflow.com/questions/20956229/has-facebook-sharer-php-changed-to-no-longer-accept-detailed-parameters
            var urlEncoded = System.Net.WebUtility.UrlEncode(urlToShare);
            return @"https://www.facebook.com/sharer/sharer.php?u=" + urlEncoded;
        }

        public static string GetFacebookHRefUrlForTrailShare(this IUrlHelper helper, Guid trail)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }
            var url = GetTrailDetailsUrl(helper, trail, true, scraping: true);
            return GetFacebookSharerLink(url);
        }

        public static string GetFacebookHRefUrlForStoryShare(this IUrlHelper helper, Guid storyID)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }
            var url = GetStoryUrl(helper, storyID, true, scraping: true);
            return GetFacebookSharerLink(url);
        }

        public static string GetFacebookHRefUrlForEventShare(this IUrlHelper helper, Guid eventID)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }
            var url = GetEventUrl(helper, eventID, true, scraping: true);
            return GetFacebookSharerLink(url);
        }

        public static string GetFacebookHRefUrlForNewsShare(this IUrlHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }
            var url = helper.GetNewsUrl(true, true);
            return GetFacebookSharerLink(url);
        }

        public static Uri MakeRelativeUri(this IUrlHelper url, Uri uri) => new Uri(url.ApplicationBase()).MakeRelativeUri(uri);

        /// <summary>
        ///
        /// </summary>
        /// <param name="url"></param>
        /// <param name="noConsent"></param>
        /// <param name="scraping">means that this URL was called by a scraper (FB or others) and the returned view should be optimized for that</param>
        /// <returns></returns>
        public static string GetNewsUrl(this IUrlHelper url, bool noConsent = false, bool scraping = false) => url.AbsoluteUrl($"?noconsent={noConsent}&scraping={scraping}");

        public static string GetTrailDetailsUrl(this IUrlHelper url, Guid trail, bool noConsent = false, bool scraping = false) => url.AbsoluteUrl($"{RouteName.TrailDetails}/{trail.ToString()}?noconsent={noConsent}&scraping={scraping}");

        public static string GetStoryUrl(this IUrlHelper url, Guid story, bool noConsent = false, bool scraping = false) => url.AbsoluteUrl($"{RouteName.StoryIndex}/{story.ToString()}?noconsent={noConsent}&scraping={scraping}");

        public static string GetEventUrl(this IUrlHelper url, Guid eventID, bool noConsent = false, bool scraping = false) => url.AbsoluteUrl($"Home/Event?id={eventID.ToString()}&noconsent={noConsent}&scraping={scraping}");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="trailExplorerBaseUrl"></param>
        /// <param name="geoJsonDownloadUrl">TREX (Version 04/2020) expects a geoJson with attribs for classification </param>
        /// <param name="style"></param>
        /// <param name="size"></param>
        /// <param name="snapshotMode"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public static string GetTrailExplorerUrl(this IUrlHelper url, string trailExplorerBaseUrl, Uri geoJsonDownloadUrl, ExplorerMapStyle style, System.Drawing.Size size, bool snapshotMode, string bbox)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (geoJsonDownloadUrl == null) throw new ArgumentNullException(nameof(geoJsonDownloadUrl));
            string mode = "default";

            if (snapshotMode)
            {
                mode = "snapshot";
            }

            string encodedDownloadUri = System.Net.WebUtility.UrlEncode(geoJsonDownloadUrl.ToString());

            string explorer = $"{GetTrailExplorerRoot(trailExplorerBaseUrl)}?mode={mode}&style={style.ToString().ToLowerInvariant()}&jsonsource={encodedDownloadUri}";

            if (!size.IsEmpty)
            {
                explorer += $"&width={size.Width}&height={size.Height}";
            }

            if (!string.IsNullOrEmpty(bbox))
            {
                string encodedbbox = System.Net.WebUtility.UrlEncode(bbox);
                explorer += $"&bbox={encodedbbox}";
            }

            return explorer;
        }

        public static string GetTrailExplorerRoot(string trailExplorerBaseUrl)
        {
            if (string.IsNullOrEmpty(trailExplorerBaseUrl)) throw new ArgumentNullException(nameof(trailExplorerBaseUrl));
            Uri baseUri = new UriBuilder(trailExplorerBaseUrl).Uri;
            Uri api = new Uri(baseUri, "/Index");
            UriBuilder builder = new UriBuilder(api.ToString());
            return builder.Uri.ToString();
        }

        public static string GetTrailExplorerUrlOrDefault(this IUrlHelper url, string trailExplorerBaseUrl, string geoJsonDownloadUrl)
        {
            return GetTrailExplorerUrlOrDefault(url, trailExplorerBaseUrl, geoJsonDownloadUrl, ExplorerMapStyle.Outdoor);
        }

        public static string GetTrailExplorerUrlOrDefault(this IUrlHelper url, string trailExplorerBaseUrl, string geoJsonDownloadUrl, ExplorerMapStyle style)
        {
            return GetTrailExplorerUrlOrDefault(url, trailExplorerBaseUrl, geoJsonDownloadUrl, style, System.Drawing.Size.Empty, false, null);
        }

        public static string GetTrailExplorerUrlOrDefault(this IUrlHelper url, string trailExplorerBaseUrl, string geoJsonDownloadUrl, ExplorerMapStyle style, System.Drawing.Size size, bool snapshotMode, string bbox)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            string mode = "default";
            if (snapshotMode)
            {
                mode = "snapshot";
            }

            if (string.IsNullOrEmpty(geoJsonDownloadUrl))
            {
                string explorer = $"{GetTrailExplorerRoot(trailExplorerBaseUrl)}?mode={mode}&style={style.ToString().ToLowerInvariant()}";

                if (!size.IsEmpty)
                {
                    explorer += $"&width={size.Width}&height={size.Height}";
                }

                if (!string.IsNullOrEmpty(bbox))
                {
                    string encodedbbox = System.Net.WebUtility.UrlEncode(bbox);
                    explorer += $"&bbox={encodedbbox}";
                }


                return explorer;
            }
            else
            {
                var u = new UriBuilder(geoJsonDownloadUrl).Uri;
                return GetTrailExplorerUrl(url, trailExplorerBaseUrl, u, style, size, snapshotMode, bbox);
            }
        }

        public static string GetStorageProxyBaseUrl(this IUrlHelper url) => url.AbsoluteUrl(RouteName.StorageProxyRoute);

        public static string GetApiTrailBaseUrl(this IUrlHelper url) => url.AbsoluteUrl(RouteName.TrailsApi);

        public static string GetUserOnBackendLink(this IUrlHelper url, Guid id) => url.AbsoluteUrl($"Backend/Users/Details/{id.ToString()}");

        public static string GetImpressumLink(this IUrlHelper url) => url.AbsoluteUrl($"Home/Impressum");

        public static string GetDatenschutzLink(this IUrlHelper url) => url.AbsoluteUrl($"Home/Datenschutz");

        public static string GetDefaultEmptyImageUrl(this IUrlHelper url) => url.AbsoluteUrl($"img/empty.png");

        public static string GetDefaultEmptyNewsImageUrl(this IUrlHelper url) => url.AbsoluteUrl($"img/empty.png");

        public static string GetBackendStoriesIndexUrl(this IUrlHelper url) => url.AbsoluteUrl(RouteName.BackendStoriesIndex);

        public static string GetStoryUrl(this IUrlHelper url, Guid id) => url.AbsoluteUrl($"{RouteName.StoryIndex}/{id.ToString()}");

    }
}
