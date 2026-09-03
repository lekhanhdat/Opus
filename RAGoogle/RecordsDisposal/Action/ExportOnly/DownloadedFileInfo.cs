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
using RAGoogle.Models;

namespace RAGoogle.RecordsDisposal.Action.ExportOnly
{
    public class DownloadedFileInfo
    {
        public string Id { get; set; }
        public string FormattedFileVersionName { get; set; }
        public string FileName { get; set; }
        public string LocalPath { get; set; }
        public string VersionId { get; set; }
        public string VersionName { get; set; }
        public DateTime ModifiedTime { get; set; }
        public bool IsCurrentVersion { get; set; }
        public string DriveName { get; set; }
        public string ParentId { get; set; }
        public string ParentIds { get; set; }
        public string FileExtension { get; set; }
        public string DownloadFileExtension { get; set; }
        public string MimeType { get; set; }
        public long? Size { get; set; }
        public List<string> Labels { get; set; }
        public string Path { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string MemberEmail { get; set; }
        public string ModifiedBy { get; set; }
        public string Description { get; set; }
        public string DriveId { get; set; }
        public List<Permissions> Permissions { get; set; }
        public List<LabelApplyInfo> LabelApplyInfos { get; set; }
        public class LabelApplyInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public string GoogleDrivePathUrl { get; set; }
        public string OriginFileName { get; internal set; }

        public DownloadedFileInfo()
        {
            Labels = new List<string>();
        }
    }

}
