#addin nuget:?package=Cake.FileHelpers&version=4.0.1
#addin nuget:?package=Cake.Rest&version=0.1.2
///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Publish");
var packageVersion = Argument("packageVersion", "0.94.0");
var url = Argument("url", "https://github.com/Dr-Noob/cpufetch/releases/download/v0.94/cpufetch_x86_windows.exe");
var url64bit = Argument("url64bit", string.Empty);
var binDir = Argument("binDir", "bin");
var tempDir = Argument("tempDir", "temp");

ChocolateyPackSettings packageInfo = null;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   Information("Running tasks...");
});

Teardown(ctx =>
{
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    DeleteFiles("./**/*.nupkg");
    DeleteFiles("./**/*.nuspec");
    DeleteFiles(System.IO.Path.Combine(binDir, "*"));
    if (DirectoryExists(binDir))
    {
        DeleteDirectory(binDir, new DeleteDirectorySettings {
        Force = true
        });
    }
    DeleteFiles(System.IO.Path.Combine(tempDir, "*"));
    if (DirectoryExists(tempDir))
    {
        DeleteDirectory(tempDir, new DeleteDirectorySettings {
        Force = true
        });
    }
});

Task(".gitignore clean")
    .Does(() =>
{
    var regexes = FileReadLines("./.gitignore");
    foreach(var regex in regexes)
    {
        DeleteFiles(regex);
    }
});

Task("Set package info")
    .Does(() =>
{
    packageInfo = new ChocolateyPackSettings
    {
        //PACKAGE SPECIFIC SECTION
        Id = "cpufetch",
        Version = packageVersion,
        PackageSourceUrl = new Uri("https://github.com/zverev-iv/choco-cpufetch"),
        Owners = new[] { "zverev-iv" },
        //SOFTWARE SPECIFIC SECTION
        Title = "Cpufetch",
        Authors = new[] {
            "Dr-Noob"
            },
        Copyright = "2021, Dr-Noob",
        ProjectUrl = new Uri("https://github.com/Dr-Noob/cpufetch"),
        DocsUrl = new Uri("https://github.com/Dr-Noob/cpufetch/blob/master/README.md"),
        BugTrackerUrl = new Uri("https://github.com/Dr-Noob/cpufetch/issues"),
        LicenseUrl = new Uri("https://github.com/Dr-Noob/cpufetch/blob/master/LICENSE"),
        RequireLicenseAcceptance = false,
        Summary = "Simplistic yet fancy CPU architecture fetching tool",
        Description = @"Simplistic yet fancy CPU architecture fetching tool",
        ReleaseNotes = new[] { "https://github.com/Dr-Noob/cpufetch/releases" },
        Files = new[] {
            new ChocolateyNuSpecContent {Source = System.IO.Path.Combine(binDir, "**"), Target = "tools"}
            },
        Tags = new[] {
            "cpufetch",
            "cpu",
            "Intel",
            "Amd",
            "Arm",
            "Snapdragon",
            "Exynos",
            }
    };

});

Task("Copy src to bin")
    .Does(() =>
{
    if (!DirectoryExists(binDir))
    {
        CreateDirectory(binDir);
    }
    CopyFiles("src/*", binDir);
});

Task("Set package args")
    .IsDependentOn("Copy src to bin")
    .Does(() =>
{
    string hash  = null;
    string hash64 = null;
    if (!DirectoryExists(tempDir))
    {
        CreateDirectory(tempDir);
    }
    if(!string.IsNullOrWhiteSpace(url))
    {
        Information("Download x86 binary");
        var uri = new Uri(url);
        var fullFileName = System.IO.Path.Combine(tempDir, System.IO.Path.GetFileName(uri.LocalPath));
        DownloadFile(url, fullFileName);
        Information("Calculate sha256 for x86 binary");
        hash = CalculateFileHash(fullFileName).ToHex();
        Information("Write x86 data in sources");
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${url}", url);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksum}", hash);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksumType}", "sha256");
    }
    if(url64bit == url && hash != null)
    {
        Information("x86 and x64 uri are the same");
        Information("Write x64 data in sources");
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${url64bit}", url);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksum64}", hash);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksumType64}", "sha256");
    }
    else if(!string.IsNullOrWhiteSpace(url64bit))
    {
        Information("Download x64 binary");
        var uri = new Uri(url64bit);
        var fullFileName = System.IO.Path.Combine(tempDir, System.IO.Path.GetFileName(uri.LocalPath));
        DownloadFile(url64bit, fullFileName);
        Information("Calculate sha256 for x86 binary");
        hash64 = CalculateFileHash(fullFileName).ToHex();
        Information("Write x64 data in sources");
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${url64bit}", url64bit);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksum64}", hash64);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksumType64}", "sha256");
    }
});

Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Set package info")
    .IsDependentOn("Set package args")
    .Does(() =>
{
    ChocolateyPack(packageInfo);
});

Task("Publish")
    .IsDependentOn("Pack")
    .Does(() =>
{
    var publishKey = EnvironmentVariable<string>("CHOCOAPIKEY", null);
    var package = $"{packageInfo.Id}.{packageInfo.Version}.nupkg";

    ChocolateyPush(package, new ChocolateyPushSettings
    {
        ApiKey = publishKey
    });
});

Task("get chocolatey versions")
    .Does(() =>
{
    //https://chocolatey.org/api/v2/Packages()?$filter=(tolower(Id)%20eq%20'dolt')%20and%20IsLatestVersion
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
