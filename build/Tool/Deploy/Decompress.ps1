Param([string]$local="")

Function Decompress-File($path,$topath) {
    $shell = new-object -com shell.application
    $zip = $shell.NameSpace($path)
    foreach ($item in $zip.items()) {
        $shell.Namespace($topath).copyhere($item)
    }
}

write-host "# Decompress files ..."
$files =  (Get-ChildItem -Path $local -Filter "*.zip") -as [array]
if(($files -eq $null) -or ($files.Length -eq 0))
{
    Write-Error "[ERROR] Failed to decompress package."
} elseif ($files.Length -gt 1) {
    Write-Error "[ERROR] Failed to decompress package."
} else {
    Decompress-File -path $files[0].FullName -topath $local
    Remove-Item -Path $files[0].FullName -Force
}