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




namespace AvePoint.Wrapper.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Configuration;
    using System.IO;
    using AvePoint.Common;
    using System.Xml;
    using AvePoint.GCommon;
    using Cloud.Sdk.Data.AosModern;
    using AvePoint.GCommon.Utility;
    using AvePoint.RA.Common;
    using AngleSharp.Common;
    #endregion

    public static class SharePointFileReadWriteOptions
    {
        /// <summary>
        /// Milliseconds
        /// </summary>
        public static int RequestTimeout = 6 * 60 * 60 * 1000;
        /// <summary>
        /// Milliseconds
        /// timeout if read or write stream didn't have any progress for time period.
        /// </summary>
        public static int ReadWriteTimeout = 10 * 60 * 1000;
    }

    public partial class WrapperConfiguration
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(WrapperConfiguration));
        public static WrapperConfigurationForBPOS BPOS_S = null;
        public static readonly string JobDirDefaultValue = SecurityUtils.SafeCombinePath(AppDomain.CurrentDomain.BaseDirectory, "AgentData/jobs");
        public static bool IsMonitorEnable { get; set; }
        public static bool IsILMode { get; set; }
        public static int RecordsOutputStreamLevel { get; set; }
        public static int ArchiverOutputStreamLevel { get; set; }
        public static int MonitorLogFileSize { get; set; }
        public static int MonitorLogFileCount { get; set; }
        public static int CheckInterval { get; set; }
        public static bool RestoredAllWebProperties { get; set; }
        public static string SpecialWebPropertyNames { get; set; }
        public static bool RemoveParentLimitedAccess { get; set; }
        public static string TempDirectory { get; set; }
        public static int ListRestoreOption { get; set; }

        public static long TempFileSize { get; set; }

        //Archiver
        public static bool UseStubAccessTimeRule { get; set; }

        //BPOS information  httpwebrequest timeout for upload file
        public static int UpLoadFileStreamTimeout { get; set; }
        public static int UploadLargeFileDefinitionSize { get; set; }
        public static bool EnableUseWorkingLanguage { get; set; }
        public static bool UpdateColumnWithEventReciever { get; set; }
        public static bool DevelopMode { get; set; }

        public static string JobDir { get; set; }

        public static bool EnableDownloadLATData { get; set; }

        public static bool CheckFileContentDismatch { get; set; }

        public static bool AddAPPByServiceAccount { get; set; }
        public static bool MoveToArchiverTierWhenArchiving { get; set; }
        public static int? MoveToAnotherTierType { get; set; }
        public static List<AveBPOSAccountInfo> accountInfo { get; set; }
        public static bool IsProcessApprovalDatasOnly { get; set; }
        public static bool IsEnableTeams { get; set; }
        public static bool IsRecheckRule { get; set; }
        public static bool IsAOSPLeaveStub { get; set; }
        public static bool HasDeleteOnlyLicense { get; set; }
        public static AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto AOSPStubSettingDto { get; set; }
        public static bool NeedToUploadIndex { get; set; }
        public static bool IsRestoreJob { get; set; }

        public static bool IsSkipCheckSystemFile { get; set; }
        public static string SpecifyReportStorageXRIString { get; set; }

        #region -- Wrapper Common --
        public static AveOpenBinaryOptions OpenBinaryOptions { get; set; }
        #endregion
        public static bool EnableRemoveRetentionLabel { get; set; }



        static WrapperConfiguration()
        {
            IsMonitorEnable = false;
            MonitorLogFileSize = 5;
            MonitorLogFileCount = 5;
            CheckInterval = 10;
            RestoredAllWebProperties = false;
            SpecialWebPropertyNames = string.Empty;
            RemoveParentLimitedAccess = false;
            TempDirectory = AveEnv.AgentTempFolder;
            ListRestoreOption = 2;
            TempFileSize = 20;
            UseStubAccessTimeRule = false;

            UpLoadFileStreamTimeout = 30 * 60;
            UploadLargeFileDefinitionSize = 100;

            EnableUseWorkingLanguage = false;
            UpdateColumnWithEventReciever = true;
            DevelopMode = false;
            JobDir = JobDirDefaultValue;
            OpenBinaryOptions = AveOpenBinaryOptions.Unprotected;
            EnableDownloadLATData = true;
            CheckFileContentDismatch = true;
            MoveToArchiverTierWhenArchiving = false;

            EnableRemoveRetentionLabel = false;
        }
        public static Dictionary<Guid, Guid> ChannelTabEntityIdMapping = new Dictionary<Guid, Guid>();
    }
}
