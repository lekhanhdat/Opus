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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    /// <summary>
    /// 承载分页信息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PageInfo
    {
        [DataMember]
        public int CurrentPage { get; set; }

        [DataMember]
        public int TotalPage { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int TotalCount { get; set; }

        [DataMember]
        public int ItemsPerPage { get; set; }

        [DataMember]
        public bool PageSizeChanged { get; set; }

        public override string ToString()
        {
            return string.Format("PageInfo[CurrentPage {0}, PageSize {1}, TotalCount {2}]", CurrentPage, PageSize, TotalCount);
        }
    }
}