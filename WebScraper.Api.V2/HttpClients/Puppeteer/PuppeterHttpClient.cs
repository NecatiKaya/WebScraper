using PuppeteerSharp;

namespace WebScraper.Api.V2.HttpClients.Puppeteer;

public class PuppeterHttpClient : CrawlerHttpClientBase
{
    public string? ExecutablePath { get; set; }

    public PuppeterHttpClient(ClientConfiguration options) : base(options)
    {

    }

    public override async Task ConfigureAsync()
    {
        string downloadPath = CheckAndCreateDirectory();

        BrowserFetcherOptions browserFetcherOptions = new BrowserFetcherOptions { Path = downloadPath };
        
        using (BrowserFetcher browserFetcher = new BrowserFetcher(browserFetcherOptions))
        {
            if (!await browserFetcher.CanDownloadAsync(BrowserFetcher.DefaultChromiumRevision))
            {
                throw new PuppeteerDownloadException();
            }

            try
            {
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            }
            catch (Exception ex)
            {
                throw new PuppeteerDownloadException(ex);
            }

            ExecutablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);
        }

        if (string.IsNullOrEmpty(ExecutablePath))
        {
            throw new PuppeteerExecutablePathException();
        }
    }

    public override void Configure()
    {
       
    }

    public override string Crawl(string url, string? cookie, string? userAgent)
    {
        throw new NotImplementedException("PuppeterHttpClient.Crawl() is not implemented. Please make use of one of CrawlAsync(...) methods");
    }

    public override async Task<string> CrawlAsync(string url, string? cookie, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(ExecutablePath))
        {
            throw new PuppeteerExecutablePathException();
        }

        LaunchOptions options = GetLaunchOptions();
        using (IBrowser browser = await PuppeteerSharp.Puppeteer.LaunchAsync(options))
        {
            using (IPage page = await browser.NewPageAsync())
            {
                await page.GoToAsync(url, new NavigationOptions()
                {
                    WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }
                });
                string bodyHTML = await page.GetContentAsync();
                return bodyHTML;
            }
        }
    }

    private string CheckAndCreateDirectory()
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

            return downloadPath;
        }
        catch (Exception ex)
        {
            throw new PuppeteerDirectoryAccessException(ex);
        }
    }

    private LaunchOptions GetLaunchOptions()
    {
        return new LaunchOptions
        {
            Headless = true,
            ExecutablePath = this.ExecutablePath,
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
    }
}