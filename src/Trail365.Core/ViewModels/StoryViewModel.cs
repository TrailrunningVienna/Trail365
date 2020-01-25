using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Trail365.Entities;
using Trail365.Internal;

namespace Trail365.ViewModels
{
    public class StoryViewModel
    {

        public List<StoryBlockViewModel> GetCurrentImageGroup(int itemIndex)
        {
            if (itemIndex < 0)
            {
                throw new IndexOutOfRangeException(nameof(itemIndex));
            }

            if (!(itemIndex < this.Blocks.Count))
            {
                throw new IndexOutOfRangeException(nameof(itemIndex));
            }

            var Model = this;
            var currentItem = this.Blocks[itemIndex];
            if (currentItem.BlockType != StoryBlockType.Image) throw new InvalidOperationException("Current Block must be from type image");
            List<StoryBlockViewModel> items = new List<StoryBlockViewModel>();
            items.Add(currentItem);

            int localIndex = itemIndex + 1;
            while (localIndex < Model.Blocks.Count)
            {
                var localItem = Model.Blocks[localIndex];
                if (localItem.BlockType != StoryBlockType.Image)
                {
                    break;
                }
                if (localItem.BlockTypeGroup != currentItem.BlockTypeGroup)
                {
                    break;
                }
                items.Add(localItem);
                localIndex += 1;
            }
            return items;
        }

        public List<StoryBlockFileViewModel> FileInfos { get; set; } = new List<StoryBlockFileViewModel>();

        public List<StoryBlockViewModel> Blocks { get; set; } = new List<StoryBlockViewModel>();

        public string TitleImageUrl { get; set; }

        public bool NoConsent { get; set; }

        public LoginViewModel Login { get; set; } = new LoginViewModel();

        public AccessLevel ListAccess { get; set; }

        public bool CanEdit()
        {
            Guard.AssertNotNull(this.Login);
            return this.Login.IsAdmin || this.Login.IsModerator;
        }

        public string GetImagesPreviewGridHtml()
        {
            string[] blocksWithImages = this.Blocks.Where(bl => string.IsNullOrEmpty(bl.ImageUrl) == false).Select(bl => bl.ImageUrl).Distinct().ToArray();

            if (blocksWithImages.Length == 0)
            {
                return string.Empty;
            }

            Guard.Assert(blocksWithImages.Length > 0);
            int rows = 1;
            int columns = 1;

            if (blocksWithImages.Length > 1)
            {
                columns = 2;
            }

            if (blocksWithImages.Length > 3)
            {
                rows = 2;
            }

            Guard.Assert(rows * columns <= blocksWithImages.Length);

            string colClass = "col-6";
            if (columns == 1)
            {
                colClass = "col-12";
            }

            Guard.Assert(rows > 0 && rows <= 3);
            Guard.Assert(columns > 0 && columns <= 2);

            var sb = new StringBuilder();
            int index = 0;
            for (int r = 0; r < rows; r++)
            {
                sb.AppendLine("<div class=\"row\">");
                {
                    for (int c = 0; c < columns; c++)
                    {
                        var currentImage = blocksWithImages[index];
                        sb.AppendLine($"<div class=\"{colClass}\">");
                        {
                            sb.AppendLine("<div class=\"story-preview-wrapper\">");
                            sb.AppendLine($"<img src=\"{currentImage}\" class=\"img-fluid\" />");
                            sb.AppendLine("</div>");
                            index += 1;
                        }
                        sb.AppendLine("</div>");
                    }
                }
                sb.AppendLine("</div>");

                sb.AppendLine("<div class=\"row mt-4 mt-lg-2 mb-1\">");
                sb.AppendLine("<div class=\"col-12\">");
                sb.AppendLine("&nbsp;");
                sb.AppendLine("</div></div>");
            }
            return sb.ToString();
        }

        public Dictionary<string, string> CreateOpenGraphTags(IUrlHelper helper, Size imageSize, string appID)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            var ogDict = new Dictionary<string, string>
            {
                {"og:url", helper.GetTrailDetailsUrl(this.ID,true)},
                {"og:type", "website"},
                {"og:title", $"{this.Name}".Trim()},
                {"og:description", $"{this.Excerpt}".Trim()}
            };

            //if (string.IsNullOrEmpty(this.PreviewUrl) == false)
            //{
            //    ogDict.Add("og:image", this.PreviewUrl);
            //    if (!imageSize.IsEmpty)
            //    {
            //        ogDict.Add("og:image:width", imageSize.Width.ToString());
            //        ogDict.Add("og:image:height", imageSize.Height.ToString());
            //    }
            //}

            if (!string.IsNullOrEmpty(appID))
            {
                ogDict.Add("fb:app_id", appID);
            }
            return ogDict;
        }

        public Guid ID { get; set; } = Guid.NewGuid();

        //public string GetHumanizedPlace()
        //{
        //    return "Veranstaltungsort";
        //}

        public string Name { get; set; }

        /// <summary>
        /// Local, not UTC!
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// local, not UTC
        /// </summary>
        public DateTime? Modified { get; set; }

        public string GetLastModifiedDateOrDefault()
        {
            DateTime? v = this.Modified;
            if (!v.HasValue)
            {
                v = this.Created;
            }
            return v.ToDateFormatForDefault();
        }

        /// <summary>
        /// Calculated ViewModel Property calculated during "InitModel" from the "Blocks"
        /// </summary>
        public string Excerpt { get; set; }

        //public string Content { get; set; }
    }
}
