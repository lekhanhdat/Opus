param(
    $properityFile = "",
    $version = ""
    )

try{
    if(!(Test-Path $properityFile))
        {
            Write-Warning "$properityFile IS NOT EXIST!"
            exit 1 
        }
        $xmldata = [XML](Get-Content -Path $properityFile)
        $oldver = $xmldata.Project.PropertyGroup.Version
        $xmldata.Project.PropertyGroup.Version = $version
        Write-Host "update version $oldver to $version"
        $xmldata.Save($properityFile) 
    }
catch
{

    Write-Error $_
    exit 1
}
   