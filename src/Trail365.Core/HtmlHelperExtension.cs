using System;
using System.Text;
using Markdig;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trail365.Entities;
using Trail365.Markdig;

namespace Trail365
{
    public static class HtmlHelperExtension
    {
        public static IHtmlContent GetImageRenderingHtml<T>(this IHtmlHelper<T> html, StoryBlockType blockType, string url, string content, bool small)
        {
            var sb = new StringBuilder();
            string cssClass = "story-content-image";
            if (small)
            {
                cssClass = "story-detail-image";
            }
            if (blockType == StoryBlockType.Title)
            {
                cssClass = "story-title-image";
            }
            sb.AppendLine($"<img class=\"img-responsive {cssClass}\" src=\"{url}\" />");
            if (blockType == StoryBlockType.Image)
            {
                if (string.IsNullOrEmpty(content) == false)
                {
                    sb.AppendLine($"<span class=\"trail-image-counter bg-red\">{content}</span>");
                }
            }
            return html.Raw(sb.ToString());
        }

        public static IHtmlContent DisplayNewsWording<T>(IHtmlHelper<T> html, string prefix, string itemName, string suffix)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            StringBuilder sb = new StringBuilder();
            string resolvedItemName = $"{itemName}".Trim();//.Replace(" ", "&nbsp;");
            sb.AppendLine($"<p>{prefix} <span class=\"font-weight-bold\">{resolvedItemName}</span> {suffix}</p>");
            return html.Raw(sb.ToString());
        }

        public static IHtmlContent EmptyRow<T>(IHtmlHelper<T> html, int margin = 1)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine($"<div class=\"col-12\"><span class=\"m-{margin}\"></span></div>");
            sb.AppendLine("</div>");
            return html.Raw(sb.ToString());
        }

        /// <summary>
        /// if markdown is empty then NO ROW is rendered!
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markDown"></param>
        /// <returns></returns>
        public static IHtmlContent DisplayMultilineMarkdownAsRowOrNothing(IHtmlHelper html, string markDown)
        {
            return DisplayMultilineMarkdownAsRow(html, markDown, true);
        }

        /// <summary>
        /// the row is rendered in every case, if markdown is empty then we create a cell with empty content!
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markDown"></param>
        /// <returns></returns>
        public static IHtmlContent DisplayMultilineMarkdownAsRow(IHtmlHelper html, string markDown)
        {
            return DisplayMultilineMarkdownAsRow(html, markDown, false);
        }

        private static IHtmlContent DisplayMultilineMarkdownAsRow(IHtmlHelper html, string markDown, bool doNotCreateEmptyRow)
        {
            string rawResult = string.Empty;

            if (string.IsNullOrEmpty(markDown) == false)
            {
                var builder = new MarkdownPipelineBuilder().UseAdvancedExtensions();
                var ext = new BootstrapExtension();
                ext.Setup(builder);
                var pipeline = builder.Build();
                rawResult = Markdown.ToHtml(markDown, pipeline);
            }
            else
            {
                if (doNotCreateEmptyRow)
                {
                    return html.Raw(string.Empty);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine($"<div class=\"col-12 mt-3 mb-3 preview-markdown multi-line\">");
            sb.Append(rawResult);
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return html.Raw(sb.ToString());
        }

        public static IHtmlContent DisplayTitleAsRow(IHtmlHelper html, string title, bool doNotCreateEmptyRow)
        {
            if (string.IsNullOrEmpty(title) == false)
            {
            }
            else
            {
                if (doNotCreateEmptyRow)
                {
                    return html.Raw(string.Empty);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine($"<div class=\"col-12 mt-3 mb-3\">");
            sb.Append($"<h4 class=\"headline mb-0 text-truncate\">{title}</h4>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return html.Raw(sb.ToString());
        }
    }
}
