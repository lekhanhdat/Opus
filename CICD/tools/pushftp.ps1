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
        $RemoteDir,
        $LocalFile,
        $ftpUser = "builder",
        $ftpPassword = "3edc4rfvT"
    )

function Push-FTPFile
  {
      param(
            $RemoteDi,
            $LocalFile,
            $ftpUser = "builder",
            $ftpPassword = "3edc4rfvT"
          )
      Write-Host "Upload $LocalFile to $RemoteDir"
      if(!(Test-Path $LocalFile))
      {
          Write-Error "Cannot find $LocalFile"
          exit 1
      }
      if(!(Test-FTPFolder -RemoteDir $RemoteDir -ftpUser $ftpUser -ftpPassword $ftpPassword))
      {
          New-FTPFolder -RemoteDir $RemoteDir -ftpUser $ftpUser -ftpPassword $ftpPassword
      }
      $filename = Split-Path -Leaf $LocalFile
      $RemoteFile = $RemoteDir.TrimEnd('/') + "/" + $filename 
      $localFileInfo = New-Object System.IO.FileInfo($LocalFile)
      $ftpRequest = [System.Net.FtpWebRequest]::Create($RemoteFile)
      $ftpRequest.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPassword)
      $ftpRequest.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile 
      $ftpRequest.ContentLength = $localFileInfo.Length
      [byte[]]$readBuffer = New-Object byte[] 1024 
      $localFileStream = New-Object IO.FileStream ($LocalFile, [IO.FileMode]::Open) 
      try
      {
          $requestStream = $ftpRequest.GetRequestStream() 
          if($requestStream -eq $null)
          {
              return
          }
          do 
          { 
              $readLength = $localFileStream.Read($readBuffer,0,1024) 
              $requestStream.Write($readBuffer,0,$readLength) 
          } 
          while ($readLength -ne 0)
          $requestStream.Close()
      }
      finally
      {
          $localFileStream.Close()
      }
      Write-Host "Upload $LocalFile to $RemoteDir successful!"
  }
function Test-FTPFolder
{
    param(
        $RemoteDir,
        $ftpUser,
        $ftpPassword
    )
    try
    {
        $request = [System.Net.FtpWebRequest]::Create($RemoteDir)
        $request.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPassword)
        $request.UsePassive = $true
        $request.Method = [System.Net.WebRequestMethods+FTP]::ListDirectory
        $response = $request.GetResponse() 
        $response.Close()
        return $true
    }
    catch
    {
        if($_.Exception.InnerException.Status -eq [System.Net.WebExceptionStatus]::ProtocolError)
        {
            if($response -ne $null -and $response.StatusCode -eq [System.Net.FtpStatusCode]::ActionNotTakenFileUnavailable)
            {
                return $false
            }
            else
            {
                $errorResponse = $_.Exception.InnerException.Response
                if($errorResponse -ne $null -and $errorResponse.StatusCode -eq [System.Net.FtpStatusCode]::ActionNotTakenFileUnavailable)
                {
                    return $false
                }
            }
        }
        throw "An error occurred while checking if the remote directory is exist`r`n$_"
    }
}
function Make-FTPFolder
{
    param(
        $RemoteDir,
        $ftpUser = "builder",
        $ftpPassword = "3edc4rfvT"
    )
    if(!(Test-FTPFolder -RemoteDir $RemoteDir -ftpUser $ftpUser -ftpPassword $ftpPassword))
    {
        $request = [System.Net.FtpWebRequest]::Create($RemoteDir)
        $request.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPassword)
        $request.UsePassive = $true
        $request.Method = [System.Net.WebRequestMethods+FTP]::MakeDirectory
        $response = $request.GetResponse() 
        $response.Close()
    }
}

Make-FTPFolder -RemoteDir $RemoteDir
Push-FTPFile -RemoteDir $RemoteDir -LocalFile $LocalFile


