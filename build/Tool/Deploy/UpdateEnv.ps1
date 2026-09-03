Param( 
      [string]$AzureAccount="",
      [string]$AzurePassword="",
      [string]$PublishSettingsFilePath="",
      [string]$SubscriptionId="",
      [string]$label="",
      [string]$rdpCert="",
      [string]$rdpUsername="",
      [string]$rdpPassword="" ,
      [string]$azureModulePath="",
      [string]$name="",
      [string]$package="",
	  [string]$config="",
      [string]$slot="",
      [string]$StorageName=""
)

Import-Module "$azureModulePath"

# Login
Set-AzureSMSubsriptionInfo -PublishSettingsFilePath $PublishSettingsFilePath -SubscriptionId $SubscriptionId

Update-CloudService -SubscriptionId $SubscriptionId -service $name -slot $slot -StorageName $StorageName -packageURL $package -configURL $config -isStart "TRUE" -label $label

# Check instance status
Check-CloudServiceInstanceStatus -service $name -slot $slot -retryCount 15

Enable-CloudServiceRDP -rdpUsername $rdpUsername -rdpPassword $rdpPassword -serviceName $name -cert $rdpCert -slot Production
