param(  
   [string]$version,
   [array]$files 
)
$basedpath = resolve-path "$PSScriptRoot\..\.."
$buildpkg = "$basedpath\CICD\records_package"
$LocalPath = "$buildpkg\output\x64\new"
$LocalPathold = "$buildpkg\output\x64"
$LocalPatholdbin = "$buildpkg\old"

$date = Get-Date -Format yyyy-MM-dd
#$datetime = Get-Date -Format HH-mm-ss
$RemoteDir = "ftp://10.1.4.5/reco/hybridpkg/$date"

mkdir $LocalPathold
mkdir $LocalPatholdbin\Cloud\Agent
$Context = New-AzStorageContext -ConnectionString $PACKAGE_SA_STRING;
Get-AzStorageBlobContent -Container reco -Blob "old/CloudAgentInstaller_15.11.0.364.wixpdb" -Destination $LocalPathold -Context $Context
Get-AzStorageBlobContent -Container reco -Blob "Agent.zip" -Destination $LocalPatholdbin -Context $Context
$AgentPath = "$LocalPatholdbin\Agent.zip" 
Expand-Archive -Path $AgentPath -DestinationPath $LocalPatholdbin\Cloud\Agent
Remove-item $AgentPath -Force -Recurse



if ($files -ne $null) {
    $oldpkgpath="$buildpkg\old\Cloud\Agent\bin"
    $newpkgpath="$buildpkg\new\Cloud\Agent\bin"
    foreach($file in $files){
        $oldDirectoryName = (ls $oldpkgpath -Include $file -Recurse).DirectoryName
        $newDirectoryName = (ls $newpkgpath -Include $file -Recurse).DirectoryName
        "$oldDirectoryName\$file"
        "$newDirectoryName\$file"
        Copy-Item "$oldDirectoryName\$file" "$newDirectoryName\$file" -Force
    }
}

docker run `
-v "$buildpkg/:c:/source/" `
-u ContainerAdministrator `
-e version=$version `
-w 'c:/source' --rm clouddevops.azurecr.io/reco/wixtoolset:1.1.0 powershell /c `
".\run.ps1"
if(!$?){ "### WIXBuild failed."; exit 1 }

$LocalFilemsi = "$LocalPath\CloudAgentInstaller_$version.msi"
$LocalFilewixpdb = "$LocalPath\CloudAgentInstaller_$version.wixpdb"
$LocalFilemsp = "$buildpkg\CloudAgentInstaller_Upgrade_$version.msp"

."$PSScriptRoot\sign.ps1" -patchPath $LocalFilemsi -signname "CloudAgentInstaller"
."$PSScriptRoot\sign.ps1" -patchPath $LocalFilemsp -signname "CloudAgentInstaller_Upgrade"

$Msi_Blob_Path = "hybridpkg\$date\CloudAgentInstaller_$version.msi"
$Wixpdb_Blob_Path = "hybridpkg\$date\CloudAgentInstaller_$version.wixpdb"
$Msp_Blob_Path = "hybridpkg\$date\CloudAgentInstaller_Upgrade_$version.msp"
Set-AzStorageBlobContent -File $LocalFilemsi -Container reco -Blob $Msi_Blob_Path -Context $Context -Force -ErrorAction Stop
Set-AzStorageBlobContent -File $LocalFilewixpdb -Container reco -Blob $Wixpdb_Blob_Path -Context $Context -Force -ErrorAction Stop
Set-AzStorageBlobContent -File $LocalFilemsp -Container reco -Blob $Msp_Blob_Path -Context $Context -Force -ErrorAction Stop

  
$LocalFileLog = "$buildpkg\ObfuscateFailedLogs.zip"
if(test-path $LocalFileLog){
    $File_Log_Blob_Path = "hybridpkg\$date\ObfuscateFailedLogs.zip";
   Set-AzStorageBlobContent -File $LocalFileLog -Container reco -Blob $File_Log_Blob_Path -Context $Context -Force -ErrorAction Stop
}

