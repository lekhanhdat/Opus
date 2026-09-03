# Restore NuGet Packages
param(
[string]$nugetPath = '',
[string]$codePath = '',
[string]$restorePath = ''
)
$nugetSrc = 'http://proget.avepoint.net/nuget/NuGet.org;http://proget.avepoint.net/nuget/AvePoint'
# Restore depends on packages.config
$cfgFiles = Get-ChildItem -Path $codePath -Recurse -Filter "packages.config"
foreach($file in $cfgFiles)
{
    Start-Process $nugetPath -ArgumentList @("restore",$file.FullName,"-source",$nugetSrc,"-PackagesDirectory",$restorePath,"-DirectDownload") -Wait
}