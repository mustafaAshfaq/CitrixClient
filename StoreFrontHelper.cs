using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using WebApplication1.Models;

namespace WebApplication1.Storefront
{
    public class StoreFrontHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string CitrixApp = @"Citrix/igt-citrix-pocWeb";
        private const string CONFIGURATION = @"home/configuration";
        private const string ExplicitLogin = @"explicitauth/login";
        private const string login = @"explicitauth/loginattempt";
        private const string listAuth = @"Authentication/GetAuthMethods";
        private IMemoryCache _memoryCache;
        public StoreFrontHelper(IHttpClientFactory factory,IMemoryCache cache) 
        {
            _httpClientFactory = factory;
            _memoryCache = cache;
        }
        private string getUri(string api) => $@"{CitrixApp}/{api}";

        public async Task<CitrixAuthCredential> GetAuthCredentialAsync(string domain,string username, string password,CancellationToken token)
        {
            var client = _httpClientFactory.CreateClient("storefront");
            await client.PostAsync(getUri(CONFIGURATION),null);
            await client.GetAsync(getUri(listAuth));
            await client.GetAsync(getUri(ExplicitLogin));
            string _authenticationBody = string.Format(@"username={0}\{1}&password={2}&saveCredentials={3}&loginBtn={4}&StateContext={5}"
                , domain, username, password, false, "Log On", "");
            StringContent _bodyContent = new StringContent(_authenticationBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            await client.PostAsync(getUri(login),_bodyContent,token);
            var _sfCredential = new CitrixAuthCredential
            {
                AuthToken = _memoryCache.Get<string>("CtxsAuthId")??string.Empty,
                CSRFToken = _memoryCache.Get<string>("CsrfToken") ?? string.Empty,
                SessionID = _memoryCache.Get<string>("ASP.NET_SessionId") ?? string.Empty,
                CookiePath = "/",
                CookieHost = client.BaseAddress?.Host??string.Empty,
                StorefrontUrl = CitrixApp
            };
            return _sfCredential;
        }

        public async Task<List<CitrixApplicationInfo>?> GetApplications(string domain, string username, string password, CancellationToken token)
        {
            var client = _httpClientFactory.CreateClient("storefront");
            var sfCredentials = _memoryCache.Get<CitrixAuthCredential>("auth");
            if(sfCredentials == null)
            {
                await client.PostAsync(getUri(CONFIGURATION), null);
                await client.PostAsync(getUri(listAuth),null);
                await client.PostAsync(getUri(ExplicitLogin),null);
                string _authenticationBody = string.Format(@"username={0}\{1}&password={2}&saveCredentials={3}&loginBtn={4}&StateContext={5}"
                    , domain, username, password, false, "Log On", "");
                StringContent _bodyContent = new StringContent(_authenticationBody, Encoding.UTF8, "application/x-www-form-urlencoded");
                var authResponse = await client.PostAsync(getUri(login), _bodyContent, token);
                if (!authResponse.IsSuccessStatusCode)
                    throw new Exception("Authorization failed");
                sfCredentials = new CitrixAuthCredential
                {
                    AuthToken = _memoryCache.Get<string>("CtxsAuthId") ?? string.Empty,
                    CSRFToken = _memoryCache.Get<string>("CsrfToken") ?? string.Empty,
                    SessionID = _memoryCache.Get<string>("ASP.NET_SessionId") ?? string.Empty,
                    CookiePath = "/",
                    CookieHost = client.BaseAddress?.Host ?? string.Empty,
                    StorefrontUrl = CitrixApp
                };
                _memoryCache.Set<CitrixAuthCredential>("auth", sfCredentials);
            }
            
            List<CitrixApplicationInfo>? citrixApplications = null;
            StringContent content = new StringContent("");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var resourceResponse = await client.PostAsync(getUri("resources/list"), content,token);
            if (resourceResponse.StatusCode == HttpStatusCode.OK)
            {
                string _resourcesJSON = await resourceResponse.Content.ReadAsStringAsync();

                JObject _resourcesBase = JObject.Parse(_resourcesJSON);

                citrixApplications = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CitrixApplicationInfo>>(_resourcesBase["resources"].ToString());
                if(citrixApplications != null && citrixApplications.Count > 0)
                    foreach (var _resource in citrixApplications)
                    {
                        _resource.Auth = sfCredentials;
                        //_resource.StorefrontURL;
                    }
            }
            return citrixApplications;
        }

        public async Task<string?>RetrieveICA(string appUrl,CancellationToken token)
        {
            var client=_httpClientFactory.CreateClient("storefront");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
            var response=await client.GetAsync(getUri(appUrl),token);
            if(response.StatusCode == HttpStatusCode.OK) 
            { 
                var icaFile= await response.Content.ReadAsStringAsync();
                return icaFile;
            }
            return null;
        }
        public async Task<byte[]?>GetImage(string imageUrl,CancellationToken token)
        {
            var client = _httpClientFactory.CreateClient("storefront");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("image/png"));
            var response=await client.GetAsync(getUri(imageUrl),token);
            if( response.StatusCode == HttpStatusCode.OK)
            { 
                var imageBytes= await response.Content.ReadAsByteArrayAsync();
                return imageBytes;
            }
            return null;
        }
        public async Task<ApplicationList?> GetApplicationsLocally(CancellationToken token)
        {
            var client = _httpClientFactory.CreateClient("local");
            var res = await client.GetFromJsonAsync<ApplicationList>("api/application");
           
            return res;
        }
        public async Task<byte[]>LaunchLocal(string appUrl,CitrixAuthCredential auth,CancellationToken token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("local");
                client.DefaultRequestHeaders.Add("CsrfToken", auth.CSRFToken);
                client.DefaultRequestHeaders.Add("CtxsAuthId", auth.AuthToken);
                client.DefaultRequestHeaders.Add("ASP.NET_SessionId", auth.SessionID);
                var data = await client.GetByteArrayAsync("api/application/launch/" + appUrl, token);
                return data;
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
            return new byte[] { };
            
        }

    }
}
