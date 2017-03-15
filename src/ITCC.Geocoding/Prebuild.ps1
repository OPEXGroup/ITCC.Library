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
	& "$NugetPath" restore "$SolutionDir\lib\ITCC.Library\src\ITCC.Geocoding\packages.config" -OutputDirectory "$SolutionDir\lib\ITCC.Library\packages"
}
Write-Host "Finished prebuild"