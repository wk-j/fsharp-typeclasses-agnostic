
//var proj = "FSharpTypeClasses/FSharpTypeClasses.fsproj";
var proj = "Source/Typeclasses/Typeclasses.fsproj";

Task("Build").Does(() => {
    MSBuild(proj);
});

Task("Rebuild").Does(() => {
    MSBuild(proj, new MSBuildSettings {
        Verbosity = Verbosity.Minimal,
        ToolVersion = MSBuildToolVersion.VS2017,
        Configuration = "Debug"
        //PlatformTarget = PlatformTarget.MSIL
    }.WithTarget("Rebuild")
    );
});

var target = Argument("target", "default");
RunTarget(target);