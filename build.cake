#addin nuget:?package=SharpZipLib
#addin nuget:?package=Cake.Compression

var currentNpmTarball = "https://registry.npmjs.org/chrome-aws-lambda/-/chrome-aws-lambda-1.11.2.tgz";

var configuration = Argument("configuration", "Release");
var version = AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Version : "0-dev";
var releaseBinPath = "./src/HeadlessChromium.Puppeter.Lambda.Dotnet/bin/Release/netcoreapp2.1";
var artifactsDirectory = "./artifacts";

var target = Argument("target", "Build");

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
            DotNetCoreBuild("./src/HeadlessChromium.Puppeter.Lambda.Dotnet.sln", settings);
	});

Task("ExtractChromiumFromNpmPackage")
	.Does(() =>
	{
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

RunTarget(target);