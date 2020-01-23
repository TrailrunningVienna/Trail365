using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Trail365.Markdig
{
    public class BootstrapExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
            pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
        }

        private static void PipelineOnDocumentProcessed(MarkdownDocument document)
        {
            foreach (var node in document.Descendants())
            {
                if (node is ParagraphBlock)
                {
                    node.GetAttributes().AddClass("text-wrap");
                }
                else if (node is LinkInline)
                {
                    LinkInline lnk = node as LinkInline;
                    lnk.GetAttributes().AddClass("fg-link text-underline");
                }
                else if (node is Inline)
                {
                    //var link = node as LinkInline;
                    //if (link != null && link.IsImage)
                    //{
                    //    link.GetAttributes().AddClass("img-fluid");
                    //}
                }
            }
        }
    }
}
