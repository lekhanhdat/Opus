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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.CommonFilter;

namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object
{
    public class EORestoreSearchFilterDto
    {
        [DataMember]
        [XmlElement("Filters")]
        public List<FilterPolicy> Filters { get; set; }

        [DataMember]
        [XmlAttribute]
        public String AdvancedExpression { get; set; }

        [DataMember]
        [XmlAttribute]
        public EOAdvanceSearchType AdvanceSearchType { get; set; }

        [DataMember]
        public Int64 StartTime { get; set; }

        [DataMember]
        public Int64 EndTime { get; set; }

        /// <summary>
        /// 做CEIP使用,TimeStampType.None = 0代表Customized类型.
        /// </summary>
        [DataMember]
        public EOTimeStampType TimeRangeOption { get; set; }

        public override String ToString()
        {
            return String.Format("Advanced Expression: {0}, Advance Search Type: {1}",
                this.AdvancedExpression,
                this.AdvanceSearchType.ToString());
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOAdvanceSearchType
    {
        [EnumMember]
        TimeBaseSearch,
        [EnumMember]
        ObjectBaseSearch,
    }
}
