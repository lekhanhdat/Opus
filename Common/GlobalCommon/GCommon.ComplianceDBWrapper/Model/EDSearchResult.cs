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

namespace AvePoint.GCommon.ComplianceDBWrapper.Model
{
    public class EDSearchResult
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public long Size { get; set; }

        public SharePointType ResultType { get; set; }
        public string IsDocument { get; set; }

        public string VersionString { get; set; }

        public List<string> HoldNames { get; set; }

        public string Summary { get; set; }

        public DateTime Created { get; set; }

        //public DateTime Modified { get; set; }

        public string Location { get; set; }

        public string ModifiedBy { get; set; }

        public string FarmName { get; set; }

        public string SiteURL { get; set; }

        public string PathMD5 { get; set; }

        public string SubJobID { get; set; }
    }

    public enum SharePointType
    {
        None = 0,

        Document = 1,

        Item = 2,

        DocumentVersion = 4,

        ItemVersion = 8
    }

    public class SearchResultPaging
    {
        //总记录条数
        public int TotalCount { get; set; }

        //总页数
        public int TotalPage { get; set; }

        //当前页数
        public int CurrentPage { get; set; }

        //每一页显示条数
        public int EveryPageCount { get; set; }

        //当前页记录
        public List<EDSearchResult> Results { get; set; }
    }
}