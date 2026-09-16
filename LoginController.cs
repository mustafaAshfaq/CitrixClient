using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebApplication1.Models;
using WebApplication1.Storefront;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private IMemoryCache _cache;
        private StoreFrontHelper _storefrontHelper;

        public LoginController(IMemoryCache cache,StoreFrontHelper helper)
        {
            _cache=cache;
            _storefrontHelper=helper;
        }
        public IActionResult Index(CancellationToken token)
        {
            return View();
        }
        public async Task<IActionResult> Process(LoginInfo loginInfo,CancellationToken token)
        {
            // Store the server address and weburl in the session for further requests. Not the best
            // implementation, but the sample is about displaying applications. Another sanple should
            // show best practices about authentication.
            HttpContext.Session.SetString("SFAddress", loginInfo.SFAddress);
            HttpContext.Session.SetString("SFWebURL", loginInfo.SFWebURL);

            //process API
            var auth = await _storefrontHelper.GetAuthCredentialAsync(loginInfo.Username, loginInfo.Password, loginInfo.Domain,token);
            _cache.Set<CitrixAuthCredential>("auth", auth);
            //redirect to the application listing page.
            return RedirectToAction("Index", "Home");
        }
    }
}
