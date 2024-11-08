using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Northwind.Helpers.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "northwind-id")]
    public class ImageTagHelper : TagHelper
    {
        public string NorthwindId { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(NorthwindId))
            {
                var url = $"/images/{NorthwindId}";
                output.Attributes.SetAttribute("href", url);
            }
        }
    }
}
