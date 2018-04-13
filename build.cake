// Install addins.
#addin nuget:https://api.nuget.org/v3/index.json?package=Cake.Figlet
#addin nuget:https://api.nuget.org/v3/index.json?package=Cake.Coveralls
#addin nuget:https://api.nuget.org/v3/index.json?package=Cake.Twitter
#addin nuget:https://api.nuget.org/v3/index.json?package=Cake.Gitter

// Install tools.
#tool "nuget:https://api.nuget.org/v3/index.json?package=gitreleasemanager"
#tool "nuget:https://api.nuget.org/v3/index.json?package=GitVersion.CommandLine"
#tool "nuget:https://api.nuget.org/v3/index.json?package=coveralls.io"
#tool "nuget:https://api.nuget.org/v3/index.json?package=OpenCover"
#tool "nuget:https://api.nuget.org/v3/index.json?package=ReportGenerator"
#tool "nuget:https://api.nuget.org/v3/index.json?package=SignClient"

// Load other scripts.
#load "./build/parameters.cake"

//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

var parameters = BuildParameters.GetParameters(Context);
var publishingError = false;
DotNetCoreMSBuildSettings msBuildSettings = null;
FilePath signClientPath;

Information(Figlet(parameters.ProductName));

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    parameters.Initialize(context);

    if(parameters.IsMainBranch && (context.Log.Verbosity != Verbosity.Diagnostic))
	{
		// On the main branch, we always want the diagnostic logging.
        context.Log.Verbosity = Verbosity.Diagnostic;
    }

	parameters.DumpToLog();

    var releaseNotes = string.Join("\n", parameters.ReleaseNotes.Notes.ToArray()).Replace("\"", "\"\"");
    msBuildSettings = new DotNetCoreMSBuildSettings()
                            .WithProperty("Version", parameters.Version.SemVersion)
                            .WithProperty("AssemblyVersion", parameters.Version.Version)
                            .WithProperty("FileVersion", parameters.Version.Version)
                            .WithProperty("PackageReleaseNotes", string.Concat("\"", releaseNotes, "\""))
							.WithProperty("WorkingDirectory", context.Directory("./src"));

    if(!parameters.IsRunningOnWindows)
    {
        var frameworkPathOverride = new FilePath(typeof(object).Assembly.Location).GetDirectory().FullPath + "/";

       // Use FrameworkPathOverride when not running on Windows.
       Information("Build will use FrameworkPathOverride={0} since not building on Windows.", frameworkPathOverride);
       msBuildSettings.WithProperty("FrameworkPathOverride", frameworkPathOverride);
    }

    signClientPath = Context.Tools.Resolve("SignClient.dll") ?? throw new Exception("Failed to locate sign tool");
});

Teardown(context =>
{
	Information("Starting Teardown...");

	if(context.Successful)
	{
		if(parameters.ShouldPublish)
		{
			if(parameters.CanPostToTwitter)
			{
				var message = "Version " + parameters.Version.SemVersion + " of Cake has just been released, https://www.nuget.org/packages/Cake.";
				TwitterSendTweet(parameters.Twitter.ConsumerKey, parameters.Twitter.ConsumerSecret, parameters.Twitter.AccessToken, parameters.Twitter.AccessTokenSecret, message);
			}

			if(parameters.CanPostToGitter)
			{
				var message = "@/all Version " + parameters.Version.SemVersion + " of the Cake has just been released, https://www.nuget.org/packages/Cake.";
				var postMessageResult = Gitter.Chat.PostMessage(
					message: message,
					messageSettings: new GitterChatMessageSettings { Token = parameters.Gitter.Token, RoomId = parameters.Gitter.RoomId}
					);

				if (postMessageResult.Ok)
				{
					Information("Message {0} succcessfully sent", postMessageResult.TimeStamp);
				}
				else
				{
					Error("Failed to send message: {0}", postMessageResult.Error);
				}
			}
		}
	}

	Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories(parameters.Paths.Directories.ToClean);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
	Information(parameters.Paths.Files.SolutionFilePath.FullPath);
    DotNetCoreRestore(parameters.Paths.Files.SolutionFilePath.FullPath, new DotNetCoreRestoreSettings
    {
        Verbosity = DotNetCoreVerbosity.Normal,
        Sources = new [] {
            "https://www.myget.org/F/xunit/api/v3/index.json",
            "https://dotnet.myget.org/F/dotnet-core/api/v3/index.json",
            "https://dotnet.myget.org/F/cli-deps/api/v3/index.json",
            "https://api.nuget.org/v3/index.json",
        },
        MSBuildSettings = msBuildSettings
    });
});

Task("UpdateVersioning")
	.Does(() =>
{
});

Task("GetVersionInfo")
    .Does(() =>
{
    var result = GitVersion(new GitVersionSettings {
        Branch = "develop",
		OutputType = GitVersionOutput.Json
    });
});

Task("IncrementVersioning")
	.Does(() =>
{
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
	.IsDependentOn("UpdateVersioning")
    .Does(() =>
{
    // Build the solution.
    DotNetCoreBuild(parameters.Paths.Files.SolutionFilePath.FullPath, new DotNetCoreBuildSettings()
    {
        Configuration = parameters.Configuration,
        MSBuildSettings = msBuildSettings
    });
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projects = GetFiles("./src/**/*.Tests.csproj");
    foreach(var project in projects)
    {
        DotNetCoreTool(project,
            "xunit",  "--no-build -noshadow -configuration " + parameters.Configuration);
    }
});

Task("Copy-Files")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    // .NET 4.6
    DotNetCorePublish("./src/Cake", new DotNetCorePublishSettings
    {
        Framework = "net461",
        VersionSuffix = parameters.Version.DotNetAsterix,
        Configuration = parameters.Configuration,
        OutputDirectory = parameters.Paths.Directories.ArtifactsBinFullFx,
        MSBuildSettings = msBuildSettings
    });

    // .NET Core
    DotNetCorePublish("./src/Cake", new DotNetCorePublishSettings
    {
        Framework = "netcoreapp1.0",
        Configuration = parameters.Configuration,
        OutputDirectory = parameters.Paths.Directories.ArtifactsBinNetCore,
        MSBuildSettings = msBuildSettings
    });

    // Copy license
    CopyFileToDirectory("./LICENSE", parameters.Paths.Directories.ArtifactsBinFullFx);
    CopyFileToDirectory("./LICENSE", parameters.Paths.Directories.ArtifactsBinNetCore);

    // Copy Cake.XML (since publish does not do this anymore)
    CopyFileToDirectory("./src/Cake/bin/" + parameters.Configuration + "/net461/Cake.xml", parameters.Paths.Directories.ArtifactsBinFullFx);
    CopyFileToDirectory("./src/Cake/bin/" + parameters.Configuration + "/netcoreapp1.0/Cake.xml", parameters.Paths.Directories.ArtifactsBinNetCore);
});

Task("Zip-Files")
    .IsDependentOn("Copy-Files")
    .Does(() =>
{
    // .NET 4.6
    var homebrewFiles = GetFiles( parameters.Paths.Directories.ArtifactsBinFullFx.FullPath + "/**/*");
    Zip(parameters.Paths.Directories.ArtifactsBinFullFx, parameters.Paths.Files.ZipArtifactPathDesktop, homebrewFiles);

    // .NET Core
    var coreclrFiles = GetFiles( parameters.Paths.Directories.ArtifactsBinNetCore.FullPath + "/**/*");
    Zip(parameters.Paths.Directories.ArtifactsBinNetCore, parameters.Paths.Files.ZipArtifactPathCoreClr, coreclrFiles);
});

Task("Create-Chocolatey-Packages")
    .IsDependentOn("Copy-Files")
    .IsDependentOn("Package")
    .WithCriteria(() => parameters.IsRunningOnWindows)
    .Does(() =>
{
    foreach(var package in parameters.Packages.Chocolatey)
    {
        var netFxFullArtifactPath = MakeAbsolute(parameters.Paths.Directories.ArtifactsBinFullFx).FullPath;
        var curDirLength =  MakeAbsolute(Directory("./")).FullPath.Length + 1;

        // Create package.
        ChocolateyPack(package.NuspecPath, new ChocolateyPackSettings {
            Version = parameters.Version.SemVersion,
            ReleaseNotes = parameters.ReleaseNotes.Notes.ToArray(),
            OutputDirectory = parameters.Paths.Directories.NugetRoot,
            Files = (GetFiles(netFxFullArtifactPath + "/**/*") + GetFiles("./nuspec/*.txt"))
                                    .Where(file => file.FullPath.IndexOf("/runtimes/", StringComparison.OrdinalIgnoreCase) < 0)
                                    .Select(file=>"../" + file.FullPath.Substring(curDirLength))
                                    .Select(file=> new ChocolateyNuSpecContent { Source = file })
                                    .ToArray()
        });
    }
});

Task("Create-NuGet-Packages")
    .IsDependentOn("Copy-Files")
    .Does(() =>
{
    // Build libraries
    var projects = GetFiles("./src/**/*.csproj");
    foreach(var project in projects)
    {
        var name = project.GetDirectory().FullPath;
        if(name.EndsWith("Cake") || name.EndsWith("Tests") || name.EndsWith("Xunit"))
        {
            continue;
        }

        DotNetCorePack(project.FullPath, new DotNetCorePackSettings {
            Configuration = parameters.Configuration,
            OutputDirectory = parameters.Paths.Directories.NugetRoot,
            NoBuild = true,
            IncludeSymbols = true,
            MSBuildSettings = msBuildSettings
        });
    }

    // Cake - Symbols - .NET 4.6
    NuGetPack("./nuspec/Cake.symbols.nuspec", new NuGetPackSettings {
        Version = parameters.Version.SemVersion,
        ReleaseNotes = parameters.ReleaseNotes.Notes.ToArray(),
        BasePath = parameters.Paths.Directories.ArtifactsBinFullFx,
        OutputDirectory = parameters.Paths.Directories.NugetRoot,
        Symbols = true,
        NoPackageAnalysis = true
    });

    var netFxFullArtifactPath = MakeAbsolute(parameters.Paths.Directories.ArtifactsBinFullFx).FullPath;
    var netFxFullArtifactPathLength = netFxFullArtifactPath.Length+1;

    // Cake - .NET 4.6
    NuGetPack("./nuspec/Cake.nuspec", new NuGetPackSettings {
        Version = parameters.Version.SemVersion,
        ReleaseNotes = parameters.ReleaseNotes.Notes.ToArray(),
        BasePath = netFxFullArtifactPath,
        OutputDirectory = parameters.Paths.Directories.NugetRoot,
        Symbols = false,
        NoPackageAnalysis = true,
        Files = GetFiles(netFxFullArtifactPath + "/**/*")
                                .Where(file => file.FullPath.IndexOf("/runtimes/", StringComparison.OrdinalIgnoreCase) < 0)
                                .Select(file=>file.FullPath.Substring(netFxFullArtifactPathLength))
                                .Select(file=> new NuSpecContent { Source = file, Target = file })
                                .ToArray()
    });

    // Cake Symbols - .NET Core
    NuGetPack("./nuspec/Cake.CoreCLR.symbols.nuspec", new NuGetPackSettings {
        Version = parameters.Version.SemVersion,
        ReleaseNotes = parameters.ReleaseNotes.Notes.ToArray(),
        BasePath = parameters.Paths.Directories.ArtifactsBinNetCore,
        OutputDirectory = parameters.Paths.Directories.NugetRoot,
        Symbols = true,
        NoPackageAnalysis = true
    });

    var netCoreFullArtifactPath = MakeAbsolute(parameters.Paths.Directories.ArtifactsBinNetCore).FullPath;
    var netCoreFullArtifactPathLength = netCoreFullArtifactPath.Length+1;

    // Cake - .NET Core
    NuGetPack("./nuspec/Cake.CoreCLR.nuspec", new NuGetPackSettings {
        Version = parameters.Version.SemVersion,
        ReleaseNotes = parameters.ReleaseNotes.Notes.ToArray(),
        BasePath = netCoreFullArtifactPath,
        OutputDirectory = parameters.Paths.Directories.NugetRoot,
        Symbols = false,
        NoPackageAnalysis = true,
        Files = GetFiles(netCoreFullArtifactPath + "/**/*")
                                .Select(file=>file.FullPath.Substring(netCoreFullArtifactPathLength))
                                .Select(file=> new NuSpecContent { Source = file, Target = file })
                                .ToArray()
    });
});

Task("Sign-Binaries")
    .IsDependentOn("Zip-Files")
    .IsDependentOn("Create-Chocolatey-Packages")
    .IsDependentOn("Create-NuGet-Packages")
    .WithCriteria(() => 
        (parameters.ShouldPublish && !parameters.SkipSigning) ||
        StringComparer.OrdinalIgnoreCase.Equals(EnvironmentVariable("SIGNING_TEST"), "True"))
    .Does(() =>
{
    // Get the secret.
    var secret = EnvironmentVariable("SIGNING_SECRET");
    if(string.IsNullOrWhiteSpace(secret)) {
        throw new InvalidOperationException("Could not resolve signing secret.");
    }
    // Get the user.
    var user = EnvironmentVariable("SIGNING_USER");
    if(string.IsNullOrWhiteSpace(user)) {
        throw new InvalidOperationException("Could not resolve signing user.");
    }

    var settings = File("./signclient.json");
    var filter = File("./signclient.filter");

    // Get the files to sign.
    var files = GetFiles(string.Concat(parameters.Paths.Directories.NugetRoot, "/", "*.nupkg"))
        + parameters.Paths.Files.ZipArtifactPathDesktop
        + parameters.Paths.Files.ZipArtifactPathCoreClr;

    foreach(var file in files)
    {
        Information("Signing {0}...", file.FullPath);

        // Build the argument list.
        var arguments = new ProcessArgumentBuilder()
            .AppendQuoted(signClientPath.FullPath)
            .Append("sign")
            .AppendSwitchQuoted("-c", MakeAbsolute(settings.Path).FullPath)
            .AppendSwitchQuoted("-i", MakeAbsolute(file).FullPath)
            .AppendSwitchQuoted("-f", MakeAbsolute(filter).FullPath)
            .AppendSwitchQuotedSecret("-s", secret)
            .AppendSwitchQuotedSecret("-r", user)
            .AppendSwitchQuoted("-n", "Cake")
            .AppendSwitchQuoted("-d", "Cake (C# Make) is a cross platform build automation system.")
            .AppendSwitchQuoted("-u", "https://cakebuild.net");

        // Sign the binary.
        var result = StartProcess("dotnet", new ProcessSettings {  Arguments = arguments });
        if(result != 0)
        {
            // We should not recover from this.
            throw new InvalidOperationException("Signing failed!");
        }
    }
});

Task("Upload-AppVeyor-Artifacts")
    .IsDependentOn("Sign-Binaries")
    .IsDependentOn("Create-Chocolatey-Packages")
    .WithCriteria(() => parameters.IsRunningOnAppVeyor)
    .Does(() =>
{
    AppVeyor.UploadArtifact(parameters.Paths.Files.ZipArtifactPathDesktop);
    AppVeyor.UploadArtifact(parameters.Paths.Files.ZipArtifactPathCoreClr);
    foreach(var package in GetFiles(parameters.Paths.Directories.NugetRoot + "/*"))
    {
        AppVeyor.UploadArtifact(package);
    }
});

Task("Upload-Coverage-Report")
    .WithCriteria(() => FileExists(parameters.Paths.Files.TestCoverageOutputFilePath))
    .WithCriteria(() => !parameters.IsLocalBuild)
    .WithCriteria(() => !parameters.IsPullRequest)
    .WithCriteria(() => parameters.IsMainCakeRepo)
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    CoverallsIo(parameters.Paths.Files.TestCoverageOutputFilePath, new CoverallsIoSettings()
    {
        RepoToken = parameters.Coveralls.RepoToken
    });
});

Task("Publish-MyGet")
    .IsDependentOn("Sign-Binaries")
    .IsDependentOn("Package")
    .WithCriteria(() => parameters.ShouldPublishToMyGet)
    .Does(() =>
{
    // Resolve the API key.
    var apiKey = EnvironmentVariable("MYGET_API_KEY");
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve MyGet API key.");
    }

    // Resolve the API url.
    var apiUrl = EnvironmentVariable("MYGET_API_URL");
    if(string.IsNullOrEmpty(apiUrl)) {
        throw new InvalidOperationException("Could not resolve MyGet API url.");
    }

    foreach(var package in parameters.Packages.All)
    {
        // Push the package.
        NuGetPush(package.PackagePath, new NuGetPushSettings {
            Source = apiUrl,
            ApiKey = apiKey
        });
    }
})
.OnError(exception =>
{
    Information("Publish-MyGet Task failed, but continuing with next Task...");
    publishingError = true;
});

Task("Publish-NuGet")
    .IsDependentOn("Sign-Binaries")
    .IsDependentOn("Create-NuGet-Packages")
    .WithCriteria(() => parameters.ShouldPublish)
    .Does(() =>
{
    // Resolve the API key.
    var apiKey = EnvironmentVariable("NUGET_API_KEY");
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve NuGet API key.");
    }

    // Resolve the API url.
    var apiUrl = EnvironmentVariable("NUGET_API_URL");
    if(string.IsNullOrEmpty(apiUrl)) {
        throw new InvalidOperationException("Could not resolve NuGet API url.");
    }

    foreach(var package in parameters.Packages.Nuget)
    {
        // Push the package.
        NuGetPush(package.PackagePath, new NuGetPushSettings {
          ApiKey = apiKey,
          Source = apiUrl
        });
    }
})
.OnError(exception =>
{
    Information("Publish-NuGet Task failed, but continuing with next Task...");
    publishingError = true;
});

Task("Publish-Chocolatey")
    .IsDependentOn("Sign-Binaries")
    .IsDependentOn("Create-Chocolatey-Packages")
    .WithCriteria(() => parameters.ShouldPublish)
    .Does(() =>
{
    // Resolve the API key.
    var apiKey = EnvironmentVariable("CHOCOLATEY_API_KEY");
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve Chocolatey API key.");
    }

    // Resolve the API url.
    var apiUrl = EnvironmentVariable("CHOCOLATEY_API_URL");
    if(string.IsNullOrEmpty(apiUrl)) {
        throw new InvalidOperationException("Could not resolve Chocolatey API url.");
    }

    foreach(var package in parameters.Packages.Chocolatey)
    {
        // Push the package.
        ChocolateyPush(package.PackagePath, new ChocolateyPushSettings {
          ApiKey = apiKey,
          Source = apiUrl
        });
    }
})
.OnError(exception =>
{
    Information("Publish-Chocolatey Task failed, but continuing with next Task...");
    publishingError = true;
});

Task("Publish-HomeBrew")
    .IsDependentOn("Sign-Binaries")
    .IsDependentOn("Zip-Files")
    .WithCriteria(() => parameters.ShouldPublish)
	.Does(() =>
{
    var hash = CalculateFileHash(parameters.Paths.Files.ZipArtifactPathDesktop).ToHex();
    Information("Hash for creating HomeBrew PullRequest: {0}", hash);
})
.OnError(exception =>
{
    Information("Publish-HomeBrew Task failed, but continuing with next Task...");
    publishingError = true;
});

Task("Publish-GitHub-Release")
    .WithCriteria(() => parameters.ShouldPublish)
    .Does(() =>
{
    GitReleaseManagerAddAssets(parameters.GitHub.UserName, parameters.GitHub.Password, "cake-build", "cake", parameters.Version.Milestone, parameters.Paths.Files.ZipArtifactPathDesktop.ToString());
    GitReleaseManagerAddAssets(parameters.GitHub.UserName, parameters.GitHub.Password, "cake-build", "cake", parameters.Version.Milestone, parameters.Paths.Files.ZipArtifactPathCoreClr.ToString());
    GitReleaseManagerClose(parameters.GitHub.UserName, parameters.GitHub.Password, "cake-build", "cake", parameters.Version.Milestone);
})
.OnError(exception =>
{
    Information("Publish-GitHub-Release Task failed, but continuing with next Task...");
    publishingError = true;
});

Task("Create-Release-Notes")
    .Does(() =>
{
    GitReleaseManagerCreate(parameters.GitHub.UserName, parameters.GitHub.Password, "cake-build", "cake", new GitReleaseManagerCreateSettings {
        Milestone         = parameters.Version.Milestone,
        Name              = parameters.Version.Milestone,
        Prerelease        = true,
        TargetCommitish   = "main"
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
  .IsDependentOn("Zip-Files")
  .IsDependentOn("Create-NuGet-Packages");

Task("Default")
//  .IsDependentOn("Package");
	.IsDependentOn("Build")
    .IsDependentOn("Restore-NuGet-Packages");

Task("AppVeyor")
  .IsDependentOn("Upload-AppVeyor-Artifacts")
  .IsDependentOn("Upload-Coverage-Report")
  .IsDependentOn("Publish-MyGet")
  .IsDependentOn("Publish-NuGet")
  .IsDependentOn("Publish-Chocolatey")
  .IsDependentOn("Publish-HomeBrew")
  .IsDependentOn("Publish-GitHub-Release")
  .Finally(() =>
{
    if(publishingError)
    {
        throw new Exception("An error occurred during the publishing of Cake.  All publishing tasks have been attempted.");
    }
});

Task("Travis")
  .IsDependentOn("Run-Unit-Tests");

Task("ReleaseNotes")
  .IsDependentOn("Create-Release-Notes");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(parameters.Target);