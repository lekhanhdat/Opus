Param([string]$service,
      [string]$version,
      [string]$buildId,
      [string]$dpmPrefix)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$tag="$version-$buildId"

echo "Update worker to $service $tag"
$url = "https://aksmgtapi.sharepointguild.com/api/configmap/$dpmPrefix-jobimage?key=$service&value=$tag"
echo $url
Invoke-WebRequest -Uri $url -Method 'PUT'
echo 'Done.'