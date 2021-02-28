# Publish self-contained apps for Windows, Linux, and macOS

$runtimes = "win-x64", "linux-x64", "osx-x64"

Foreach($r in $runtimes)
{
	dotnet publish .\src\Kentico.Kontent.ModelGenerator\ -c Release --runtime $r -o (".\artifacts\" + $r) -p:PublishSingleFile=true -p:PublishTrimmed=true
	Compress-Archive (".\artifacts\" + $r + "\*") (".\artifacts\KontentModelGenerator-" + $r + ".zip")
	Remove-Item (".\artifacts\" + $r) -Recurse
}