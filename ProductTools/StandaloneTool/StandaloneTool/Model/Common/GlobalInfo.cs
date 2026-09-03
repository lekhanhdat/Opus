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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using Storage;
using System.ComponentModel;
using System.IO;

namespace StandaloneTool.Model.Common
{
    public static class GlobalInfo
    {
        public const string AVEPOINT_STORAGE_ID = "6A040C17-AF8A-4F1F-96C1-7CEB2E23B1F3";

        public static bool IsUsingAveStorage { get; set; }

        public static Module Module { get; set; }

        public static string ExtractZipLocation => $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export Tool")}";

        public static string EncryptExportDBPassword { get; set; }

        public static Dictionary<string, string> EncryptionInfoCache = new Dictionary<string, string>();

        public static string SftpPrivateKeyFileContent { get; set; } 

        public static string ExportDBFilePath { get; set; }

        public static string ExportLocation { get; set; }

        public static LocationType ExportOption { get; set; }

        public static long TotalExportedSize { get; set; }

        public static JobStatus FinalJobStatus { get; set; }

        #region Physical device
        public static bool IsSkipAPData { get; set; }
        public static IXSystem? AvepointMappingStorage { get; set; }

        public static IXSystem? TargetStorage { get; set; }

        public static StorageDeviceType TargetStorageType { get; set; }

        #endregion

    }

    public enum LocationType
    {
        [Description("Local location")]
        LocalLocation = 0,
        [Description("Microsoft Azure Blob Storage")]
        MSAzureBlob = 1,
        [Description("SFTP")]
        SFTP = 2,
    }

    public enum Module
    {
        [Description("SharePoint Online")]
        SharePointOnline = 1,
        [Description("OneDrive")]
        OneDrive = 6,
        [Description("Teams")]
        Teams = 11,
    }
}


