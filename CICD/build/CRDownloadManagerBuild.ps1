param(  
   [string]$version
)

$basedpath = resolve-path "$PSScriptRoot\..\.."
$DManagersln = "c:\source\Hybrid\CloudRecordDownloadManager\CloudRecordDownloadManager.sln"
$registry_url="harbor.avepoint.net/reco"
$LocalPath = "$basedpath\Hybrid\CloudRecordDownloadManager\bin\x64\Release\CloudAgentDownloader.exe"
$date = Get-Date -Format yyyy-MM-dd


docker run `
-v "$basedpath/:c:/source/" `
-u ContainerAdministrator `
-w 'c:/source' --rm clouddevops.azurecr.io/winbuildbox:2019-latest cmd /c `
"nuget restore $DManagersln -ConfigFile c:/source/NuGet.Config && MSBuild $DManagersln -t:Rebuild -p:Configuration=Release" 
if(!$?){ "### MSBuild failed."; exit 1 }

ren $LocalPath "CloudAgentDownloader_$version.exe"
$LocalFile = "$basedpath\Hybrid\CloudRecordDownloadManager\bin\x64\Release\CloudAgentDownloader_$version.exe"

."$PSScriptRoot\sign.ps1" -patchPath $LocalFile -signname "CloudAgentDownloader"

$Context = New-AzStorageContext -ConnectionString $PACKAGE_SA_STRING
$Exe_Blob_Path = "CloudAgentDownloader\$date\CloudAgentDownloader_$version.exe"
Set-AzStorageBlobContent -File $LocalFile -Container reco -Blob $Exe_Blob_Path -Context $Context -Force -ErrorAction Stop
