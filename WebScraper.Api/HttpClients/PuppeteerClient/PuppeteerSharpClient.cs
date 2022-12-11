using Flurl;
using PuppeteerSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using WebScraper.Api.Utilities;

namespace WebScraper.Api.HttpClients.PuppeteerClient;

public class PuppeteerSharpClient
{
    public string? ExecutablePath { get; set; }

    public async Task Get()
    {
        try
        {
            Stopwatch wa = Stopwatch.StartNew();

            string? currentDirectory = Directory.GetCurrentDirectory();
            string downloadPath = Path.Combine(currentDirectory, "..", "..", "CustomChromium");
            Console.WriteLine($"Attemping to set up puppeteer to use Chromium found under directory {downloadPath} ");

            if (!Directory.Exists(downloadPath))
            {
                Console.WriteLine("Custom directory not found. Creating directory");
                Directory.CreateDirectory(downloadPath);
            }

            BrowserFetcherOptions browserFetcherOptions = new BrowserFetcherOptions { Path = downloadPath };
            BrowserFetcher browserFetcher = new BrowserFetcher(browserFetcherOptions);
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            string executablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);

            if (string.IsNullOrEmpty(executablePath))
            {
                return;
            }

            Console.WriteLine($"Attemping to start Chromium using executable path: {executablePath}");

            LaunchOptions options = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = executablePath,
                Args = new[] {
                    "--proxy-server=http://premium.residential.proxyrack.net:9000",
                    "--disable-gl-drawing-for-tests",
                    "--autoplay-policy=user-gesture-required",
                      "--disable-background-networking",
                      "--disable-background-timer-throttling",
                      "--disable-backgrounding-occluded-windows",
                      "--disable-breakpad",
                      "--disable-client-side-phishing-detection",
                      "--disable-component-update",
                      "--disable-default-apps",
                      "--disable-dev-shm-usage",
                      "--disable-domain-reliability",
                      "--disable-extensions",
                      "--disable-features=AudioServiceOutOfProcess",
                      "--disable-hang-monitor",
                      "--disable-ipc-flooding-protection",
                      "--disable-notifications",
                      "--disable-offer-store-unmasked-wallet-cards",
                      "--disable-popup-blocking",
                      "--disable-print-preview",
                      "--disable-prompt-on-repost",
                      "--disable-renderer-backgrounding",
                      "--disable-setuid-sandbox",
                      "--disable-speech-api",
                      "--disable-sync",
                      "--hide-scrollbars",
                      "--ignore-gpu-blacklist",
                      "--metrics-recording-only",
                      "--mute-audio",
                      "--no-default-browser-check",
                      "--no-first-run",
                      "--no-pings",
                      "--no-sandbox",
                      "--no-zygote",
                      "--password-store=basic",
                      "--use-gl=swiftshader",
                      "--use-mock-keychain",
                }
            };

            using (IBrowser browser = await Puppeteer.LaunchAsync(options))
            {
                using (IPage page = await browser.NewPageAsync())
                {
                    await page.AuthenticateAsync(new Credentials() { Username = "_spider_", Password = "91b871-ad0b42-17c6fd-734129-7bb591" });

                    wa.Start();
                    List<string> urls = new List<string>();
                    for (int i = 0; i < 20; i++)
                    {
                        urls.Add("https://www.amazon.com.tr/Arzum-AR5035-Sense-D%C3%BCzle%C5%9Ftici-Siyah/dp/B0799BJTFG/ref=sr_1_1?__mk_tr_TR=%C3%85M%C3%85%C5%BD%C3%95%C3%91&crid=19T05OYJBJUTP&keywords=8693184956779&qid=1666634861&qu=eyJxc2MiOiIwLjAyIiwicXNhIjoiMC4wMCIsInFzcCI6IjAuMDAifQ%3D%3D&sprefix=8693184956779%2Caps%2C119&sr=8-1");
                    }

                    var result = Task.WhenAll(
                        urls.Select(url => Task.Factory.StartNew(async () =>
                        {
                            var page = await browser.NewPageAsync();
                            await page.GoToAsync(url, new NavigationOptions()
                            {
                                WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded}
                            });
                            string bodyHTML = await page.GetContentAsync();
                            return bodyHTML;
                        })));


                    wa.Stop();
                    Console.WriteLine(wa.Elapsed.TotalSeconds);

                    //await page.GoToAsync("");
                    //string bodyHTML = await page.GetContentAsync();



                    ////Console.WriteLine(bodyHTML);
                    //wa.Reset();

                    //IElementHandle[] elements = await page.XPathAsync("//span[@class='a-price-whole']");
                    //if (elements != null && elements.Length > 0)
                    //{
                    //    IElementHandle firstItem = elements[0];
                    //    var innerText = await firstItem.EvaluateFunctionAsync<string>("e => e.innerText");
                    //    wa.Stop();
                    //    Console.WriteLine(wa.Elapsed.TotalSeconds);
                    //}

                }
            }
        }
        catch (Exception ex)
        {

        }

        return;
    }

    public async Task Prepare()
    {
        try
        {
            string? currentDirectory = Directory.GetCurrentDirectory();
            string downloadPath = Path.Combine(currentDirectory, "..", "..", "CustomChromium");
            Console.WriteLine($"Attemping to set up puppeteer to use Chromium found under directory {downloadPath} ");

            if (!Directory.Exists(downloadPath))
            {
                Console.WriteLine("Custom directory not found. Creating directory");
                Directory.CreateDirectory(downloadPath);
            }

            BrowserFetcherOptions browserFetcherOptions = new BrowserFetcherOptions { Path = downloadPath };
            BrowserFetcher browserFetcher = new BrowserFetcher(browserFetcherOptions);

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            ExecutablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);

            if (string.IsNullOrEmpty(ExecutablePath))
            {
                try
                {
                    throw new Exception("ExecutablePath is null");
                }
                catch (Exception ex)
                {
                    await LogHelper.SaveErrorLog(ex, message: "Puppeteer executable path is absent", httpStatus: -7001);
                }
            }
        }
        catch (Exception ex)
        {
            await LogHelper.SaveErrorLog(ex, message: "Puppeteer can not be downloaded", httpStatus: -7000);
        }
    }

    public async Task<string[]> GetNewCookies(string url)
    {
        try
        {
            List<string> cookies = new List<string>();
            if (ExecutablePath == null)
            {
                throw new Exception("Executable Path is null");
            }

            LaunchOptions options = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = ExecutablePath,
                Args = new[] {
                    "--disable-gl-drawing-for-tests",
                    "--autoplay-policy=user-gesture-required",
                      "--disable-background-networking",
                      "--disable-background-timer-throttling",
                      "--disable-backgrounding-occluded-windows",
                      "--disable-breakpad",
                      "--disable-client-side-phishing-detection",
                      "--disable-component-update",
                      "--disable-default-apps",
                      "--disable-dev-shm-usage",
                      "--disable-domain-reliability",
                      "--disable-extensions",
                      "--disable-features=AudioServiceOutOfProcess",
                      "--disable-hang-monitor",
                      "--disable-ipc-flooding-protection",
                      "--disable-notifications",
                      "--disable-offer-store-unmasked-wallet-cards",
                      "--disable-popup-blocking",
                      "--disable-print-preview",
                      "--disable-prompt-on-repost",
                      "--disable-renderer-backgrounding",
                      "--disable-setuid-sandbox",
                      "--disable-speech-api",
                      "--disable-sync",
                      "--hide-scrollbars",
                      "--ignore-gpu-blacklist",
                      "--metrics-recording-only",
                      "--mute-audio",
                      "--no-default-browser-check",
                      "--no-first-run",
                      "--no-pings",
                      "--no-sandbox",
                      "--no-zygote",
                      "--password-store=basic",
                      "--use-gl=swiftshader",
                      "--use-mock-keychain",
                }
            };

            using (IBrowser browser = await Puppeteer.LaunchAsync(options))
            {
                using (IPage page = await browser.NewPageAsync())
                {
                    for (int i = 0; i < 50; i++)
                    {
                        await page.GoToAsync(url, new NavigationOptions()
                        {
                            WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }
                        });
                        CookieParam[] cookieParams = await page.GetCookiesAsync(url);
                        if (cookieParams != null)
                        {
                            string _cookie = string.Join(';', cookieParams.Select(eachParam => $"{eachParam.Name}={eachParam.Value}"));
                            cookies.Add(_cookie);
                        }
                    }
                }
                await browser.CloseAsync();
            }
            return cookies.ToArray();
        }
        catch (Exception ex)
        {

        }

        return new string[0];
    }
}