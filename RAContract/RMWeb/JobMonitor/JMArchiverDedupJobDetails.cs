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
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMArchiverDedupJobDetails : JMJobDetails
    {
        public long DedupTime { get; set; }  //Dedup time
        public string DedupTimeStr { get; set; } // convert by Date
        public long Size { get; set; }
        public string SizeStr { get; set; }
        public string SrcURL { get; set; }  // DAO: SrcURL
        public string SubJobId { get; set; }
        public string Name { get; set; } // Name, DAO: Remark9
        public long ModifyTime { get; set; } // modify time, DAO: Remark10
        public string ModifyTimeStr { get; set; } // convert by Remark10
        public string BackupSubJobId { get; set; } //backup subsubjobid, DAO: Remark11
        public string NewFileStoragePath { get; set; } // Dedup Source file storage path, DAO: Remark12
        public string OldFileStoragePath { get; set; } // Duplicate file storage path, DAO: Remark13
    }
}
