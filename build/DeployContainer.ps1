Param([string]$service,
      [string]$version,
      [string]$buildId,
	  [string]$dpmPrefix)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$tag="$version-$buildId"

echo "Deploying $service $tag"
$url = "https://aksmgtapi.sharepointguild.com/api/service/$($dpmPrefix)-$($service)?ImageTag=$($tag)"
Invoke-WebRequest -Uri $url -Method 'PUT'
echo 'Deploy done.'