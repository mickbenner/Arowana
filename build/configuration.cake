public class BuildConfiguration
{
    private ICakeContext _context;
    private BuildSystem _buildSystem;

    private BuildConfiguration(ICakeContext context)
    {
        _context = context;
        _buildSystem = context.BuildSystem();
        IsLocalBuild = _buildSystem.IsLocalBuild;
        IsRunningOnUnix = _context.IsRunningOnUnix();
        IsRunningOnWindows = _context.IsRunningOnWindows();
    }

    public static BuildConfiguration Initialize(ICakeContext context)
    {
        var configuration = new BuildConfiguration(context);
        configuration.Configuration = context.Argument("configuration", "debug");;
        configuration.TaskName = context.Argument("target", "default");
        configuration.FrameworkTarget = context.Argument("FrameworkTarget", "all");

        context.Information(logAction => logAction("Builder Configuration{0}---------------------{0}{1}", Environment.NewLine, configuration.ToString()));
        return configuration;
    }

    /// <summary>Gets the build configuration. It is one of the following: 
    ///     debug - Used for normal development work.
    ///     release - Used for the typical production deployment builds.
    ///     development - Used for out-of-band patch releases for emergency fixes.
    //  Note: The above values may change.  They are defined in the build.ps1 Power Shell script as the "Configuration" parameter.
    /// </summary>
    public string Configuration {get; private set;}

    /// <summary>Gets the build target. It is one of the following: 
    ///     netcoreapp2.2 - Builds only the .NET Core 2.2 version.
    ///     netstandard2.0 - Builds only the .NET Standard 2.0 version.
    ///     net48 - Builds only the .NET Framework 4.8 version.
    ///     all - Builds all targeted versions.
    //  Note: The above values may change.  They are defined in the build.ps1 Power Shell script as the "Target" parameter.
    /// </summary>
    public string FrameworkTarget {get; private set;}

    public string TaskName {get; private set;}

    public string ProductName {get;} = "Arowana";
    public string CompanyName {get;} = "Blue Pine Tree Soffware";

    public bool IsLocalBuild {get;}
    public bool IsPublishBuild {get;} = false;
    public bool IsReleaseBuild {get;} = true;
    public bool IsRunningOnUnix {get;}
    public bool IsRunningOnWindows {get;}

    public bool SkipGitVersion => !IsLocalBuild;

    public override string ToString()
    {
        var builder = new StringBuilder();
        const string indent = "";

        builder.AppendFormat("{0}Product Name: {1}{2}", indent, ProductName, Environment.NewLine);
        builder.AppendFormat("{0}Company Name: {1}{2}", indent, CompanyName, Environment.NewLine);
        builder.AppendFormat("{0}Task Name: {1}{2}", indent, TaskName, Environment.NewLine);
        builder.AppendFormat("{0}Framework Target: {1}{2}", indent, FrameworkTarget, Environment.NewLine);
        builder.AppendFormat("{0}Configuration: {1}{2}", indent, Configuration, Environment.NewLine);

        builder.AppendFormat("{0}Is Local Build: {1}{2}", indent, IsLocalBuild, Environment.NewLine);
        builder.AppendFormat("{0}Is Publish Build: {1}{2}", indent, IsPublishBuild, Environment.NewLine);
        builder.AppendFormat("{0}Is Release Build: {1}{2}", indent, IsReleaseBuild, Environment.NewLine);
        builder.AppendFormat("{0}Is Running On Unix: {1}{2}", indent, IsRunningOnUnix, Environment.NewLine);
        builder.AppendFormat("{0}Is Running On Windows: {1}{2}", indent, IsRunningOnWindows, Environment.NewLine);

        return builder.ToString();
    }
}
