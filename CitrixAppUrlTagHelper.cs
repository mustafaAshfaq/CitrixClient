using Microsoft.AspNetCore.Razor.TagHelpers;
using WebApplication1.Models;

namespace WebApplication1.TagHelpers
{
    public class CitrixAppUrlTagHelper : TagHelper
    {
        public ApplicationDetail AppItem { get; set; }
        public string AppUrl { get; set; }
        public string WindowTarget { get; set; }
        public CitrixAppUrlTagHelper()
        {
            
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            if (AppItem != null)
            {
                if (AppItem.ClientTypes.Contains("content"))
                {
                    output.Attributes.Add("href", AppItem.Content);
                }
                else
                {
                    output.Attributes.Add("href", $"Home/Launch?AppID={AppItem.Id}&AppUrl={AppUrl}");
                }
            }
            else if(!string.IsNullOrEmpty(AppUrl)) 
            {
                output.Attributes.Add("href", $"Home/Launch?AppID={AppItem?.Id}&AppUrl={AppUrl}");
            }
           
            output.Attributes.Add("target", WindowTarget);
            return Task.CompletedTask;

        }
    }
}
