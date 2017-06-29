
var proj = "FSharpTypeClasses/FSharpTypeClasses.fsproj";

Task("Build").Does(() => {
    MSBuild(proj);
});

var target = Argument("target", "default");
RunTarget(target);