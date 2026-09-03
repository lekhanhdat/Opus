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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Records.Core.Utilities.Extensions;
using DataExportCore.Utils;
using System.Collections.Concurrent;
using System.Security;

namespace DataExportCore
{
    public static class GlobalCache
    {
        public static string MasterKey = string.Empty;
        public static string IndexDeviceId = string.Empty;
        public static SecureString CustomPassword;
        public static string ExportLocation = string.Empty;
        public static StorageDeviceType TargetStorageType;
        public static ConcurrentBag<SummaryReportDto> SummaryReportDtos = [];
        public static ConcurrentBag<TeamsSummaryReportDto> TeamsSummaryReportDtos = [];
        public static HashSet<string> TopicItemIds = new HashSet<string>();
        public static Dictionary<string, long> ItemCreateTimeInfo = new Dictionary<string, long>();
        public static HashSet<string> FileNames = new();
        public static bool IsSkipAPData = false;

        public static void InitializeGlobalCache(Dictionary<string, string> settingProfiles, ExportOption exportOption)
        {
            MasterKey = settingProfiles[ExportUtility.MasterKeyProfileName];
            IndexDeviceId = settingProfiles[ExportUtility.IndexDeviceProfileName];
            ExportLocation = exportOption.ExportLocation;
            TargetStorageType = exportOption.TargetStorageType;
            IsSkipAPData = exportOption.IsSkipAPData;
        }
    }
}
