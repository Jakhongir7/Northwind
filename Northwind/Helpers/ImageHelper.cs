using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Northwind.Helpers
{
    public static class ImageHelper
    {
        public static IHtmlContent ImageLink(this IHtmlHelper htmlHelper, string imageId, string linkContent)
        {
            var url = $"/images/{imageId}";
            var anchorTag = new TagBuilder("a");
            anchorTag.Attributes["href"] = url;
            anchorTag.Attributes["target"] = "_blank"; // Add target attribute for opening in a new tab
            anchorTag.InnerHtml.AppendHtml(linkContent); // Append raw HTML content

            using (var writer = new System.IO.StringWriter())
            {
                anchorTag.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                return new HtmlString(writer.ToString());
            }
        }
    }
}
