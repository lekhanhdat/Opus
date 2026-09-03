Param([string]$ftpURL="",
      [string]$tempPath="",
      [string]$modulePath="")

Import-Module "$modulePath"
write-output "# Downloading files from ftp ..."
write-output "# Login ftp ..."
$securitypassword = ConvertTo-SecureString "anonymous" -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential("anonymous",$securitypassword)
Set-FTPConnection -Credentials $credential -Server $ftpURL -Session FTPSession -UsePassive
$Session = Get-FTPConnection -Session FTPSession 

if(Test-Path -Path $tempPath)
{
    Remove-Item -Path $tempPath -Recurse -Force
    New-Item -Path $tempPath -ItemType directory
} else {
    New-Item -Path $tempPath -ItemType directory
}

$files = Get-FTPChildItem -Session $Session -Path $ftpURL 
foreach($file in $files) {
    $filename = $file.Name
    $file | Get-FTPItem -Session $Session -LocalPath $tempPath -Overwrite
}
exit 0