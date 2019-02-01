using Microsoft.Extensions.Logging;
using Mono.Unix;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace HeadlessChromium.Puppeter.Lambda.Dotnet
{
    public class ChromiumExtractor
    {
        public static string ChromiumPath = "/tmp/chromium";

        private static readonly object SyncObject = new object();
        private readonly ILogger<ChromiumExtractor> logger;

        public ChromiumExtractor(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<ChromiumExtractor>();
        }

        /// <summary>
        /// Extracts chromium to temp path, if not already completed
        /// </summary>
        /// <returns>Path to chromium bin</returns>
        public string ExtractChromium()
        {
            if (!Directory.Exists("/tmp"))
            {
                logger.LogDebug("/tmp doesn't exist.  Is this running on lambda?");
            }

            // Quick bale if exec exists
            if (File.Exists(ChromiumPath))
            {
                return ChromiumPath;
            }

            logger.LogDebug("Chromium doesn't exist, extracting");

            lock (SyncObject)
            {
                if (!File.Exists(ChromiumPath))
                {
                    var compressedFile = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "chromium-*.br").FirstOrDefault();

                    logger.LogDebug($"Found compressed file {compressedFile}");

                    using (var writeFile = File.OpenWrite(ChromiumPath))
                    using (var readFile = File.OpenRead(compressedFile))
                    {
                         logger.LogDebug($"Extracting chromium to {ChromiumPath}");

                        using (var bs = new BrotliStream(readFile, CompressionMode.Decompress))
                        {
                            bs.CopyTo(writeFile);
                            bs.Dispose();
                        }

                        var fileInfo = new UnixFileInfo(ChromiumPath);
                        fileInfo.FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute | FileAccessPermissions.GroupReadWriteExecute;
                    }

                    logger.LogInformation("Extracted chromium to {ChromiumPath}", ChromiumPath);
                }
            }

            return ChromiumPath;
        }
    }
}