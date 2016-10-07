#tool "nuget:?package=xunit.runner.console"
#addin Cake.HipChat

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

var buildDir = Directory("./ConsoleApplication1/bin") + Directory(configuration);


Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./ConsoleApplication1.sln");
});

Task("Build-obsolete")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./ConsoleApplication1.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./ConsoleApplication1.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetBuild("./ConsoleApplication1.sln", settings =>
    settings.SetConfiguration("Debug")
        .SetVerbosity(Verbosity.Minimal)
        .WithTarget("Rebuild")
        .WithProperty("TreatWarningsAsErrors", "false"));

    var authToken = EnvironmentVariable("HGBGiHluWxpVIoq8QJFeaotuDhJRR1yqyizTXkXA");  
    var roomId = EnvironmentVariable("Tele-Service");
    var senderName = EnvironmentVariable("Build Authomation");         

    try
    {
        SendMessage(authToken, roomId, senderName, "Test message from Build Automation script");
    }
    catch(Exception ex)
    {
        Error("{0}", ex);
    }
});

Task("RunUnitTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2("./**/bin/" + configuration + "/*.Tests.dll");
});

Task("Default")
  .IsDependentOn("RunUnitTests");

RunTarget(target);