public class BuildVersion
{
    public string Version { get; private set; }
    public string SemVersion { get; private set; }
    public string DotNetAsterix { get; private set; }
    public string Milestone { get; private set; }
    public string CakeVersion { get; private set; }
	private BuildParameters Parameters { get; set; }

	public override string ToString()
	{
		var version = new StringBuilder();
		version.AppendFormat("{0,20} : {1}", "Version", Version);
		version.AppendLine();
		version.AppendFormat("{0,20} : {1}", "Semantic Version", SemVersion);
		version.AppendLine();
		if (Parameters.Verbose)
		{
			version.AppendFormat("{0,20} : {1}", ".NET Asterix", DotNetAsterix);
			version.AppendLine();
			version.AppendFormat("{0,20} : {1}", "Milestone", Milestone);
			version.AppendLine();
			version.AppendFormat("{0,20} : {1}", "Cake Version", CakeVersion);
		}

		return version.ToString();
	}

    public static BuildVersion Calculate(ICakeContext context, BuildParameters parameters)
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        string version = null;
        string semVersion = null;
        string milestone = null;

        if (context.IsRunningOnWindows() && !parameters.SkipGitVersion)
        {
            if (parameters.IsLocalBuild || parameters.IsPublishBuild || parameters.IsReleaseBuild)
            {
                var gitVersion = context.GitVersion(new GitVersionSettings{
													UpdateAssemblyInfoFilePath = "./src/SolutionInfo.cs",
													UpdateAssemblyInfo = true,
													OutputType = GitVersionOutput.BuildServer
                });

                version = context.EnvironmentVariable("GitVersion_MajorMinorPatch");
                semVersion = context.EnvironmentVariable("GitVersion_LegacySemVerPadded");
                milestone = string.Concat("v", version);
            }

            GitVersion assertedVersions = context.GitVersion(new GitVersionSettings
            {
                OutputType = GitVersionOutput.Json,
            });

            version = assertedVersions.MajorMinorPatch;
            semVersion = assertedVersions.LegacySemVerPadded;
            milestone = string.Concat("v", version);
        }

        if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(semVersion))
        {
            context.Information("Fetching version from first SolutionInfo...");
            version = ReadSolutionInfoVersion(context);
            semVersion = version;
            milestone = string.Concat("v", version);
        }

        var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();

        return new BuildVersion
        {
			Parameters = parameters,
            Version = version,
            SemVersion = semVersion,
            DotNetAsterix = semVersion.Substring(version.Length).TrimStart('-'),
            Milestone = milestone,
            CakeVersion = cakeVersion
        };
    }

    private static string ReadSolutionInfoVersion(ICakeContext context)
    {
        var solutionInfo = context.ParseAssemblyInfo("./src/SolutionInfo.cs");
        if (!string.IsNullOrEmpty(solutionInfo.AssemblyVersion))
        {
            return solutionInfo.AssemblyVersion;
        }

        throw new CakeException("Could not parse version.");
    }
}
