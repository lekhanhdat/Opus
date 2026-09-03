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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    #endregion

    [Table(IndexConstants.TableNameArchiveSiteConfiguration)]
    public class ArchiverSiteConfigurationIndex
          : IndexBase
    {
        [Column("COL_GUID")]
        public String Guid { get; set; }

        [Column("COL_JOB_ID")]
        public String JobId { get; set; }

        [Column("COL_ARCHIVE_TIME")]
        public long ArchiveTime { get; set; }

        [Column("COL_SITE_INFO")]
        public String SiteInfo { get; set; }

        [Column("COL_SITE_URL")]
        public String SiteUrl { get; set; }

        [Column("COL_STATUS")]
        public int Status { get; set; }

        [Column("COL_REMARK1")]
        public String Remark1 { get; set; }

        [Column("COL_REMARK2")]
        public int Remark2 { get; set; }

        [Column("COL_REMARK3")]
        public long Remark3 { get; set; }

        [Column("COL_VERSION")]
        public int Version { get; set; }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ArchiveSiteConfigurationIndex: JobId:");
            sb.Append(this.JobId);
            sb.Append(" SiteInfo: ");
            sb.Append(this.SiteInfo);
            sb.Append(" SiteUrl: ");
            sb.Append(this.SiteUrl);
            sb.Append(" Status: ");
            sb.Append(this.Status);
            sb.Append(" Version: ");
            sb.Append(this.Version);
            return sb.ToString();
        }

        public override Dictionary<String, Object> GenerateInsertDatabaseParameters()
        {
            Dictionary<String, Object> dic = new Dictionary<String, Object>();
            dic.Add("@COL_GUID", this.Guid);
            dic.Add("@COL_JOB_ID", this.JobId);
            dic.Add("@COL_ARCHIVE_TIME", this.ArchiveTime);
            dic.Add("@COL_SITE_INFO", this.SiteInfo);
            dic.Add("@COL_SITE_URL", this.SiteUrl);
            dic.Add("@COL_STATUS", this.Status);
            dic.Add("@COL_REMARK1", this.Remark1);
            dic.Add("@COL_REMARK2", this.Remark2);
            dic.Add("@COL_REMARK3", this.Remark3);
            dic.Add("@COL_VERSION", this.Version);
            return dic;
        }
    }
}