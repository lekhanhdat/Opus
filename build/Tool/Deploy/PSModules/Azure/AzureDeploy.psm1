function Set-AzureRMSubsriptionInfo
{
    param (
        [string]$AzureAccount,
        [string]$AzurePassword,
        [string]$SubscriptionId
    )
    Write-Host "[INFO] Login."
    $AzurePSPassword = ConvertTo-SecureString -String $AzurePassword -AsPlainText -Force
    $AzureCredential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $AzureAccount,$AzurePSPassword
    Login-AzureRmAccount -Credential $AzureCredential -ErrorAction Stop
    Set-AzureRmContext -SubscriptionId $SubscriptionId -ErrorAction Stop
    Write-Host "[INFO]  Successfully login."
}

function Set-AzureSMSubsriptionInfo
{
    param (
        [string]$PublishSettingsFilePath,
        [string]$SubscriptionId
    )
    Write-Host "[INFO] Import publish settins file."
    Import-AzurePublishSettingsFile -PublishSettingsFile $PublishSettingsFilePath
    Select-AzureSubscription -SubscriptionId $SubscriptionId
    Write-Host "[INFO] Successfully import publish settins file."
}

function Create-ResourceGroup
{
    param (
        [string]$ResourceGroupName,
        [string]$Location
    )
    Write-Host "[INFO] Create resource group." -ForegroundColor DarkGreen
    $ResoruceGroup = Get-AzureRmResourceGroup -Location $Location -Name $ResourceGroupName -ErrorAction SilentlyContinue
    if ($ResoruceGroup -eq $null)
    {
        Write-Host "[INFO] Resource Group: $ResourceGroupName doesn't exist. Begin to create."
        $status = (New-AzureRmResourceGroup -Location $Location -Name $ResourceGroupName).ProvisioningState
        if ( $status -ne "Succeeded")
        {
            Write-Host "[ERROR] Cannot create azure resource group: $ResourceGroupName."
            exit 1
        } else {
            Write-Host "[INFO] Successfully create resource group: $ResourceGroupName."
        }
    } else {
        Write-Host "[INFO] Resource Group: $ResourceGroupName exists."
    }
    Write-Host "[Report] Resource Group: $ResourceGroupName"
}

function Create-SQLServer
{
    param (
        [string]$ResourceGroupName,
        [string]$Location,
        [string]$DBServerName,
        [string]$ServerVersion,
        [string]$DBAccount,
        [string]$DBPassword
    )

    Write-Host "[INFO] Create database server."
    $DBServer = Get-AzureRmSqlServer -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -ErrorAction SilentlyContinue
    if ($DBServer -eq $null)
    {
        Write-Host "[INFO] SQL Server: $DBServerName  doesn't exist. Begin to create $DBServerName."
        $DBPSPassword = ConvertTo-SecureString -String $DBPassword -AsPlainText -Force
        $DBCredential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $DBAccount, $DBPSPassword
        $DBServer = New-AzureRmSqlServer -Location $Location -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -SqlAdministratorCredentials $DBCredential -ServerVersion $ServerVersion  -ErrorAction Stop
        if ($DBServer -eq $null) 
        {
            Write-Host "[ERROR] Failed to create azure database server: $DBServerName."
            exit 1
        } else {
            Write-Host "[INFO] Successfully create azure database server: $DBServerName."
        }
    }  else {
        Write-Host "[INFO] SQL Server: $DBServerName exist. #####################"
    }
    Write-Host "[Report] DB Server Instance: $DBServerName.database.windows.net"
}

function Create-FirewallRule
{
    param (
        [string]$ResourceGroupName,
        [string]$DBServerName,
        [string]$FirewallRuleName,
        [string]$StartIpAddress,
        [string]$EndIpAddress
    )

    Write-Host "[INFO] Set firewall rule for database server: $DBServerName."
    $rule = Get-AzureRmSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -FirewallRuleName $FirewallRuleName -ErrorAction SilentlyContinue
    if ($rule -eq $null)
    {
        Write-Host "[INFO] Filewall Rule: $FirewallRuleName doesn't exist. Begin to create."
        if ($FirewallRuleName -ne "AllowAllAzureIPs") {
            $rule = New-AzureRmSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -FirewallRuleName $FirewallRuleName -StartIpAddress $StartIpAddress -EndIpAddress $EndIpAddress -ErrorAction Stop
        } else {
            $rule = New-AzureRmSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -AllowAllAzureIPs -ErrorAction Stop
        }
        if ($rule -eq $null) {
            Write-Host "[ERROR] Failed to create firewall rule: $FirewallRuleName."
            exit 1
        } else {
            Write-Host "[INFO] Successfully create firewall rule: $FirewallRuleName."
        }

    } else {
        Write-Host "[INFO] Fire wall rule exists: $FirewallRuleName."
    }
}

function Create-SQLDatabase
{
    param (
        [string]$ResourceGroupName,
        [string]$DBServerName,
        [string]$DBName,
        [string]$Edition
    )
    Write-Host "[INFO] Create database: $DBName in $DBServerName."
    $DB = Get-AzureRmSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -DatabaseName $DBName -ErrorAction SilentlyContinue
    if ($DB -eq $null)
    {
        Write-Host "[INFO] DB: $DBName doesn't exist. Begin to create."
        $DB = (New-AzureRmSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -DatabaseName $DBName -Edition $Edition -ErrorAction Stop)
        if ($DB -eq $null)
        {
            Write-Host "[ERROR] Failed to create DB: $DB."
            exit 1
        } else {
            Write-Host "[INFO] Successfully create DB: $DB."
        }
    } else {
        Write-Host "[INFO] DB: $DB exists."
    }
    Write-Host "[Report] DB Name: $DB"
}

function Get-SQLConnectionString
{
    param (
        [string]$ResourceGroupName,
        [string]$DBServerName,
        [string]$DBName,
        [string]$DBAccount,
        [string]$DBPassword
    )
    Write-Host "[INFO] Get database info."
    $DB = Get-AzureRmSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -DatabaseName $DBName -ErrorAction SilentlyContinue
    if ($DB -eq $null) {
        Write-Host "[ERROR] DB doesn't exist."
        exit 1
    } else {
        return "Server=tcp:$DBServerName.database.windows.net,1433;Initial Catalog=$DBName;Persist Security Info=False;User ID=$DBAccount;Password=pt{$DBPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    }

}

function Create-SQLElasticPool 
{
    param (
        [string]$ResourceGroupName,
        [string]$SQLElasticPoolName,
        [string]$DBServerName,
        [string]$Edition,
        [int]$Dtu
    )

    $elasticPool = Get-AzureRmSqlElasticPool -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -ElasticPoolName $SQLElasticPoolName -ErrorAction SilentlyContinue
    if($elasticPool -eq $null)
    {
        Write-Host "SQL elastic pool $SQLElasticPoolName doesn't exist. Create a new one."
        $elasticPool = New-AzureRmSqlElasticPool -ElasticPoolName $SQLElasticPoolName -ResourceGroupName $ResourceGroupName -ServerName $DBServerName -Edition $Edition -Dtu $Dtu
        if ($elasticPool -eq $null)
        {
            Write-Host "Failed to create elastic pool $SQLElasticPoolName."
            exit 1
        } else {
            Write-Host "Successfully create elastic pool $SQLElasticPoolName."
        }
    } else {
        Write-Host "SQL elastic pool $SQLElasticPoolName exists."
    }
}

function Create-SMServiceBus 
{
    param (
        [string]$ServiceBusName,
        [string]$Location,
        [string]$NamespaceType
    )
    Write-Host "[INFO] Create service bus: $ServiceBusName."
    $ServiceBus = Get-AzureSBNamespace -Name $ServiceBusName
    if($ServiceBus -eq $null)
    {
        Write-Host "[INFO] Service bus: $ServiceBus doesn't exist. Begin to create."
        $ServiceBus = New-AzureSBNamespace -Name $ServiceBusName -Location $Location -NamespaceType $NamespaceType -CreateACSNamespace $false
        if ($ServiceBus -eq $null) {
            Write-Host "[ERROR] Failed to create service bus: $ServiceBus."
            exit 1
        } else {
            Write-Host "[INFO] Successfully create service bus: $ServiceBus."
        }
    } else {
        Write-Host "[INFO] Service bus: $ServiceBus exits."
    }
}

function Get-SMServiceBusConnectionString
{
    param (
        [string]$ServiceBusName,
        [string]$Location
    )
    Write-Host "[INFO] Get service bus info."
    $ServiceBus = Get-AzureSBNamespace -Name $ServiceBusName
    if ($ServiceBus -eq $null)
    {
        Write-Host "[ERROR] Service bus doesn't exit."
        eixt 1
    } else {
        $JobQueueConnectionString=(Get-AzureSBAuthorizationRule -Namespace $ServiceBusNamespace).ConnectionString
        return $JobQueueConnectionString
    }
}

function Create-RMServiceBus
{
    param (
        [string]$ResourceGroupName,
        [string]$ServiceBusName,
        [string]$Location,
        [string]$SkuName
    )
    
    Write-Host "[INFO] Create service bus: $ServiceBusName."
    $ServiceBus =  Get-AzureRmServiceBusNamespace -NamespaceName $ServiceBusName -ResourceGroup $ResourceGroupName -ErrorAction SilentlyContinue
    if($ServiceBus -eq $null)
    {
        Write-Host "[INFO] Service Bus: $ServiceBusName doesn't exist. Begin to create."
        $status = (New-AzureRmServiceBusNamespace -Location $Location -ResourceGroupName $ResourceGroupName -NamespaceName $ServiceBusName -SkuName $SkuName).ProvisioningState
        if ($status -ne "Succeeded")
        {
            Write-Host "[ERROR] Failed to create $ServiceBusName."
            exit 1
        } else {
            Write-Host "[INFO] Successfully create $ServiceBusName."
        }
    } else {
        Write-Host "[INFO] Service Bus Namespace: $ServiceBusName exists."
    }   
}

function Get-RMServiceBusConnectionString
{
    param (
        [string]$ResourceGroupName,
        [string]$ServiceBusName,
        [string]$Location
    )
    
    Write-Host "[INFO] Get service bus info."
    $ServiceBus =  Get-AzureRmServiceBusNamespace -NamespaceName $ServiceBusName -ResourceGroup $ResourceGroupName -ErrorAction SilentlyContinue
    if($ServiceBus -eq $null)
    {
        Write-Host "[ERROR] Service bus doesn't exist."
        exit 1
    } else {
        $JobQueueConnectionString = (Get-AzureRmServiceBusKey -NamespaceName $ServiceBusName -ResourceGroup $ResourceGroupName -Name RootManageSharedAccessKey).PrimaryConnectionString
        return $JobQueueConnectionString
    }
}

function Create-ClassicStorage
{
    param (
        [string]$StorageName,
        [string]$Location,
        [string]$Type
    )
    Write-Host "[INFO] Create storage:$StorageName."
    $storage = Get-AzureStorageAccount -StorageAccountName $StorageName -ErrorAction SilentlyContinue
    if ($storage -eq $null)
    {
        Write-Host "Stroage Account: $StorageName doesn't exist. Begin to create $StorageName."
        $storage = New-AzureStorageAccount -Location $Location -StorageAccountName $StorageName -Type $Type -ErrorAction Stop
        if ($storage -eq $null) {
            Write-Host "[ERROR] Failed to create $StorageName" 
            exit 1
        } else {
            Write-Host "[INFO] Successfully create $StorageName" 
        }
    } else {
        Write-Host "[INFO] Storage $StorageName exists." 
    }
    $StorageKey=(Get-AzureStorageKey -StorageAccountName $StorageName).Primary
    return $StorageKey
}

function Create-BlobStorage
{
    param (
        [string]$ResourceGroupName,
        [string]$StorageName,
        [string]$Location,
        [string]$SkuName,
        [string]$AccessTier,
        [string]$Kind
    )
    Write-Host "[INFO] Create storage:$StorageName."
    $storage = Get-AzureRmStorageAccount -Name $StorageName -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue
    if ($storage -eq $null)
    {
        Write-Host "Stroage Account: $StorageName doesn't exist. Begin to create $StorageName."
        
        $storage = New-AzureRmStorageAccount -Location $Location -Name $StorageName -ResourceGroupName $ResourceGroupName -SkuName $SkuName -AccessTier $AccessTier -Kind $Kind -ErrorAction Stop
        
        if ($storage -eq $null) {
            Write-Host "[ERROR] Failed to create $StorageName" 
            exit 1
        } else {
            Write-Host "[INFO] Successfully create $StorageName" 
        }
    } else {
        Write-Host "[INFO] Storage $StorageName exists." 
    }
}

function Get-AzureBlobStorageKey
{
    param (
        [string]$ResourceGroupName,
        [string]$StorageName,
        [string]$Location
    )
    Write-Host "[INFO] Get storage info."
    $storage = Get-AzureRmStorageAccount -Name $StorageName -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue
    if ($storage -eq $null)
    {
        Write-Host "[ERROR] Storage doesn't exit."
        exit 1
    } else {
        $StorageKey=(Get-AzureRmStorageAccountKey -Name $StorageName -ResourceGroupName $ResourceGroupName -ErrorAction Stop).Value[0]
        return $StorageKey
    }
}

function Get-AzureBlobStorageConnectionString
{
    param (
        [string]$ResourceGroupName,
        [string]$StorageName,
        [string]$Location
    )
    Write-Host "[INFO] Get storage info."
    $storage = Get-AzureRmStorageAccount -Name $StorageName -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue
    if ($storage -eq $null)
    {
        Write-Host "[ERROR] Storage doesn't exit."
        exit 1
    } else {
        $StorageKey=(Get-AzureRmStorageAccountKey -Name $StorageName -ResourceGroupName $ResourceGroupName -ErrorAction Stop).Value[0]
        return "DefaultEndpointsProtocol=https;AccountName=$StorageName;AccountKey=$StorageKey;EndpointSuffix=core.windows.net"
    }
}

function Create-Container
{
    param (
        [string]$ContainerName,
        [string]$StorageName,
        [string]$StorageKey
    )
    Write-Host "[INFO] Create container:$ContainerName."
    $StorageContext=New-AzureStorageContext -StorageAccountName $StorageName -StorageAccountKey $StorageKey -ErrorAction Stop
    $container = Get-AzureStorageContainer -Context $StorageContext -Name $ContainerName -ErrorAction SilentlyContinue
    if($container -eq $null)
    {
        Write-Host "[INFO] Container: $ContainerName doesn't exist. Begin to create."
        $container = New-AzureStorageContainer -Name $ContainerName -Context $StorageContext -ErrorAction Stop
        if ($container -eq $null)
        {
            Write-Host "[ERROR] Failed to create container: $ContainerName"
            exit 1
        } else {
            Write-Host "[INFO] Successfully create container: $ContainerName"
        }
    } else {
        Write-Host "[INFO] Container: $ContainerName exists."
    }
}

function Get-ContianerXri
{
    param (
        [string]$BlobEndPoint,
        [string]$ContainerName,
        [string]$StorageName,
        [string]$StorageKey,
        [string]$XriCliPath,
        [string]$TempFilePath
    )

    $argumentList = $BlobEndPoint,$ContainerName,$StorageName,$StorageKey

    if(Test-Path -Path $TempFilePath)
    {
        Remove-Item -Path $TempFilePath -Force
    }

    Start-Process -FilePath $XriCliPath -ArgumentList $argumentList -RedirectStandardOutput $TempFilePath -Wait
    $content = Get-Content -Path $TempFilePath

    return $content
}

function Create-WebApp
{
    param (
        [string]$ResourceGroupName,
        [string]$Location,
        [string]$WebAppName
    )
    Write-Host "[INFO] Create Web App:$WebAppName."
    $WebApp = Get-AzureRmWebApp -Name $WebAppName -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue
    if ($WebApp -eq $null) {
        Write-Host "[INFO] Web App: $WebAppName doesn't exist. Begin to create."
        $WebApp = New-AzureRmWebApp -ResourceGroupName $ResourceGroupName -Location $Location -Name $WebAppName -ErrorAction Stop
        if ($WebApp -eq $null) {
            Write-Host "[ERROR] Failed to Web App: $WebAppName"
            exit 1
        } else {
            Write-Host "[INFO] Successfully create Web App: $WebAppName"
        }
    } else {
        Write-Host "[INFO] Web App: $WebAppName exists."
    }
    
}

function Update-WebApp
{
    param (
        [string]$WebAppName,
        [string]$packageURL
    )

    Write-Host "[INFO] Update Web App:$WebAppName."
    $WebApp = Get-AzureWebsite -Name $WebAppName -ErrorAction SilentlyContinue
    if ($WebApp -eq $null) {
        Write-Host "[ERROR] $WebAppName could not be found. Please create and configure your web app first."
        exit 1
    } else {
        Write-Host "[INFO] Start to update Web App:" $WebAppName
        Publish-AzureWebsiteProject -Name $WebAppName -Package $packageURL -ErrorAction Stop
        Write-Host "[INFO] Successfully update Web App: $WebAppName" 
    }
}

function Update-CloudService
{
    param (
	[string]$SubscriptionId,
        [string]$service,
        [string]$slot,
	[string]$StorageName,
        [string]$packageURL,
        [string]$configURL,
        [string]$isStart,
        [string]$label
    )
    Write-Host "[INFO] Update $service $slot environment."
	Set-AzureSubscription -SubscriptionId $SubscriptionId -CurrentStorageAccountName $StorageName
    $deployment = Get-AzureDeployment -ServiceName $service -Slot $slot -ErrorAction silentlycontinue 
    if ($deployment.Name -eq $null) {
        Write-Host "[INFO] No deployment is detected in $service $slot. Creating a new deployment. "
        $operationStatus = ""

        if($isStart -eq "TRUE") {
            $operationStatus = (New-AzureDeployment -ServiceName $service -Slot $slot -Package $packageURL -Configuration $configURL -Label $label).OperationStatus
        } else {
            $operationStatus = (New-AzureDeployment -ServiceName $service -Slot $slot -Package $packageURL -Configuration $configURL -DoNotStart -Label $label).OperationStatus
        }

        if ($operationStatus -eq "Succeeded")
        {
            Write-Host "[INFO] New Deployment created in $service $slot environment."
        } else {
            Write-Host "[INFO] Failed to create new deployment in $service $slot environment."
            exit 1
        }
    } else {
        Write-Host "[INFO] Deployment is detected in $service $slot. Update the deployment. "
        $operationStatus = (Set-AzureDeployment -Upgrade -ServiceName $service -Slot $slot -Package $packageURL -Configuration $configURL -Force -Label $label).OperationStatus
        if ($operationStatus -eq "Succeeded")
        {
            Write-Host "[INFO] Successfully update $service $slot deployment."
        } else {
            Write-Host "[INFO] Failed to update $service $slot deployment."
            exit 1
        }
    }
}

Function Check-CloudServiceInstanceStatus
{
    param (
        [string]$service,
        [string]$slot,
        [string]$retryCount
    )
     $hasBusyRole = $true
     $CheckCount=0
     while ($hasBusyRole)
     {
        if($CheckCount -gt $retryCount)
        {
          Write-Host "[ERROR] The deployment for $service cloud't be ready. There is something wrong!!!"
          exit 1
        }

        $instances = Get-AzureRole -ServiceName $service -Slot $slot -InstanceDetails
        $count = 0

        foreach ($instance in $instances)
        {

            if ($instance.InstanceStatus -ne "ReadyRole")
            {
                break                
            }

            else
            {
                Write-Host "[INFO] '$instance.InstanceName' is Ready. "
                $count++
            }
        }

        Write-Host "[INFO] The count of instances with ReadyRole status is $count"

        if ($count -eq $instances.Count)
        {
            break
        }
               
               
        Write-Host "[INFO] Not all instances are ready. Please wait ..."
        Start-Sleep -Seconds 180
        $CheckCount++
     }
     Write-Host "[INFO] The deployment for $service is ready now!"
}

Function Enable-CloudServiceRDP
{
    param (
        [string]$rdpUsername,
        [string]$rdpPassword,
        [string]$serviceName,
        [string]$slot,
        [string]$cert
    )

    $rdp = Get-AzureServiceRemoteDesktopExtension -Slot $slot -ServiceName $serviceName -ErrorAction SilentlyContinue
    if($rdp -eq $null)
    {
        $securepassword =  ConvertTo-SecureString -String $rdpPassword -AsPlainText -Force -ErrorAction Stop
        $credential =New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $rdpUsername,$securepassword -ErrorAction Stop
        $CertificateThumbprint = "$cert"
        $today = Get-Date
        $expiry = $today.AddDays(90).ToString("yyyy-MM-dd")
        $rdp = Set-AzureServiceRemoteDesktopExtension -Credential $credential -ServiceName $serviceName -CertificateThumbprint $CertificateThumbprint -Expiration $expiry
        if($rdp -eq $null)
        {
            Write-Host "Failed to set remote desktop."
            exit 1
        }
    }

}

Function Create-AppServicePlan 
{
    param(
        [string]$Location,
        [string]$Name,
        [string]$ResourceGroupName,
        [string]$Tier,
        [string]$WorkerSize
    )

    $plan = Get-AzureRmAppServicePlan -ResourceGroupName $ResourceGroupName -Name $Name -ErrorAction SilentlyContinue
    if($plan -eq $null) {
        $plan = New-AzureRmAppServicePlan -Location $Location -Name $Name -ResourceGroupName $ResourceGroupName -Tier $Tier -WorkerSize $WorkerSize
        if($plan -eq $null) { 
            Write-Host "Service Plan: $Name - failed to create it."
            exit 1
        } else {
            Write-Host "Service Plan: $Name - successfully create it."
        }
    } else {
        Write-Host "Service Plan: $Name - exists."
    }
}

Function Create-AppService
{
    param(
        [string]$Location,
        [string]$Name,
        [string]$ResourceGroupName,
        [string]$AppServicePlanName
    )

    $appService = Get-AzureRmWebApp -ResourceGroupName $ResourceGroupName -Name $Name -ErrorAction SilentlyContinue
    if($appService -eq $null) {
        $appService = New-AzureRmWebApp -ResourceGroupName $ResourceGroupName -Name $Name -Location $Location -AppServicePlan $AppServicePlanName
         if($appService -eq $null) {
            Write-Host "App Service: $Name - failed to create it."
            exit 1
         } else {
            Write-Host "App Service: $Name - successfully create it."
         }
    } else {
        Write-Host "App Service: $Name - exists."
    }
}

Function Set-WebAppSettings
{
    param(
        [string]$Name,
        [string]$ResourceGroupName,
        $AppSettings = @{}
    )
    $WebApp = Get-AzureRmWebApp -ResourceGroupName $ResourceGroupName -Name $Name -ErrorAction SilentlyContinue
    if($WebApp -eq $null) {
        Write-Host "Web App: $Name doesn't exist."
        exit 1
    } else {
        $appSettingList = $WebApp.SiteConfig.AppSettings
        ForEach ($kvp in $appSettingList) {
            $AppSettings[$kvp.Name] = $kvp.Value
        }
        $webApp = Set-AzureRmWebApp -Name $Name -ResourceGroupName $ResourceGroupName -AppSettings $AppSettings
        if($WebApp -eq $null)
        {
            Write-Host "Web App: $Name - failed to set app settings."
            exit 1
        } else {
            Write-Host "Web App: $Name - successfully set app settings."
        }
    }
}

Function Upload-CertificateToWebApp
{
    param(
    [string]$ResourceGroupName = '',
    [string]$WebAppName = '',
    [string]$CertPath = '',
    [string]$CertPassword = ''
    )

    $sPassword = ConvertTo-SecureString -String $CertPassword -AsPlainText -Force
    $cData = Get-PfxData -FilePath $CertPath  -Password $sPassword
    $thumbprint = $cData.EndEntityCertificates.Thumbprint

    $WebApp = Get-AzureRmWebApp -ResourceGroupName $ResourceGroupName -Name $WebAppName
    if($WebApp -eq $null)
    {
        Write-Host "Web App: $WebAppName doesn't exist."
        exit 1
    }

    $hostname = $WebApp.DefaultHostName
    New-AzureRmWebAppSSLBinding -ResourceGroupName $ResourceGroupName -WebAppName $WebAppName -CertificateFilePath $CertPath -CertificatePassword $CertPassword -Name $hostname -ErrorAction SilentlyContinue
  }

Function Upload-CertificateToService
{
    param(
        [string]$ServiceName,
        [string]$CertPath,
        [string]$CertPassword,
        [string]$ThumbprintAlgorithm #SHA1
    )
    $sPassword = ConvertTo-SecureString -String $CertPassword -AsPlainText -Force
    $cData = Get-PfxData -FilePath $CertPath  -Password $sPassword
    $thumbprint = $cData.EndEntityCertificates.Thumbprint

    $cert = Get-AzureCertificate -ServiceName $ServiceName -Thumbprint $thumbprint -ThumbprintAlgorithm $ThumbprintAlgorithm -ErrorAction SilentlyContinue
    if($cert -eq $null)
    {
        $cert = Add-AzureCertificate -ServiceName $ServiceName -CertToDeploy $CertPath -Password $CertPassword
        if($cert.OperationStatus -ne "Succeeded")
        {
            Write-Host "Azure Certificate: $thumbprint - failed to create it."
            exit 1
        } else {
            Write-Host "Azure Certificate: $thumbprint - successfully create it."
        }
    } else {
        Write-Host "Azure Certificate: $thumbprint exists."
    }
}

Function Create-CloudService
{
    param(
        [string]$ServiceName,
        [string]$Location
    )

    $service = Get-AzureService -ServiceName $ServiceName -ErrorAction SilentlyContinue
    if($service -eq $null)
    {
        $service = New-AzureService -ServiceName $ServiceName -Location $Location -ErrorAction SilentlyContinue
        if($service -eq $null)
        {
            Write-Host "Cloud Service: $ServiceName - failed to create it."
            exit 1
        } else {
            Write-Host "Cloud Service: $ServiceName - successfully create it."
        }
    } else {
        Write-Host "Cloud Service: $ServiceName exists."
    }
}

Export-ModuleMember -Function Create-CloudService
Export-ModuleMember -Function Upload-CertificateToService
Export-ModuleMember -Function Upload-CertificateToWebApp
Export-ModuleMember -Function Set-WebAppSettings
Export-ModuleMember -Function Create-AppService
Export-ModuleMember -Function Create-AppServicePlan
Export-ModuleMember -Function Set-AzureRMSubsriptionInfo
Export-ModuleMember -Function Set-AzureSMSubsriptionInfo
Export-ModuleMember -Function Create-ResourceGroup
Export-ModuleMember -Function Create-SQLServer
Export-ModuleMember -Function Create-FirewallRule
Export-ModuleMember -Function Create-SQLDatabase
Export-ModuleMember -Function Create-SQLElasticPool 
Export-ModuleMember -Function Create-SMServiceBus 
Export-ModuleMember -Function Create-RMServiceBus 
Export-ModuleMember -Function Create-ClassicStorage
Export-ModuleMember -Function Create-BlobStorage
Export-ModuleMember -Function Create-Container
Export-ModuleMember -Function Get-ContianerXri
Export-ModuleMember -Function Create-WebApp
Export-ModuleMember -Function Update-WebApp
Export-ModuleMember -Function Update-CloudService
Export-ModuleMember -Function Check-CloudServiceInstanceStatus
Export-ModuleMember -Function Get-SQLConnectionString
Export-ModuleMember -Function Get-SMServiceBusConnectionString
Export-ModuleMember -Function Get-RMServiceBusConnectionString
Export-ModuleMember -Function Get-AzureBlobStorageKey
Export-ModuleMember -Function Get-AzureBlobStorageConnectionString
Export-ModuleMember -Function Enable-CloudServiceRDP