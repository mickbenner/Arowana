public class BuildPaths
{
    public BuildFiles Files { get; private set; }
    public BuildDirectories Directories { get; private set; }

    public static BuildPaths GetPaths(
        ICakeContext context,
        string configuration,
        string semVersion
        )
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        if (string.IsNullOrEmpty(configuration))
        {
            throw new ArgumentNullException("configuration");
        }

        if (string.IsNullOrEmpty(semVersion))
        {
            throw new ArgumentNullException("semVersion");
        }

		var srcDir = (DirectoryPath)context.Directory("./src");
        var artifactsDir = (DirectoryPath)(context.Directory("./artifacts") + context.Directory("v" + semVersion));
        var artifactsBinDir = artifactsDir.Combine("bin");
        var artifactsBinFullFx = artifactsBinDir.Combine("net461");
        var artifactsBinNetCore = artifactsBinDir.Combine("netcoreapp2.0");
        var testResultsDir = artifactsDir.Combine("test-results");
        var nugetRoot = artifactsDir.Combine("nuget");

        var zipArtifactPathCoreClr = artifactsDir.CombineWithFilePath("Acuo.Store-bin-coreclr-v" + semVersion + ".zip");
        var zipArtifactPathDesktop = artifactsDir.CombineWithFilePath("Acuo.Store-bin-net461-v" + semVersion + ".zip");
        var testCoverageOutputFilePath = testResultsDir.CombineWithFilePath("OpenCover.xml");
		var solutionFilePath = srcDir.CombineWithFilePath("Acuo.Store.sln");

        // Directories
        var buildDirectories = new BuildDirectories(
		    srcDir,
            artifactsDir,
            testResultsDir,
            nugetRoot,
            artifactsBinDir,
            artifactsBinFullFx,
            artifactsBinNetCore);

        // Files
        var buildFiles = new BuildFiles(
            context,
            zipArtifactPathCoreClr,
            zipArtifactPathDesktop,
            testCoverageOutputFilePath,
			solutionFilePath);

        return new BuildPaths
        {
            Files = buildFiles,
            Directories = buildDirectories
        };
    }
}

public class BuildFiles
{
    public FilePath ZipArtifactPathCoreClr { get; private set; }
    public FilePath ZipArtifactPathDesktop { get; private set; }
    public FilePath TestCoverageOutputFilePath { get; private set; }
	public FilePath SolutionFilePath { get; private set; }

    public BuildFiles(
        ICakeContext context,
        FilePath zipArtifactPathCoreClr,
        FilePath zipArtifactPathDesktop,
        FilePath testCoverageOutputFilePath,
		FilePath solutionFilePath
        )
    {
        ZipArtifactPathCoreClr = zipArtifactPathCoreClr;
        ZipArtifactPathDesktop = zipArtifactPathDesktop;
        TestCoverageOutputFilePath = testCoverageOutputFilePath;
		SolutionFilePath = solutionFilePath;
    }
}

public class BuildDirectories
{
    public DirectoryPath Artifacts { get; private set; }
    public DirectoryPath TestResults { get; private set; }
    public DirectoryPath NugetRoot { get; private set; }
    public DirectoryPath ArtifactsBin { get; private set; }
    public DirectoryPath ArtifactsBinFullFx { get; private set; }
    public DirectoryPath ArtifactsBinNetCore { get; private set; }
	public DirectoryPath Source { get; private set; }
    public ICollection<DirectoryPath> ToClean { get; private set; }

    public BuildDirectories(
	    DirectoryPath srcDir,
        DirectoryPath artifactsDir,
        DirectoryPath testResultsDir,
        DirectoryPath nugetRoot,
        DirectoryPath artifactsBinDir,
        DirectoryPath artifactsBinFullFx,
        DirectoryPath artifactsBinNetCore
        )
    {
		Source = srcDir;
        Artifacts = artifactsDir;
        TestResults = testResultsDir;
        NugetRoot = nugetRoot;
        ArtifactsBin = artifactsBinDir;
        ArtifactsBinFullFx = artifactsBinFullFx;
        ArtifactsBinNetCore = artifactsBinNetCore;
        ToClean = new[] {
            Artifacts,
            TestResults,
            NugetRoot,
            ArtifactsBin,
            ArtifactsBinFullFx,
            ArtifactsBinNetCore
        };
    }
}