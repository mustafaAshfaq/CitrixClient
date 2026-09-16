namespace WebApplication1.Models
{
    public class CitrixAuthCredential
    {
        public string AuthToken { get; set; }
        public string SessionID { get; set; }
        public string CSRFToken { get; set; }
        public string CookiePath { get; set; }
        public string CookieHost { get; set; }
        public string StorefrontUrl { get; set; }
        public CitrixAuthCredential()
        {

        }
    }
}
