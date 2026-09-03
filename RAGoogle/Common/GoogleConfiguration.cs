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
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using AvePoint.Wrapper.Common;
using RAArchiverCommon;
using RAGoogle.Report;
using RAGoogle.Util;
using ActionType = RAGoogle.Models.Enums.ActionType;

namespace RAGoogle.Common
{
    public class GoogleConfiguration
    {
        public Rule CurrentRule { get; set; }

        public string JobId { get; set; }

        public RMTerm? CurrentTerm { get; set; }

        public GoogleDriveTreeNodeDto SelectedNode { get; set; }

        public RMAosGoogleAppProfile AppProfile { get; set; }

        public ReportCenter ReportCenter { get; set; }

        public RecordManager RecordManager { get; set; }

        public RMGoogleSetting GoogleSetting { get; set; }

        public RuleManager RuleManager { get; set; }

        //archive
        public DateTime ArchiverUNCTime;
        public Rule currentRule => CurrentRule;
        public Dictionary<string, GDriveBackupRequest> CachedBackupJob = new(StringComparer.OrdinalIgnoreCase);
        public BackgroundSettings BackgroundSettings { get; private set; }
        public bool IsILMode = false;
        public string CurrentIndexJobID = string.Empty;
        public Dictionary<int, Rule> RuleCollection = null;
        public string ArchiveTemp { get; private set; }
        public string ScanDBName { get; private set; }
        public bool IsEndUserJob = false;
        public bool AutoApproval { get; private set; }
        public ActionType Action { get; set; }
        public Dictionary<ActionTab, List<JMArchiverActionJobDetails>> ActionApproveReports { get; set; }
        public void Init()
        {
            BackgroundSettings = BackgroundSettings.GetInstance();
            WrapperConfiguration.RecordsOutputStreamLevel = (int)BackgroundSettings.GoogleOutputStreamLevel;
            WrapperConfiguration.ArchiverOutputStreamLevel = (int)BackgroundSettings.ArchiverOutputStreamLevel;
            ArchiveTemp = BackgroundSettings.ArchiveTemp;
            if (!System.IO.Directory.Exists(ArchiveTemp))
            {
                Directory.CreateDirectory(ArchiveTemp);
            }
            ArchiverUNCTime = DateTime.UtcNow;
            ScanDBName = string.Format("scan.{0}.db", Guid.NewGuid().ToString());
            ActionApproveReports = new Dictionary<ActionTab, List<JMArchiverActionJobDetails>>();
        }
    }
}
