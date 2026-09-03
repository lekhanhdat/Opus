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
using AvePoint.GCommon;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPBackup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.SharePoint.EnforceRuleAction.LeaveStub
{
    public class OnPremSPLeaveStubDocExport : AveSPDoc, IDisposable
    {
        internal AveSPSite parentSite;
        internal AveSPWeb parentWeb;
        internal AveSPList parentList;
        internal AveSPFolder parentFolder;
        internal IAveFile file;
        public bool NeedExportRoleAssignments = true;
        private byte[] linkFileContent;//RECO-72

        public OnPremSPLeaveStubDocExport(AveSPFolder parentFolder, IAveFile file, byte[] linkFileContent)
            : base(parentFolder, file.UniqueId, file.Item.ID, file.UIVersion, file.ServerRelativeUrl)
        {
            this.file = file;
            this.parentFolder = parentFolder;
            this.parentList = parentFolder.AveList;
            this.parentWeb = parentFolder.AveList.ParentWeb;
            this.parentSite = parentFolder.AveList.ParentSite;
            this.linkFileContent = linkFileContent;//RECO-72
        }

        public void ExportSPFile(IAveBackupStream stream)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.LinkDocument.ExportSPFile", addToStatistics: true))
            {
                //ExportParentInfo(stream);
                ExportSPFileVersion(stream, file.UIVersion);
            }
        }

        private void ExportParentInfo(IAveBackupStream stream)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.LinkDocument.ExportParentInfo", addToStatistics: true))
            {
                ExportParentSiteInfo(stream);
                ExportParentWebInfo(stream);
                ExportParentListInfo(stream);
                if (!this.parentList.ServerRelativeUrl.Equals(this.parentFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    ExportParentFolderInfo(stream);
                }
            }
        }

        private void ExportSPFileVersion(IAveBackupStream stream, int version)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.LinkDocument.ExportSPFileVersion", addToStatistics: true))
            {
                stream.BeginWriteMetadata();
                //单独备份lookup column value，Restore时单独还原.
                ExportMetadata(stream, new SPItemMetadataBackupOption() { BackupItemTPGUIDofLookupValue = true });
                this.AveSPItem.ExportUserDataInfo(stream, null, true);
                this.AveSPItem.ExportDataJunctionInfo(stream, true);
                if (this.AveSPItem.HasUniqueRoleAssignments && !this.AveSPItem.IsVersion)
                {
                    this.AveSPItem.ExportRoleAssignments(stream, true);
                }
                stream.EndWriteMetadata();
                //stream.FlushMetadata(0);
                stream.FlushMetadata(linkFileContent.Length);
                stream.WriteContent(linkFileContent, 0, linkFileContent.Length);
            }
        }

        //需要考虑多层folder的情况
        private void ExportParentFolderInfo(IAveBackupStream stream)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.LinkDocument.ExportParentFolderInfo", addToStatistics: true))
            {
                stream.BeginWriteMetadata();
                parentFolder.ExportMetadata(stream, new SPItemMetadataBackupOption());
                stream.EndWriteMetadata();
            }
        }

        //backup content type & column Info
        private void ExportParentListInfo(IAveBackupStream stream)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.LinkDocument.ExportParentListInfo", addToStatistics: true))
            {
                stream.BeginWriteMetadata();
                AveSPListInfo aveSPListInfo = new AveSPListInfo(parentList);
                aveSPListInfo.Export(stream);
                stream.EndWriteMetadata();
                stream.FlushMetadata(0);
            }
        }

        private void ExportParentWebInfo(IAveBackupStream stream)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.LinkDocument.ExportParentWebInfo", addToStatistics: true))
            {
                stream.BeginWriteMetadata();
                AveSPWebInfo aveSPWebInfo = new AveSPWebInfo(parentWeb);
                aveSPWebInfo.Export(stream);
                stream.EndWriteMetadata();
                stream.FlushMetadata(0);
            }
        }

        private void ExportParentSiteInfo(IAveBackupStream stream)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.LinkDocument.ExportParentSiteInfo", addToStatistics: true))
            {
                stream.BeginWriteMetadata();
                AveSPSiteInfo aveSPSiteInfo = new AveSPSiteInfo(parentSite);
                aveSPSiteInfo.Export(stream);
                stream.EndWriteMetadata();
                stream.FlushMetadata(0);
            }
        }


        public void Dispose()
        {
        }

        private void DisposeObj(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }
    }
}
