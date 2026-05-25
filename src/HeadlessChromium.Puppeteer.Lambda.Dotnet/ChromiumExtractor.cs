using Microsoft.Extensions.Logging;
using Mono.Unix;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using HeadlessChromium.Puppeteer.Lambda.Dotnet.Tar;

namespace HeadlessChromium.Puppeteer.Lambda.Dotnet
{
    public class ChromiumExtractor
    {
        private static string FontConfigEnvVariable = "FONTCONFIG_PATH";
        private static string FontConfigValue = "/tmp";
        private static string LdLibEnvVariable = "LD_LIBRARY_PATH";
        private static string LdLibValue = "/tmp/lib";

        public string ChromiumPath => DefaultChromiumPath;
        public static string DefaultChromiumPath => "/tmp/chromium";

        private static readonly object SyncObject = new object();
        private readonly ILogger<ChromiumExtractor> logger;
        private readonly ILoggerFactory loggerFactory;

        private string awsOperatingSystem;
        public string AwsOperatingSystem
        {
            get
            {
                if (awsOperatingSystem != null)
                {
                    return awsOperatingSystem;
                }

                if(File.Exists("/etc/system-release-cpe"))
                {
                    var osDetails = File
                        .ReadLines("/etc/system-release-cpe")
                        .FirstOrDefault() ?? string.Empty;

                    if(osDetails.EndsWith("amazon:amazon_linux:2"))
                    {
                        awsOperatingSystem = "al2";
                    }
                    else if(osDetails.EndsWith("amazon:amazon_linux:2023"))
                    {
                        awsOperatingSystem = "al2023";
                    }
                }

                return awsOperatingSystem;
            }

            set => awsOperatingSystem = value;
        }

        public ChromiumExtractor(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<ChromiumExtractor>();
        }

        /// <summary>
        /// Extracts chromium to temp path, if not already completed
        /// </summary>
        /// <returns>Path to chromium bin</returns>
        public string ExtractChromium()
        {
            SetEnvironmentVariables();

            if (!Directory.Exists("/tmp"))
            {
                logger.LogDebug("/tmp doesn't exist.  Is this running on lambda?");
            }

            if (File.Exists(DefaultChromiumPath))
            {
                try { ValidateBinary(DefaultChromiumPath); return DefaultChromiumPath; }
                catch (ChromiumExtractionException) { /* fall through to lock */ }
            }

            logger.LogDebug("Chromium doesn't exist, extracting");

            lock (SyncObject)
            {
                var sourceDirectory = FindSourceDirectory();

                if (!string.IsNullOrEmpty(AwsOperatingSystem))
                {
                    ExtractDependencies($"{AwsOperatingSystem}.tar.br", "/tmp", sourceDirectory);
                }
                else
                {
                    logger.LogWarning("Operating environment unexpected. Unable to extract correct dependencies.");
                }

                ExtractDependencies("fonts.tar.br", "/tmp", sourceDirectory);
                ExtractDependencies("swiftshader.tar.br", "/tmp", sourceDirectory);

                var compressedFile = Path.Combine(sourceDirectory, "chromium.br");

                logger.LogDebug($"Found compressed file {compressedFile}");

                using (var writeFile = File.OpenWrite(DefaultChromiumPath))
                using (var readFile = File.OpenRead(compressedFile))
                {
                    logger.LogDebug($"Extracting chromium to {DefaultChromiumPath}");

                    using (var bs = new BrotliStream(readFile, CompressionMode.Decompress))
                    {
                        bs.CopyTo(writeFile);
                        bs.Dispose();
                    }

                    var fileInfo = new UnixFileInfo(DefaultChromiumPath);
                    fileInfo.FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute |
                                                     FileAccessPermissions.GroupReadWriteExecute;
                }

                logger.LogInformation("Extracted chromium to {ChromiumPath}", DefaultChromiumPath);
            }

            return DefaultChromiumPath;
        }

        internal void ValidateBinary(string path)
        {
            if (!File.Exists(path))
                throw new ChromiumExtractionException(
                    $"Chromium binary not found: {path}");

            if (new FileInfo(path).Length == 0)
                throw new ChromiumExtractionException(
                    $"Chromium binary is empty (0 bytes): {path}");

            if (!new UnixFileInfo(path).FileAccessPermissions
                    .HasFlag(FileAccessPermissions.UserExecute))
                throw new ChromiumExtractionException(
                    $"Chromium binary is not executable: {path}");
        }

        internal IEnumerable<string> GetCandidateDirectories()
        {
            yield return AppDomain.CurrentDomain.BaseDirectory;

            var execAsm = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(execAsm))
            {
                var d = Path.GetDirectoryName(execAsm);
                if (!string.IsNullOrEmpty(d)) yield return d;
            }

            var entryAsm = Assembly.GetEntryAssembly()?.Location;
            if (!string.IsNullOrEmpty(entryAsm))
            {
                var d = Path.GetDirectoryName(entryAsm);
                if (!string.IsNullOrEmpty(d)) yield return d;
            }
        }

        internal string FindSourceDirectory()
        {
            var deduped = GetCandidateDirectories()
                .Distinct()
                .Where(d => !string.IsNullOrEmpty(d))
                .ToList();

            var found = deduped.FirstOrDefault(d => File.Exists(Path.Combine(d, "chromium.br")));

            if (found == null)
                throw new ChromiumExtractionException(
                    "Could not find chromium.br in any candidate directory: " +
                    string.Join(", ", deduped));

            return found;
        }

        private void SetEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("HOME", "/tmp");

            var fontConfig = Environment.GetEnvironmentVariable(FontConfigEnvVariable);
            if (string.IsNullOrEmpty(fontConfig) || !fontConfig.Contains(FontConfigValue))
            {
                var newValue = string.IsNullOrEmpty(fontConfig) ? FontConfigValue : $"{fontConfig}:{FontConfigValue}";
                logger.LogDebug("Setting {FontConfigEnvVariable} to {FontConfigValue}", FontConfigEnvVariable, newValue);
                Environment.SetEnvironmentVariable(FontConfigEnvVariable, newValue);
            }

            var ldLibPath = Environment.GetEnvironmentVariable(LdLibEnvVariable);
            if (string.IsNullOrEmpty(ldLibPath) || !ldLibPath.Contains(LdLibValue))
            {
                var newValue = string.IsNullOrEmpty(ldLibPath) ? LdLibValue : $"{ldLibPath}:{LdLibValue}";
                logger.LogDebug("Setting {LdLibEnvVariable} to {LdLibValue} ", LdLibEnvVariable, newValue);
                Environment.SetEnvironmentVariable(LdLibEnvVariable, newValue);
            }
        }

        private void ExtractDependencies(string fileName, string path, string sourceDirectory)
        {
            var compressedFile = Path.Combine(sourceDirectory, fileName);

            logger.LogDebug($"Found compressed file {compressedFile}");
            using (var stream = new MemoryStream())
            using (var readFile = File.OpenRead(compressedFile))
            {
                using (var bs = new BrotliStream(readFile, CompressionMode.Decompress))
                {
                    bs.CopyTo(stream);
                    bs.Dispose();
                }

                stream.Seek(0, SeekOrigin.Begin);

                var tarReader = new TarReader(stream, loggerFactory);
                tarReader.ReadToEnd(path);
            }
        }
    }
}
