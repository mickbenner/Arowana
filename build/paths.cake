public class BuildPaths
{
    public FilePath ZipArtifactPathCoreClr { get; private set; }
    public FilePath ZipArtifactPathDesktop { get; private set; }
    public FilePath TestCoverageOutputFilePath { get; private set; }
	public FilePath SolutionFilePath { get; private set; }

    public DirectoryPath Artifacts { get; private set; }
    public DirectoryPath TestResults { get; private set; }
    public DirectoryPath NugetRoot { get; private set; }
    public DirectoryPath ArtifactsBin { get; private set; }
    public DirectoryPath ArtifactsBinFullFx { get; private set; }
    public DirectoryPath ArtifactsBinNetCore { get; private set; }
	public DirectoryPath Source { get; private set; }
    public ICollection<DirectoryPath> ToClean { get; private set; }

    public static BuildPaths GetBuildPaths(ICakeContext context, string configuration, string version)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (string.IsNullOrWhiteSpace(configuration)) throw new ArgumentException("The configuration was not set.", nameof(configuration));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("The version was not set.", nameof(version));

		var srcDir = (DirectoryPath)context.Directory("./src");
        var artifactsDir = (DirectoryPath)(context.Directory("./artifacts") + context.Directory("v" + version));
        var artifactsBinDir = artifactsDir.Combine("bin");
        var artifactsBinFullFx = artifactsBinDir.Combine("net461");
        var artifactsBinNetCore = artifactsBinDir.Combine("netcoreapp2.0");
        var testResultsDir = artifactsDir.Combine("test-results");
        var nugetRoot = artifactsDir.Combine("nuget");

        var zipArtifactPathCoreClr = artifactsDir.CombineWithFilePath("Acuo.Store-bin-coreclr-v" + version + ".zip");
        var zipArtifactPathDesktop = artifactsDir.CombineWithFilePath("Acuo.Store-bin-net461-v" + version + ".zip");
        var testCoverageOutputFilePath = testResultsDir.CombineWithFilePath("OpenCover.xml");
		var solutionFilePath = srcDir.CombineWithFilePath("Acuo.Store.sln");

        return new BuildPaths
            {
                Artifacts = artifactsBinDir,
            }
    }
}
