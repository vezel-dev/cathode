#addin nuget:?package=Cake.DoInDirectory&version=6.0.0
#addin nuget:?package=Cake.Npm&version=4.0.0

#nullable enable

// Arguments

var target = Argument("t", "default");
var configuration = Argument("c", "Debug");

// Environment

var githubToken = EnvironmentVariable("GITHUB_TOKEN");
var nugetToken = EnvironmentVariable("NUGET_TOKEN");

// Paths

var root = Context.Environment.WorkingDirectory;
var cathodeProj = root.CombineWithFilePath("cathode.proj");
var doc = root.Combine("doc");
var trimmingCsproj = root.Combine("src").Combine("trimming").CombineWithFilePath("trimming.csproj");
var @out = root.Combine("out");
var outLogDotnet = @out.Combine("log").Combine("dotnet");
var outPkg = @out.Combine("pkg");

// Globs

var githubGlob = new GlobPattern(outPkg.Combine("debug").CombineWithFilePath("*.nupkg").FullPath);
var nugetGlob = new GlobPattern(outPkg.Combine("release").CombineWithFilePath("*.nupkg").FullPath);

// Utilities

DotNetMSBuildSettings ConfigureMSBuild(string target)
{
    var prefix = $"{target}_{Environment.UserName}_{Environment.MachineName}_";
    var time = DateTime.Now;

    string name;

    do
    {
        name = $"{prefix}{time:yyyy-MM-dd_HH_mm_ss}.binlog";
        time = time.AddSeconds(1);
    }
    while (System.IO.File.Exists(name));

    return new()
    {
        // TODO: https://github.com/dotnet/msbuild/issues/6756
        NoLogo = true,
        BinaryLogger = new()
        {
            Enabled = true,
            FileName = outLogDotnet.CombineWithFilePath(name).FullPath,
        },
        ConsoleLoggerSettings = new()
        {
            NoSummary = true,
        },
        ArgumentCustomization = args => args.Append("-ds:false"),
    };
}

// Tasks

Task("default")
    .IsDependentOn("build")
    .IsDependentOn("pack");

Task("default-editor")
    .IsDependentOn("build")
    .IsDependentOn("pack");

Task("restore-core")
    .Does(() =>
        DotNetRestore(
            cathodeProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("restore"),
            }));

Task("restore-doc")
    .Does(() => DoInDirectory(doc, () => NpmInstall()));

Task("restore")
    .IsDependentOn("restore-core")
    .IsDependentOn("restore-doc");

Task("build-core")
    .IsDependentOn("restore-core")
    .Does(() =>
        DotNetBuild(
            cathodeProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("build"),
                Configuration = configuration,
                NoRestore = true,
            }));

Task("build-trimming")
    .IsDependentOn("build-core")
    .Does(() =>
        DotNetPublish(
            trimmingCsproj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("publish"),
                Configuration = configuration,
                NoBuild = true,
            }));

Task("build-doc")
    .IsDependentOn("restore-doc")
    .Does(() => DoInDirectory(doc, () => NpmRunScript("build")));

Task("build")
    .IsDependentOn("build-trimming")
    .IsDependentOn("build-doc");

Task("pack-core")
    .IsDependentOn("build-core")
    .Does(() =>
        DotNetPack(
            cathodeProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("pack"),
                Configuration = configuration,
                NoBuild = true,
            }));

Task("pack")
    .IsDependentOn("pack-core");

Task("upload-core-github")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref == "refs/heads/master")
    .WithCriteria(configuration == "Debug")
    .IsDependentOn("pack-core")
    .Does(() =>
        DotNetTool(
            null,
            "gpr push",
            new ProcessArgumentBuilder()
                .AppendQuoted(githubGlob)
                .AppendSwitchQuotedSecret("-k", githubToken)));

Task("upload-core-nuget")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(configuration == "Release")
    .IsDependentOn("pack-core")
    .Does(() =>
    {
        foreach (var pkg in GetFiles(nugetGlob))
            DotNetNuGetPush(
                pkg,
                new()
                {
                    Source = "https://api.nuget.org/v3/index.json",
                    ApiKey = nugetToken,
                    SkipDuplicate = true,
                });
    });

RunTarget(target);
