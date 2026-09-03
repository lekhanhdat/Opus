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
    [DataContract (Namespace = ContractConstants.Namespace)]
    public  class CASiteCollectionSearchableColumnsOperation : CAOperation
    {
        [DataMember]
        public string SiteCollectionUrl { get; set; }
        /// <summary>
        /// 后台判断sharepoint里此功能是否激活
        /// </summary>
        [DataMember]
        public bool IsFeatureActived { get; set; }
        /// <summary>
        /// 前台传给后台的用户的是否激活的决定
        /// </summary>
        [DataMember]
        public bool EnableActiveFeature { get; set; }
        [DataMember]
        public List<NoCrawlColumnInfo> NoCrawlColumns { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NoCrawlColumnInfo : IComparable<NoCrawlColumnInfo>
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Group { get; set; }
        [DataMember]
        public bool Checked { get; set; }

        #region IComparable<NoCrawlColumnInfo> Members

        public int CompareTo(NoCrawlColumnInfo other)
        {           
            if (other == null) return 1;            
            return string.Compare(this.Title, other.Title, StringComparison.Ordinal);
        }

        #endregion
    }
}
