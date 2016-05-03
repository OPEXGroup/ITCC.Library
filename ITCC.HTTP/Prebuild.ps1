param(
	[string]$SolutionDir,
	[string]$SolutionPath
)

Write-Host "Started prebuild"
Write-Host "Solution directory $SolutionDir"
Write-Host "Solution path: $SolutionPath"
$NugetPath = "$SolutionDir\nuget.exe"
if (Test-Path "$NugetPath") {
  
} else {
	(New-Object Net.WebClient).DownloadFile('https://nuget.org/nuget.exe', "$NugetPath")
}
& "$NugetPath" restore $SolutionPath
Write-Host "Finished prebuild"