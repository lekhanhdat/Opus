$processInfo = Get-Process "SonarQube.Scanner.MSBuild"
while($processInfo)
{
    Write-Output "SonarQube.Scanner.MSBuild is running..."
    Start-Sleep -Seconds 30
    $processInfo = Get-Process "SonarQube.Scanner.MSBuild"
}