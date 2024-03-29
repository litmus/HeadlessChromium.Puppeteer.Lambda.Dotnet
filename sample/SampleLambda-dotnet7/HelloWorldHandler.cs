﻿using Amazon.Lambda.Core;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;

namespace SampleLambda
{
    public class HelloWorldHandler
    {
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<byte[]> Handle(ILambdaContext context)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var browserLauncher = new HeadlessChromiumPuppeteerLauncher(loggerFactory);

            await using (var browser = await browserLauncher.LaunchAsync())
            await using (var page = await browser.NewPageAsync())
            {
                await page.GoToAsync("https://www.google.com");
                await page.ScreenshotAsync("./google.png");
            }

            return await File.ReadAllBytesAsync("./google.png");
        }
    }
}