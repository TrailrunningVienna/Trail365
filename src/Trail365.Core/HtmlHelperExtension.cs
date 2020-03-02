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
            //if (blockType == StoryBlockType.Title)
            //{
            //    cssClass = "story-title-image";
            //}
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
        public static IHtmlContent DisplayMultilineMarkdownAsRowOrNothing(IHtmlHelper html, string markDown, string hrefUrl)
        {
            return DisplayMultilineMarkdownAsRow(html, markDown, true, hrefUrl);
        }

        /// <summary>
        /// the row is rendered in every case, if markdown is empty then we create a cell with empty content!
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markDown"></param>
        /// <returns></returns>
        public static IHtmlContent DisplayMultilineMarkdownAsRow(IHtmlHelper html, string markDown)
        {
            return DisplayMultilineMarkdownAsRow(html, markDown, false, string.Empty);
        }
        /// <summary>
        /// single row, one column
        /// </summary>
        /// <param name="cellHtml"></param>
        /// <param name="hrefUrl"></param>
        /// <returns></returns>
        private static StringBuilder GetRowStringBuilder(string cellHtml, string additionalColumnClasses, string additionalRowClasses, string hrefUrl)
        {
            //https://getbootstrap.com/docs/4.4/utilities/stretched-link/
            var colClass = "col preview-markdown multi-line";
            if (!string.IsNullOrEmpty(additionalColumnClasses))
            {
                colClass += " " + additionalColumnClasses.Trim();
            }

            string rowClass = "row";
            if (!string.IsNullOrEmpty(hrefUrl))
            {
                colClass += " position-static";
                rowClass += " position-relative";
            }

            if (!string.IsNullOrEmpty(additionalRowClasses))
            {
                rowClass += " " + additionalRowClasses.Trim();
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<div class=\"{rowClass}\">");
            sb.AppendLine($"<div class=\"{colClass}\">");
            if (!string.IsNullOrEmpty(cellHtml))
            {
                sb.AppendLine(cellHtml);
            }
            sb.AppendLine($"<a href=\"{hrefUrl}\" class=\"stretched-link\"></a>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return sb;
        }

        private static IHtmlContent DisplayMultilineMarkdownAsRow(IHtmlHelper html, string markDown, bool doNotCreateEmptyRow, string hrefUrl)
        {
            return DisplayMultilineMarkdownAsRow(html, markDown, doNotCreateEmptyRow, string.Empty, string.Empty, hrefUrl);
        }

        public static IHtmlContent DisplayMultilineMarkdownAsRow(IHtmlHelper html, string markDown, bool doNotCreateEmptyRow, string additionalColumnClasses, string additionalRowClasses, string hrefUrl)
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
            var rawHtml = GetRowStringBuilder(rawResult, additionalColumnClasses, additionalRowClasses, hrefUrl).ToString();
            return html.Raw(rawHtml);
        }

        public static IHtmlContent DisplayTitleAsRow(IHtmlHelper html, string title)
        {
            int marginBottom = 2;
            return DisplayTitleAsRow(html, title, false, null, marginBottom, null);
        }

        public static IHtmlContent DisplayTitleAsRow(IHtmlHelper html, string title, int? mt, int? mb)
        {
            return DisplayTitleAsRow(html, title, false, null, mt, mb);
        }


        public static IHtmlContent DisplayTitleAsRow(IHtmlHelper html, string title, bool doNotCreateEmptyRow, string hrefUrl, int? mt, int? mb)
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
            string margins = string.Empty;
            if (mb.HasValue)
            {
                margins += $" mb-{mb.Value}";
            }

            if (mt.HasValue)
            {
                margins += $" mt-{mt.Value}";
            }
            var sb = GetRowStringBuilder($"<h4 class=\"mb-0 text-truncate\">{title}</h4>", margins.Trim(), string.Empty, hrefUrl);
            return html.Raw(sb.ToString());
        }
    }
}
