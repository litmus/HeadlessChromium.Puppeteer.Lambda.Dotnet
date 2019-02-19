#addin nuget:?package=SharpZipLib
#addin nuget:?package=Cake.Compression

using System.IO;
using Newtonsoft.Json;

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
	});
	
Task("UploadNugetPackages")
	.Does(() => {
		if(!AppVeyor.IsRunningOnAppVeyor)
		{
			return;
		}

		var files = GetFiles(artifactsDirectory + "/*.nupkg");
		foreach(var file in files)
		{
			AppVeyor.UploadArtifact(file);
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