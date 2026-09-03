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

using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.Move
{
    public class SPMoveFolderExport : AveSPFolder, IDisposable
    {
        internal AveSPSite parentSite;
        internal AveSPWeb parentWeb;
        internal AveSPList parentList;
        internal AveSPFolder parentFolder;
        public SPMoveFolderExport(AveSPFolder spFolder, string folderName, Guid folderID, int folderRowID, int folderVersion)
            : base(spFolder, folderName, folderID, folderRowID, folderVersion)
        {
            this.parentFolder = spFolder;
            this.parentList = spFolder.AveList;
            this.parentWeb = spFolder.AveList.ParentWeb;
            this.parentSite = spFolder.AveList.ParentSite;
        }

        public void ExportSPFolder(IAveBackupStream stream)
        {
            ExportParentInfo(stream);
            ExportSPFolderInfo(stream);
        }

        private void ExportSPFolderInfo(IAveBackupStream stream)
        {
            stream.BeginWriteMetadata();
            //TODO:Originally ExportMetadata, after merging, change to ExportDocInfo
            ExportMetadata(stream, new SPItemMetadataBackupOption() { BackupItemTPGUIDofLookupValue = true });
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
        }

        private void ExportParentInfo(IAveBackupStream stream)
        {
            ExportParentSiteInfo(stream);
            ExportParentWebInfo(stream);
            ExportParentListInfo(stream);
        }

        private void ExportParentSiteInfo(IAveBackupStream stream)
        {
            stream.BeginWriteMetadata();
            AveSPSiteInfo aveSPSiteInfo = new AveSPSiteInfo(parentSite);
            aveSPSiteInfo.Export(stream);
            //parentSite.ExportBaseInfo(stream);
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
        }

        private void ExportParentWebInfo(IAveBackupStream stream)
        {
            stream.BeginWriteMetadata();
            AveSPWebInfo webInfo = new AveSPWebInfo(parentWeb);
            webInfo.Export(stream);
            //parentWeb.ExportBaseInfo(stream);
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
        }

        //backup content type & column Info
        private void ExportParentListInfo(IAveBackupStream stream)
        {
            stream.BeginWriteMetadata();
            var listInfo = new AveSPListInfo(parentList);
            listInfo.Export(stream);
            //parentList.ExportBaseInfo(stream);
            parentList.ExportFields(stream, false);
            parentList.ExportContentTypes(stream);
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
        }

        public void Dispose()
        {
            //DisposeObj(parentFolder);
            //DisposeObj(parentList);
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
