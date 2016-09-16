param(
	[string]$SolutionDir
)

Write-Host "Started prebuild"
Write-Host "Solution directory $SolutionDir"
$NugetPath = "$SolutionDir\nuget.exe"
if (Test-Path "$NugetPath") {
  
} else {
	(New-Object Net.WebClient).DownloadFile('https://nuget.org/nuget.exe', "$NugetPath")
}
if ("$SolutionDir".Contains("ITCC.Library")) {
	Write-Host "Nothing to do"
} else {
	& "$NugetPath" restore "$SolutionDir\ITCC.Library\ITCC.Geocoding\packages.config" -OutputDirectory "$SolutionDir\ITCC.Library\packages"
}
Write-Host "Finished prebuild"