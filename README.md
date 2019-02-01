# HeadlessChromium.Puppeteer.Lambda.Dotnet
Packages everything you need to run [PuppeteerSharp](https://github.com/kblok/puppeteer-sharp) in AWS Lambda on Chromium into a Nuget Package

[![Build status](https://ci.appveyor.com/api/projects/status/m2tryiprv3o4qqdn?svg=true)](https://ci.appveyor.com/project/brianfeucht/headlesschromium-puppeteer-lambda-dotnet)
[![Nuget status](https://img.shields.io/nuget/v/HeadlessChromium.Puppeteer.Lambda.Dotnet.svg?style=flat)](https://www.nuget.org/packages/HeadlessChromium.Puppeteer.Lambda.Dotnet)

# Description
The chromium binary for this project has been extracted from the NPM project [chrome-aws-lambda](https://github.com/alixaxel/chrome-aws-lambda).  It is automatically extracted to `/tmp/chromium` at runtime.

# Usage
Screenshot a URL as a byte[].  This project requires lambda to be configured as `netcoreapp2.1`
```
var browserLauncher = new HeadlessChromiumPuppeterLauncher(logger);

using(var browser = await browserLauncher.LaunchAsync())
using(var page = await browser.NewPageAsync())
{
    await page.GoToAsync(url);
    return await page.ScreenshotDataAsync();
}
```

For more use cases see the [PuppeteerSharp documentation](http://www.puppeteersharp.com/api/index.html)

# Building
To build locally:
```
.\build.ps1 -Target Build
```
