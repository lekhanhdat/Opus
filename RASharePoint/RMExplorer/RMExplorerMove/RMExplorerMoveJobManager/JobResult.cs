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
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class JobResult
    {
        public JobDetailsStatus Status { get; set; } = JobDetailsStatus.Successful;

        public string ErrorMessage { get; set; }
        public MoveDestStub DestStub { get; set; }

    }
    public class MoveDestStub
    {
        public Guid OriginalScopeId { get; internal set; }
        public Guid OriginalNodeId { get; internal set; }
        public Guid ListId { get; internal set; }
        public Guid WebId { get; internal set; }
        public Guid FolderId { get; internal set; }
        public string FullPath { get; internal set; }
        public int ItemRowId { get; internal set; }
        public Guid ItemId { get; internal set; }
        public string LeafName { get; internal set; }
        public long DateModified { get; internal set; } 
        public string DirPath { get; internal set; }
        public Guid ParentId { get; internal set; }
        public int DestFlag { get; internal set; }

        //Add for job report destination url
        public int UIVersion { get; internal set; }
    }
}
