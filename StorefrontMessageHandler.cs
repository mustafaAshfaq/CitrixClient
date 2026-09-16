using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace WebApplication1.Storefront
{
    public class StorefrontMessageHandler : DelegatingHandler
    {
        IMemoryCache cache;
        public StorefrontMessageHandler(IMemoryCache _cache)
        {
            cache = _cache;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? csrfToken;
            cache.TryGetValue<string>("CsrfToken", out csrfToken);
            if(csrfToken!=null)
            {
                request.Headers.Add("Csrf-Token", csrfToken);
                CookieContainer cookieContainer = getCookieContainer(request.RequestUri!);
                if (cookieContainer != null)
                {
                    request.Headers.Add("Cookie", cookieContainer.GetCookieHeader(request.RequestUri!));
                }
            }
            //ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            _ = new NetEventListener();
            var response = await base.SendAsync(request, cancellationToken);
            SetCookies(response);
            return response;
        }

        private void SetCookies(HttpResponseMessage response)
        {
            List<string> cookies = cache.Get<List<string>>("Set-Cookie") ?? new List<string>();
            foreach (var header in response.Headers.Where(i => i.Key == "Set-Cookie"))
            {
                foreach (string cookieValue in header.Value)
                {
                    //"ASP.NET_SessionId=miphlcqdo53dwdipdxj3vp4i; path=/; HttpOnly"
                    string[] cookieElements = cookieValue.Split(';');
                    string[] keyValueElements = cookieElements[0].Split('=');
                    if (cache.Get<string>(keyValueElements[0]) == null)
                    {
                        cache.Set<string>(keyValueElements[0], keyValueElements[1]);
                        cookies.Add(keyValueElements[0]);
                    }
                    else if (string.Equals(keyValueElements[0], "CsrfToken"))
                        cache.Set<string>(keyValueElements[0], keyValueElements[1]);
                }
            }
            cache.Set<List<string>>("Set-Cookie", cookies);
        }
        private CookieContainer getCookieContainer(Uri cookiUri)
        {
            CookieContainer container = new CookieContainer();
            foreach (var key in cache.Get<List<string>>("Set-Cookie"))
            {
                container.Add(cookiUri, new Cookie(key, cache.Get<string>(key), "/", cookiUri.Host));
            }
            return container;
        }
    }
}
