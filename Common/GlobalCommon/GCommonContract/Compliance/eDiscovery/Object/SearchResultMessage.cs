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



using System.Collections.Generic;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object;

    [DataContract]
    public class SearchResultMessage : EDBaseMessage
    {
        [DataMember]
        public List<SearchResultPage> Results { get; set; }
        [DataMember]
        public int DocAveStartNumber { get; set; }
        [DataMember]
        public List<QueryInfo> QueryInfoList { get; set; }
        [DataMember]
        public int CountPerPage { get; set; }
        /// <summary>
        /// 此次返回结果中是否包含最后一页
        /// </summary>
        [DataMember]
        public bool HasLastPage { get; set; }
        [DataMember]
        public int HasRetrievedCount { get; set; }
        public SearchResultMessage()
        {
            Results = new List<SearchResultPage>();
            QueryInfoList = new List<QueryInfo>();
        }

        [DataMember]
        public bool OutOfSize { get; set; }

        [DataMember]
        public bool NotHaveAvailableAgent { get; set; }
    }

    [DataContract]
    public class SearchResultPage
    {
        /// <summary>
        /// Gui需要缓存此属性,用于Manager直接定位Data Source位置.
        /// </summary>
        [DataMember]
        public SearchDataSource PageDownSource { get; set; }

        /// <summary>
        /// Gui需要缓存此属性,用于Manager直接定位Data Source位置.
        /// </summary>
        [DataMember]
        public SearchDataSource PageUpSource { get; set; }

        [DataMember]
        public List<SearchResult> Results { get; set; }
        [DataMember]
        public List<PageInfo> PageInfoList { get; set; }
        [DataMember]
        public int PageNumber { get; set; }
        public SearchResultPage()
        {
            Results = new List<SearchResult>();
            PageInfoList = new List<PageInfo>();
        }
    }
    /// <summary>
    /// 传给sharepoint API的查询语句信息
    /// </summary>
    [DataContract]
    public class QueryInfo
    {
        /// <summary>
        /// 收集页面信息，整理出来的查询语句 格式：contentclass:STS_Listitem_ size>0 and not(contenttype:folder)
        /// </summary>
        [DataMember]
        public string QueryText { get; set; }
        /// <summary>
        /// 基于sharepoint的location的信息
        /// </summary>
        [DataMember]
        public LocationInfo Location { get; set; }
        [DataMember]
        public string SSAName { get; set; }
        /// <summary>
        /// 基于sharepoint的location在当前查询的使用情况
        /// </summary>
        [DataMember]
        public LocationUsage LocationUsageStatus { get; set; }
        /// <summary>
        /// 判断是sharepoint ssa还是fast ssa
        /// </summary>
        [DataMember]
        public bool IsSPSSA { get; set; }
    }

    [DataContract]
    public class LocationInfo
    {
        [DataMember]
        public string LocationName { get; set; }
        /// <summary>
        /// 如果该location已经读完 StartItem的值比该location含有记录的条数多1
        /// </summary>
        [DataMember]
        public int StartItem { get; set; }
    }

    [DataContract]
    public class PageInfo
    {
        [DataMember]
        public int PageNumber { get; set; }
        [DataMember]
        public int SPStartNumber { get; set; }
        [DataMember]
        public int SPEndNumber { get; set; }
        [DataMember]
        public string QueryText { get; set; }
        [DataMember]
        public string LocationName { get; set; }
        [DataMember]
        public string SSAName { get; set; }
    }

    [DataContract]
    public class LocationUsage
    {
        [DataMember]
        public bool HasReadToEnd { get; set; }
        /// <summary>
        /// 每次取完数据之后，所到达的page数，对每个location来说这个数累加，最后剩的数据不足一页也加一 
        /// </summary>
        [DataMember]
        public int PageNumber { get; set; }
        /// <summary>
        /// 最后不足一页的条数
        /// </summary>
        [DataMember]
        public int recordCountNotEnoughOnePage { get; set; }
    }
}
