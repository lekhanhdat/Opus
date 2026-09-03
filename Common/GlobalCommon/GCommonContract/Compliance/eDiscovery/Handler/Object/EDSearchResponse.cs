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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract]
    public class EDSearchResponse : EDiscoveryResponse
    {
        /// <summary>
        /// 在Service端应用,用于多数据源时,对结果对象的初始化.
        /// </summary>
        public bool IsCleanResult { get; set; }

        /// <summary>
        /// Service端应用,剩余记录数,这个是用于在各个数据源获得数据后,如果少页的话,根据这个属性向下一个数据源获得补全.
        /// </summary>
        public int HasRetrievedCount { get; set; }

        /// <summary>
        /// 返回的页面信息,按照页分类.
        /// </summary>
        [DataMember]
        public List<SearchResultPage> ResultPages { get; set; }

        [DataMember]
        public bool OutOfSize { get; set; }

        [DataMember]
        public bool NotHaveAvailableAgent { get; set; }

        /// <summary>
        /// 当前结果集中释放包含最后一页.
        /// </summary>
        [DataMember]
        public bool IsLastPage { get; set; }

        [DataMember]
        public List<QueryInfo> QueryInfoList { get; set; } //Agent下次查询时的应用信息.

        /// <summary>
        /// 用于多数据源时,使用的干净可以用的返回结果.
        /// </summary>
        /// <returns></returns>
        public static EDSearchResponse CreateCleanOne()
        {
            EDSearchResponse resp = new EDSearchResponse();
            resp.IsLastPage = false;
            resp.IsCleanResult = true;
            resp.ResultPages = new List<SearchResultPage>();
            return resp;
        }
    }
}
