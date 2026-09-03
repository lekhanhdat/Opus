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

  param(
	[String] $WebUrl,
    [String] $ListId,
    [String] $FieldName,
    [Guid] $ClientSideComponentId,
    [Guid] $ClientID,
    [Guid] $TenantID,
    [String] $Thumbprint    # you must upload the certificate to the Azure AD Custom App and install on the local machine used to execute this powershell script.
)

Class Logger {
    hidden static [String]$LogDir = "Logs";
    hidden static [String] $LogPrefixName = "\SetFieldClientSideComponentId_";
    hidden static [String] $LogFilePath = "";

    static [void] Initialize($currentTime) {
        if(!(Test-Path ([Logger]::LogDir))) {
            mkdir ([Logger]::LogDir) | Out-Null;
        }
        [Logger]::LogFilePath = [Logger]::LogDir + [Logger]::LogPrefixName + $currentTime + ".log";
    }

    static [void] Info($message) {
        [Logger]::WriteToHost($message, "White");
        [Logger]::WriteToLogFile($message);
    }

    static [void] Warn($message) {
        [Logger]::WriteToHost($message, "Yellow");
        [Logger]::WriteToLogFile($message);
    }

    static [void] Error($message) {
        [Logger]::WriteToHost($message, "Red");
        [Logger]::WriteToLogFile($message);
    }

    static [void] Error($message, $exception) {
        $exceptionMessage = $exception.Exception.ToString() + "`n" + $exception.ScriptStackTrace;
        [Logger]::WriteToHost($message, "Red");
        [Logger]::WriteToLogFile($message);
        [Logger]::WriteToLogFile($exceptionMessage);
    }

    static [void] WriteToHost($message, $color) {
        Write-Host -Object $message -ForegroundColor $color;
    }

    static [void] WriteToLogFile($message) {
        $dateTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss";
        ($dateTime + " " + $message) | Out-File -Append -FilePath ([Logger]::LogFilePath);
    }
}

function ConnectPnPOnline($siteUrl) {
    $configreConnect = Connect-PnPOnline -Url $siteUrl -ClientId $ClientId -Tenant $TenantID -Thumbprint $Thumbprint;
    return $configreConnect;
}

function DisconnectPnPOnline($connect) {
    if (($null -ne $connect) -and ($null -ne $connect.Context)) {
        Disconnect-PnPOnline -Connection $connect	
    }
}

function CheckParameter() {
	[Logger]::Info("Please check parameters.");
	Write-Host 'Examples:'
	Write-Host '.\UpdateFieldClientSideComponentId.ps1 -WebUrl "https://contoso.sharepoint.com/sites/targetsite" -ListId "99a00f6e-fb81-4dc7-8eac-e09c6f9132fe" -FieldName "EndDate" -ClientSideComponentId "35ad4c90-eba1-49cc-8477-35b91df918e3" -ClientID "Azure AD Application Client ID" -Thumbprint "Azure AD Application Certificate Thumbprint" -TenantID "Microsoft 365 Tenant ID"';
}

function Startup() {
	try {
		$currentTime = Get-Date -Format "yyyy-MM-dd_HH-mm-ss";
		[Logger]::Initialize($currentTime);
		[Logger]::Info("-----------------------Start-----------------------");
		if (!$WebUrl -or !$ListId -or !$FieldName -or !$ClientSideComponentId -or !$ClientID -or !$TenantID -or !$Thumbprint) {
			CheckParameter;
			return;
		}
		Connect-PnPOnline -Url $WebUrl -ClientId $ClientId -Tenant $TenantID -Thumbprint $Thumbprint;

		$web = Get-PnPWeb
		$list = Get-PnPList -Identity $ListId
        #Get the Field from List
        $Field = Get-PnPField -List $list -Identity $FieldName -ErrorAction Stop
        #Set the Field ClientSideComponentId
        $Field.ClientSideComponentId = $ClientSideComponentId
        $Field.Update()
		Invoke-PnPQuery
        [Logger]::Info("-----------------------Success-----------------------");
	}
	catch {
		[Logger]::Error($_.Exception.Message, $_);
	}
	finally {
		[Logger]::Info("-----------------------End-----------------------");
	}
}

Startup;


