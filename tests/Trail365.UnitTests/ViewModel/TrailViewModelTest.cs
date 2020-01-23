using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Trail365.ViewModels;
using Xunit;

namespace Trail365.UnitTests.ViewModel
{
    [Trait("Category", "BuildVerification")]
    public class TrailViewModelTest
    {
        public IUrlHelper Url => HelperExtensions.EmptyUrlHelper;

        public TrailViewModel EmptyViewModel { get; private set; } = new TrailViewModel();

        [Fact]
        public void ShouldCreateOpenGraphTags()
        {
            this.EmptyViewModel.CreateOpenGraphTags(this.Url, System.Drawing.Size.Empty, "1234");
        }

        [Fact]
        public void ShouldReturnTrailExplorerUrl()
        {
            var url = this.Url.GetTrailExplorerUrlOrDefault(this.EmptyViewModel.GpxUrl, ExplorerMapStyle.Outdoor, System.Drawing.Size.Empty, true, true);
            Debug.WriteLine(url);
        }
    }
}
