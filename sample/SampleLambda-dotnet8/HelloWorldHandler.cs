using Amazon.Lambda.Core;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// This class is used to register the input event and return type for the FunctionHandler method with the System.Text.Json source generator.
    /// There must be a JsonSerializable attribute for each type used as the input and return type or a runtime error will occur
    /// from the JSON serializer unable to find the serialization information for unknown types.
    /// </summary>
    [JsonSerializable(typeof(string))]
    public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
    {
        // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
        // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
        // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
    }
}