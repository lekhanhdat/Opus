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
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RestoreJob.Restore.Content
{
    public class RestoreSettingAndTree
    {
        public List<SPTreeNodeDto> Tree;
        public RestoreInfo Setting;
        public string SiteGroupId;
        public string JobId;//for recenter or simulate restore job
        public bool IsEndUserJob;//for recenter
        public string ConnectionString;//for recenter
        public bool IsOpusArchivedDownloadJob;
        //public string IndexString;
        public int NodeType;
        public string RealRunJobUser;//for recenter
        public bool IsRecenterExport;//for recenter
        public string oopStubUrl;//for recenter
        public string BackUpJobId;//for recenter
        public bool IsSearchAllRestore; // for restore all data
        public bool IsRestoreToSPOLocation;
    }
}
