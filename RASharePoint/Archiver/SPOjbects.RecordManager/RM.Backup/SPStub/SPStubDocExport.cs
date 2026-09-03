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

namespace AvePoint.RA.SharePoint.Archiver
{
    public class SPStubDocExport : AveSPDoc, IDisposable
    {
        internal AveSPSite parentSite;
        internal AveSPWeb parentWeb;
        internal AveSPList parentList;
        internal AveSPFolder parentFolder;
        internal IAveFile file;
        public bool NeedExportRoleAssignments = true;
        private byte[] linkFileContent;//RECO-72

        public SPStubDocExport(AveSPFolder parentFolder, IAveFile file, byte[] linkFileContent)
            : base(parentFolder, file.UniqueId, file.Item.ID, file.UIVersion, file.ServerRelativeUrl)
        {
            this.file = file;
            this.parentFolder = parentFolder;
            this.parentList = parentFolder.AveList;
            this.parentWeb = parentFolder.AveList.ParentWeb;
            this.parentSite = parentFolder.AveList.ParentSite;
            this.linkFileContent = linkFileContent;//RECO-72
        }

        public SPStubDocExport(AveSPFolder parentFolder, IAveFile file)
            : base(parentFolder, file.UniqueId, file.Item.ID, file.UIVersion, file.ServerRelativeUrl)
        {
            this.file = file;
            this.parentFolder = parentFolder;
            this.parentList = parentFolder.AveList;
            this.parentWeb = parentFolder.AveList.ParentWeb;
            this.parentSite = parentFolder.AveList.ParentSite;
        }

        public void ExportSPFile(IAveBackupStream stream)
        {
            using (AvePerformanceScope pc1 = new AvePerformanceScope("ArchiverDeletion.LinkDocument.ExportSPFile"))
            {
                //ExportParentInfo(stream);
                ExportSPFileVersion(stream, file.UIVersion);
            }
        }

     

        private void ExportSPFileVersion(IAveBackupStream stream, int version)
        {
            using (AvePerformanceScope pc1 = new AvePerformanceScope("ArchiverDeletion.LinkDocument.ExportSPFileVersion"))
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
                if (linkFileContent == null)
                {
                    stream.FlushMetadata(0);
                }
                else
                {
                    stream.FlushMetadata(linkFileContent.Length);
                    stream.WriteContent(linkFileContent, 0, linkFileContent.Length);
                }
            }
        }

       


        public void Dispose()
        {
        }
    }
}
