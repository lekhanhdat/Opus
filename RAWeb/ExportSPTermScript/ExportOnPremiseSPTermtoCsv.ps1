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
# .\ExportSPTermtoCsv.ps1 "http://leon-sp13:8888" "Managed Metadata Service" "BusinessClassification" "Department"

 param
	(
		$centralAdminUrl = $null,
		$termStoreName=$null,
		$termGroupName=$null
	);

if(($centralAdminUrl -eq $null) -or ($termStoreName -eq $null) -or ($termGroupName -eq $null))
{
    Write-Host "Please check parameters." -ForegroundColor Yellow;
	Write-Host "SYNTAX`t.\ExportSPTermtoCsv.ps1 CentralAdminUrl TermStoreName TermGroupName " -ForegroundColor Yellow;
	Write-Host "EXAMPLE`t.\ExportSPTermtoCsv.ps1 ""http://OnpremiseSP:8888"" ""Managed Metadata Service"" ""BusinessClassification"" " -ForegroundColor Yellow;
    return;
}

Add-PSSnapin microsoft.sharepoint.powershell;
$Global:csvFileStream=$null;
$Global:csvStreamWriter = $null;
$Global:logFileStream=$null;
$Global:logStreamWriter=$null;
[int] $Global:succedCount=0;
[int] $Global:failedCount=0;
[bool] $Global:hasError=$false;
$Global:csvName=$null;
$Global:logName=$null;
$Global:prefix=$null;


function buildName()
{
	$time=(Get-Date).tostring("yyyy.MM.dd_HH.mm.ss");
	$name=(Get-Location).Path + [string]::Format("\Terms_{0}", $time);
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
    $Global:logName=[string]::Format("{0}_log.txt", $Global:prefix);
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
		WriteLog -msg "Url:[$centralAdminUrl] TermStoreName:[$termStoreName] TermGroupName:[$termGroupName] ";

		$site = Get-SPSite $centralAdminUrl -ErrorAction Stop;
		$taxSession = new-object Microsoft.SharePoint.Taxonomy.TaxonomySession($site, $true) -ErrorAction Stop;
        if($taxSession -eq $null)
        {
            throw "Cannot find the taxonomy session with Url: [$centralAdminUrl]";
        }
		$termStore = $taxSession.TermStores[$termStoreName];
        if($termStore -eq $null)
        {
            throw "Cannot find the term store with name:[$termStoreName]";
        }
		$termGroup=$termStore.Groups[$termGroupName];
        if($termGroup -eq $null)
        {
            throw "Cannot find the term group with name:[$termGroupName]";
        }
		

        if (($termStore -ne $null) -and ($termGroup -ne $null) )
		{
		    InitCsvFile;
			#CSV title
			$title = BuildOutPut "TermGroup" "TermGroupID" "TermSet" "TermSetID" "Term" "TermID" "ParentID" "IsDeprecated" "Description";
			WriteToCSV $title;
            #term group
            #$bytes = [System.Text.Encoding]::UTF8.GetBytes($termGroup.Description);
			$termGroupRow = BuildOutPut $(ConvertComma($termGroup.Name)) $termGroup.Id "" "" "" "" "" "" $(ConvertComma($termGroup.Description));
			WriteToCSV $termGroupRow;
			WriteLog -msg $termGroupRow;
            Write-Host "Get term group successfully.Name:[$termGroupName]";
			$Global:succedCount++;
		    $termSets=$termGroup.TermSets;
			foreach($termSet in $termSets)
			{
				$termSetRow = BuildOutPut $(ConvertComma($termGroup.Name)) $termGroup.Id $termSet.Name $termSet.Id "" "" "" "" $(ConvertComma($termSet.Description));
				 #term set
			    $termSetRow = BuildOutPut $(ConvertComma($termGroup.Name)) $termGroup.Id $termSet.Name $termSet.Id "" "" "" "" $(ConvertComma($termSet.Description));
			    WriteToCSV $termSetRow;
			    WriteLog -msg $termSetRow;
				$termSetName = $termSet.Name;
                Write-Host "Get term set successfully.Name:[$termSetName]";
			     $Global:succedCount++;
			    #terms
			    GetChildTerms $termSet.Terms $(ConvertComma($termGroup.Name)) $termGroup.Id $(ConvertComma($termSet.Name)) $termSet.Id $termSet.Id;
			}
		}
		else
		{
			$Global:hasError=$true
			$msg="Can not find termGroup.";
			WriteLog -msg $msg -type "ERROR";
			Write-Host $msg -ForegroundColor Red;
			Write-Host "Url:[$centralAdminUrl] TermStoreName:[$termStoreName] TermGroupName:[$termGroupName]" -ForegroundColor Red;
		}
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
		if($site -ne $null)
		{
			$site.Dispose();
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
			#if($termName.Contains(","))
			#{
			#	$termName=$termName.Replace(",",";");
			#}
			$text = BuildOutPut $termGroup $termGroupId $termSet $termSetId $termName $term.Id $parentId $term.IsDeprecated.toString() $(ConvertComma($term.GetDescription()));
			WriteToCSV $text;

			$Global:succedCount++;
			$msg=[string]::Format("Get term successfully.[{0}]",$text);
			WriteLog -msg $msg;
            $msg = [string]::Format("Get term successfully.TermName:[{0}]",$term.Name);
			Write-Host $msg;
            if($term.TermsCount -gt 0)
			{
				GetChildTerms $term.Terms $termGroup $termGroupId $termSet $termSetId $term.Id
			}
		}
		catch
		{
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

Export-SPTerms;