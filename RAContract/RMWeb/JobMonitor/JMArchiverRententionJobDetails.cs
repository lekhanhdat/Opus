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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMArchiverRententionJobDetails : JMJobDetails
    {
        public string SiteUrl { get; set; }
        public string JobId { get; set; }
        public string SrcStorageName { get; set; }
        public string DesStorageName { get; set; }
        public string Size { get; set; }
        public string SizeStr { get; set; }
        public string Action { get; set; }
    }

    public class JMArchiverRententionDashboardDetails : JMArchiverRententionJobDetails
    {
        public JMArchiverRententionDashboardDetails(JMArchiverRententionJobDetails baseObject)
        {
            SiteUrl = baseObject.SiteUrl;
            FileName = Path.GetFileName(SiteUrl);
            JobId = baseObject.JobId;
            SrcStorageName = baseObject.SrcStorageName;
            DesStorageName = baseObject.DesStorageName;
            Size = baseObject.Size;
            SizeStr = baseObject.SizeStr;
            Action = baseObject.Action;
        }
        public string FileName { get; set; }
        public int SourceFlag { get; set; }
        public string RetentionSource { get; set; }
        public int RetentionKeepDate { get; set; }
        public int RetentionKeepDateUnit { get; set; }
    }

    public class JMDeleteOrphanDatasJobDetails : JMJobDetails
    {
        public string SiteUrl { get; set; }
        public string JobId { get; set; }
        public string Size { get; set; }
        public string SizeStr { get; set; }
    }

    public class JMArchiverRententionMigrationDetails : JMArchiverRententionJobDetails
    {
        public string SharePointUrl { get; set; }
        public string BlobPath { get; set; }
    }
}
