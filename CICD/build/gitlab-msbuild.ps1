param(  
   [string]$version
)

$basedpath = resolve-path "$PSScriptRoot\..\.."
$buildxml = "c:\source\CICD\Hybrid\HybridAgentServiceBuild.xml"
$hybridsln = "c:\source\RAAgent.sln"
$netcoreproperty = "$basedpath\Directory.Build.targets"
$binpath = "$basedpath\CICD\records_package\new\Cloud\Agent\bin"
#$LocalFile = "$basedpath\CICD\msbuild_$version.zip"
$Obfuscate = "$basedpath\CICD\build\Obfuscate"
$FPtemp = "$Obfuscate\FPLauncher\HistoryJobInfo\TempFiles"
$Obfuscatelog = "$basedpath\CICD\records_package\ObfuscateFailedLogs.zip"

$date = Get-Date -Format yyyy-MM-dd
$RemoteDir = "ftp://10.1.4.5/reco/MSBuildfile/$date"

.\CICD\build\ContinuousIntegrationExtendedTool.exe -cv .  modifyAssemblyInfoVersion $version
.\CICD\build\ContinuousIntegrationExtendedTool.exe -cv .  modifyRcFileVersion $version
#.\CICD\build\ContinuousIntegrationExtendedTool.exe -cv .  modifypackageJSONVersion $version
.\CICD\build\update-netcore-version.ps1 -properityFile $netcoreproperty -version $version

docker run `
-v "$basedpath/:c:/source/" `
-u ContainerAdministrator `
-w 'c:/source' --rm clouddevops.azurecr.io/winbuildbox:2019-10-latest cmd /c `
"nuget restore $hybridsln -ConfigFile c:/source/US-Nuget.config && MSBuild $buildxml /t:MainDeploy /clp:ErrorsOnly /verbosity:Quiet" 
if(!$?){ "### MSBuild failed."; exit 1 }

"# make bin"
if(!(test-path $binpath)){
   mkdir $binpath | Out-Null
}

Copy-Item "$Obfuscate\apeg-win\IncludeInPackage.xml" "$basedpath\bin\Agent\Release" -Force
if ($CI_SERVER_HOST -eq "gitlab.avepoint.net"){Copy-Item "$Obfuscate\FPLauncher\us-FP.Launcher.exe.config" "$Obfuscate\FPLauncher\FP.Launcher.exe.config" -recurse} 
. $PSScriptRoot\Obfuscate\FPLauncher\FP.Launcher.exe -new RecordsHybird 1.0.0 All "$basedpath\bin\Agent\Release"
$fileName = (ls $FPtemp | Sort-Object LastWriteTime -Descending | Select-Object -First 1).name
Compress-Archive -Path "$basedpath\bin\Agent\ObfuscateFiles\**\*.log" -DestinationPath $Obfuscatelog

$finalpath = "$basedpath\bin\Agent\Release\*"
"############finalpath############"
$finalpath 

$files = get-content -Path "$PSScriptRoot\signname.txt"
$root = "$basedpath\bin\Agent\Release"
foreach($file in $files){
    $patchPath = ""
    $patchPath = "$root\$file"
    $signname = ""
    $signname = $file.Split(".")[0]
    if(Test-Path $patchPath){       
        $patchPath
        $signname
        ."$PSScriptRoot\sign.ps1" -patchPath $patchPath -signname $signname
    }
}

"############move build file############"
Copy-Item -Path $finalpath -Destination $binpath -Force -Recurse

$sourceSubDirs = Get-ChildItem -Path "$basedpath\bin\Agent\Release" -Directory -ErrorAction SilentlyContinue
foreach($subDir in $sourceSubDirs) {
    $sourcePath = "$($subDir.FullName)\*"
    $destPath = "$binpath\$($subDir.Name)"
    if(!(Test-Path $destPath)) {
        New-Item -ItemType Directory -Path $destPath -Force | Out-Null
    }
    Copy-Item -Path $sourcePath -Destination $destPath -Force -Recurse
}
"############move build file done############"

# "############Compressfile##############"
# Compress-Archive -Path "$binpath\*" -DestinationPath $LocalFile
# "############Compressfiledone##############"

# "############pushftp##############"
# ."$basedpath\CICD\tools\pushftp.ps1" -RemoteDir $RemoteDir -LocalFile $LocalFile
# "############pushftpdone##############"

# rm $LocalFile
