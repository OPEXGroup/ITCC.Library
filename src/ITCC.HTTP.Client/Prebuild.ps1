param(
	[string]$SolutionDir
)

$LibraryName = "ITCC.Library"
$ProjectName = "ITCC.HTTP.Client"

Write-Host "Started prebuild"
Write-Host "Solution directory $SolutionDir"

$NugetPath = "$SolutionDir\nuget.exe"

if (Test-Path "$NugetPath") {
	Write-Host "Nuget.exe has been already downloaded"
} else {
	(New-Object Net.WebClient).DownloadFile('https://dist.nuget.org/win-x86-commandline/latest/nuget.exe', "$NugetPath")
}
if ("$SolutionDir".Contains("$LibraryName")) {
	Write-Host "Nothing to do"
} else {
	$PackagesConfigPath = "$SolutionDir\lib\$LibraryName\src\$ProjectName\packages.config"
	$PackagesBinariesPath = "$SolutionDir\lib\$LibraryName\packages\"
	Write-Host "packages.config file $PackagesConfigPath"
	Write-Host "packages binaries directory $PackagesBinariesPath"
	& "$NugetPath" restore "$PackagesConfigPath" -OutputDirectory "$PackagesBinariesPath"
}
Write-Host "Finished prebuild"