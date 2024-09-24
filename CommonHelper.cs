using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1
{
    public static class CommonHelper
    {
        public static string ToBase64String(byte[] arr)
        {
            return Convert.ToBase64String(arr);
        }
    }
}
