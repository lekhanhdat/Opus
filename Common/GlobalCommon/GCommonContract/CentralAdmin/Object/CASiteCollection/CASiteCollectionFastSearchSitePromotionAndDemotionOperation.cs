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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASiteCollectionFastSearchSitePromotionAndDemotionOperation : CAOperation
    {
        [DataMember]
        public string SiteCollectionURL { get; set; }
        [DataMember]
        public List<SitePromotionAndDemotionInfo> SitePromotions { get; set; }
        [DataMember]
        public List<SitePromotionAndDemotionInfo> SiteDemotions { get; set; }
        /// <summary>
        /// 判断是否是操作Promotion,false则为操作Demotion
        /// </summary>
        [DataMember]
        public bool IsPromotionOperation { get; set; }
        /// <summary>
        /// 所有的可选择的UserContexts
        /// </summary>
        [DataMember]
        public List<String> AllUserContexts { get; set; }
        /// <summary>
        /// 标志是Add还是Edit操作
        /// </summary>
        [DataMember]
        public Mode SitePromotionAndDemotionMode { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SitePromotionAndDemotionInfo : IComparable<SitePromotionAndDemotionInfo>
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string NewTitle { get; set; }
        [DataMember]
        public List<String> SelectedSiteURLs { get; set; }
        [DataMember]
        public List<String> SelectedUserContexts { get; set; }
        [DataMember]
        public string StartDate { get; set; }
        [DataMember]
        public string EndDate { get; set; }
        [DataMember]
        public string Keyword { get; set; }

        #region IComparable<SitePromotionAndDemotionInfo> Members

        public int CompareTo(SitePromotionAndDemotionInfo other)
        {           
            if (other == null) return 1;           
            return string.Compare(this.Title, other.Title, StringComparison.Ordinal);
        }

        #endregion
    }
}
