# Publish self-contained apps for Windows, Linux, and macOS

$runtimes = "win-x64", "linux-x64", "osx-x64"
$targets = "net8.0"

Foreach($r in $runtimes)
{
	Foreach($t in $targets)
	{
		dotnet publish .\src\Kontent.Ai.ModelGenerator\ -c Release --runtime $r --framework $t --self-contained -o (".\artifacts\" + $r + "\" + $t) -p:PublishSingleFile=true -p:PublishTrimmed=true
		Compress-Archive (".\artifacts\" + $r + "\" + $t + "\*") (".\artifacts\KontentModelGenerator-" + $r + "-" + $t + ".zip")
		Remove-Item (".\artifacts\" + $r + "\" + $t) -Recurse
	}
	Remove-Item (".\artifacts\" + $r) -Recurse
}