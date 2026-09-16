using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Diagnostics;
using WebApplication1.Models;
using WebApplication1.Storefront;

namespace WebApplication1.TagHelpers
{
    public class CitrixAppIconTagHelper : TagHelper
    {
        public ApplicationDetail AppItem { get; set; }

        public int ImageSize { get; set; }

        public string AppID { get; set; }
        private readonly StoreFrontHelper _storeFrontHelper;
        public CitrixAppIconTagHelper(StoreFrontHelper frontHelper)
        {
            _storeFrontHelper = frontHelper;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            try
            {
                output.TagName = "img";

                output.Attributes.Add("app-id", AppID);

                if (ImageSize != 0)
                {
                    output.Attributes.Add("height", ImageSize);
                    output.Attributes.Add("width", ImageSize);
                }

                var image = AppItem.IconUrl; //await _storeFrontHelper.GetImage(AppItem.AppIconUrl, new CancellationToken());

                if (image != null)
                {
                    //var b64Image = CommonHelper.ToBase64String(image);
                    output.Attributes.Add("src", $"data:image/jpeg;base64,{image}");
                }
                else
                {
                    //load default 
                    output.Attributes.Add("src", "http://via.placeholder.com/150x150");
                }


            }
            catch (Exception Err)
            {
                Debug.WriteLine(Err.InnerException);
            }

            //output.Attributes.Add("src","testing.jpg");
        }
    }
}
