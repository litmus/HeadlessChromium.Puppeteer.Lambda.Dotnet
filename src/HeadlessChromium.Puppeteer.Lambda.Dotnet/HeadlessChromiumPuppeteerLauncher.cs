using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.Threading.Tasks;

namespace HeadlessChromium.Puppeteer.Lambda.Dotnet
{
    public class HeadlessChromiumPuppeteerLauncher
    {
        public static string[] DefaultChromeArgs = new[] 
        {
            "--disable-background-timer-throttling",
            "--disable-breakpad",
            "--disable-client-side-phishing-detection",
            "--disable-cloud-import",
            "--disable-default-apps",
            "--disable-dev-shm-usage",
            "--disable-extensions",
            "--disable-gesture-typing",
            "--disable-hang-monitor",
            "--disable-infobars",
            "--disable-notifications",
            "--disable-offer-store-unmasked-wallet-cards",
            "--disable-offer-upload-credit-cards",
            "--disable-popup-blocking",
            "--disable-print-preview",
            "--disable-prompt-on-repost",
            "--disable-setuid-sandbox",
            "--disable-speech-api",
            "--disable-sync",
            "--disable-tab-for-desktop-share",
            "--disable-translate",
            "--disable-voice-input",
            "--disable-wake-on-wifi",
            "--disk-cache-size=33554432",
            "--enable-async-dns",
            "--enable-simple-cache-backend",
            "--enable-tcp-fast-open",
            "--enable-webgl",
            "--hide-scrollbars",
            "--ignore-gpu-blacklist",
            "--media-cache-size=33554432",
            "--metrics-recording-only",
            "--mute-audio",
            "--no-default-browser-check",
            "--no-first-run",
            "--no-pings",
            "--no-sandbox",
            "--no-zygote",
            "--password-store=basic",
            "--prerender-from-omnibox=disabled",
            "--use-gl=swiftshader",
            "--use-mock-keychain",
            "--single-process",
        };

        private readonly ILoggerFactory loggerFactory;

        public HeadlessChromiumPuppeteerLauncher(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public Task<Browser> LaunchAsync()
        {
            return LaunchAsync(DefaultChromeArgs);
        }

        public async Task<Browser> LaunchAsync(string[] chromeArgs)
        {
            var chromeLocation = new ChromiumExtractor(loggerFactory).ExtractChromium();

            var launchOptions = new LaunchOptions()
            {
                ExecutablePath = chromeLocation,
                Args = chromeArgs,
                Headless = true
            };

            return await new Launcher(loggerFactory).LaunchAsync(launchOptions);
        }
    }
}