param(
[string] $packagepath = '',
[string] $timestamp = '',
[string] $extension = ''
)

$hash = @{"BUILD_TIMESTAMP" = $timestamp}
$md5 = [System.Security.Cryptography.MD5]::Create()
$packages =  Get-ChildItem -Path $packagepath -Filter $extension
foreach($package in $packages)
{
    $name = $package.Name
    $fileReader = new-object System.IO.FileStream $package.FullName, "Open"
    $localMD5 = [System.Convert]::ToBase64String($md5.ComputeHash($fileReader))
    $hash[$name] = $localMD5
    $fileReader.Close()
}

$hash.GetEnumerator() | Select Key,Value | Export-Csv -Path $packagepath\md5.csv -Force -NoTypeInformation