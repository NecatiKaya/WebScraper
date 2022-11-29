using Flurl.Http.Configuration;
using System.Net;

namespace WebScraper.Api.HttpClients;

public class ProxyHttpClientFactory : DefaultHttpClientFactory
{
    public ProxyHttpClientFactory(string userName, string password, string proxyServerAddress)
    {
        UserName = userName;
        Password = password;
        ProxyServerAddress = proxyServerAddress;
    }

    public string UserName { get; }
    public string Password { get; }
    public string ProxyServerAddress { get; }

    public override HttpMessageHandler CreateMessageHandler()
    {
        WebProxy proxy = new WebProxy();
        proxy.Address = new Uri(ProxyServerAddress);
        proxy.Credentials = new NetworkCredential(UserName, Password);

        return new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = true
        };
    }
}