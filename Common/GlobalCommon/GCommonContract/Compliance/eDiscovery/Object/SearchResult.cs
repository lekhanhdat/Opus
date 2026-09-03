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

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object;
    [DataContract]
    public class SearchResult
    {
        [DataMember]
        public string PathMD5;

        [DataMember]
        public string SiteURL { get; set; }

        [DataMember]
        public string SubJobId { get; set; }

        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Author { get; set; }
        [DataMember]
        public string Size { get; set; }
        [DataMember]
        public SharePointType ResultType { get; set; }
        [DataMember]
        public string IsDocument { get; set; }
        [DataMember]
        public string VersionString { get; set; }
        [DataMember]
        public List<string> HoldNames { get; set; }
        [DataMember]
        public List<HoldItemDto> HoldItems { get; set; }
        [DataMember]
        public string Summary { get; set; }
        [DataMember]
        public string Created { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public string ModifiedBy { get; set; }
        [DataMember]
        public Guid SiteId { get; set; }
        [DataMember]
        public Guid WebId { get; set; }
        [DataMember]
        public Guid ListId { get; set; }
        [DataMember]
        public Guid ItemId { get; set; }
        [DataMember]
        public string FarmName { get; set; }
        public SearchResult()
        {
            HoldNames = new List<string>();
        }
    }

    [DataContract]
    public class SearchResultPaging
    {

        [DataMember]
        public int PlanType { get; set; }

        //总记录条数
        [DataMember]
        public int TotalCount { get; set; }

        //总页数
        [DataMember]
        public int TotalPage { get; set; }

        //当前页数
        [DataMember]
        public int CurrentPage { get; set; }

        //每一页显示条数
        [DataMember]
        public int EveryPageCount { get; set; }

        [DataMember]
        public OrderColumn OrderColumn { get; set; }

        [DataMember]
        public OrderType OrderType { get; set; }

        [DataMember]
        public string SearchKeyWord { get; set; }

        [DataMember]
        public SharePointType TypeFilter { get; set; }

        //当前页记录
        [DataMember]
        public List<SearchResult> Results { get; set; }
    }




}
