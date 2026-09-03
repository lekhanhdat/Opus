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
using System.Text;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;

namespace RAArchiverMaintenance.Retention;

public static class GDriveArchiverUtil
{
    private static readonly IRALogger _logger = new RALogger(typeof(GDriveArchiverUtil));
    
    public static long ValidateModifiedTime(int keepValue, DateUnit dateUnit, DateTime dateTimeNow)
    {
        if (keepValue < 0)
        {
            _logger.Info($"keep value is zero,return false");
            return 0;
        }
        var tempDateTimeNow = dateTimeNow;
        switch (dateUnit)
        {
            case DateUnit.Year:
                tempDateTimeNow = tempDateTimeNow.AddYears(-keepValue);
                break;
            case DateUnit.Month:
                tempDateTimeNow = tempDateTimeNow.AddMonths(-keepValue);
                break;
            case DateUnit.Week:
                tempDateTimeNow = tempDateTimeNow.AddDays(-keepValue * 7);
                break;
            case DateUnit.Day:
                tempDateTimeNow = tempDateTimeNow.AddDays(-keepValue);
                break;
        }
        return tempDateTimeNow.Ticks;
    }
    
    public static void DecryptSecretForGoogleStorage(List<ArchiverPruningJob> archiverPruningJobs)
    {
        if(archiverPruningJobs is { Count: > 0 })
        {
            foreach(var archiverPruningJob in archiverPruningJobs)
            {
                if(archiverPruningJob.DataLogicalDevice != null)
                    DecryptGoogleStorageSecret(archiverPruningJob.DataLogicalDevice);
                if (archiverPruningJob.IndexLogicalDevice != null)
                    DecryptGoogleStorageSecret(archiverPruningJob.IndexLogicalDevice);
                if (archiverPruningJob.DestinationDevice != null)
                    DecryptGoogleStorageSecret(archiverPruningJob.DestinationDevice);
            }
        }
    }
    
    private static void DecryptGoogleStorageSecret(LogicalDeviceDto dto)
    {
        if(dto.PhysicalDrives is { Count: > 0 })
        {
            foreach(var physicalDrive in dto.PhysicalDrives)
            {
                string begin = "-----BEGIN PRIVATE KEY-----";
                string end = "-----END PRIVATE KEY-----";
                if (physicalDrive.Type == (int)StorageDeviceType.Google)
                {
                    if(physicalDrive.Password is { Count: > 0 })
                    {
                        string[] keyValue = physicalDrive.Password[0].Split(new char[] { '=' });
                        if (!keyValue[0].EndsWith("tokensecret") && !(keyValue[1].StartsWith(begin) && keyValue[1].Contains(end)))
                        {
                            keyValue[1] = PhysicalDeviceDto.XRIUtil.ValueEncode(UnWrapKey(PhysicalDeviceDto.XRIUtil.ValueDecode(keyValue[1])));
                        }
                        physicalDrive.UpdatePassword(new List<string> { keyValue[0] + "=" + keyValue[1] });
                    }
                }
            }
        }
    }
    
    private static string UnWrapKey(string password)
    {
        var result = CspCrossPlatformExchangeWrapper.UnWrapKey(password);
        return Encoding.UTF8.GetString(result, 0, result.Length);
    }
}