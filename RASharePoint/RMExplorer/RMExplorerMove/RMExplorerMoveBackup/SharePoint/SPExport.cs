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
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class SPExport : AveSPDoc, IDisposable
    {
        //private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(SPExport));
        internal AveSPSite parentSite;
        internal AveSPWeb parentWeb;
        internal AveSPList parentList;
        internal AveSPFolder parentFolder;
        internal IAveFile file;
        public bool NeedExportRoleAssignments = true;
        
        private bool IsFirstItem = true;

        public SPExport(AveSPFolder parentFolder, IAveFile file, bool IsFirstItem, int fileVersion)
                    : base(parentFolder, file.UniqueId, file.Item.ID, fileVersion, file.ServerRelativeUrl)
        {
            this.file = file;
            this.parentFolder = parentFolder;
            this.parentList = parentFolder.AveList;
            this.parentWeb = parentFolder.AveList.ParentWeb;
            this.parentSite = parentFolder.AveList.ParentSite;
            this.IsFirstItem = IsFirstItem;
        }


        //public void ExportSPFile(IAveBackupStream stream, IStreamConvertor streamConvertor)
        public void ExportSPFile(IAveBackupStream stream)
        {
            ExportParentInfo(stream);
            //ExportSPFileVersion(stream, file.UIVersion, false, streamConvertor);
            ExportSPFileVersion(stream, file.UIVersion, false);
        }

        //public void ExportSPFileVersion(IAveBackupStream stream, int version, IStreamConvertor streamConvertor)
        public void ExportSPFileVersion(IAveBackupStream stream, int version)
        {
            //ExportSPFileVersion(stream, version, true, streamConvertor);
            ExportSPFileVersion(stream, version, true);
        }

        private void ExportParentInfo(IAveBackupStream stream)
        {
            ExportParentSiteInfo(stream);
            ExportParentWebInfo(stream);
            ExportParentListInfo(stream);
            if (!this.parentList.ServerRelativeUrl.Equals(this.parentFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                ExportParentFolderInfo(stream);
            }
        }
        private void ExportParentSiteInfo(IAveBackupStream stream)
        {
            stream.BeginWriteMetadata();
            AveSPSiteInfo aveSPSiteInfo = new AveSPSiteInfo(parentSite);
            aveSPSiteInfo.Export(stream);
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
        }
        private void ExportParentWebInfo(IAveBackupStream stream)
        {
            stream.BeginWriteMetadata();
            AveSPWebInfo webInfo = new AveSPWebInfo(parentWeb);
            webInfo.Export(stream);
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
        }
        private void ExportParentListInfo(IAveBackupStream stream)
        {
            stream.BeginWriteMetadata();
            var listInfo = new AveSPListInfo(parentList);
            listInfo.Export(stream);
            parentList.ExportFields(stream, false);
            parentList.ExportContentTypes(stream);
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
        }
        //需要考虑多层folder的情况
        private void ExportParentFolderInfo(IAveBackupStream stream)
        {
            stream.BeginWriteMetadata();

            //TODO:Originally ExportMetadata, after merging, change to ExportDocInfo
            //parentFolder.ExportMetadata(stream, new SPItemMetadataBackupOption());
            parentFolder.ExportDocInfo(stream);
            stream.EndWriteMetadata();
        }
        //private void ExportSPFileVersion(IAveBackupStream stream, int version, bool exportParentInfo, IStreamConvertor streamConvertor)
        private void ExportSPFileVersion(IAveBackupStream stream, int version, bool exportParentInfo)
        {
            if (exportParentInfo)
            {
                ExportParentInfo(stream);
            }
            stream.BeginWriteMetadata();
            //单独备份lookup column value，Restore时单独还原.
            ExportMetadata(stream, new SPItemMetadataBackupOption() { BackupItemTPGUIDofLookupValue = true });
            /*
            var securityOption = new SPRoleAssignmentsBakupOption { IncludeUsers = true, IncludeGroups = true, ForceBackup = true };
            ExportRoleAssignments(stream, securityOption);
            */
            //ExportAlerts(stream);
            stream.EndWriteMetadata();
            //ExportContent(stream, streamConvertor);
            ExportContent(stream);
        }
        public void Dispose()
        {
            DisposeObj(parentFolder);
            DisposeObj(parentList);
            DisposeObj(parentWeb);
            DisposeObj(parentSite);
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
