using Amazon.Lambda.Core;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace SampleLambda
{
    public class HelloWorldHandler
    {
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<byte[]> Handle(ILambdaContext context)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var browserLauncher = new HeadlessChromiumPuppeteerLauncher(loggerFactory);

            var launchArgs = HeadlessChromiumPuppeteerLauncher.DefaultChromeArgs
                .Append("--enable-logging")
                .Append("--log-level=0")
                .Append("--single-process")
                .ToArray();


            var chromeLocation = new ChromiumExtractor(loggerFactory).ExtractChromium();

            var launchOptions = new LaunchOptions()
            {
                ExecutablePath = chromeLocation,
                Args = launchArgs,
                Headless = true,
                DumpIO = true,
                EnqueueTransportMessages = false,
                UserDataDir = "/tmp/",
            };
            
            await using (var browser = await new Launcher(loggerFactory).LaunchAsync(launchOptions))
            await using (var page = await browser.NewPageAsync())
            {
                await page.GoToAsync("https://www.google.com");
                await page.ScreenshotAsync("./google.png");
            }

            return await File.ReadAllBytesAsync("./google.png");
        }
    }
}