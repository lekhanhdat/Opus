/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */

using AvePoint.GCommon.Utility;
using System.Text;

namespace MediaDataIO;

public class MediaDataPathGenerator : IDataPathGenerator
{
    public string ModulePath { get; }
    public string JobId { get; }
    public string Container { get; }
    public string TempFolder;
    public bool NeedToWeakUp;

    public MediaDataPathGenerator(DataModule module, string jobId, string container,bool needToWeakUp,string tempFolder)
    {
        ModulePath = ConvertModule(module);
        JobId = jobId;
        Container = container;
        TempFolder = tempFolder;
        NeedToWeakUp = needToWeakUp;
    }

    public string GenerateDataVolume(string tempName = "")
    {
        if (NeedToWeakUp)
        {
            return $"{ModulePath}/{TempFolder}/{Container}";
        }
        else
        {
            return $"{ModulePath}/DataVolume/{Container}";
        }
    }

    public string GenerateIndexVolume()
    {
        return $"{ModulePath}/IndexVolume/{Container}";
    }

    public string GenerateFileName(long prefixNumber, long fileNumber, FileType fileType)
    {
        return $"{JobId}_{GetDataFileType(fileType)}_{fileNumber}.dat";
        
    }
    public string GenerateFileNamePath(long prefixNumber, long fileNumber, FileType fileType)
    {
        return $"{GenerateDataVolume()}/{GenerateFileName(prefixNumber, fileNumber, fileType)}";
    }

    private String ConvertModule(DataModule module) => module switch
    {
        //DataModule.GranularPlatform => DataPathConstants.GranularModulePath,
        //DataModule.PowerPlatform => DataPathConstants.PowerPlatformModulePath,
        DataModule.TeamsPlatform => DataPathConstants.TeamsModulePath,
        DataModule.EXOPlatform => DataPathConstants.EXOModulePath,
        DataModule.GDrivePlatform => DataPathConstants.GDriveModulePath,
        DataModule.SitePlatform => DataPathConstants.ArchiverModulePath,
        _ => throw new NotSupportedException(module.ToString())
    };

    private static string GetDataFileType(FileType fileType)
    {
        return fileType switch
        {
            FileType.Content => DataPathConstants.ContentFileNamePrefix,
            FileType.MetaData => DataPathConstants.MetaFileNamePrefix,
            _ => throw new NotSupportedException(fileType.ToString())
        };
    }
}

public class TeamsMediaDataPathGenerator : MediaDataPathGenerator
{
    public TeamsMediaDataPathGenerator(DataModule module, string jobId, string emailAddress,bool needToWeakUp = false,string tempFolder = "") 
        : base(module, jobId, ConvertEmailToPath(emailAddress), needToWeakUp, tempFolder)
    {
    }
    
    private static string ConvertEmailToPath(string emailAddress)
    {
        var (domain, name) = ParseEmailPath(emailAddress);
        return $"{domain}/{name}";
    }

    private static (string, string) ParseEmailPath(string emailAddress)
    {
        if (string.IsNullOrEmpty(emailAddress))
        {
            throw new ArgumentException("Email address cannot be null or empty.");
        }
        var parts = emailAddress.Split('@');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid email address format.");
        }
        var name = parts[0];
        var domain = parts[1];
        return (domain, name);
    }
}

public class SiteMediaDataPathGenerator : MediaDataPathGenerator
{
    public SiteMediaDataPathGenerator(DataModule module, string jobId, string sitePath, bool needToWeakUp, string tempFolder) 
        : base(module, jobId, ConvertSitePathToPath(sitePath), needToWeakUp, tempFolder)
    {
    }

    private static string ConvertSitePathToPath(string sitePath)
    {
        String webAppName, siteName;
        ParseSitePath(sitePath, out webAppName, out siteName);
        return SecurityUtils.SafeCombinePath( webAppName, siteName);

    }

    static void ParseSitePath(String siteURL, out String webAppName, out String siteName)
    {
        int index = -1;
        StringBuilder tmp = new StringBuilder();
        index = siteURL.IndexOf(":", StringComparison.OrdinalIgnoreCase);
        tmp.Append(siteURL.Substring(0, index)).Append("#");
        string temp = siteURL.Substring(index + 3);
        index = -1;
        index = temp.IndexOf(":", StringComparison.OrdinalIgnoreCase);
        if (index == -1)
        {
            tmp.Append(80).Append("#");
            index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                tmp.Append(temp.Substring(0, index));
                temp = temp.Substring(index + 1);
            }
            else
            {
                tmp.Append(temp);
                temp = "";
            }
        }
        else
        {
            String machineName = temp.Substring(0, index);
            temp = temp.Substring(index + 1);
            index = -1;
            index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                tmp.Append(temp.Substring(0, index));
                temp = temp.Substring(index + 1);
            }
            else
            {
                tmp.Append(temp);
                temp = "";
            }
            tmp.Append("#").Append(machineName);
        }
        webAppName = tmp.ToString();
        tmp.Remove(0, tmp.Length);
        tmp.Append("#");
        if (temp.Length > 0)
        {
            temp = temp.Replace(';', '#');
            tmp.Append(temp.Replace('/', '#'));
        }
        siteName = tmp.ToString();
    }
}