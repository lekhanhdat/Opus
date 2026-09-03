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
    [Parameter(Mandatory=$true)][String]$source,
    [Parameter(Mandatory=$true)][String]$destination
    )

    $base = $source
    
    Function DownloadFtpFile
    {
        param(
            [Parameter(Mandatory=$true,Position=0)][String]$Path,
            [Parameter(Mandatory=$true,Position=1)][String]$Target
            )

        if($Path.EndsWith('/')){
            Write-Host("###### Skip directory ######")
            return
        }

        $webclient = New-Object System.Net.WebClient 

        $Uri = New-Object System.Uri($Path) 
        $webclient.DownloadFile($Uri,$Target)

    }

    Function Download-FtpDir
    {
        param(
            [Parameter(Mandatory=$true)][String]$url,
            [Parameter(Mandatory=$true)][String]$targetRoot
            )
        $request = [Net.WebRequest]::Create($url)
        $request.Method = [System.Net.WebRequestMethods+FTP]::ListDirectoryDetails
        $response = $request.GetResponse()
        $reader = New-Object IO.StreamReader $response.GetResponseStream() 
        $results = $reader.ReadToEnd()
        $reader.Close()
        $response.Close()
        $lines = ($results -split "`r`n")

        $base = $base.TrimEnd("/")

        $parentTrim = $base.Split("/")[-1]
        Write-Host("parentTrim")
        Write-Host($parentTrim)
        $upperLevel = $base.Replace(("/" + $parentTrim),"")
        Write-Host("upperLevel")
        Write-Host($upperLevel)

        $parentDir = $url.Replace(($upperLevel + "/"),"")
        $parentDir = $parentDir.Replace("/","\")
        Write-Host("parentDir")
        Write-Host($parentDir)
        if(!(Test-Path $parentDir)){
            new-item "$targetRoot\$parentDir" -itemtype directory
            Write-Host("###### Created directory: "+"$targetRoot\$parentDir")
        }

        ForEach($line in $lines){
            if ($line.indexOf("<DIR>") -gt -1) {
                $subfolder = $line.Split(" ")[-1]
                #Write-Host($url + "/" + $subfolder)
                $longsubfolder = $url + "/" + $subfolder

                Download-FTPDir -url $longsubfolder -targetRoot $targetRoot
            }elseif($line.length -gt 0) {
                $file = [regex]::split($line, '\s{5,20}\d+\s')[-1]
                Write-Host("###### Start download: " + $url + "/" + $file)
                $longfile = $url + "/" + $file
                $localpath = $longfile.Replace(($upperLevel + "/"),"")
                $localpath = $targetRoot + "\" + $localpath.Replace("/","\")
                Write-Host("###### Downloaded: "+ $localpath)
                DownloadFtpFile -Path "$longfile" -Target "$localpath"
            }
        }
  
    }

    
    Download-FTPDir -url $source -targetRoot $destination

