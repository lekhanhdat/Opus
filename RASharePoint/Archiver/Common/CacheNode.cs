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



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Common;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Backup;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.SharePoint.Archiver
{
    class CacheNode : IDisposable
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private FileHeaderStatus backupStatus = FileHeaderStatus.Failed;
        public object WrapperObject { get; set; }
        public string Name = string.Empty;
        public BackupInfoSender Sender { get; set; }
        public XmlElement FileHeader { get; set; }
        public FileHeaderStatus BackupStatus { get { return backupStatus; } set { backupStatus = value; } }
        public ScheduleConfiguration Configuration { get; set; }
        public ArchiveApproveReport Node { get; set; }

        public bool DoDelete { get; set; }
        public Action CustomizedDisposeAction;
        // 判断 是不是rootFolder 
        public bool IsRootFolder { get; set; }
        public bool IsCurrentVersion { get; set; }
        public bool IsSkipVersion { get; set; }
        public bool IsSkipDeleteVersion { get; set; }
        public bool IsVaultCacheNode { get; set; }
        public bool IsSiteLevel { get; set; }
        /*public Dictionary<Guid, List<AveRoleAssignmentInfo>> RoleAssignmentCache
        {
            get
            {
                if (roleAssignmentCache == null)
                    return roleAssignmentCache = new Dictionary<Guid, List<AveRoleAssignmentInfo>>();
                else
                    return roleAssignmentCache;
            }
        }*/

        public void Dispose()
        {
            if (CustomizedDisposeAction != null)
            {
                CustomizedDisposeAction();
            }

            if (WrapperObject is IDisposable)
            {
                try
                {
                    ((IDisposable)WrapperObject).Dispose();
                }
                catch (Exception e)
                {
                    mLog.Warn("There is something wrong with dispose node.ErrorMessage: {0}.", e.ToString());
                }
            }

        }
        /*
        private void SendFileSecondFileHeader()
        {
            if (IsRootFolder || IsCurrentVersion)
            { return; }
            else
            {
                if (FileHeader != null)
                {
                    FileHeader.SetAttribute(KeyWord.DoDelete, DoDelete.ToString());
                    Sender.BackupSecondFileHeader(FileHeader, BackupStatus);
                }
            }
        }*/

        /// <summary>
        /// 1.BackwardDependenceNodeCache<T1> Close方法调用SecondHeader逻辑.
        /// 2.MultiItemBackup SafeReleaseNode 方法调用SecondHeader逻辑.
        /// </summary>
        /// <returns></returns>
        public string GenerateSecondFileHeader()
        {
            if (IsRootFolder || IsCurrentVersion || IsSkipDeleteVersion ||
                (Configuration?.ArchiveJobSplitedDBInfo?.IsLatestSplitedDB != true && Node?.IsRepeatProcess == true))
            {
                return string.Empty;
            }
            if (FileHeader == null)
            {
                throw new ArgumentNullException("FileHeader");
            }
            FileHeader.SetAttribute(KeyWord.DoDelete, DoDelete.ToString());
            FileHeader.SetAttribute("fileHeaderType", ((int)FileHeaderType.Second).ToString());
            if (this.Node?.ManifestDocumentSnapshot != null)
            {
                FileHeader.SetAttribute("ManifestDocumentSnapshot", SerializerHelper.SerializeByJsonSerializer(this.Node.ManifestDocumentSnapshot));
            }
            XmlElement stubInfo = (XmlElement)FileHeader.ChildNodes[0];
            stubInfo.SetAttribute("status", BackupStatus.ToString());
            return FileHeader.OuterXml;
        }
    }
}
