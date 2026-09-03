function Modify-RevIMOnlineWebConfig
{
    param(
        [string]$filePath,
        [string]$condition,
        [string]$value
    )

    Write-Host "[INFO] Modify $filePath $xPath.configuration.appSettings.add with $value."
    try {
        $xmldata = [xml](Get-Content -Path $filePath)
        $nodes = $xmldata.configuration.appSettings.add | Where-Object { $_.key -eq $condition }
        foreach ($node in $nodes) {
            $node.value = $value
        }
        $xmldata.Save($filePath)
    } catch [System.Exception] {
        Write-Host $_.Exception.ToString()
        Write-Host "[ERROR] Modify $filePath failed."
        exit 1
    }
    Write-Host "[INFO] Successfully modify $filePath."
}

function Modify-RevIMOnlineCscfg
{
    param(
        [string]$filePath,
        [string]$condition,
        [string]$value
    )

    Write-Host "[INFO] Modify $filePath $xPath.ServiceConfiguration.Role.ConfigurationSettings.Setting with $value."
    try {
        $xmldata = [xml](Get-Content -Path $filePath)
        $nodes = $xmldata.ServiceConfiguration.Role.ConfigurationSettings.Setting | Where-Object { $_.name -eq $condition }
        foreach ($node in $nodes) {
            $node.value = $value
        }
        $xmldata.Save($filePath)
    } catch [System.Exception] {
        Write-Host $_.Exception.ToString()
        Write-Host "[ERROR] Modify $filePath failed."
        exit 1
    }
    Write-Host "[INFO] Successfully modify $filePath."
}


function Extract-File
{
    param(
        [string]$filePath,
        [string]$target
    )

    Write-Host "[INFO] Extract $filePath to $target."
    $shell = New-Object -com shell.application
    $zip = $shell.NameSpace("$filePath")

    if (Test-Path -Path $target)
    {
        Remove-Item -Path $target -Recurse
    }
    New-Item -Path $target -ItemType "directory"

    foreach($item in $zip.items())
    {
        $shell.Namespace($target).copyhere($item)
    }
    Write-Host "[INFO] Successfuly extract $filePath."
}

Export-ModuleMember -Function Modify-RevIMOnlineWebConfig
Export-ModuleMember -Function Modify-RevIMOnlineCscfg
Export-ModuleMember -Function Extract-File