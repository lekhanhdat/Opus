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



namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    using System.ComponentModel;
    using System.Text;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using global::Media.Common;
    using RAFileSystem.FileSystem.FileSystem.Backup.Storage;
    #endregion

    public class ArchiverBrowseInfo
    {
        public String WebAppUrl { get; set; }
        public String SiteUrl { get; set; }
        public String Path { get; set; }
        public Int64 StartTime { get; set; }
        public Int64 EndTime { get; set; }
        public Int32 OffSet { get; set; }
        public Int32 Length { get; set; }
        public String BackupJobId { get; set; }
        public String FarmName { get; set; }
        public String BackupPlanId { get; set; }
        public String BackupCycleID { get; set; }
        public String StorageInfo { get; set; }
        public TreeMode TreeMode { get; set; }
        public GCommon.Contract.Storage.Entity.StorageDeviceDto IndexLogicalDevice { get; set; }

        public ArchiverBrowseInfo()
        { }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ArchiverBrowseInfo: ");
            sb.Append(FarmName);
            sb.Append(" ");
            sb.Append(SiteUrl);
            sb.Append(" ");
            sb.Append(WebAppUrl);
            sb.Append(" ");
            sb.Append(Path);
            return sb.ToString();
        }
    }
}