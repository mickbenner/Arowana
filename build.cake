#addin nuget:?package=Cake.Figlet&version=1.3.0
#tool nuget:?package=GitVersion.CommandLine&version=5.0.0-beta3-4

#load "./build/configuration.cake"
#load "./build/version.cake"


BuildConfiguration _configuration;
BuildVersion _version;

Setup(context =>
    {
        Information("Setting up...");
        _configuration = BuildConfiguration.Initialize(context);

        Information(Figlet($"{_configuration.ProductName} -  {_configuration.CompanyName}"));
        _version = BuildVersion.Calculate(context, _configuration);

        Information(Figlet($"{_version.Version}"));
    });

Teardown(context => 
    {
        Information("Tearing down...");
    });

Task("Clean")
    .Does(() =>
        {
            //CleanDirectories(_parameters.Paths.Directories.ToClean);
        });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
        {
            /*
            Information(_parameters.Paths.Files.SolutionFilePath.FullPath);
            DotNetCoreRestore(_parameters.Paths.Files.SolutionFilePath.FullPath, new DotNetCoreRestoreSettings
            {
                Verbosity = DotNetCoreVerbosity.Normal,
                    Sources = new [] 
                    {
                        "https://www.myget.org/F/xunit/api/v3/index.json",
                        "https://dotnet.myget.org/F/dotnet-core/api/v3/index.json",
                        "https://dotnet.myget.org/F/cli-deps/api/v3/index.json",
                        "https://api.nuget.org/v3/index.json",
                    },
                MSBuildSettings = _buildSettings
            });
            */
        });

Task("Package")
    .IsDependentOn("Clean");

Task("Build")
    .IsDependentOn("Clean");

Task("default")
    .IsDependentOn("Package")
    .IsDependentOn("Build")
    .IsDependentOn("Restore-NuGet-Packages");

RunTarget("default");



// #tool nuget:?package=Cake.GitHubUtility&version=0.4.0
// #tool nuget:?package=GitReleaseNotes&version=0.7.1
// #tool nuget:?package=GitReleaseManager&version=0.8.0
// #tool nuget:?package=GitVersion&version=4.0.0-beta0010
// #tool nuget:?package=coveralls.io&version=1.4.2
// #tool nuget:?package=OpenCover&version=4.7.922
// #tool nuget:?package=ReportGenerator&version=4.1.8
// #addin nuget:?package=Cake.Figlet&version=1.3.0
// #addin nuget:?package=Cake.Coveralls&version=0.10.0
// #addin nuget:?package=Cake.Twitter&version=0.10.0
// #addin nuget:?package=Cake.Gitter&version=0.11.0
// // Load other scripts.

// #load "./build/parameters.cake"
// var _parameters = BuildParameters.GetParameters(Context);
// var _publishingError = false;
// DotNetCoreMSBuildSettings _buildSettings = null;
// Information(Figlet(_parameters.ProductName));
// Setup(context =>
// {
//     _parameters.Initialize(context);
//     if(_parameters.IsMainBranch && (context.Log.Verbosity != Verbosity.Diagnostic))
//     {
//         context.Log.Verbosity = Verbosity.Diagnostic;
//     }
//     _parameters.DumpToLog();
//     var releaseNotes = string.Join("\n", _parameters.ReleaseNotes.Notes.ToArray()).Replace("\"", "\"\"");
//     _buildSettings = new DotNetCoreMSBuildSettings()
//                             .WithProperty("Version", _parameters.Version.SemVersion)
//                             .WithProperty("AssemblyVersion", _parameters.Version.Version)
//                             .WithProperty("FileVersion", _parameters.Version.Version)
//                             .WithProperty("PackageReleaseNotes", string.Concat("\"", releaseNotes, "\""))
//                             .WithProperty("WorkingDirectory", context.Directory("./src"));
//     if(!_parameters.IsRunningOnWindows)
//     {
//         var frameworkPathOverride = new FilePath(typeof(object).Assembly.Location).GetDirectory().FullPath + "/";
//         // Use FrameworkPathOverride when not running on Windows.
//         Information("Build will use FrameworkPathOverride={0} since not building on Windows.", frameworkPathOverride);
//         _buildSettings.WithProperty("FrameworkPathOverride", frameworkPathOverride);
//     }
// });
// Teardown(context =>
// {
//     Information("Starting Teardown...");
//     if(context.Successful)
//     {
//         if(_parameters.ShouldPublish)
//         {
//             if(_parameters.CanPostToTwitter)
//             {
//                 var message = "Version " + _parameters.Version.SemVersion + " of Cake has just been released, https://www.nuget.org/packages/Cake.";
//                 TwitterSendTweet(_parameters.Twitter.ConsumerKey, _parameters.Twitter.ConsumerSecret, _parameters.Twitter.AccessToken, _parameters.Twitter.AccessTokenSecret, message);
//             }
//             if(_parameters.CanPostToGitter)
//             {
//                 var message = "@/all Version " + _parameters.Version.SemVersion + " of the Cake has just been released, https://www.nuget.org/packages/Cake.";
//                 var postMessageResult = Gitter.Chat.PostMessage(
//                     message: message,
//                     messageSettings: new GitterChatMessageSettings { Token = _parameters.Gitter.Token, RoomId = _parameters.Gitter.RoomId}
//                     );
//                 if (postMessageResult.Ok)
//                 {
//                     Information("Message {0} succcessfully sent", postMessageResult.TimeStamp);
//                 }
//                 else
//                 {
//                     Error("Failed to send message: {0}", postMessageResult.Error);
//                 }
//             }
//         }
//     }
//     Information("Finished running tasks.");
// });
// Task("UpdateVersioning")
// 	.Does(() =>
// {
// });
// Task("GetVersionInfo")
//     .Does(() =>
// {
//     var result = GitVersion(new GitVersionSettings {
//         Branch = "develop",
// 		OutputType = GitVersionOutput.Json
//     });
// });
// Task("IncrementVersioning")
// 	.Does(() =>
// {
// });
// Task("Build")
//     .IsDependentOn("Restore-NuGet-Packages")
// 	.IsDependentOn("UpdateVersioning")
//     .Does(() =>
// {
//     // Build the solution.
//     DotNetCoreBuild(_parameters.Paths.Files.SolutionFilePath.FullPath, new DotNetCoreBuildSettings()
//     {
//         Configuration = _parameters.Configuration,
//         MSBuildSettings = _buildSettings
//     });
// });
// Task("Run-Unit-Tests")
//     .IsDependentOn("Build")
//     .Does(() =>
// {
//     var projects = GetFiles("./src/**/*.Tests.csproj");
//     foreach(var project in projects)
//     {
//         DotNetCoreTool(project,
//             "xunit",  "--no-build -noshadow -configuration " + _parameters.Configuration);
//     }
// });
// Task("Copy-Files")
//     .IsDependentOn("Run-Unit-Tests")
//     .Does(() =>
// {
//     // .NET 4.6
//     DotNetCorePublish("./src/Cake", new DotNetCorePublishSettings
//     {
//         Framework = "net461",
//         VersionSuffix = _parameters.Version.DotNetAsterix,
//         Configuration = _parameters.Configuration,
//         OutputDirectory = _parameters.Paths.Directories.ArtifactsBinFullFx,
//         MSBuildSettings = _buildSettings
//     });
//     // .NET Core
//     DotNetCorePublish("./src/Cake", new DotNetCorePublishSettings
//     {
//         Framework = "netcoreapp1.0",
//         Configuration = _parameters.Configuration,
//         OutputDirectory = _parameters.Paths.Directories.ArtifactsBinNetCore,
//         MSBuildSettings = _buildSettings
//     });
//     // Copy license
//     CopyFileToDirectory("./LICENSE", _parameters.Paths.Directories.ArtifactsBinFullFx);
//     CopyFileToDirectory("./LICENSE", _parameters.Paths.Directories.ArtifactsBinNetCore);
//     // Copy Cake.XML (since publish does not do this anymore)
//     CopyFileToDirectory("./src/Cake/bin/" + _parameters.Configuration + "/net461/Cake.xml", _parameters.Paths.Directories.ArtifactsBinFullFx);
//     CopyFileToDirectory("./src/Cake/bin/" + _parameters.Configuration + "/netcoreapp1.0/Cake.xml", _parameters.Paths.Directories.ArtifactsBinNetCore);
// });
// Task("Zip-Files")
//     .IsDependentOn("Copy-Files")
//     .Does(() =>
// {
//     // .NET 4.6
//     var homebrewFiles = GetFiles( _parameters.Paths.Directories.ArtifactsBinFullFx.FullPath + "/**/*");
//     Zip(_parameters.Paths.Directories.ArtifactsBinFullFx, _parameters.Paths.Files.ZipArtifactPathDesktop, homebrewFiles);
//     // .NET Core
//     var coreclrFiles = GetFiles( _parameters.Paths.Directories.ArtifactsBinNetCore.FullPath + "/**/*");
//     Zip(_parameters.Paths.Directories.ArtifactsBinNetCore, _parameters.Paths.Files.ZipArtifactPathCoreClr, coreclrFiles);
// });
// Task("Create-Chocolatey-Packages")
//     .IsDependentOn("Copy-Files")
//     .IsDependentOn("Package")
//     .WithCriteria(() => _parameters.IsRunningOnWindows)
//     .Does(() =>
// {
//     foreach(var package in _parameters.Packages.Chocolatey)
//     {
//         var netFxFullArtifactPath = MakeAbsolute(_parameters.Paths.Directories.ArtifactsBinFullFx).FullPath;
//         var curDirLength =  MakeAbsolute(Directory("./")).FullPath.Length + 1;
//         // Create package.
//         ChocolateyPack(package.NuspecPath, new ChocolateyPackSettings {
//             Version = _parameters.Version.SemVersion,
//             ReleaseNotes = _parameters.ReleaseNotes.Notes.ToArray(),
//             OutputDirectory = _parameters.Paths.Directories.NugetRoot,
//             Files = (GetFiles(netFxFullArtifactPath + "/**/*") + GetFiles("./nuspec/*.txt"))
//                                     .Where(file => file.FullPath.IndexOf("/runtimes/", StringComparison.OrdinalIgnoreCase) < 0)
//                                     .Select(file=>"../" + file.FullPath.Substring(curDirLength))
//                                     .Select(file=> new ChocolateyNuSpecContent { Source = file })
//                                     .ToArray()
//         });
//     }
// });
// Task("Create-NuGet-Packages")
//     .IsDependentOn("Copy-Files")
//     .Does(() =>
// {
//     // Build libraries
//     var projects = GetFiles("./src/**/*.csproj");
//     foreach(var project in projects)
//     {
//         var name = project.GetDirectory().FullPath;
//         if(name.EndsWith("Cake") || name.EndsWith("Tests") || name.EndsWith("Xunit"))
//         {
//             continue;
//         }
//         DotNetCorePack(project.FullPath, new DotNetCorePackSettings {
//             Configuration = _parameters.Configuration,
//             OutputDirectory = _parameters.Paths.Directories.NugetRoot,
//             NoBuild = true,
//             IncludeSymbols = true,
//             MSBuildSettings = _buildSettings
//         });
//     }
//     // Cake - Symbols - .NET 4.6
//     NuGetPack("./nuspec/Cake.symbols.nuspec", new NuGetPackSettings {
//         Version = _parameters.Version.SemVersion,
//         ReleaseNotes = _parameters.ReleaseNotes.Notes.ToArray(),
//         BasePath = _parameters.Paths.Directories.ArtifactsBinFullFx,
//         OutputDirectory = _parameters.Paths.Directories.NugetRoot,
//         Symbols = true,
//         NoPackageAnalysis = true
//     });
//     var netFxFullArtifactPath = MakeAbsolute(_parameters.Paths.Directories.ArtifactsBinFullFx).FullPath;
//     var netFxFullArtifactPathLength = netFxFullArtifactPath.Length+1;
//     // Cake - .NET 4.6
//     NuGetPack("./nuspec/Cake.nuspec", new NuGetPackSettings {
//         Version = _parameters.Version.SemVersion,
//         ReleaseNotes = _parameters.ReleaseNotes.Notes.ToArray(),
//         BasePath = netFxFullArtifactPath,
//         OutputDirectory = _parameters.Paths.Directories.NugetRoot,
//         Symbols = false,
//         NoPackageAnalysis = true,
//         Files = GetFiles(netFxFullArtifactPath + "/**/*")
//                                 .Where(file => file.FullPath.IndexOf("/runtimes/", StringComparison.OrdinalIgnoreCase) < 0)
//                                 .Select(file=>file.FullPath.Substring(netFxFullArtifactPathLength))
//                                 .Select(file=> new NuSpecContent { Source = file, Target = file })
//                                 .ToArray()
//     });
//     // Cake Symbols - .NET Core
//     NuGetPack("./nuspec/Cake.CoreCLR.symbols.nuspec", new NuGetPackSettings {
//         Version = _parameters.Version.SemVersion,
//         ReleaseNotes = _parameters.ReleaseNotes.Notes.ToArray(),
//         BasePath = _parameters.Paths.Directories.ArtifactsBinNetCore,
//         OutputDirectory = _parameters.Paths.Directories.NugetRoot,
//         Symbols = true,
//         NoPackageAnalysis = true
//     });
//     var netCoreFullArtifactPath = MakeAbsolute(_parameters.Paths.Directories.ArtifactsBinNetCore).FullPath;
//     var netCoreFullArtifactPathLength = netCoreFullArtifactPath.Length+1;
//     // Cake - .NET Core
//     NuGetPack("./nuspec/Cake.CoreCLR.nuspec", new NuGetPackSettings {
//         Version = _parameters.Version.SemVersion,
//         ReleaseNotes = _parameters.ReleaseNotes.Notes.ToArray(),
//         BasePath = netCoreFullArtifactPath,
//         OutputDirectory = _parameters.Paths.Directories.NugetRoot,
//         Symbols = false,
//         NoPackageAnalysis = true,
//         Files = GetFiles(netCoreFullArtifactPath + "/**/*")
//                                 .Select(file=>file.FullPath.Substring(netCoreFullArtifactPathLength))
//                                 .Select(file=> new NuSpecContent { Source = file, Target = file })
//                                 .ToArray()
//     });
// });
// // Task("Sign-Binaries")
// //     .IsDependentOn("Zip-Files")
// //     .IsDependentOn("Create-Chocolatey-Packages")
// //     .IsDependentOn("Create-NuGet-Packages")
// //     .WithCriteria(() => 
// //         (parameters.ShouldPublish && !parameters.SkipSigning) ||
// //         StringComparer.OrdinalIgnoreCase.Equals(EnvironmentVariable("SIGNING_TEST"), "True"))
// //     .Does(() =>
// // {
// //     // Get the secret.
// //     var secret = EnvironmentVariable("SIGNING_SECRET");
// //     if(string.IsNullOrWhiteSpace(secret)) {
// //         throw new InvalidOperationException("Could not resolve signing secret.");
// //     }
// //     // Get the user.
// //     var user = EnvironmentVariable("SIGNING_USER");
// //     if(string.IsNullOrWhiteSpace(user)) {
// //         throw new InvalidOperationException("Could not resolve signing user.");
// //     }
// //     var settings = File("./signclient.json");
// //     var filter = File("./signclient.filter");
// //     // Get the files to sign.
// //     var files = GetFiles(string.Concat(parameters.Paths.Directories.NugetRoot, "/", "*.nupkg"))
// //         + parameters.Paths.Files.ZipArtifactPathDesktop
// //         + parameters.Paths.Files.ZipArtifactPathCoreClr;
// //     foreach(var file in files)
// //     {
// //         Information("Signing {0}...", file.FullPath);
// //         // Build the argument list.
// //         var arguments = new ProcessArgumentBuilder()
// //             .AppendQuoted(signClientPath.FullPath)
// //             .Append("sign")
// //             .AppendSwitchQuoted("-c", MakeAbsolute(settings.Path).FullPath)
// //             .AppendSwitchQuoted("-i", MakeAbsolute(file).FullPath)
// //             .AppendSwitchQuoted("-f", MakeAbsolute(filter).FullPath)
// //             .AppendSwitchQuotedSecret("-s", secret)
// //             .AppendSwitchQuotedSecret("-r", user)
// //             .AppendSwitchQuoted("-n", "Cake")
// //             .AppendSwitchQuoted("-d", "Cake (C# Make) is a cross platform build automation system.")
// //             .AppendSwitchQuoted("-u", "https://cakebuild.net");
// //         // Sign the binary.
// //         var result = StartProcess("dotnet", new ProcessSettings {  Arguments = arguments });
// //         if(result != 0)
// //         {
// //             // We should not recover from this.
// //             throw new InvalidOperationException("Signing failed!");
// //         }
// //     }
// // });
// // Task("Upload-AppVeyor-Artifacts")
// //     .IsDependentOn("Sign-Binaries")
// //     .IsDependentOn("Create-Chocolatey-Packages")
// //     .WithCriteria(() => parameters.IsRunningOnAppVeyor)
// //     .Does(() =>
// // {
// //     AppVeyor.UploadArtifact(parameters.Paths.Files.ZipArtifactPathDesktop);
// //     AppVeyor.UploadArtifact(parameters.Paths.Files.ZipArtifactPathCoreClr);
// //     foreach(var package in GetFiles(parameters.Paths.Directories.NugetRoot + "/*"))
// //     {
// //         AppVeyor.UploadArtifact(package);
// //     }
// // });
// Task("Upload-Coverage-Report")
//     .WithCriteria(() => FileExists(_parameters.Paths.Files.TestCoverageOutputFilePath))
//     .WithCriteria(() => !_parameters.IsLocalBuild)
//     .WithCriteria(() => !_parameters.IsPullRequest)
//     .WithCriteria(() => _parameters.IsMainCakeRepo)
//     .IsDependentOn("Run-Unit-Tests")
//     .Does(() =>
// {
//     CoverallsIo(_parameters.Paths.Files.TestCoverageOutputFilePath, new CoverallsIoSettings()
//     {
//         RepoToken = _parameters.Coveralls.RepoToken
//     });
// });
// Task("Publish-MyGet")
// //    .IsDependentOn("Sign-Binaries")
//     .IsDependentOn("Package")
//     .WithCriteria(() => _parameters.ShouldPublishToMyGet)
//     .Does(() =>
// {
//     // Resolve the API key.
//     var apiKey = EnvironmentVariable("MYGET_API_KEY");
//     if(string.IsNullOrEmpty(apiKey)) {
//         throw new InvalidOperationException("Could not resolve MyGet API key.");
//     }
//     // Resolve the API url.
//     var apiUrl = EnvironmentVariable("MYGET_API_URL");
//     if(string.IsNullOrEmpty(apiUrl)) {
//         throw new InvalidOperationException("Could not resolve MyGet API url.");
//     }
//     foreach(var package in _parameters.Packages.All)
//     {
//         // Push the package.
//         NuGetPush(package.PackagePath, new NuGetPushSettings {
//             Source = apiUrl,
//             ApiKey = apiKey
//         });
//     }
// })
// .OnError(exception =>
// {
//     Information("Publish-MyGet Task failed, but continuing with next Task...");
//     _publishingError = true;
// });
// Task("Publish-NuGet")
//     // .IsDependentOn("Sign-Binaries")
//     .IsDependentOn("Create-NuGet-Packages")
//     .WithCriteria(() => _parameters.ShouldPublish)
//     .Does(() =>
// {
//     // Resolve the API key.
//     var apiKey = EnvironmentVariable("NUGET_API_KEY");
//     if(string.IsNullOrEmpty(apiKey)) {
//         throw new InvalidOperationException("Could not resolve NuGet API key.");
//     }
//     // Resolve the API url.
//     var apiUrl = EnvironmentVariable("NUGET_API_URL");
//     if(string.IsNullOrEmpty(apiUrl)) {
//         throw new InvalidOperationException("Could not resolve NuGet API url.");
//     }
//     foreach(var package in _parameters.Packages.Nuget)
//     {
//         // Push the package.
//         NuGetPush(package.PackagePath, new NuGetPushSettings {
//           ApiKey = apiKey,
//           Source = apiUrl
//         });
//     }
// })
// .OnError(exception =>
// {
//     Information("Publish-NuGet Task failed, but continuing with next Task...");
//     _publishingError = true;
// });
// Task("Publish-Chocolatey")
//     // .IsDependentOn("Sign-Binaries")
//     .IsDependentOn("Create-Chocolatey-Packages")
//     .WithCriteria(() => _parameters.ShouldPublish)
//     .Does(() =>
// {
//     // Resolve the API key.
//     var apiKey = EnvironmentVariable("CHOCOLATEY_API_KEY");
//     if(string.IsNullOrEmpty(apiKey)) {
//         throw new InvalidOperationException("Could not resolve Chocolatey API key.");
//     }
//     // Resolve the API url.
//     var apiUrl = EnvironmentVariable("CHOCOLATEY_API_URL");
//     if(string.IsNullOrEmpty(apiUrl)) {
//         throw new InvalidOperationException("Could not resolve Chocolatey API url.");
//     }
//     foreach(var package in _parameters.Packages.Chocolatey)
//     {
//         // Push the package.
//         ChocolateyPush(package.PackagePath, new ChocolateyPushSettings {
//           ApiKey = apiKey,
//           Source = apiUrl
//         });
//     }
// })
// .OnError(exception =>
// {
//     Information("Publish-Chocolatey Task failed, but continuing with next Task...");
//     _publishingError = true;
// });
// Task("Publish-HomeBrew")
//     // .IsDependentOn("Sign-Binaries")
//     .IsDependentOn("Zip-Files")
//     .WithCriteria(() => _parameters.ShouldPublish)
// 	.Does(() =>
// {
//     var hash = CalculateFileHash(_parameters.Paths.Files.ZipArtifactPathDesktop).ToHex();
//     Information("Hash for creating HomeBrew PullRequest: {0}", hash);
// })
// .OnError(exception =>
// {
//     Information("Publish-HomeBrew Task failed, but continuing with next Task...");
//     _publishingError = true;
// });
// Task("Publish-GitHub-Release")
//     .WithCriteria(() => _parameters.ShouldPublish)
//     .Does(() =>
// {
//     GitReleaseManagerAddAssets(_parameters.GitHub.UserName, _parameters.GitHub.Password, "cake-build", "cake", _parameters.Version.Milestone, _parameters.Paths.Files.ZipArtifactPathDesktop.ToString());
//     GitReleaseManagerAddAssets(_parameters.GitHub.UserName, _parameters.GitHub.Password, "cake-build", "cake", _parameters.Version.Milestone, _parameters.Paths.Files.ZipArtifactPathCoreClr.ToString());
//     GitReleaseManagerClose(_parameters.GitHub.UserName, _parameters.GitHub.Password, "cake-build", "cake", _parameters.Version.Milestone);
// })
// .OnError(exception =>
// {
//     Information("Publish-GitHub-Release Task failed, but continuing with next Task...");
//     _publishingError = true;
// });
// Task("Create-Release-Notes")
//     .Does(() =>
// {
//     GitReleaseManagerCreate(_parameters.GitHub.UserName, _parameters.GitHub.Password, "cake-build", "cake", new GitReleaseManagerCreateSettings {
//         Milestone         = _parameters.Version.Milestone,
//         Name              = _parameters.Version.Milestone,
//         Prerelease        = true,
//         TargetCommitish   = "main"
//     });
// });
// //////////////////////////////////////////////////////////////////////
// // TASK TARGETS
// //////////////////////////////////////////////////////////////////////
// Task("Package")
//   .IsDependentOn("Zip-Files")
//   .IsDependentOn("Create-NuGet-Packages");
// Task("Default")
// //  .IsDependentOn("Package");
// 	.IsDependentOn("Build")
//     .IsDependentOn("Restore-NuGet-Packages");
// Task("AppVeyor")
// //   .IsDependentOn("Upload-AppVeyor-Artifacts")
//   .IsDependentOn("Upload-Coverage-Report")
//   .IsDependentOn("Publish-MyGet")
//   .IsDependentOn("Publish-NuGet")
//   .IsDependentOn("Publish-Chocolatey")
//   .IsDependentOn("Publish-HomeBrew")
//   .IsDependentOn("Publish-GitHub-Release")
//   .Finally(() =>
// {
//     if(_publishingError)
//     {
//         throw new Exception("An error occurred during the publishing of Cake.  All publishing tasks have been attempted.");
//     }
// });
// Task("Travis")
//   .IsDependentOn("Run-Unit-Tests");
// Task("ReleaseNotes")
//   .IsDependentOn("Create-Release-Notes");
// //////////////////////////////////////////////////////////////////////
// // EXECUTION
// //////////////////////////////////////////////////////////////////////
// 
