using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Template.Web.TagHelpers
{
    [HtmlTargetElement("test-element")]
    public class TestTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("style", "background-color: red; color: white; padding: 10px;");
            output.Content.SetContent("This test tag helper is working!");
        }
    }
}
