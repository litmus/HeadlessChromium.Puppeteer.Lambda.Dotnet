# HeadlessChromium.Puppeteer.Lambda.Dotnet
Packages everything you need to run [PuppeteerSharp](https://github.com/kblok/puppeteer-sharp) in AWS Lambda on Chromium into a Nuget Package

[![Build status](https://ci.appveyor.com/api/projects/status/m2tryiprv3o4qqdn?svg=true)](https://ci.appveyor.com/project/brianfeucht/headlesschromium-puppeteer-lambda-dotnet)
[![CodeFactor](https://www.codefactor.io/repository/github/litmus/headlesschromium.puppeteer.lambda.dotnet/badge)](https://www.codefactor.io/github/litmus/headlesschromium.puppeteer.lambda.dotnet)
[![Nuget status](https://img.shields.io/nuget/v/HeadlessChromium.Puppeteer.Lambda.Dotnet.svg?style=flat)](https://www.nuget.org/packages/HeadlessChromium.Puppeteer.Lambda.Dotnet)

# Description
The chromium binary for this project has been extracted from the NPM project [Sparticuz/chromium](https://github.com/Sparticuz/chromium).  It is automatically extracted to `/tmp/chromium` at runtime.

# Usage
Screenshot a URL as a byte[].  This project requires lambda to be configured as `netcoreapp3.1`
```
var browserLauncher = new HeadlessChromiumPuppeteerLauncher(logger);

using(var browser = await browserLauncher.LaunchAsync())
using(var page = await browser.NewPageAsync())
{
    await page.GoToAsync(url);
    return await page.ScreenshotDataAsync();
}
```

For more use cases see the [PuppeteerSharp documentation](http://www.puppeteersharp.com/api/index.html)

# Projects using this library
These projects are using this library and are good examples of how you might consume this nuget package
- [HtmlToPDF](https://github.com/unnijeevan/HtmlToPDF)

# Building
To build locally:
```
.\build.ps1 -Target Build
```
