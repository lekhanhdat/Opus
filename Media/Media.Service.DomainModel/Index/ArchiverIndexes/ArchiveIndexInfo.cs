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
using AvePoint.Media.Service.DomainModel;
using System.Text;

namespace RAFileSystem.FileSystem.FileSystem.Backup.CoreIndex.CoreIndexCommon
{
    [Table(IndexConstants.TableNameArchiveIndexInfo)]
    public class ArchiveIndexInfo
    : IndexBase
    {
        [Column("COL_GUID")]
        public String Guid { get; set; }

        [Column("COL_UNC_PATH")]
        public String UNCPath { get; set; }
        [Column("COL_CONNECTIONID")]
        public String ConnectionId { get; set; }

        [Column("COL_JOB_ID")]
        public String JobId { get; set; }
        [Column("COL_ARCHIVER_TIME")]
        public long ArchiveTime { get; set; }
        [Column("COL_CONNECTION_PATH")]
        public String ConnectionPath { get; set; }

        [Column("COL_EXTENSION_3")]
        public int Extension3 { get; set; }

        [Column("COL_EXTENSION_4")]
        public long Extension4 { get; set; }

        [Column("COL_EXTENSION_5")]
        public long Extension5 { get; set; }

        [Column("COL_EXTENSION_6")]
        public String Extension6 { get; set; }

        [Column("COL_EXTENSION_7")]
        public String Extension7 { get; set; }

        [Column("COL_EXTENSION_8")]
        public String Extension8 { get; set; }


        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ArchiverJobInfoIndex: JobId:");
            sb.Append(this.JobId);
            return sb.ToString();
        }

        public override Dictionary<String, Object> GenerateInsertDatabaseParameters()
        {
            Dictionary<String, Object> dic = new Dictionary<String, Object>();
            dic.Add("@COL_GUID", this.Guid);
            dic.Add("@COL_JOB_ID", this.JobId);
            dic.Add("@COL_UNC_PATH", this.UNCPath);
            dic.Add("@COL_CONNECTION_PATH", this.ConnectionPath);
            dic.Add("@COL_ARCHIVER_TIME", this.ArchiveTime);
            dic.Add("@COL_CONNECTIONID", this.ConnectionId);
            dic.Add("@COL_EXTENSION_3", this.Extension3);
            dic.Add("@COL_EXTENSION_4", this.Extension4);
            dic.Add("@COL_EXTENSION_5", this.Extension5);
            dic.Add("@COL_EXTENSION_6", this.Extension6);
            dic.Add("@COL_EXTENSION_7", this.Extension7);
            dic.Add("@COL_EXTENSION_8", this.Extension8);
            return dic;
        }
    }
}
