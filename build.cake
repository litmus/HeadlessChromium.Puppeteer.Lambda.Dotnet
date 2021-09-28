#addin nuget:?package=SharpZipLib&version=1.3.1
#addin nuget:?package=Cake.Compression&version=0.2.6

using System.IO;
using Newtonsoft.Json;
using ICSharpCode.SharpZipLib.GZip;

var configuration = Argument("configuration", "Release");
var version = AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Version : "0-dev";
var releaseBinPath = "./src/HeadlessChromium.Puppeteer.Lambda.Dotnet/bin/Release/netcoreapp2.1";
var artifactsDirectory = "./artifacts";

var target = Argument("target", "Default");

Task("Setup")
	.Does(() => { 
		CreateDirectory(artifactsDirectory);
	});
	
Task("Build")
  .IsDependentOn("ExtractChromiumFromNpmPackage")
  .Does(() => {
            var settings = new DotNetCoreBuildSettings
            {
                Configuration = configuration,
                OutputDirectory = artifactsDirectory,
                VersionSuffix = version,
            };
            DotNetCoreBuild("./src/HeadlessChromium.Puppeteer.Lambda.Dotnet.sln", settings);
	});

Task("ExtractChromiumFromNpmPackage")
	.Does(() =>
	{
		var currentNpmTarball = GetNpmTarballUrl();
		DownloadFile(currentNpmTarball, "chrome-aws-lambda.tgz");
		GZipUncompress("chrome-aws-lambda.tgz", "chrome-aws-lambda");

		if(!FileExists("./chrome-aws-lambda/package/bin/chromium.br"))
		{
			throw new Exception("chromium.br did not extract to expected location");
		}
	});
	
Task("UploadNugetPackages")
	.Does(() => {
		var files = GetFiles(artifactsDirectory + "/*.*nupkg");
		foreach(var file in files)
		{
			Information("Found artifact to publish - {0}", file);
			if(AppVeyor.IsRunningOnAppVeyor)
			{
				AppVeyor.UploadArtifact(file, 
					settings => settings.SetArtifactType(AppVeyorUploadArtifactType.NuGetPackage));
			}
		}
	});

Task("Default")
	.IsDependentOn("Build")
	.IsDependentOn("UploadNugetPackages");

string GetNpmTarballUrl()
{
	var json = System.IO.File.ReadAllText("package.json");
	var package = JsonConvert.DeserializeObject<dynamic>(json);
	var version = package.dependencies["chrome-aws-lambda"];
	return $"https://registry.npmjs.org/chrome-aws-lambda/-/chrome-aws-lambda-{version}.tgz";
}

RunTarget(target);