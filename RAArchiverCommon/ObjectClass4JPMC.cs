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
using AvePoint.RA.Contract;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public class ArchiveApproveReport4JPMC
    {
        public string NodeId { get; set; }

        public string LeafName { get; set; }

        public string FullPath { get; set; }

        public string ParentId { get; set; }

        public string ScanJobID { get; set; }

        public string SortTicks { set; get; }//use for ReadFrom db method

        public long ScanTime { get; set; }

        public long ArchivedTime { get; set; }

        public long LastModifiedTime { get; set; }

        public int LibRowId { get; set; }

        public int NodeType { get; set; }

        public int SPNodeLevel { get; set; }

        public int CacheNodeType { get; set; }

        public string RuleId { get; set; }

        public string RuleName { get; set; }

        public long DocumentSize { set; get; }

        public long Created { set; get; }

        public string CreatedBy { set; get; }

        public long Modified { set; get; }

        public string ModifiedBy { set; get; }

        public bool ActionTaken { set; get; }

        public string SiteUrl { get; set; }

        public string WebID { get; set; }

        public string ListID { get; set; }

        public string ClassCode { get; set; }

        public string CountryCode { get; set; }

        public string RecordStatus { get; set; }
    }

    public class ArchiveApproveReport4JPMGroupBy {
        public string ClassCode { get; set; }

        public string CountryCode { get; set; }

        public string RecordStatus { get; set; }
        
        public long TotalCount { get; set; }
    }

    public class ArchiveApproveReport4JPMTotalSize
    {
        public long TotalSize{ get; set; }
        public long TotalCount { get; set; }
    }
}
