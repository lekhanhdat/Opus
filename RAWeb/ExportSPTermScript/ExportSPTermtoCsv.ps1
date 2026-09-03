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
# Outputs CSV of the specified termset from the specificed termstore/group
# Example call:
# .\ExportSPTermtoCsv.ps1 "https://tenant-admin.sharepoint.com" "BusinessClassification" "Department"

 param
	(
		$centralAdminUrl = $null,
		$termGroupName=$null
	);

if(($centralAdminUrl -eq $null) -or ($termGroupName -eq $null))
{
    Write-Host "Please check parameters." -ForegroundColor Yellow;
	Write-Host "SYNTAX`t.\ExportSPTermtoCsv.ps1 CentralAdminUrl TermGroupName" -ForegroundColor Yellow;
	Write-Host "EXAMPLE`t.\ExportSPTermtoCsv.ps1 ""https://tenant-admin.sharepoint.com"" ""BusinessClassification"" " -ForegroundColor Yellow;
    return;
}

function LoadDll()
{
    $currentDir = get-location;
	$binDir=  $currentDir.Path + "\Office365\";
	[System.Reflection.Assembly]::LoadFrom($binDir +"Microsoft.SharePoint.Client.dll") | Out-Null;
	[System.Reflection.Assembly]::LoadFrom($binDir +"Microsoft.SharePoint.Client.Runtime.dll") | Out-Null;
	[System.Reflection.Assembly]::LoadFrom($binDir +"Microsoft.SharePoint.Client.Taxonomy.dll") | Out-Null;
    [System.Reflection.Assembly]::LoadFrom($binDir +"OfficeDevPnP.Core.dll") | Out-Null;
    [System.Reflection.Assembly]::LoadFrom($binDir +"Microsoft.IdentityModel.Clients.ActiveDirectory.dll") | Out-Null;
}
$Global:csvFileStream=$null;
$Global:csvStreamWriter = $null;
$Global:logFileStream=$null;
$Global:logStreamWriter=$null;
[int] $Global:succedCount=0;
[int] $Global:failedCount=0;
[int] $Global:retryCount=0;
[bool] $Global:hasError=$false;
$Global:csvName=$null;
$Global:logName=$null;
[string] $Global:prefix=$null;
$Global:context=$null;


function buildName()
{
    $currentDir = get-location;
    $reportDir=  $currentDir.Path + "\Reports";
    if(![System.IO.Directory]::Exists($reportDir))
    {
        $null = [System.IO.Directory]::CreateDirectory($reportDir)
    }
	$time=(Get-Date).tostring("yyyy.MM.dd_HH.mm.ss");
	$name=(Get-Location).Path.ToString() + [string]::Format("\Reports\Terms_{0}", $time);
	return $name;
}

function InitCsvFile()
{
	$Global:csvName=[string]::Format("{0}.csv", $Global:prefix);
	$Global:csvFileStream = New-Object System.IO.FileStream($csvName,[System.IO.FileMode]::OpenOrCreate,[System.IO.FileAccess]::ReadWrite);
	$Global:csvStreamWriter = New-Object system.IO.StreamWriter($csvFileStream,[System.Text.Encoding]::UTF8);
    WriteLog -msg "Init csv file successfully.";
}

function InitLogFile()
{
    $Global:logName=[string]::Format("{0}.log", $Global:prefix);
	$Global:logFileStream=New-Object System.IO.FileStream($logName,[System.IO.FileMode]::OpenOrCreate,[System.IO.FileAccess]::ReadWrite);
	$Global:logStreamWriter=New-Object system.IO.StreamWriter($logFileStream,[System.Text.Encoding]::UTF8);
	WriteLog -msg "Init log file successfully.";
}

function WriteLog($msg,$type=$null)
{
	if($type -eq $null)
	{
		$type="Info";
	}
	$curTime = (Get-Date).tostring("yyyy/MM/dd HH:mm:ss");
	$text=[string]::Format("{0}`t{1}`t{2}",$curTime,$type,$msg);
	$logStreamWriter.WriteLine($text);
    $logStreamWriter.Flush();
}

function Export-SPTerms()
{
    $site=$null;
	try
	{
        $Global:prefix=buildName;
        InitLogFile;
		WriteLog -msg "Start to get terms.";
		WriteLog -msg "Url:[$centralAdminUrl] TermStoreName:[$termStoreName] TermGroupName:[$termGroupName]";

        $site = $centralAdminUrl
        #$user = Read-Host -Prompt "Please enter your user email" 
        #$password =  Read-Host -Prompt "Please enter your password" -AsSecureString
        #$user = "mark@zcyrus.onmicrosoft.com"
        #$password = ConvertTo-SecureString "1qaz2wsxE" -AsPlainText -Force     

        $context = New-Object Microsoft.SharePoint.Client.ClientContext($site)
        #$creds = New-Object Microsoft.SharePoint.Client.SharePointOnlineCredentials($user,$password)
        #$context.Credentials = $creds

        $creds = Get-Credential -Message "Enter the site collection credentials";
	    $context.Credentials = New-Object Microsoft.SharePoint.Client.SharePointOnlineCredentials($creds.UserName,$creds.Password);
        
        #mms
        $taxonomySession = [Microsoft.SharePoint.Client.Taxonomy.TaxonomySession]::GetTaxonomySession($context)
        try
        {
            $context.Load($taxonomySession)
            $context.ExecuteQuery()
        }
        catch
        {
            $authManager = New-Object OfficeDevPnP.Core.AuthenticationManager;
            $context = $authManager.GetWebLoginClientContext($centralAdminUrl);
            $taxonomySession = [Microsoft.SharePoint.Client.Taxonomy.TaxonomySession]::GetTaxonomySession($context)
            $context.Load($taxonomySession)
            $context.ExecuteQuery()
            #throw "Cannot find the taxonomy session with Url: [$centralAdminUrl]";
        }
        if($taxonomySession -eq $null)
        {
            throw "Cannot find the taxonomy session with Url: [$centralAdminUrl]";
        }
        #term store
        $termStores = $taxonomySession.TermStores;
        $context.Load($termStores)
        $context.ExecuteQuery()
		$termStore = $termStores[0];
        if($termStore -eq $null)
        {
            throw "Cannot find the default term store.";
        }
        #term group
        $termGroup = $TermStore.Groups.GetByName($termGroupName);
        try
        {
            $context.Load($termGroup)
            $context.ExecuteQuery()
        }
        catch
        {
            throw "Cannot find the term group with name:[$termGroupName]";
        }
        if($termGroup -eq $null)
        {
            throw "Cannot find the term group with name:[$termGroupName]";
        }
        #term set
        $termSets=$termGroup.TermSets
        $context.Load($termSets)
        $context.ExecuteQuery()
        InitCsvFile;
        #CSV title
		$title = BuildOutPut "TermGroup" "TermGroupID" "TermSet" "TermSetID" "Term" "TermID" "ParentID" "IsDeprecated" "Description";
        WriteToCSV $title;
        #term group
		$termGroupRow = BuildOutPut $(ConvertComma($termGroup.Name)) $termGroup.Id "" "" "" "" "" "" $(ConvertComma($termGroup.Description));
		WriteToCSV $termGroupRow;
        WriteLog -msg $termGroupRow;
        Write-Host "Get term group successfully.Name:[$termGroupName]";
        $Global:succedCount++;
        foreach($termSet in $termSets)
        {
            $termSetRow = BuildOutPut $(ConvertComma($termGroup.Name)) $termGroup.Id $termSet.Name $termSet.Id "" "" "" "" $(ConvertComma($termSet.Description));
			WriteToCSV $termSetRow;
			WriteLog -msg $termSetRow;
            $msg = [string]::Format("Get term set successfully.Name:[{0}]",$termSet.Name);
			Write-Host $msg;
			$Global:succedCount++;
            #terms
            $terms = $termSet.Terms
            $context.Load($terms)
            $context.ExecuteQuery()

			GetChildTerms $terms $(ConvertComma($termGroup.Name)) $termGroup.Id $(ConvertComma($termSet.Name)) $termSet.Id $termSet.Id;

			if($hasError)
			{
				$msg="Finish with exception. SucceedCount:[$succedCount] FailedCount:[$failedCount]"
			}
			else
			{
				$msg="Finish. SucceedCount:[$succedCount] FailedCount:[$failedCount]"
			}
            Write-Host "CSV Path:[$Global:csvName]";
            Write-Host "Log Path:[$Global:logName]";
			WriteLog -msg $msg;
			Write-Host $msg;
        }
	}
	catch
	{
		$Global:hasError=$true;
		$msg = $_.Exception.Message;
		Write-Host $msg -ForegroundColor Red;
		WriteLog -msg $_.Exception -type "ERROR";
	}
	finally
	{
		Dispose;
		if($context -ne $null)
		{
			$context.Dispose();
		}
	}
}

function BuildOutPut($termGroup,$termGroupId,$termSet,$termSetId,$name,$guid,$parentId,$isDeprecated,$description)
{
	return [string]::Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",$termGroup,$termGroupId,$termSet,$termSetId,$name,$guid,$parentId,$isDeprecated,$description)
}

function GetChildTerms($tempTerms,$termGroup,$termGroupId,$termSet,$termSetId,$parentId)
{
    foreach($term in $tempTerms)
    {
		try
		{
			$msg=[string]::Format("Process term.TermName:[{0}]",$term.Name);
			WriteLog -msg $msg;
			$termName  = ConvertComma($term.Name);
			$text = BuildOutPut $termGroup $termGroupId $termSet $termSetId $termName $term.Id $parentId $term.IsDeprecated.toString() $(ConvertComma($term.Description));
			WriteToCSV $text;

			$Global:succedCount++;
			$msg=[string]::Format("Get term successfully.[{0}]",$text);
			WriteLog -msg $msg;
            $msg = [string]::Format("Get term successfully.TermName:[{0}]",$term.Name);
			Write-Host $msg;

            $context.Load($term.Terms)
            $context.ExecuteQuery()
            if($term.TermsCount -gt 0)
			{
			    $msg=[string]::Format("Process sub term term.TermName:[{0}] [{1}]",$term.Name,$term.TermsCount);
				WriteLog -msg $msg;
				GetChildTerms $term.Terms $termGroup $termGroupId $termSet $termSetId $term.Id
				$Global:retryCount = 0;
			}
		}
		catch
		{
		    while($Global:retryCount -lt 10 )
			{
			     try
				 {
				     Start-Sleep 10;
			         $msg=[string]::Format("Retry Process sub term term.TermName:[{0}] count[{1}]",$term.Name,$Global:retryCount);
				     WriteLog -msg $msg;
			         GetChildTerms $term.Terms $termGroup $termGroupId $termSet $termSetId $term.Id
					 $Global:retryCount = 0;
					 break;
				 }
				 catch
				 {
				     $Global:retryCount++;
				 }
			}
			$Global:failedCount++;
			$Global:hasError=$true;
			$msg=[string]::Format("An error occurred when getting term.TermName:[{0}] TermId:[{1}] `r`n",$term.Name,$term.Id);
			$msg+=$_.Exception.ToString();
			WriteLog -msg $msg -type "ERROR";
		}
    }
}

function WriteToCSV($text)
{
	if(![String]::IsNullOrEmpty($text))
    {
        $csvStreamWriter.WriteLine($text);
        $csvStreamWriter.Flush();
    }
}

function Dispose()
{
	if($Global:csvStreamWriter -ne $null)
	{
		$Global:csvStreamWriter.Close();
		$Global:csvStreamWriter.Dispose();
	}
	if($Global:csvFileStream -ne $null)
	{
		$Global:csvFileStream.Close();
		$Global:csvFileStream.Dispose();
	}

	if($Global:logStreamWriter -ne $null)
	{
		$Global:logStreamWriter.Close();
		$Global:logStreamWriter.Dispose();
	}
	if( $Global:logFileStream -ne $null)
	{
		$Global:logFileStream.Close();
		$Global:logFileStream.Dispose();
	}
}

function ConvertComma($text)
{
    return $text.Replace(",","(RevIM_Comma)").Replace("\","(RevIM_Backslash)").Replace("`n","(RevIM_Enter)");
}
LoadDll;
Export-SPTerms;