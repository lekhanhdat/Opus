$baseName="avepointregistry.azurecr.io/reco"
$tag=[System.DateTime]::UtcNow.ToString("yyyyMMdd")

function Build-Image($service)
{
    $imageName="$baseName/$service" + 'base:' + $tag
    echo "Build $imageName"
    docker build -t $imageName -f $service .
    docker push $imageName
}

Build-Image 'web'
Build-Image 'timer'

echo 'Done.'
Read-Host