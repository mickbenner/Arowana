// #load "./paths.cake"
// #load "./packages.cake"
// #load "./version.cake"
// #load "./credentials.cake"

public class BuildParameters
{
    public string Target { get; private set; }
    public string Configuration { get; private set; }
	public string ProductName { get; private set; }

    public bool IsLocalBuild { get; private set; }
    public bool IsRunningOnWindows { get; private set; }
    public bool IsRunningOnUnix { get; private set; }
    public bool IsRunningOnAppVeyor { get; private set; }
    public bool IsPullRequest { get; private set; }
    public bool IsMainCakeRepo { get; private set; }
    public bool IsMainBranch { get; private set; }
    public bool IsDevelopCakeBranch { get; private set; }
    public bool IsTagged { get; private set; }
    public bool IsPublishBuild { get; private set; }
    public bool IsReleaseBuild { get; private set; }

    public bool SkipGitVersion { get; private set; }
    public bool SkipOpenCover { get; private set; }
    public bool SkipSigning { get; private set; }
	public bool Verbose { get; private set; }

    public ReleaseNotes ReleaseNotes { get; private set; }

	public ICakeContext Context { get; private set; }

    /// <summary>Gets a value indicating whether or not the build should be published.</summary>
    public bool ShouldPublish
    {
        get
        {
            return !IsLocalBuild && !IsPullRequest && IsMainCakeRepo && IsMainBranch && IsTagged;
        }
    }

    /// <summary>Gets a value indicating whether or not the build should be published to MyGet.</summary>
    public bool ShouldPublishToMyGet
    {
        get
        {
            return !IsLocalBuild && !IsPullRequest && IsMainCakeRepo && (IsTagged || IsDevelopCakeBranch);
        }
    }


    public void Initialize(ICakeContext context)
    {
        Version = BuildVersion.Calculate(context, this);
        Paths = BuildPaths.GetPaths(context, Configuration, Version.SemVersion);
        Packages = BuildPackages.GetPackages(
            Paths.Directories.NugetRoot,
            Version.SemVersion,
            new [] { "Cake", "Cake.Core", "Cake.Common", "Cake.Testing", "Cake.CoreCLR", "Cake.NuGet" },
            new [] { "cake.portable" });
		Context = context;
    }

	public void DumpToLog()
	{
		Context.Information("Building version {0} of {1} ({2}, {3}) using version {4} of Cake.",
			Version.SemVersion,
			ProductName,
			Configuration,
			Target,
			Version.CakeVersion);

		Context.Information("");

		var diagnostic = new StringBuilder();
		diagnostic.AppendFormat($"{ProductName,20} - Parameters");
		diagnostic.AppendLine();
		diagnostic.Append(new string('-', 40));
		diagnostic.AppendLine();
		diagnostic.Append(Version.ToString());
		diagnostic.AppendLine();

		if (Verbose) 
		{
			diagnostic.AppendFormat("{0,20} : {1}", "Product Name", ProductName);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "Target", Target);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "Configuration", Configuration);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsLocalBuild", IsLocalBuild);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsRunningOnUnix", IsRunningOnUnix);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsRunningOnWindows", IsRunningOnWindows);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsRunningOnAppVeyor", IsRunningOnAppVeyor);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsPullRequest", IsPullRequest);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsMainCakeRepo", IsMainCakeRepo);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsMainBranch", IsMainBranch);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsDevelopCakeBranch", IsDevelopCakeBranch);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsTagged", IsTagged);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "Coveralls", Coveralls);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsPublishBuild", IsPublishBuild);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "IsReleaseBuild", IsReleaseBuild);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "SkipSigning", SkipSigning);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "SkipGitVersion", SkipGitVersion);
			diagnostic.AppendLine();
			diagnostic.AppendFormat("{0,20} : {1}", "SkipOpenCover", SkipOpenCover);
			diagnostic.AppendLine();
		}

		diagnostic.Append(new string('-', 40));
		diagnostic.AppendLine();
		Context.Information(diagnostic.ToString());
	}

    public static BuildParameters GetParameters(ICakeContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        var target = context.Argument("target", "Default");
        var buildSystem = context.BuildSystem();

		context.Information($"Repository Name: {buildSystem.AppVeyor.Environment.Repository.Name}");
		context.Information($"Repository Name: {buildSystem.AppVeyor.Environment.Repository.Branch}");
		context.Information($"Repository Name: {buildSystem.AppVeyor.Environment.Repository}");

		var isMainBranch = StringComparer.OrdinalIgnoreCase.Equals("main", buildSystem.AppVeyor.Environment.Repository.Branch);
        return new BuildParameters {
			ProductName = "Arowana",
            Target = target,
            Configuration = context.Argument("configuration", "Release"),
            IsLocalBuild = buildSystem.IsLocalBuild,
            IsRunningOnUnix = context.IsRunningOnUnix(),
            IsRunningOnWindows = context.IsRunningOnWindows(),
            IsRunningOnAppVeyor = buildSystem.AppVeyor.IsRunningOnAppVeyor,
            IsPullRequest = buildSystem.AppVeyor.Environment.PullRequest.IsPullRequest,
            IsMainCakeRepo = StringComparer.OrdinalIgnoreCase.Equals("cake-build/cake", buildSystem.AppVeyor.Environment.Repository.Name),
            IsMainBranch = isMainBranch,
            IsDevelopCakeBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", buildSystem.AppVeyor.Environment.Repository.Branch),
            IsTagged = IsBuildTagged(buildSystem),
            ReleaseNotes = context.ParseReleaseNotes("./ReleaseNotes.md"),
            IsPublishBuild = IsPublishing(target),
            IsReleaseBuild = IsReleasing(target),
            SkipSigning = StringComparer.OrdinalIgnoreCase.Equals("True", context.Argument("skipsigning", "False")),
            SkipGitVersion = StringComparer.OrdinalIgnoreCase.Equals("True", context.EnvironmentVariable("CAKE_SKIP_GITVERSION")),
            SkipOpenCover = StringComparer.OrdinalIgnoreCase.Equals("True", context.EnvironmentVariable("CAKE_SKIP_OPENCOVER")),
			Verbose = isMainBranch || context.Log.Verbosity == Verbosity.Diagnostic
        };
    }

    private static bool IsBuildTagged(BuildSystem buildSystem)
    {
        return buildSystem.AppVeyor.Environment.Repository.Tag.IsTag
            && !string.IsNullOrWhiteSpace(buildSystem.AppVeyor.Environment.Repository.Tag.Name);
    }

    private static bool IsReleasing(string target)
    {
        var targets = new [] { "Publish", "Publish-NuGet", "Publish-Chocolatey", "Publish-HomeBrew", "Publish-GitHub-Release" };
        return targets.Any(t => StringComparer.OrdinalIgnoreCase.Equals(t, target));
    }

    private static bool IsPublishing(string target)
    {
        var targets = new [] { "ReleaseNotes", "Create-Release-Notes" };
        return targets.Any(t => StringComparer.OrdinalIgnoreCase.Equals(t, target));
    }
}