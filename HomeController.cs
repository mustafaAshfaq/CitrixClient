using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Resources;
using System.Text;
using WebApplication1.Models;
using WebApplication1.Storefront;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StoreFrontHelper _storeFrontHelper;
        private const string username = "username";//args[0];
        const string domain = "domain";
        const string password = "password";
        IMemoryCache _memoryCache;

        public HomeController(ILogger<HomeController> logger,StoreFrontHelper storeFrontHelper,IMemoryCache cache)
        {
            _logger = logger;
            _storeFrontHelper = storeFrontHelper;
            _memoryCache = cache;
        }

        public async Task<IActionResult> Index(CancellationToken token)
        {
            try
            {
                var applications = _memoryCache.Get<ApplicationList>("apps")?? await _storeFrontHelper.GetApplicationsLocally(token);//.GetApplications(domain,username, password, token);
                if (applications != null)
                {
                    HttpContext.Session.SetString("appList", JsonConvert.SerializeObject(applications.Applications));
                    _memoryCache.Set<ApplicationList>("apps", applications);
                    _memoryCache.Set<CitrixAuthCredential>("auth", applications.Auth);
                    return View(applications.Applications);
                }
                    
                return Error();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Error();
            }
           
        }
        public async Task<IActionResult> Launch(string AppID,[FromQuery]string AppUrl,CancellationToken token)
        {
            var tempArr = AppUrl.Split("/");
            var launchUrl = tempArr[tempArr.Length - 1];
            if(string.IsNullOrEmpty(launchUrl))
                return Error();
            tempArr = launchUrl.Split(".");
            launchUrl = tempArr[0];
            var applist = JsonConvert.DeserializeObject<List<ApplicationDetail>>(HttpContext.Session.GetString("appList"));

            var application = applist?.SingleOrDefault(appInfo => appInfo.Id == AppID);

            if (application != null)
            {
                var auth = _memoryCache.Get<CitrixAuthCredential>("auth");
                var fileData = await _storeFrontHelper.LaunchLocal(launchUrl, auth, token);
                if (fileData != null)
                {
                    return File(fileData, "application/x-ica", application.Title + "_" + Guid.NewGuid().ToString()+".ica");
                }
                
                _logger.LogError("Error in getting ica file");
            }

            return Error();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}