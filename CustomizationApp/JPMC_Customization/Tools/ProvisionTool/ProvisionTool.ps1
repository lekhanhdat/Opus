# /********************************************************************
# *
# *  PROPRIETARY and CONFIDENTIAL
# *
# *  This file is licensed from, and is a trade secret of:
# *
# *                   AvePoint, Inc.
# *                   525 Washington Blvd, Suite 1400
# *                   Jersey City, NJ 07310
# *                   United States of America
# *                   Telephone: +1-201-793-1111
# *                   WWW: www.avepoint.com
# *
# *  Refer to your License Agreement for restrictions on use,
# *  duplication, or disclosure.
# *
# *  RESTRICTED RIGHTS LEGEND
# *
# *  Use, duplication, or disclosure by the Government is
# *  subject to restrictions as set forth in subdivision
# *  (c)(1)(ii) of the Rights in Technical Data and Computer
# *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
# *  FAR 52.227-19 (C) (June 1987).
# *
# *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
# *
# *  Unpublished - All rights reserved under the copyright laws of the United States.
# */
  Param(
    [String] $Type,
    [String] $SPAdminCenterUrl,
	[String] $WebUrl,
    [String] $PropertyName,
    [String] $PropertyValue,
    [String] $FieldName,
    [String] $Thumbprint,  # you must upload the certificate to the Azure AD Custom App and install on the local machine used to execute this powershell script.
    [String] $ConfigFilePath,
    [String] $FilePath,
    [String] $AOSClientSecret,
    [String] $WebhookRegisterBaseURL = "https://graph-us.avepointonlineservices.com/records",
    [String] $WebhookNotificationBaseURL = "https://usrecocsdapp.avepointonlineservices.com/",
    [String] $AosIdentityServiceAddress = "https://identity.avepointonlineservices.com",
    [Guid] $ClientSideComponentId,
    [Guid] $ClientID,
    [Guid] $TenantID,
    [Guid] $AOSClientID,
    [Guid] $ListID
)

$script:SystemList = "FormServerTemplates", "SiteAssets", "SiteCollectionDocuments", "Style Library", "HoldReports", "Reporting Templates",
"Websiteobjekte", "Formatbibliothek", "Formularvorlagen", "Dokumente der Websitesammlung",
"Pièces jointes", "Documents de la collection de sites", "Rapports de suspension", "Modèles de formulaire", "Bibliothèque de styles",
"フォーム テンプレート", "サイトのリソース ファイル", "サイト コレクションのドキュメント", "スタイル ライブラリ", "保留リスト レポート";
$script:UseWebLogin=$false
$currentTime=Get-Date -Format 'yyyy-MM-dd_HH-mm-ss';
$LogDir="Logs";
$logName=$LogDir+"\ProvisionTool_"+$currentTime+".log";
$reportName=$LogDir+"\ProvisionTool_"+$currentTime+"_FailedReport.csv";
$currentPath = Split-Path -Parent $MyInvocation.MyCommand.Definition;
$script:FailedReportFileCreated=$false;
$script:HasFailedItems = $false;

$script:UserName=$null;
$script:Password=$null;
$script:Cert=$null;

$script:AosTokenEndpoint=$null;
$script:OpusApiToken=$null;
$script:OpusApiTokenExpired=[DateTime]::UtcNow;

#URL CSV
$script:CSV_Column_SC = "Site Collection";
$script:CSV_Column_Site = "Site";
$script:CSV_Column_List = "Library";


enum MessageLevel {
	None = 0;
	Info = 1;
	Warning = 2;
	Error = 3;
}

enum ContainerLevel {
	Unknown = 0
	SiteCollection = 1
	Site = 2
	List = 3
}

function CreateLogPath()
{
	if(!(Test-Path $script:LogDir))
	{
		md $script:LogDir | Out-Null
	}
}

function ReportFailedNode($node, $reason) {
    if(!$script:FailedReportFileCreated) {
        "Site Collection,Site,Library,Reason" | Out-File -FilePath $reportName -Encoding UTF8;
        $script:FailedReportFileCreated = $true;
    }
    
    $scPart = $node.$script:CSV_Column_SC;
    $sitePart = $node.$script:CSV_Column_Site;
    $listPart = $node.$script:CSV_Column_List;
	
    $lineObj = [PSCustomObject]@{
        "Site Collection" = $scPart
        Site = $sitePart
        Library = $listPart
        Reason = $reason
    }

    $lineObj | Export-Csv -NoTypeInformation -Append -Path $reportName -Encoding UTF8
    $nodeFullUrl = GetContainerFullUrl -node $node;
    OutputToHostAndLog "Process container [$nodeFullUrl] failed, reason: $reason" "Error";
}

function OutputToHostAndLog($msg, [MessageLevel] $msgLevel = [MessageLevel]::Info)
{
    $color = "White"
	switch ($msgLevel) {
		([MessageLevel]::Info.ToString()) { $color = "White"; break; }
		([MessageLevel]::Warning.ToString()) { $color = "Yellow"; break; }
		([MessageLevel]::Error.ToString()) { $color = "Red"; break; }
		Default { $color = "White"; break; }
	}
	Write-Host -Object $msg -ForegroundColor $color;
	OutputToLog $msg $msgLevel;
}

function OutputToLog($msg, [MessageLevel] $msgLevel = [MessageLevel]::Info) {
	$dateTime = Get-Date -Format 'yyyy-MM-dd HH:mm:ss';
	$message = "$($msgLevel.ToString()) $($dateTime) $($msg)";
	$message | Out-File -Append -filepath $logName -Encoding UTF8;
}

function LoadDll($currentPath)
{
	$binDir= $currentPath+"\ThirdDlls\";
	[System.Reflection.Assembly]::UnsafeLoadFrom($binDir +"Microsoft.SharePoint.Client.dll") | Out-Null;
	[System.Reflection.Assembly]::UnsafeLoadFrom($binDir +"Microsoft.SharePoint.Client.Runtime.dll") | Out-Null;
	[System.Reflection.Assembly]::UnsafeLoadFrom($binDir +"Microsoft.SharePoint.Client.Taxonomy.dll") | Out-Null;
    [System.Reflection.Assembly]::UnsafeLoadFrom($binDir +"OfficeDevPnP.Core.dll") | Out-Null;
	[System.Reflection.Assembly]::UnsafeLoadFrom($binDir +"Microsoft.IdentityModel.Clients.ActiveDirectory.dll") | Out-Null;
    [System.Reflection.Assembly]::UnsafeLoadFrom($binDir +"Microsoft.Online.SharePoint.Client.Tenant.dll") | Out-Null;
}

function ExecuteQuery($ctx, [ScriptBlock] $loadData)
{
	$retryCount=2;
    $currentCount=0;
    $specialExceptionCount=0;
	do
	{
		try
		{
            if($loadData) {
                $loadData.Invoke();
            }
			$ctx.ExecuteQuery();
			return;
		}
		#catch [Net.WebException]
		catch
		{
            #if($_.Exception.Response.StatusCode -eq [Net.HttpStatusCode]::Forbidden)
            #{
            #    $specialExceptionCount++;
            #    if($specialExceptionCount -gt 3)
            #    {
            #        throw "The specified user does not have sufficient permissions to perform the action.";
            #    }
            #}
			if($_.Exception.Message.Contains("(403) Forbidden"))
            {
                $specialExceptionCount++;
                if($specialExceptionCount -gt 3)
                {
					OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Error";
                    throw "The specified user does not have sufficient permissions to perform the action.";
                }
            }
            $currentCount++;
			if($currentCount -ge $retryCount)
			{
				throw;
			}
			OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Warning";
            OutputToLog "Start to sleep 5 seconds. Retry count:[$currentCount]" "Warning";
			Start-Sleep –s 1
		}
	}
	while($currentCount -lt $retryCount)
}

function GetCert() {
	if ($null -ne $script:Cert) {
		OutputToLog "Get the certificate file from cache."
		return $script:Cert
	}
	else {
		OutputToLog "Get the certificate file from current server."
		$cert = Get-ChildItem -Path Cert:\CurrentUser\My -Recurse | Where-Object { $_.Thumbprint -eq $Thumbprint }
		if ($null -eq $cert) {
			$cert = Get-ChildItem -Path Cert:\LocalMachine\My -Recurse | Where-Object { $_.Thumbprint -eq $Thumbprint }
			if ($null -eq $cert) {
				throw "Cannot find the certificate file (.pfx) on the current server."
			}
		}
		$script:Cert = $cert;
		return $cert;
	}
}

function InitContext($url) {
	if ($script:UseWebLogin) {
		$authManager = New-Object OfficeDevPnP.Core.AuthenticationManager;
		$ctx = $authManager.GetWebLoginClientContext($url);    
	}
	else {
		$cert = GetCert;
		$authManager = New-Object OfficeDevPnP.Core.AuthenticationManager;
		$ctx = $authManager.GetAzureADAppOnlyAuthenticatedContext($url, $ClientID, $TenantID, $cert);
	}
	if ($null -eq $ctx) {
		throw "Init context failed."
	}
	return $ctx;
}

function IsUrlAccessible($url)
{
	OutputToLog "Checking url:[$url]"
    try
    {
        $site= Invoke-WebRequest $url -Method Head -UseBasicParsing;
        if($site.StatusCode -eq 200)
        {
            return $true;
        }
    }
    catch
    {
        OutputToLog $_.Exception.ToString();
    }
	OutputToLog "Finish checking url:[$url]"
    return $false;
}

function Retry([ScriptBlock]$action)
{
    $retryCount=1;
    $currentCount=0;
	$specialExceptionCount=0;
    do
    {
        try
        {
            & $action;
            break;
        }
        catch
        {
			if($_.Exception.Message.Contains("(403) Forbidden"))
            {
                $specialExceptionCount++;
                if($specialExceptionCount -gt 3)
                {
					OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace);
                    throw "The specified user does not have sufficient permissions to perform the action.";
                }
            }
			$currentCount++;
			if($currentCount -ge $retryCount)
			{
				throw;
			}
			OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace)  "Error";
			OutputToHostAndLog "Sleep 5 seconds. Retry count:[$currentCount]" "Error";
			Start-Sleep –s 5
        }
    } while ($currentCount -lt $retryCount)
}

function ExecuteCommand()
{
	if ($Type -eq 'BatchUpdateSiteProperty') {
		BatchUpdateSiteProperty
		return;
	}
	
	try
	{
		
		$url = $WebUrl.TrimStart().TrimEnd();
		if (!(IsUrlAccessible $url)) {
			throw "Failed to connect to the site collection. Make sure the entered URL is correct."
		}

		$ctx = InitContext $url
        if ($Type -eq 'UpdateSiteProperty') {
            Retry { UpdateSiteProperty $ctx }
        } 
        elseif ($Type -eq 'UpdateFieldClientSideComponentId') {
            Retry { UpdateFieldClientSideComponentId $ctx }
        }
        elseif ($Type -eq 'UpdateColumnToReadOnly') {
            Retry { UpdateColumnToReadOnly $ctx }
        }
        elseif ($Type -eq 'DisableParserForLibrary') {
            Retry { DisableParserForLibrary $ctx }
        }
	}
	catch
	{
		OutputToHostAndLog $_.Exception.Message "Error"
		OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Error";
	}
    finally
    {
        if($ctx -ne $null)
        {
            $ctx.Dispose();
        }
    }
}

function BatchUpdateSiteProperty() {
	$urls = Get-Content $FilePath;
	$total = $urls.Length;
	OutputToHostAndLog "There are [$total] Site Collection URLs in the input file."
	foreach ($url in $urls) {
		try {
			$ctx = InitContext $url
			UpdateSiteProperty $ctx
		}
		catch
		{
			OutputToHostAndLog $_.Exception.Message "Error"
			OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Error";
		}
		finally
		{
			if($ctx -ne $null)
			{
				$ctx.Dispose();
			}
		}
	}
}

function UpdateSiteProperty($ctx)
{
    OutputToHostAndLog "Start set property value of [$PropertyName] to [$PropertyValue]"

    $caUrl = $SPAdminCenterUrl.TrimStart().TrimEnd();
    if (!(IsUrlAccessible $caUrl)) {
        throw "Failed to connect to the SP Admin Center. Make sure the entered SP Admin Center URL is correct."
    }
    $caCtx = InitContext $caUrl

    try
	{
        $loadSiteBlock = {
            $ctx.Load($ctx.Site);
            $ctx.Load($ctx.Site.RootWeb.AllProperties);
        }
        ExecuteQuery $ctx $loadSiteBlock;
    
        $oldPropValue = $ctx.Site.RootWeb.AllProperties[$PropertyName];
        OutputToHostAndLog "Old property value of [$PropertyName] is [$oldPropValue]"
    
        $tenant = New-Object Microsoft.Online.SharePoint.TenantAdministration.Tenant($caCtx)
        $siteProperties = $tenant.GetSitePropertiesByUrl($ctx.Site.Url, $true)
        
        $loadSiteBlock = {
            $caCtx.Load($siteProperties)
        }
        ExecuteQuery $caCtx $loadSiteBlock;
        
        $needResetDenyCustomizePagesStatus = $false;
        
        if($siteProperties.AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled -ne $true)
        {
            OutputToHostAndLog "Set AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled to true."
            $needResetDenyCustomizePagesStatus = $true;
            $siteProperties.AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled = $true;
            $siteProperties.Update()
            ExecuteQuery $caCtx
        }
    
        $ctx.Site.RootWeb.AllProperties[$PropertyName] = $PropertyValue;
        $ctx.Site.RootWeb.Update();
        ExecuteQuery $ctx;
    
        if($needResetDenyCustomizePagesStatus -eq $true) 
        {
            $siteProperties.AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled = $false;
            $siteProperties.Update()
            ExecuteQuery $caCtx;
            OutputToHostAndLog "Reset AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled to false."
        }
    
        OutputToHostAndLog "Successfully update site property."
	}
    finally
    {
        if($caCtx -ne $null)
        {
            $caCtx.Dispose();
        }
    }
}

function UpdateFieldClientSideComponentId($ctx)
{
    OutputToHostAndLog "Start update ClientSideComponentId of field [$FieldName] to [$ClientSideComponentId]"

	$fields = $ctx.Web.Fields
	$ctx.Load($fields)
	$ctx.ExecuteQuery()

    $field = $fields | ? { $_.InternalName -eq $FieldName }

    if (!$field) {
        OutputToHostAndLog "Failed to update. There is not exists a field that named [$FieldName]." "Error"
    }
	elseif ($field.ClientSideComponentId -ne $ClientSideComponentId) {
		$field.ClientSideComponentId = $ClientSideComponentId
		$field.UpdateAndPushChanges($true)
		$ctx.ExecuteQuery()
		OutputToHostAndLog "Successfully update ClientSideComponentId."
	}
    else {
        OutputToHostAndLog "ClientSideComponentId no change." "Warning"
    }
}

function UpdateColumnToReadOnly($ctx)
{
    OutputToHostAndLog "Start update field [$FieldName] to read-only."

	$fields = $ctx.Web.Fields
	$ctx.Load($fields)
	$ctx.ExecuteQuery()

    $field = $fields | ? { $_.InternalName -eq $FieldName }

	if (!$field) {
        OutputToHostAndLog "Failed to update. There is not exists a field that named [$FieldName]." "Error"
    }
	elseif ($field.ReadOnlyField -ne $true) {
		$field.ReadOnlyField = $true
		$field.UpdateAndPushChanges($true)
		$ctx.ExecuteQuery()
		OutputToLog "Field successfully update to read-only."
	}
    else {
        OutputToLog "Field already read-only." "Warning"
    }
}

function DisableParserForLibrary($ctx)
{
    OutputToHostAndLog "Start disable parser for library [$ListID]."

	$list = $ctx.Web.Lists.GetById($ListID)
	$ctx.Load($list)
	$ctx.ExecuteQuery()

	if ($list.ParserDisabled -eq $true) {
        OutputToHostAndLog "Parser is already disabled for library [$ListID]." "Warning"
        return;
    }
	
    $list.ParserDisabled = $true
    $list.Update()
    $ctx.ExecuteQuery()
    OutputToHostAndLog "Parser successfully disabled for library [$ListID]."
}

function RegisterWebhooks()
{
	$nodes = Import-Csv -Path $ConfigFilePath;
	foreach ($node in $nodes) {
		try {
			ProcessContainer $node
		}
		catch {
			OutputToHostAndLog "$($_.Exception.ToString())`n$($_.ScriptStackTrace)" "Error"
		}
	}
}

function ProcessContainer($node) {
	if (!(ValidateContainer -node $node)) {
		return
	}

	$nodeFullUrl = GetContainerFullUrl -node $node;
	OutputToHostAndLog "Processing container [$nodeFullUrl]"
    $script:HasFailedItems = $false;

	[ContainerLevel] $containerLevel = GetContainerLevel $node
	switch ($containerLevel) {
		([ContainerLevel]::SiteCollection.ToString()) { ProcessSCNode -node $node; break; }
		([ContainerLevel]::Site.ToString()) { ProcessSiteNode -node $node; break; }
		([ContainerLevel]::List.ToString()) { ProcessListNode -node $node ; break; }
	}
}

function ProcessSCNode($node) {
    try
	{
        $scUrl = $node.$script:CSV_Column_SC.TrimStart().TrimEnd().TrimEnd("/");
		if (!(IsUrlAccessible $scUrl)) {
			ReportFailedNode $node "Failed to connect to the site since the URL [$scUrl] is incorrect. Make sure the URL is correct."
            return;
		}

        $scUrl = $node.$script:CSV_Column_SC.TrimStart().TrimEnd().TrimEnd("/");
        $ctx = InitContext $scUrl
        $web = $ctx.Site.RootWeb
        $ctx.Load($web);
        ProcessSPSite $ctx $web;

        if($script:HasFailedItems) {
            ReportFailedNode $node "Failed to process some lists of the site collection [$scUrl]."
        }
	}
	catch
	{
        ReportFailedNode $node $_.Exception.Message
		OutputToHostAndLog $_.Exception.Message "Error"
		OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Error";
	}
    finally
    {
        if($ctx -ne $null)
        {
            $ctx.Dispose();
        }
    }
}

function ProcessSiteNode($node) {
	try {
		$siteUrl = GetSiteUrl -node $node;
		if (!(IsUrlAccessible $siteUrl)) {
			ReportFailedNode $node "Failed to connect to the site since the URL [$siteUrl] is incorrect. Make sure the URL is correct."
            return;
		}
		$ctx = InitContext $siteUrl
        $web = $ctx.Web
        $ctx.Load($web);
        ProcessSPSite $ctx $web

        if($script:HasFailedItems) {
            ReportFailedNode $node "Failed to process the web [$siteUrl]."
        }
	}
    catch
	{
        ReportFailedNode $node $_.Exception.Message
		OutputToHostAndLog $_.Exception.Message "Error"
		OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Error";
	}
	finally {
		if($ctx -ne $null)
        {
            $ctx.Dispose();
        }
	}
}


function ProcessListNode($node) {
	try {
		$siteUrl = GetSiteUrl -node $node;
		if (!(IsUrlAccessible $siteUrl)) {
			ReportFailedNode $node "Failed to connect to the site since the URL [$siteUrl] is incorrect. Make sure the URL is correct."
            return;
		}
		$ctx = InitContext $siteUrl
        $listFullUrl = GetListFullUrl $node;
        $listRelativeUrl = GetListRelativeUrl $listFullUrl;
        $list = $ctx.Web.GetList($listRelativeUrl);

        $loadListBlock = {
            $ctx.Load($list);
        }
        ExecuteQuery $ctx $loadListBlock;

		ProcessSPList $ctx $list $siteUrl

        if($script:HasFailedItems) {
            ReportFailedNode $node "Failed to process the list [$listFullUrl]."
        }
	}
    catch
	{
        ReportFailedNode $node $_.Exception.Message
		OutputToHostAndLog $_.Exception.Message "Error"
		OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Error";
	}
	finally {
		if($ctx -ne $null)
        {
            $ctx.Dispose();
        }
	}
}

function ProcessSPSite($ctx, $web) {
	try {
        $lists = $web.Lists;
        $loadListsBlock = {
            $ctx.Load($lists);
        }
        ExecuteQuery $ctx $loadListsBlock;
        
        $webUrl = $web.Url;
        OutputToHostAndLog "Processing site [$webUrl]";
        foreach ($list in $lists) {
            ProcessSPList $ctx $list $webUrl
        }

        #check subwebs
        $subWebs = $web.Webs;
        $loadSubWebsBlock = {
            $ctx.Load($subWebs);
        }
        ExecuteQuery $ctx $loadSubWebsBlock;
        foreach ($subWeb in $subWebs) {
            ProcessSPSite $ctx $subWeb;
        }
	}
	catch
	{
		OutputToHostAndLog $_.Exception.Message "Error"
		OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Error";
        $script:HasFailedItems = $true;
	}
}

function ProcessSPList($ctx, $list, $webUrl) {
    try {
        if(!$list.BaseTemplate) {
            OutputToHostAndLog "Failed to load the list" "Error"
            $script:HasFailedItems = $true;
            return;
        }

        if ($list.BaseTemplate -ne [Convert]::ToInt16([Microsoft.SharePoint.Client.ListTemplateType]::DocumentLibrary)) {
            OutputToHostAndLog "[$($list.Title)] is not a document library."
            return;
        }
        
        $loadRootFolderBlock = {
            $ctx.Load($list.RootFolder);
        }
        ExecuteQuery $ctx $loadRootFolderBlock;
        if ($list.RootFolder.Name -and $script:SystemList.Contains($list.RootFolder.Name)) {
            OutputToHostAndLog "Skip system list: [$($list.Title)]"
            return;
        }

        OutputToHostAndLog "Processing list [$($list.RootFolder.ServerRelativeUrl)]"
        if (ExistsCustomWebhookSubscription $ctx $list.ID $webUrl) {
            return;
        }
        else {
            OutputToHostAndLog "Register webhook for list [$($list.RootFolder.ServerRelativeUrl)]"

            $listInfo = @{
                'TenantID' = $TenantID
                'WebUrl' = $webUrl
                'ListID' = $list.ID
            }
            if(!(InnerRegisterWebHook $listInfo)) {
                OutputToHostAndLog "Register webhook for list failed" "Error"
                $script:HasFailedItems = $true;
            }
        }
	}
	catch
	{
		OutputToHostAndLog $_.Exception.Message "Error"
		OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Error";
        $script:HasFailedItems = $true;
	}
	
}

function ExistsCustomWebhookSubscription($ctx, $listId, $webUrl) {
    $result = $false;
	try {
        $restUrl = "$webUrl/_api/web/lists(guid'$listId')/subscriptions"
        $token = GetSPOAccessToken($ctx);
		$response = Invoke-WebRequest $restUrl -UseBasicParsing -Method GET -Headers @{"Authorization" = "Bearer $token"};
        $content = [xml]$response.Content
        $subscriptions = $content.feed.entry
        if(!$subscriptions) {
            return $result;
        }
        $subscriptions | ForEach-Object {
            $notificationUrl = $_.content.properties.notificationUrl;
            $expiredTime = $_.content.properties.expirationDateTime;
            if($_.content.properties.notificationUrl.StartsWith($WebhookNotificationBaseURL)) {
                OutputToHostAndLog "Already register webhook: [$($notificationUrl)], ExpirationDateTime: [$($expiredTime.InnerText)]"
                $result = $true;
                return;
            }
        }
	}
	catch {
        OutputToHostAndLog $_.Exception.Message "Error"
		OutputToLog ($_.Exception.ToString() + "`n" + $_.ScriptStackTrace) "Error";
	}
	return $result;
}

function GetSPOAccessToken($ctx) {
    return [Microsoft.SharePoint.Client.ClientContextExtensions]::GetAccessToken($ctx);
}

function GetOpusPublicApiToken () {
    try {
        if($script:OpusApiToken -and $script:OpusApiTokenExpired -gt [DateTime]::UtcNow.AddMinutes(5).Ticks) {
            return $script:OpusApiToken;
        }

        if(!$script:AosTokenEndpoint) {
            # Discover token endpoint from metadata
            $disco = Invoke-WebRequest -Uri "$AosIdentityServiceAddress/.well-known/openid-configuration"
            if ($disco.StatusCode -ne 200) {
                OutputToHostAndLog "Error discovering endpoint: $($disco.StatusCode)" "Error"
                OutputToLog ($disco.Content | ConvertFrom-Json) "Error"
                return ''
            }

            $discoData = $disco.Content | ConvertFrom-Json
            $script:AosTokenEndpoint = $discoData.token_endpoint
        }
        
        # Make request for client credentials token
        $authData = @{
            "client_id" = $AOSClientID
            "client_secret" = $AOSClientSecret
            "grant_type" = "client_credentials"
            "scope" = "records.readwrite.all"
        }

        $tokenResponse = Invoke-WebRequest -Method Post -Uri $script:AosTokenEndpoint -ContentType "application/x-www-form-urlencoded" -Body $authData
        if ($tokenResponse.StatusCode -ne 200) {
            OutputToHostAndLog "Failed getting Opus API token: $($tokenResponse.StatusCode)" "Error"
            OutputToLog ($tokenResponse.Content | ConvertFrom-Json) "Error"
            return ''
        }

        # Extract the access token
        $tokenData = $tokenResponse.Content | ConvertFrom-Json
        $script:OpusApiTokenExpired = $tokenData.expires_in
        $script:OpusApiToken = $tokenData.access_token
        return $script:OpusApiToken
    } catch {
        OutputToHostAndLog $_.Exception.Message "Error"
		OutputToLog ($_.Exception.ToString() + "`n" + $_.ScriptStackTrace) "Error";
        return ''
    }
}

function InnerRegisterWebHook ($payload) {
    $apiUrl = "$WebhookRegisterBaseURL/api/provision/RegisterWebhook";
    $token = GetOpusPublicApiToken
    try {
        # Prepare the headers with the token for authorization
        $headers = @{
            'Authorization' = "Bearer $token"
            'Content-Type' = 'application/json'
        }

        $jsonPayload = $payload | ConvertTo-Json -Depth 10

        # Make a POST request to the API endpoint
        $response = Invoke-WebRequest -Method Post -Uri $apiUrl -Headers $headers -Body $jsonPayload -ContentType 'application/json'

        if ($response.StatusCode -eq 200) {
            return ($response.Content | ConvertFrom-Json)
        } else {
            OutputToHostAndLog "Error calling API: $($response.StatusCode)" "Error"
            OutputToLog ($response.Content | ConvertFrom-Json) "Error"
            return $null
        }
    } catch {
        OutputToHostAndLog $_.Exception.Message "Error"
		OutputToLog ($_.Exception.ToString() + "`n" + $_.ScriptStackTrace) "Error";
        return $null
    }
}

function ValidateContainer($node) {
	[ContainerLevel] $containerLevel = GetContainerLevel -node $node
	if ($containerLevel -eq [ContainerLevel]::Unknown) {
		ReportFailedNode $node "The path is invalid."
		return $false
	}

	$scUrl = $node.$script:CSV_Column_SC.TrimStart().TrimEnd().TrimEnd("/");
	if (!(IsUrlAccessible $scUrl)) {
		ReportFailedNode $node "Failed to connect to the site collection since the URL [$scUrl] is incorrect. Make sure the URL is correct."
		return $false
	}
	
	return $true
}

function GetSiteUrl($node) {
	$scUrl = $node.$script:CSV_Column_SC.Trim().TrimEnd("/");
    $sitePart = $node.$script:CSV_Column_Site.Trim().Trim("/").Trim();
    $siteUrl = $scUrl;
	if ($sitePart -and ($sitePart -ne ".")) {
        $siteUrl += "/" + $sitePart;
	}
	return $siteUrl;
}

function GetListFullUrl($node) {
	$siteUrl = GetSiteUrl $node;
    $listPart = $node.$script:CSV_Column_List.Trim().Trim("/").Trim();
	if ($node.$script:CSV_Column_List.StartsWith("/")) {
		return ($siteUrl += $listPart)
	}
	else {
		return ($siteUrl += "/" + $listPart)
	}
}

function GetListRelativeUrl($listFullUrl) {
    $uri = [System.Uri] $listFullUrl
    return $uri.LocalPath.TrimEnd('/');
}

function GetContainerFullUrl($node) {
	$containerLevel = GetContainerLevel $node
	switch ($containerLevel) {
		([ContainerLevel]::SiteCollection.ToString()) { return $node.$script:CSV_Column_SC.TrimStart().TrimEnd().TrimEnd("/") }
		([ContainerLevel]::Site.ToString()) { return (GetSiteUrl -node $node) }
		([ContainerLevel]::List.ToString()) { return (GetListFullUrl -node $node) }
	}
}

function GetContainerLevel($node) {
    $noSCPart = [String]::IsNullOrEmpty($node.$script:CSV_Column_SC);
    $noSitePart = [String]::IsNullOrEmpty($node.$script:CSV_Column_Site);
    $noListPart = [String]::IsNullOrEmpty($node.$script:CSV_Column_List);
	if ($noSCPart) {
		return [ContainerLevel]::Unknown;
	}
	elseif ($noSitePart -and $noListPart) {
		return [ContainerLevel]::SiteCollection;
	}
	elseif ($noListPart) {
		return [ContainerLevel]::Site;
	}
	else {
		return [ContainerLevel]::List;
	}
}

function IsConfigFileExist() {
    if (!$ConfigFilePath) {
        CheckParameter;
        return;
    }
    if(!($ConfigFilePath.IndexOf(':') -gt 0)) {
        $ConfigFilePath = [System.IO.Path]::Combine($currentPath, $ConfigFilePath);
    }
	
	if (!(Test-Path $ConfigFilePath)) {
		OutputToHostAndLog "Cannot find the config CSV file by: [$ConfigFilePath]" -msgLevel "Warning"
		return $false;
	}
	return $true;
}

function CheckParameter()
{
	OutputToHostAndLog "Please check parameters." -msgLevel "Error";
	Write-Host 'Examples:'
    Write-Host '1. Update Site Property:'
    Write-Host '.\ProvisionTool.ps1 -Type "UpdateSiteProperty" -SPAdminCenterUrl "https://contoso-admin.sharepoint.com" -WebUrl "https://contoso.sharepoint.com/sites/targetsite" -PropertyName "SiteType" -PropertyValue "OBR"  -ClientID "Azure AD Application Client ID" -Thumbprint "Azure AD Application Certificate Thumbprint" -TenantID "Microsoft 365 Tenant ID"'
	Write-Host '2. Batch Update Site Property:'
    Write-Host '.\ProvisionTool.ps1 -Type "BatchUpdateSiteProperty" -SPAdminCenterUrl "https://contoso-admin.sharepoint.com" -FilePath "C:\SiteCollections.txt" -PropertyName "SiteType" -PropertyValue "OBR"  -ClientID "Azure AD Application Client ID" -Thumbprint "Azure AD Application Certificate Thumbprint" -TenantID "Microsoft 365 Tenant ID"'
    Write-Host '3. Update Field [ClientSideComponentId]:'
    Write-Host '.\ProvisionTool.ps1 -Type "UpdateFieldClientSideComponentId" -WebUrl "https://contoso.sharepoint.com/sites/targetsite" -FieldName "EndDate" -ClientSideComponentId "35ad4c90-eba1-49cc-8477-35b91df918e3" -ClientID "Azure AD Application Client ID" -Thumbprint "Azure AD Application Certificate Thumbprint" -TenantID "Microsoft 365 Tenant ID"'
    Write-Host '4. Update Column to read-only:'
    Write-Host '.\ProvisionTool.ps1 -Type "UpdateColumnToReadOnly" -WebUrl "https://contoso.sharepoint.com/sites/targetsite" -FieldName "EndDate" -ClientID "Azure AD Application Client ID" -Thumbprint "Azure AD Application Certificate Thumbprint" -TenantID "Microsoft 365 Tenant ID"'
    Write-Host '5. Register Webhook:'
    Write-Host '.\ProvisionTool.ps1 -Type "RegisterWebhook" -ConfigFilePath "Containers.csv" -ClientID "Azure AD Application Client ID" -Thumbprint "Azure AD Application Certificate Thumbprint" -TenantID "Microsoft 365 Tenant ID" -AOSClientID "AOS Application Client ID" -AOSClientSecret "AOS Client Secret"'
    Write-Host '6. Disable parser for Library:'
    Write-Host '.\ProvisionTool.ps1 -Type "DisableParserForLibrary" -WebUrl "https://contoso.sharepoint.com/sites/targetsite" -ListID "List ID" -ClientID "Azure AD Application Client ID" -Thumbprint "Azure AD Application Certificate Thumbprint" -TenantID "Microsoft 365 Tenant ID"'
}



function Startup()
{
    try
    {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Ssl3 -bor [Net.SecurityProtocolType]::Tls -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls12;
        CreateLogPath;
        OutputToHostAndLog "-----------------------Start-----------------------"
        $ErrorActionPreference="Stop";
        
        LoadDll $currentPath;

        if (!$Type -or !$ClientID -or !$TenantID -or !$Thumbprint) {
            CheckParameter;
			return;
        }
        elseif ($Type -eq 'RegisterWebhook') {
            if (!$AOSClientID -or !$AOSClientSecret) {
                CheckParameter;
                return;
            }
            if (!(IsConfigFileExist)) {
                return;
            }
            RegisterWebhooks;
            return;
		}
        elseif ($Type -eq 'BatchUpdateSiteProperty') {
            if (!$FilePath -or !$SPAdminCenterUrl -or !$PropertyName -or !$PropertyValue) {
                CheckParameter;
                return;
            }
        } 
        elseif (!$WebUrl) {
			CheckParameter;
			return;
        }
        elseif ($Type -eq 'UpdateSiteProperty') {
            if (!$SPAdminCenterUrl -or !$PropertyName -or !$PropertyValue) {
                CheckParameter;
                return;
            }
        } 
        elseif ($Type -eq 'UpdateFieldClientSideComponentId') {
            if (!$FieldName -or !$ClientSideComponentId) {
                CheckParameter;
                return;
            }
        }
        elseif ($Type -eq 'UpdateColumnToReadOnly') {
            if (!$FieldName) {
                CheckParameter;
                return;
            }
        }
        elseif ($Type -eq 'DisableParserForLibrary') {
            if (!$ListID) {
                CheckParameter;
                return;
            }
        }
        else {
            OutputToHostAndLog "Invalid command type: [$Type]" "Error"
            CheckParameter;
            return;
        }

        ExecuteCommand;
    }
    catch
    {
        OutputToHostAndLog $_.Exception.Message "Error";
        OutputToLog ($_.Exception.ToString() +"`n"+$_.ScriptStackTrace) "Error";
    }
    finally
    {
        OutputToHostAndLog "-----------------------End-----------------------"
    }
}

Startup;
