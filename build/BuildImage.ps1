Param([string]$service,
      [string]$version,
      [string]$buildId,
      [string]$buildPath,
	  [string]$buildFolder,
	  [string]$dockerfile)

$baseName="avepointregistry.azurecr.io/reco"
$tag="$version-$buildId"

$imageName="$baseName/$service" + ':' + $tag

echo "Build image $imageName"

if($dockerfile)
{
	docker build -t $imageName $buildPath\$buildFolder -f $buildPath\$buildFolder\$dockerfile
}
else
{
	docker build -t $imageName $buildPath\$buildFolder
}

echo 'Push image to acr'
docker push $imageName
if(!$?)
{
	throw "Failed to push image to acr"
}

echo 'Done.'